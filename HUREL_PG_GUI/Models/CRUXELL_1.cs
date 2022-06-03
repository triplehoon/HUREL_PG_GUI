using CyUSB;
using HUREL_PG_GUI.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUREL_PG_GUI.Models
{
    public partial class CRUXELL_Original
    {
        public delegate void DataPassEventHandler(string data);
        public event DataPassEventHandler DataPassEvent;


        ///// ------------------------- /////
        ///// --- Directory Setting --- /////
        ///// ------------------------- /////
        List<double[]> corrFactors = new List<double[]>(4);
        string corrFactorsDirectory = @"C:\Users\admin\Desktop\Jaerin\00. GUI_Project\HUREL_PG_GUI\corrFactors.txt";


        ///// --------------------------- /////
        ///// --- Important Parameter --- /////
        ///// --------------------------- /////
        public bool is_measuring;
        private string PpxInfo = "16";
        private string QueueInfo = "256";
        //private string QueueInfo = "128"; // MSPGC_GUI 2022-05-27


        ///// -------------------------- /////
        ///// --- Important Instance --- /////
        ///// -------------------------- /////
        //public Variables_FPGA variables_FPGA = new Variables_FPGA();
        public int tb_0x11, tb_0x12, tb_0x13, tb_0x14, tb_0x15, tb_0x16, tb_0x17, tb_0x18, tb_0x19, tb_0x1a;
        public PGdists PGdistribution = new PGdists();  // View에 보여지는 PG distribution (Bar Chart)
        CountMode countMode;

        ///// --------------------------- /////
        ///// ------- Static Data ------- /////
        ///// --------------------------- /////
        public List<PGStruct> PG_raw = new List<PGStruct>();

        ///// --------------------------------- /////
        ///// ------- SMC Configuration ------- /////
        ///// --------------------------------- /////
        private int SMC_TimeIndex = 1;
        private int SMC_LayerNumber = 1;
        private int SMC_TuningIndex = 1;
        private bool SMC_isLayer = false;
        private double SMC_Threshold = 2.5;
        private int[] SMC_Trigger_temp = new int[2];


        /// <summary>
        /// Initialize
        /// </summary>
        public CRUXELL_Original()
        {
            //Console.WriteLine("CRUXELL_1 Loaded");
            //MessageBox.Show("CRUXELL_1.cs Initialized");

            init_data_and_buffer();
            init_USB();

            countMode = CountMode.Continue;
        }



        #region Command

        public bool Command_MonitoringStart(out string status, string binaryFileDir)
        {

            if (SelectedDevice.DeviceName == "")
            {
                Trace.WriteLine("No selected Devices");
                status = "No selected Devices";
                return false;
            }
            
            if (is_measuring == false)
            {
                DIR_BinaryFile = binaryFileDir;
                status = "DAQ Setting...";

                trash_count = default_trash_count;
                Command_Reset_PGdist();
                get_data_from_box();
                timer_start();

                Trace.WriteLine("HY : Start");

                // 1. 입력칸 비활성화 및 설정
                Trace.WriteLine("HY : [Try] input_disable ");
                input_disable(false);

                byte[] Item;
                while (test_buffer.TryTake(out Item, 1))
                {
                }

                Trace.WriteLine("HY : [Try] Send 0000... ");
                usb_setting(3); // send 00000

                // 2. 버퍼 비우기 
                Trace.WriteLine("HY : [Try] XferData reset loop");
                EndPointListSelectIdx = 1;
                EndPoint.TimeOut = 500;

                bool bResult = true;
                int xferLen = 4096;
                byte[] inData = new byte[xferLen];
                while (bResult)
                {
                    bResult = EndPoint.XferData(ref inData, ref xferLen);
                }
                EndPoint.TimeOut = 500;

                Trace.WriteLine("HY : [Try] send setting value");
                // 3. 셋팅값 전송
                usb_setting(1);

                // 4. 데이터 Read
                EndPointListSelectIdx = 1;

                BufSz = EndPoint.MaxPktSize * Convert.ToUInt16(PpxInfo);      // 16 ?
                p_error = EndPoint.MaxPktSize;                                // 16384 ?
                QueueSz = Convert.ToUInt16(QueueInfo); // 값 1로 고정         // 128 ? 
                PPX = Convert.ToUInt16(PpxInfo);                              // 16 ?

                EndPoint.XferSize = BufSz;
                Trace.WriteLine($"HY : [Try] BufSz[{BufSz}]QueueSz[{QueueSz}]MaxPktSize[{EndPoint.MaxPktSize}]"); // 262144, 128, 16384 ?
                if (EndPoint is CyIsocEndPoint)
                    IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
                else
                    IsoPktBlockSize = 0;

                bRunning = true;
                bFinalCall = 0;
                thread_run = true;

                Trace.WriteLine("HY : [Try] Start XferThread");
                b_first_recevied = 0;
                tListen = new Task(new Action(XferThread));
                tListen.Start();
                Trace.WriteLine("HY : [Try] Start ParsingThread");

                is_measuring = true;
                
                //init_bin_save_dir();
                tParsing = new Task(new Action(ParsingThread));
                tParsing.Start();

                status = "Data Acquisition Start!";
                return true;
            }
            else
            {
                Trace.WriteLine("HY : [Try] Stop Button Clicked / send Final Call");
                bFinalCall = 2;
                bRunning = false; // 20.04.22 test

                // usb단
                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;

                Trace.WriteLine("HY : [Try] tListen.Wait()");
                tListen.Wait();
                tListen = null;

                Trace.WriteLine("HY : [Try] tParsing.Wait()");
                thread_run = false;
                tParsing.Wait();
                tParsing = null;

                Trace.WriteLine("HY : [Try] send stop 0000...");
                usb_setting(3); // 모든처리 끝났을때 stop

                DATA_BUFFER_read_count = 0;
                read_buffer_count = 0;

                terminate_file_save();
                Trace.WriteLine("HY : [Try] input_disable enabled");
                input_disable(true);    // 입력칸 enable
                //timer_end();
                b_first_recevied = 99;

                is_measuring = false;

                status = "Idle";
                return false;
            }
        }

        public bool Command_Convert()
        {
            open_bin_path = ShowFileOpenDialog();
            if (open_bin_path.CompareTo("") == 0)
                return false;
            //BT_convert.Text = "Wait..";
            read_buffer_count = 0;
            trig_on_off = 0;
            pri_et = 0;
            temp_trig_time = 0;
            is_init = 0;
            init_file_save();

            is_convert = true;
            tConvert = null;
            tConvert = new Task(new Action(ConvertThread));
            tConvert.Start();

            tConvert.Wait();
            is_convert = false;
            //BT_convert.Text = "Convert";

            return true;
        }

        public void Command_Reset_PGdist() // View에 보여지는 CRUXELL.PGdistribution 의 내부 데이터
        {
            for (int i = 0; i < 36; ++i)
            {
                PGdistribution.cnt_row1[i] = 0;
                PGdistribution.cnt_row2[i] = 0;
                PGdistribution.cnt_row3[i] = 0;
                PGdistribution.cnt_row4[i] = 0;

                PGdistribution.cnt_top[i] = 0;
                PGdistribution.cnt_bot[i] = 0;
            }
            for (int i = 0; i < 72; ++i)
            {
                PGdistribution.cnt_PGdist72[i] = 0;
            }
            for (int i = 0; i < 71; ++i)
            {
                PGdistribution.cnt_PGdist71[i] = 0;
            }
        }

        #endregion

        #region Sub-Functions (of Command)

        private void GetPGdistribution() // View에 보여지는 CRUXELL.PGdistribution 의 내부 데이터 업데이트
        {
            #region Temp Variables Definition
            double[] temp_cnt_row1 = new double[36];
            double[] temp_cnt_row2 = new double[36];
            double[] temp_cnt_row3 = new double[36];
            double[] temp_cnt_row4 = new double[36];

            double[] temp_cnt_top = new double[36];
            double[] temp_cnt_bot = new double[36];

            double[] temp_cnt_PGdist72 = new double[72];
            double[] temp_cnt_PGdist71 = new double[71];
            #endregion

            // SDATA_BUFFER 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  98  97  96  95  94  93  92  91  90  89  88  87  86  85  84  83  82  81 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 116 115 114 113 112 111 110 109 108 107 106 105 104 103 102 101 100  99 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 134 133 132 131 130 129 128 127 126 125 124 123 122 121 120 119 118 117 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 152 151 150 149 148 147 146 145 144 143 142 141 140 139 138 137 136 135 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            // ChCounts 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            #region MissingVal (0, 18, 36, 27, 26) // (1, 19, 37, 28, 27)

            //SDATA_BUFFER[0] = SDATA_BUFFER[1];
            //SDATA_BUFFER[18] = SDATA_BUFFER[19];
            //SDATA_BUFFER[36] = SDATA_BUFFER[37];

            //double sum1 = 0;
            //double sum2 = 0;
            //double ratio = 0;

            //for (int k = 1; k < 18; k++)
            //{
            //    sum1 += SDATA_BUFFER[k] + SDATA_BUFFER[81 + k];
            //    sum2 += SDATA_BUFFER[18 + k] + SDATA_BUFFER[99 + k];
            //}
            //ratio = sum2 / sum1;

            //SDATA_BUFFER[27] = (ushort)Math.Round(SDATA_BUFFER[9] * ratio);
            //SDATA_BUFFER[26] = (ushort)Math.Round(SDATA_BUFFER[8] * ratio);

            //SDATA_BUFFER[26] = SDATA_BUFFER[31];
            //SDATA_BUFFER[20] = SDATA_BUFFER[22];

            #endregion

            for (int i = 0; i < 18; i++)
            {
                temp_cnt_row1[i] = (double)SDATA_BUFFER[98 - i];
                temp_cnt_row2[i] = (double)SDATA_BUFFER[116 - i];
                temp_cnt_row3[i] = (double)SDATA_BUFFER[134 - i];
                temp_cnt_row4[i] = (double)SDATA_BUFFER[152 - i];

                temp_cnt_row1[i + 18] = (double)SDATA_BUFFER[17 - i];
                temp_cnt_row2[i + 18] = (double)SDATA_BUFFER[35 - i];
                temp_cnt_row3[i + 18] = (double)SDATA_BUFFER[53 - i];
                temp_cnt_row4[i + 18] = (double)SDATA_BUFFER[71 - i];
            }

            for (int i = 0; i < 36; i++)
            {
                temp_cnt_top[i] = temp_cnt_row1[i] + temp_cnt_row2[i];
                temp_cnt_bot[i] = temp_cnt_row3[i] + temp_cnt_row4[i];
            }

            for (int i = 0; i < 36; i++)
            {
                PGdistribution.cnt_row1[i] += temp_cnt_row1[i];
                PGdistribution.cnt_row2[i] += temp_cnt_row2[i];
                PGdistribution.cnt_row3[i] += temp_cnt_row3[i];
                PGdistribution.cnt_row4[i] += temp_cnt_row4[i];

                PGdistribution.cnt_top[i] += temp_cnt_top[i];
                PGdistribution.cnt_bot[i] += temp_cnt_bot[i];
            }

            for (int i = 0; i < 36; i++)
            {
                PGdistribution.cnt_PGdist72[2 * i + 1] = PGdistribution.cnt_top[i];
                PGdistribution.cnt_PGdist72[2 * i] = PGdistribution.cnt_bot[i];
            }

            for (int i = 0; i < 71; i++)
            {
                PGdistribution.cnt_PGdist71[i] = (PGdistribution.cnt_PGdist72[i] + PGdistribution.cnt_PGdist72[i + 1]) / 2;
            }
        }

        
        

        private void parsing_save()
        {
            {
                // [0]...[71] : B1 Ch0~71
                // (int)(72<<16) || (73) : Trigger input Start Time # 1	
                // (int)(74<<16) || (75) : Trigger input End Time # 1	
                // [76] : Trigger ADC data # 1       // used ADC data
                // [77] : Trigger input Signal # 1
                // [78 ~ 79] : Time_Count (int)(79 << 16) | 80
                // [80] : Reserved 

                // [81]...[152] : B2 Ch0~71
                // (int)(153<<16) || (154) : Trigger input Start Time # 2
                // (int)(155<<16) || (156) : Trigger input End Time # 2	
                // [157] : Trigger ADC data # 2
                // [158] : Trigger input Signal # 2
                // [159~160] : Time_Count (int)(160 << 16) | 159
                // [162] : RB mode
                // [163] : Pre margin
                // [164] : Post margin
                // [165 ~ 166] : interval (int)(166<<16) | (165)
            }

            if (is_convert) // Convert
            {
                if (is_init == 0)
                {
                    is_init = 1;
                    print_init(SDATA_BUFFER[162], SDATA_BUFFER[163], SDATA_BUFFER[164], ((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])));
                }

                if (SDATA_BUFFER[162] == 1) // Continuous mode
                {
                    bool is_empty = true;
                    for (int ch = 0; ch < 72; ++ch)
                    {
                        if (SDATA_BUFFER[ch] != 0)
                            is_empty = false;
                    }
                    for (int ch = 81; ch < 153; ++ch)
                    {
                        if (SDATA_BUFFER[ch] != 0)
                            is_empty = false;
                    }

                    if (!is_empty)
                    {
                        //int c1 = (read_buffer_count - 1) * ((int)((int)SDATA_BUFFER[162] << 16) | ((int)SDATA_BUFFER[161]));
                        //int c2 = read_buffer_count * ((int)((int)SDATA_BUFFER[162] << 16) | ((int)SDATA_BUFFER[161]));
                        //uint time_count = ((uint)((uint)SDATA_BUFFER[78] << 16) | ((uint)SDATA_BUFFER[79]));// Time_Count (int)(79 << 16) | 80
                        uint time_count = ((uint)((uint)SDATA_BUFFER[79] << 16) | ((uint)SDATA_BUFFER[78]));// Time_Count (int)(79 << 16) | 80

                        file_main.Write($"counts  {time_count,14}    {time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10),14}        ");
                        for (int ch = 0; ch < 72; ++ch) // board 0
                        {
#if false
                            file_main.Write($"{(SDATA_BUFFER[ch]):X4} ");
#else
                            file_main.Write($"{(SDATA_BUFFER[ch]):D5} ");
#endif
                        }
                        for (int ch = 0; ch < 72; ++ch) // board 1
                        {
#if false
                            file_main.Write($"{(SDATA_BUFFER[ch + 81]):X4} ");
#else
                            file_main.Write($"{(SDATA_BUFFER[ch + 81]):D5} ");
#endif
                        }
                        file_main.Write("\n");
                    }

                    // Trig On? Off?
                    uint error_filter = numberOfSetBits((uint)(SDATA_BUFFER[77])); // 노이즈때문에 튀는경우있어서 처리를 이렇게함
                    if (error_filter >= 8)
                        error_filter = 65535;
                    else
                        error_filter = 0;

                    uint temp_trig_on_off = error_filter;
                    if (temp_trig_on_off != trig_on_off) // 상태변화 catch를 위해
                    {
                        if (temp_trig_on_off == 65535)
                        {
                            // TrigOn 신호
                            //uint st_time = /*(read_buffer_count - 1) * temp_send_buffer[1] +*/ ((uint)((uint)SDATA_BUFFER[72] << 16) | ((uint)SDATA_BUFFER[73]));
                            uint st_time = /*(read_buffer_count - 1) * temp_send_buffer[1] +*/ ((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72]));//wj
                            temp_trig_time += st_time;
                            file_main.WriteLine($"TrigOn  {st_time,14}    {st_time,14}        trigger on signal");
                        }
                        else if (temp_trig_on_off == 0)
                        {
                            // TrigOff 신호
                            //uint ed_time = /*(read_buffer_count - 1) * temp_send_buffer[1] +*/ ((uint)((uint)SDATA_BUFFER[74] << 16) | ((uint)SDATA_BUFFER[75]));
                            uint ed_time = /*(read_buffer_count - 1) * temp_send_buffer[1] +*/ ((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74]));
                            temp_trig_time += ed_time;
                            file_main.WriteLine($"Trigoff {ed_time,14}    {ed_time,14}        trigger off signal");
                        }
                        trig_on_off = temp_trig_on_off;
                    }
                }
                else
                {
                    // TRIG mode
                    //uint new_st = ((uint)((uint)SDATA_BUFFER[72] << 16) | ((uint)SDATA_BUFFER[73]));
                    //uint new_et = ((uint)((uint)SDATA_BUFFER[74] << 16) | ((uint)SDATA_BUFFER[75]));
                    uint new_st = ((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72]));
                    uint new_et = ((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74]));
                    //new_st += pri_et;
                    //new_et += new_st;
                    file_main.Write($"{read_buffer_count,6}  {new_st,14}    {new_et,14}        ");
                    for (int ch = 0; ch < 72; ++ch) // board 0
                    {
                        file_main.Write($"{(uint)(SDATA_BUFFER[ch]),5} ");
                    }
                    for (int ch = 0; ch < 72; ++ch) // board 1
                    {
                        file_main.Write($"{(uint)(SDATA_BUFFER[ch + 81]),5} ");
                    }
                    pri_et = new_et;
                    file_main.WriteLine();
                }

                #region Convert할 때, adc에 대한 데이터 파일을 따로 생성할지 묻는 라디오버튼에 체크하게 되면 데이터 파일을 생성하도록 -> 추후 구현
                // ADC
                //if (CB_save_adc.Checked) // write file 
                //{
                //    //20210223 add time for contiuous mode
                //    uint new_et = 0;
                //    if (SDATA_BUFFER[162] == 1)
                //    {
                //        uint time_count = ((uint)((uint)SDATA_BUFFER[79] << 16) | ((uint)SDATA_BUFFER[78]));// Time_Count (int)(79 << 16) | 80
                //        new_et = time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10);
                //    }
                //    else
                //    {
                //        new_et = ((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74]));
                //    }

                //    file_adc.WriteLine($"{new_et,14}\t{(double)(SDATA_BUFFER[76]) / 4096.0 * 5.0}");
                //}
                #endregion
            }
            else // Real-time Monitoring mode
            {
                if (ViewModels.VM_MainWindow.selectedView == "SpotScanningView")
                {
                    if (SDATA_BUFFER[162] == 1) // Continuous mode
                    {
                        // SDATA_BUFFER 기준
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
                        //  98  97  96  95  94  93  92  91  90  89  88  87  86  85  84  83  82  81 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
                        // 116 115 114 113 112 111 110 109 108 107 106 105 104 103 102 101 100  99 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
                        // 134 133 132 131 130 129 128 127 126 125 124 123 122 121 120 119 118 117 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
                        // 152 151 150 149 148 147 146 145 144 143 142 141 140 139 138 137 136 135 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

                        // ChCounts 기준
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
                        //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
                        // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
                        // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
                        // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

                        #region Cruxell Code(Activated)

                        uint error_filter = numberOfSetBits((uint)(SDATA_BUFFER[77])); // 노이즈때문에 튀는경우있어서 처리를 이렇게함
                        if (error_filter >= 8)
                            error_filter = 65535;
                        else
                            error_filter = 0;

                        uint temp_trig_on_off = error_filter;
                        if (temp_trig_on_off != trig_on_off) // 상태변화 catch를 위해[Cruxell]
                        {
                            if (temp_trig_on_off == 65535)
                            {
                                current_tr++;
                            }
                            trig_on_off = temp_trig_on_off;
                        }

                        #endregion

                        PGStruct PG_raw_temp = new PGStruct();

                        for (int i = 0; i < 72; i++)
                        {
                            PG_raw_temp.ChannelCount[i] = SDATA_BUFFER[i];
                            PG_raw_temp.ChannelCount[i + 72] = SDATA_BUFFER[i + 81];
                        }

                        uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];

                        PG_raw_temp.TriggerInputStartTime = Convert.ToInt32(time_count);
                        PG_raw_temp.TriggerInputEndTime = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));

                        PG_raw_temp.SumCounts = PG_raw_temp.ChannelCount.ToList().Sum();

                        PG_raw.Add(PG_raw_temp);

                        #region OLD Algorithm

                        //int ChCountsSum = 0;
                        //for (int ch = 0; ch < 72; ch++) // ch: 0 ~ 71
                        //{
                        //    ChCountsSum += SDATA_BUFFER[ch];
                        //}
                        //for (int ch = 81; ch < 153; ch++) // ch: 81 ~ 152
                        //{
                        //    ChCountsSum += SDATA_BUFFER[ch];
                        //}

                        //if (ChCountsSum > 5)
                        //{
                        //    GetPGdistribution(); // [Only View] View에 보여지는 CRUXELL.PGdistribution 의 내부 데이터 업데이트 (Noise는 제거함: 기준 5)
                        //}

                        //CounterData TempData = new CounterData();

                        //uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];
                        //TempData.Time_Start = Convert.ToInt32(time_count);
                        //TempData.Time_End = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));

                        //for (int i = 0; i < 72; i++)
                        //{
                        //    TempData.ChCounts[i] = SDATA_BUFFER[i];
                        //}
                        //for (int i = 81; i < 153; i++)
                        //{
                        //    TempData.ChCounts[i - 9] = SDATA_BUFFER[i];
                        //}

                        //TempData.RealTime_Start = MeasurementStartTime.AddMilliseconds(TempData.Time_Start / 1000);
                        //TempData.RealTime_End = MeasurementStartTime.AddMilliseconds(TempData.Time_End / 1000);
                        //TempData.ChCounts_Sum = TempData.ChCounts.ToList().Sum();

                        //#region Correction for Broken Scintillaotr (Off)
                        ////bool CorrectionBrokenScintillator = false;
                        ////if (CorrectionBrokenScintillator)
                        ////{
                        ////    TempData.ChCounts[0] = TempData.ChCounts[1];
                        ////    TempData.ChCounts[18] = TempData.ChCounts[19];
                        ////    TempData.ChCounts[36] = TempData.ChCounts[37];

                        ////    TempData.ChCounts[26] = TempData.ChCounts[25];
                        ////    TempData.ChCounts[27] = TempData.ChCounts[28];
                        ////}
                        //#endregion

                        //FPGADatasTemp.Add(TempData);               // FPGADatasTemp에 전체 기록함
                        //SplitContinuousMeasuredDataToSpot(50, 30); // FPGADatasTemp를 가지고 Hysteresis 적용함(PG Counts)

                        #endregion

                    }
                    else // Trigger mode
                    {
                        #region Trigger Mode(디버깅 안함, 사용 안할것이라서 안봄)

                        FPGADataModel_71Ch TempFPGAData = new FPGADataModel_71Ch();

                        GetPGdistribution(); // Only for View  

                        bool missingVal = true;
                        bool corrFactorsCheck = true;

                        double[] ChannelCount = new double[144];

                        double[] Counts_72slit = new double[72];
                        double[] Counts_71Slit = new double[71];

                        double[] Real_Counts_71Slit = new double[71];

                        double[] cnt_row1 = new double[36];
                        double[] cnt_row2 = new double[36];
                        double[] cnt_row3 = new double[36];
                        double[] cnt_row4 = new double[36];

                        double[] cnt_top = new double[36];
                        double[] cnt_bot = new double[36];


                        for (int i = 0; i < 72; i++)
                        {
                            ChannelCount[i] = SDATA_BUFFER[i];
                        }
                        for (int i = 81; i < 153; i++)
                        {
                            ChannelCount[i - 9] = SDATA_BUFFER[i];
                        }

                        if (missingVal == true)
                        {
                            ChannelCount[0] = ChannelCount[1];
                            ChannelCount[18] = ChannelCount[19];
                            ChannelCount[36] = ChannelCount[37];

                            // for 27, 28 (26, 27 in C#) channel

                            //ChCounts[26] = ChCounts[8];
                            //ChCounts[27] = ChCounts[9];

                            double sum1 = 0;
                            double sum2 = 0;
                            double ratio = 0;

                            sum1 = 0;
                            sum2 = 0;
                            ratio = 0;

                            for (int k = 1; k < 18; k++)
                            {
                                sum1 += ChannelCount[k] + ChannelCount[72 + k];
                                sum2 += ChannelCount[18 + k] + ChannelCount[90 + k];
                            }
                            ratio = sum2 / sum1;

                            ChannelCount[27] = (int)Math.Round(ChannelCount[9] * ratio);
                            ChannelCount[26] = (int)Math.Round(ChannelCount[8] * ratio);
                        }

                        if (corrFactorsCheck == true)
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                Counts_72slit[2 * i] = ChannelCount[125 - i] * corrFactors[2][i] + ChannelCount[143 - i] * corrFactors[3][i];
                                Counts_72slit[2 * i + 1] = ChannelCount[89 - i] * corrFactors[0][i] + ChannelCount[107 - i] * corrFactors[1][i];

                                Counts_72slit[2 * i + 36] = ChannelCount[53 - i] * corrFactors[2][i + 18] + ChannelCount[71 - i] * corrFactors[3][i + 18];
                                Counts_72slit[2 * i + 1 + 36] = ChannelCount[17 - i] * corrFactors[0][i + 18] + ChannelCount[35 - i] * corrFactors[1][i + 18];
                            }
                            for (int i = 0; i < 71; i++)
                            {
                                Real_Counts_71Slit[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                            }
                        }
                        else // corrFactorsCheck == false
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                cnt_row1[i] = ChannelCount[89 - i];
                                cnt_row2[i] = ChannelCount[107 - i];
                                cnt_row3[i] = ChannelCount[125 - i];
                                cnt_row4[i] = ChannelCount[143 - i];

                                cnt_row1[i + 18] = ChannelCount[17 - i];
                                cnt_row2[i + 18] = ChannelCount[35 - i];
                                cnt_row3[i + 18] = ChannelCount[53 - i];
                                cnt_row4[i + 18] = ChannelCount[71 - i];
                            }
                            for (int i = 0; i < 36; i++)
                            {
                                cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                                cnt_top[i] = cnt_row1[i] + cnt_row2[i];
                            }
                            for (int i = 0; i < 36; i++)
                            {
                                Counts_72slit[2 * i] = cnt_bot[i];
                                Counts_72slit[2 * i + 1] = cnt_top[i];
                            }
                            for (int i = 0; i < 71; i++)
                            {
                                TempFPGAData.ChannelCount[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                            }
                        }

                        int Time_Start = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72])));
                        int TriggerInputEndTime = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74])));

                        TempFPGAData.TriggerADCHeight = Convert.ToInt32((uint)(SDATA_BUFFER[76]));

                        TempFPGAData.StartEpochTime = MeasurementStartTime.AddMilliseconds(Time_Start / 1000);
                        TempFPGAData.EndEpochTime = MeasurementStartTime.AddMilliseconds(TriggerInputEndTime / 1000);

                        FPGADatas_71Ch.Add(TempFPGAData);

                        #endregion
                    }
                }
                else if(ViewModels.VM_MainWindow.selectedView == "LineScanningView")
                {
                    if (SDATA_BUFFER[162] == 1) // Continuous mode
                    {
                        // SDATA_BUFFER 기준
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
                        //  98  97  96  95  94  93  92  91  90  89  88  87  86  85  84  83  82  81 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
                        // 116 115 114 113 112 111 110 109 108 107 106 105 104 103 102 101 100  99 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
                        // 134 133 132 131 130 129 128 127 126 125 124 123 122 121 120 119 118 117 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
                        // 152 151 150 149 148 147 146 145 144 143 142 141 140 139 138 137 136 135 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

                        // ChCounts 기준
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
                        //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
                        // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
                        // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
                        // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
                        // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

                        #region Cruxell Code(Activated)

                        uint error_filter = numberOfSetBits((uint)(SDATA_BUFFER[77])); // 노이즈때문에 튀는경우있어서 처리를 이렇게함
                        if (error_filter >= 8)
                            error_filter = 65535;
                        else
                            error_filter = 0;

                        uint temp_trig_on_off = error_filter;
                        if (temp_trig_on_off != trig_on_off) // 상태변화 catch를 위해[Cruxell]
                        {
                            if (temp_trig_on_off == 65535)
                            {
                                current_tr++;
                            }
                            trig_on_off = temp_trig_on_off;
                        }

                        #endregion

                        PGStruct PG_raw_temp = new PGStruct();

                        for (int i = 0; i < 72; i++)
                        {
                            PG_raw_temp.ChannelCount[i] = SDATA_BUFFER[i];
                            PG_raw_temp.ChannelCount[i + 72] = SDATA_BUFFER[i + 81];
                        }

                        uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];

                        PG_raw_temp.TriggerInputStartTime = Convert.ToInt32(time_count);
                        PG_raw_temp.TriggerInputEndTime = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));

                        PG_raw_temp.ADC = SDATA_BUFFER[76] / 4096.0 * 5.0;

                        PG_raw_temp.SumCounts = PG_raw_temp.ChannelCount.ToList().Sum();

                        PG_raw.Add(PG_raw_temp);

                        ////////////////////////////////////////////////////////////////
                        // ADC 확인하여 Trigger 발생하도록 하는 함수 추가(2022-03-01) //
                        ////////////////////////////////////////////////////////////////

                        if (SMC_isLayer == false) // Active HIGH
                        {
                            if (PG_raw_temp.ADC < SMC_Threshold)
                            {
                                SMC_isLayer = true;
                                SMC_Trigger_temp[0] = PG_raw_temp.TriggerInputEndTime;

                                // CurrentLayer = SMC_LayerNumber;
                                // CurrentLayerRatio = Math.Round((double)100 * SMC_LayerNumber / Plan_TotalLayer);
                            }
                        }
                        else
                        {
                            if (PG_raw_temp.ADC > SMC_Threshold)
                            {
                                SMC_isLayer = false;
                                SMC_Trigger_temp[1] = PG_raw_temp.TriggerInputEndTime;

                                if (SMC_Trigger_temp[1] - SMC_Trigger_temp[0] < 10000) // 짧으면 Tuning 빔임
                                {
                                    SMC_TuningIndex++;
                                }
                                else // 빔 조사시간이 10000 us (=10 ms) 보다 길 때
                                {
                                    SMC_LayerNumber++;
                                    SMC_TuningIndex = 1;

                                    VM_LineScanning.isStart = true;
                                    VM_LineScanning._EventTransfer.RaiseEvent(); // 이벤트만 발생시켜줌(레이어가 끝났다는 것을 VM에 알려줌)
                                }
                            }
                        }

                        PG_raw_temp = new PGStruct();

                    }
                    else // Trigger mode
                    {
                        #region Trigger Mode(디버깅 안함, 사용 안할것이라서 안봄)

                        FPGADataModel_71Ch TempFPGAData = new FPGADataModel_71Ch();

                        GetPGdistribution(); // Only for View  

                        bool missingVal = true;
                        bool corrFactorsCheck = true;

                        double[] ChannelCount = new double[144];

                        double[] Counts_72slit = new double[72];
                        double[] Counts_71Slit = new double[71];

                        double[] Real_Counts_71Slit = new double[71];

                        double[] cnt_row1 = new double[36];
                        double[] cnt_row2 = new double[36];
                        double[] cnt_row3 = new double[36];
                        double[] cnt_row4 = new double[36];

                        double[] cnt_top = new double[36];
                        double[] cnt_bot = new double[36];


                        for (int i = 0; i < 72; i++)
                        {
                            ChannelCount[i] = SDATA_BUFFER[i];
                        }
                        for (int i = 81; i < 153; i++)
                        {
                            ChannelCount[i - 9] = SDATA_BUFFER[i];
                        }

                        if (missingVal == true)
                        {
                            ChannelCount[0] = ChannelCount[1];
                            ChannelCount[18] = ChannelCount[19];
                            ChannelCount[36] = ChannelCount[37];

                            // for 27, 28 (26, 27 in C#) channel

                            //ChCounts[26] = ChCounts[8];
                            //ChCounts[27] = ChCounts[9];

                            double sum1 = 0;
                            double sum2 = 0;
                            double ratio = 0;

                            sum1 = 0;
                            sum2 = 0;
                            ratio = 0;

                            for (int k = 1; k < 18; k++)
                            {
                                sum1 += ChannelCount[k] + ChannelCount[72 + k];
                                sum2 += ChannelCount[18 + k] + ChannelCount[90 + k];
                            }
                            ratio = sum2 / sum1;

                            ChannelCount[27] = (int)Math.Round(ChannelCount[9] * ratio);
                            ChannelCount[26] = (int)Math.Round(ChannelCount[8] * ratio);
                        }

                        if (corrFactorsCheck == true)
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                Counts_72slit[2 * i] = ChannelCount[125 - i] * corrFactors[2][i] + ChannelCount[143 - i] * corrFactors[3][i];
                                Counts_72slit[2 * i + 1] = ChannelCount[89 - i] * corrFactors[0][i] + ChannelCount[107 - i] * corrFactors[1][i];

                                Counts_72slit[2 * i + 36] = ChannelCount[53 - i] * corrFactors[2][i + 18] + ChannelCount[71 - i] * corrFactors[3][i + 18];
                                Counts_72slit[2 * i + 1 + 36] = ChannelCount[17 - i] * corrFactors[0][i + 18] + ChannelCount[35 - i] * corrFactors[1][i + 18];
                            }
                            for (int i = 0; i < 71; i++)
                            {
                                Real_Counts_71Slit[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                            }
                        }
                        else // corrFactorsCheck == false
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                cnt_row1[i] = ChannelCount[89 - i];
                                cnt_row2[i] = ChannelCount[107 - i];
                                cnt_row3[i] = ChannelCount[125 - i];
                                cnt_row4[i] = ChannelCount[143 - i];

                                cnt_row1[i + 18] = ChannelCount[17 - i];
                                cnt_row2[i + 18] = ChannelCount[35 - i];
                                cnt_row3[i + 18] = ChannelCount[53 - i];
                                cnt_row4[i + 18] = ChannelCount[71 - i];
                            }
                            for (int i = 0; i < 36; i++)
                            {
                                cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                                cnt_top[i] = cnt_row1[i] + cnt_row2[i];
                            }
                            for (int i = 0; i < 36; i++)
                            {
                                Counts_72slit[2 * i] = cnt_bot[i];
                                Counts_72slit[2 * i + 1] = cnt_top[i];
                            }
                            for (int i = 0; i < 71; i++)
                            {
                                TempFPGAData.ChannelCount[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                            }
                        }

                        int Time_Start = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72])));
                        int TriggerInputEndTime = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74])));

                        TempFPGAData.TriggerADCHeight = Convert.ToInt32((uint)(SDATA_BUFFER[76]));

                        TempFPGAData.StartEpochTime = MeasurementStartTime.AddMilliseconds(Time_Start / 1000);
                        TempFPGAData.EndEpochTime = MeasurementStartTime.AddMilliseconds(TriggerInputEndTime / 1000);

                        FPGADatas_71Ch.Add(TempFPGAData);

                        #endregion
                    }
                }
                else
                {
                    MessageBox.Show($"ViewModels.VM_MainWindow.selectedView: {ViewModels.VM_MainWindow.selectedView}");
                }                
            }
        }

        private void SplitContinuousMeasuredDataToSpot(int ULD, int LLD) // 50, 30
        {
            int NumberOfComponents = FPGADatasTemp.Count - 1;

            if (NumberOfComponents > 7)
            {
                if (isBeamOn == false) // TRIG OFF -> TRIG ON
                {
                    if (Sum_ChCounts_LastFiveDatas(NumberOfComponents) > ULD)
                    {
                        BeamOnIndex = NumberOfComponents - 4; // 1~4 tick margin
                        isBeamOn = true;
                    }
                }
                else if (isBeamOn == true) // TRIG ON -> TRIG OFF
                {
                    if (Sum_ChCounts_LastFiveDatas(NumberOfComponents) < LLD)
                    {
                        BeamOffIndex = NumberOfComponents - 1; // 1~4 tick margin
                        isBeamOn = false;

                        SplitToSpot(BeamOnIndex, BeamOffIndex);
                    }
                }
            }
        }

        private double Sum_ChCounts_LastFiveDatas(int Num)
        {
            double tempSum = 0;
            for (int i = 0; i < Num; i++)
            {
                tempSum += FPGADatasTemp[Num - i].ChCounts_Sum;
            }

            return tempSum;
        }

        private void SplitToSpot(int On, int Off)
        {
            CounterData tempData = new CounterData();

            tempData.Time_Start = FPGADatasTemp[On].Time_Start;
            tempData.Time_End = FPGADatasTemp[Off].Time_End;

            tempData.RealTime_Start = MeasurementStartTime.AddMilliseconds(tempData.Time_Start / 1000);
            tempData.RealTime_End = MeasurementStartTime.AddMilliseconds(tempData.Time_End / 1000);

            for (int i = 0; i < 144; i++)
            {
                double temp = 0;
                for (int j = On; j < Off + 1; j++)
                {
                    temp = temp + FPGADatasTemp[j].ChCounts[i];
                }

                tempData.ChCounts[i] = temp;
            }

            FPGADatas.Add(tempData); // Fin. 데이터는 FPGADatas 에 저장됨.
        }

        private string ShowFileOpenDialog() // Convert
        {
            //파일오픈창 생성 및 설정
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open bin";
            ofd.FileName = "name";
            ofd.Filter = "데이터 (*.bin) | *.bin; | 모든 파일 (*.*) | *.*";

            //파일 오픈창 로드
            DialogResult dr = ofd.ShowDialog();

            //OK버튼 클릭시
            if (dr == DialogResult.OK)
            {
                //File명과 확장자를 가지고 온다.
                string fileName = ofd.SafeFileName;
                //File경로와 File명을 모두 가지고 온다.
                string fileFullName = ofd.FileName;
                //File경로만 가지고 온다.
                string filePath = fileFullName.Replace(fileName, "");

                return fileFullName;
            }
            //취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
            else if (dr == DialogResult.Cancel)
            {
                return "";
            }

            return "";
        }

        #endregion

        public event EventHandler<MoveEventHandlerArgs> MoveEvent;
        public class MoveEventHandlerArgs
        {
            public int Position { get; set; }
            public MoveEventHandlerArgs(int position)
            {
                Position = position;
            }
        }



        private int _position;
        // private int _destination;

        private void get_data_from_box()
        {
            tb_0x19 = 1;
            tb_0x1a = 1;
        }

    }
}