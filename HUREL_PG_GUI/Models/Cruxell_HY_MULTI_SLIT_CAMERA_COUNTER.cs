using CyUSB;
using HUREL_PG_GUI.Models;
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

namespace MSPGC_GUI.Model
{
    public partial class CRUXELLMSPGC
    {
        ///// --------------------------------- /////
        ///// ------- SMC Configuration ------- /////
        ///// --------------------------------- /////
        private int SMC_TimeIndex = 1;
        private int SMC_LayerNumber = 1;
        private int SMC_TuningIndex = 1;
        private bool SMC_isLayer = false;
        private double SMC_Threshold = 2.5;
        private int[] SMC_Trigger_temp = new int[2];

        private enum CountMode
        {
            Continue,
            TRIG1,
            TRIG2
        }
        CountMode countMode;
        ///// --------------------------- /////
        ///// ------- Static Data ------- /////
        ///// --------------------------- /////
        public static List<PGStruct> PG_raw = new List<PGStruct>();

        //------------------------------+---------------------------------------------------------------
        // [변수] 재린 추가
        //------------------------------+---------------------------------------------------------------
        public static bool CheckForIllegalCrossThreadCalls;

        //public static List<FPGADataModel> FPGADatas = new List<FPGADataModel>();
        public static DateTime MeasurementStartTime = new DateTime();

        // for Continuous Measurement Mode
        //public static List<FPGADataModelTemp> FPGADatasTemp = new List<FPGADataModelTemp>();
        //public static int isBeamOn;
        //public static int BeamOnIndex;
        //public static int BeamOffIndex;

        //public static int DelayBeamIndexCount = 0;

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing 관련
        //------------------------------+---------------------------------------------------------------
        bool thread_run = false;
        bool parsing_run = false;
        public enum PS // Parsing State
        {
            CUT_TRASH,                  // FE 들어오기 전 까지 다 버림
            FE_FIRST,                   // 첫 FE 확인
            FE_SECOND,                  // 두번째 FE 확인
            DATA_READ                   // 514 DATA_BUFFER 채우기
        }
        PS parsing_status = PS.CUT_TRASH;

        byte[] DATA_BUFFER; // FEFE제외하고 데이터만 담은 514 data buffer
        ushort[] SDATA_BUFFER; // short 버퍼
        int DATA_BUFFER_read_count; // 514 DATA_BUFFER 채울때 count

        //------------------------------+---------------------------------------------------------------
        // [변수] 파일저장
        //------------------------------+---------------------------------------------------------------
        public System.Windows.Forms.Timer save_timer { get; set; }
        StreamWriter file_main;
        StreamWriter file_adc;

        public string tb_bin_dir;
        public string tb_main_dir;
        public string tb_adc_dir;

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing - File save
        //------------------------------+---------------------------------------------------------------
        StringBuilder sb = new StringBuilder(65536);

        public uint trig_on_off = 0;
        public uint pri_et = 0;
        public uint temp_trig_time = 0;
        public double trig_adc_a = 0;



        int[] temp_send_buffer = new int[1024];

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing - 상태에 따라 파싱
        //------------------------------+---------------------------------------------------------------
        public int read_buffer_count = 0; // 514 데이터 버퍼 읽은 갯수 (종료시 0초기화 필요)
        public int read_fefe_count = 0; // FEFE단위 읽은 갯수



        public void HistogramReset()
        {
            for (int i = 0; i < 71; i++)
            {
                chart_PGdistribution_sum[i] = 0;
            }
            for (int i = 0; i < 36; i++)
            {
                chart_Slit1[i] = 0;
                chart_Slit2[i] = 0;

                chart_Scintillator1[i] = 0;
                chart_Scintillator2[i] = 0;
                chart_Scintillator3[i] = 0;
                chart_Scintillator4[i] = 0;
            }
        }






        public static string GetUniqueFilePath(string filepath)
        {
            if (File.Exists(filepath))
            {
                string folder = Path.GetDirectoryName(filepath);
                string filename = Path.GetFileNameWithoutExtension(filepath);
                string extension = Path.GetExtension(filepath);
                int number = 1;

                Match regex = Regex.Match(filepath, @"(.+) \((\d+)\)\.\w+");

                if (regex.Success)
                {
                    filename = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }

                do
                {
                    number++;
                    filepath = Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
                }
                while (File.Exists(filepath));
            }

            return filepath;
        }

        private void init_bin_save_dir()
        {
            #region tb_bin_dir 을 get, set으로 받기
            //tb_bin_dir = TB_bin_save_dir.Text;
            #endregion

            tb_bin_dir = "";

            //tb_main_dir = TB_data_save_dir.Text;
            //tb_adc_dir = TB_trig_save_dir.Text;

            // 1. 비어있는경우 기본경로로 대체
            if (tb_bin_dir.Length == 0)
                tb_bin_dir = Application.StartupPath + "\\data\\data.bin";

            tb_bin_dir = GetUniqueFilePath(tb_bin_dir);

            if (Directory.Exists(Path.GetDirectoryName(tb_bin_dir)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(tb_bin_dir));

            #region Cruxell 주석
            //string d_fullPath = TB_data_save_dir.Text;
            //string t_fullPath = TB_trig_save_dir.Text;

            //if(d_fullPath.Length == 0)
            //    d_fullPath = Application.StartupPath + "\\data (1)\\data_main.txt";
            //if (t_fullPath.Length == 0)
            //    t_fullPath = Application.StartupPath + "\\data (1)\\data_adc.txt";

            //string d_fileName = Path.GetFileName(d_fullPath);
            //string t_fileName = Path.GetFileName(t_fullPath);

            //string path = Path.GetDirectoryName(d_fullPath);
            //string fwonumber = "";
            //int number = 0;
            //string newPath = path;

            //Match regex = Regex.Match(path, @"(.+) \((\d+)\)");
            //if (regex.Success)
            //{
            //    fwonumber = regex.Groups[1].Value;
            //    number = int.Parse(regex.Groups[2].Value);
            //}

            //while (Directory.Exists(newPath))
            //{
            //    number++;
            //    newPath = Path.Combine( string.Format("{0} ({1})", fwonumber, number));
            //}

            //TB_data_save_dir.Text = Path.Combine(newPath, d_fileName);
            //TB_trig_save_dir.Text = Path.Combine(newPath, t_fileName);
            #endregion
        }

        private void init_data_and_buffer()
        {
            // 1. 버퍼 초기화
            DATA_BUFFER = new byte[334];
            SDATA_BUFFER = new ushort[167];

            // 2. 초기값 설정 (ini 확인 후 있으면 적용, 없으면 작성)
            if (System.IO.File.Exists(INI_PATH))
            {
                Read_current_ini();
            }
            else
            {
                Set_first_setting();
                Write_current_ini();
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] ParsingThread
        //------------------------------+---------------------------------------------------------------
        private void ParsingThread()
        {
            trig_on_off = 0;
            pri_et = 0;
            temp_trig_time = 0;

            trig_adc_a = 0;

            Trace.WriteLine("HY : ParsingThread start");
            while (thread_run)
            {
                CheckForIllegalCrossThreadCalls = false;
                using (BinaryWriter writer = new BinaryWriter(File.Open(tb_bin_dir, FileMode.Append)))
                {
                    byte[] Item;
                    while (test_buffer.TryTake(out Item, 1))
                    {
                        writer.Write(Item);
                        writer.Flush();

                        for (int i = 0; i < Item.Length; ++i)
                            check_parsing_state(Item[i]);
                        Thread.Sleep(0);
                    }
                }
            }
            close_file();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] 최초 셋팅 값 한번만 보내기 용
        //------------------------------+---------------------------------------------------------------
        private void reset_send_buffer(ref byte[] outData, ref int outData_BufSz)
        {
            for (int i = 0; i < 1024; ++i)
                temp_send_buffer[i] = 0;
            for (int i = 0; i < outData_BufSz; ++i)
                outData[i] = 0;
        }
        private unsafe void tryparse_send(ref byte[] outData, ref int outData_BufSz, int data, int add)
        {
            reset_send_buffer(ref outData, ref outData_BufSz);

            int get_data = data;

            for (int i = 0; i < 700; ++i)
                temp_send_buffer[i] = (add << 24) | (get_data & 0xFFFFFF);

            Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

            ///////
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //EndPoint.TimeOut = 10;
            for (int i = 0; i < 1000; ++i)
            {
                if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    break;
                //Thread.Sleep(10);
            }
            sw.Stop();
            Trace.WriteLine($"[JJR] Ellapsed Time: {sw.ElapsedMilliseconds} ms");

        }

        private void SetOutputData(int flag)
        {
            // 1. out endpoint로 설정
            //EndPointsComboBox.SelectedIndex = 0;
            EndPointListSelectIdx = 0;

            // 2. USB endpoint 연결
            int outData_BufSz = 4096;

            EndPoint.XferSize = outData_BufSz;

            if (EndPoint is CyIsocEndPoint)
                IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(outData_BufSz);
            else
                IsoPktBlockSize = 0;

            byte[] outData = new byte[outData_BufSz];
            if (flag == 1)
            {
                tryparse_send(ref outData, ref outData_BufSz, 0, 0x19);                                       // 0x19 ADC Rate 시작전에 0던지기
                // 최초 셋팅값 전송
                tryparse_send(ref outData, ref outData_BufSz, tb_0x11, 0x11);                                 // Mode
                tryparse_send(ref outData, ref outData_BufSz, tb_0x13, 0x12);                                 // Interval Time
                tryparse_send(ref outData, ref outData_BufSz, (int)(tb_0x12 / 5000.0 * 65535.0 + 0.5), 0x13); // Trig Vref
                tryparse_send(ref outData, ref outData_BufSz, tb_0x14, 0x14);                                 // TRIG 1 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x15, 0x15);                                 // TRIG 1 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x16, 0x16);                                 // TRIG 2 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x17, 0x17);                                 // TRIG 2 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, (int)(1.0 / tb_0x18 * 1000.0), 0x18);           // TRIG ADC Rate

                tryparse_send(ref outData, ref outData_BufSz, 1, 0x19); // TRIG ADC Rate
                //tryparse_send(ref outData, ref outData_BufSz, 1, 0x1a); // TRIG ADC Rate
                // 런신호 (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                for (int i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
            }
            else if (flag == 2)
            {
                // Final call (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[769] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    //EndPoint.XferData(ref outData, ref outData_BufSz);
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : flag2 {i}");
            }
            else if (flag == 3)
            {
                Trace.WriteLine("HY : [Try] Reset_send_buffer [3]");
                // Stop (전부 0)
                reset_send_buffer(ref outData, ref outData_BufSz);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : [Try] 0000 send done : {i}");
            }

            //EndPointsComboBox.SelectedIndex = 1;
            EndPointListSelectIdx = 1;
        }

        private void SetOutputData2(int flag)
        {
            // 1. out endpoint로 설정
            //EndPointsComboBox.SelectedIndex = 0;
            EndPointListSelectIdx = 0;

            // 2. USB endpoint 연결
            int outData_BufSz = 4096;

            EndPoint.XferSize = outData_BufSz;

            if (EndPoint is CyIsocEndPoint)
                IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(outData_BufSz);
            else
                IsoPktBlockSize = 0;

            byte[] outData = new byte[outData_BufSz];
            if (flag == 1)
            {
                tryparse_send(ref outData, ref outData_BufSz, 0, 0x19);                                       // 0x19 ADC Rate 시작전에 0던지기
                // 최초 셋팅값 전송
                tryparse_send(ref outData, ref outData_BufSz, tb_0x11, 0x11);                                 // Mode
                tryparse_send(ref outData, ref outData_BufSz, tb_0x12, 0x12);                                 // Interval Time
                //tryparse_send(ref outData, ref outData_BufSz, tb_0x13, 0x12);                                 // Interval Time
                tryparse_send(ref outData, ref outData_BufSz, (int)(tb_0x13 / 5000.0 * 65535.0 + 0.5), 0x13); // Trig Vref
                //tryparse_send(ref outData, ref outData_BufSz, (int)(tb_0x12 / 5000.0 * 65535.0 + 0.5), 0x13); // Trig Vref
                tryparse_send(ref outData, ref outData_BufSz, tb_0x14, 0x14);                                 // TRIG 1 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x15, 0x15);                                 // TRIG 1 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x16, 0x16);                                 // TRIG 2 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x17, 0x17);                                 // TRIG 2 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, (int)(1.0 / tb_0x18 * 1000.0), 0x18);           // TRIG ADC Rate

                tryparse_send(ref outData, ref outData_BufSz, 1, 0x19); // TRIG ADC Rate
                //tryparse_send(ref outData, ref outData_BufSz, 1, 0x1a); // TRIG ADC Rate
                // 런신호 (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                for (int i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
            }
            else if (flag == 2)
            {
                // Final call (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[769] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    //EndPoint.XferData(ref outData, ref outData_BufSz);
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : flag2 {i}");
            }
            else if (flag == 3)
            {
                Trace.WriteLine("HY : [Try] Reset_send_buffer [3]");
                // Stop (전부 0)
                reset_send_buffer(ref outData, ref outData_BufSz);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : [Try] 0000 send done : {i}");
            }

            //EndPointsComboBox.SelectedIndex = 1;
            EndPointListSelectIdx = 1;
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] parsing - 상태에 따라 파싱
        //------------------------------+---------------------------------------------------------------
        private void check_parsing_state(byte result)
        {
            switch (parsing_status)
            {
                case PS.CUT_TRASH: // FE만날때 까지 버리기 -> FE면 다음FE확인(FE_SECOND)
                    if (result == 0xFE)
                    {
                        parsing_status = PS.FE_SECOND;
                        //file_main.WriteLine();
                        //file_main.Write($"{result:X2} ");
                    }
                    else
                    {
                        //file_main.Write($"{result:X2} ");
                    }
                    break;
                case PS.FE_FIRST: // 다시 첫 FE가 등장했는가?
                    if (result == 0xFE)
                    {
                        //file_main.Write($"\n{result:X2} ");
                        read_buffer_count++;
                        //Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 514 byte버퍼 -> 257 short버퍼
#if false
                        //20210111 마지막줄 저장안되는 버그 수정을 위해 옮김
                        Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 334개 쌓고 왜 ???
#endif

                        read_fefe_count++;
                        /// 실제 출력
                        //if (thread_run) // 1sec start mode
                        //	change_state();
                        //if (!thread_run) // convert modef
#if false
                        parsing_save();
#endif
                        parsing_status = PS.FE_SECOND;
                    }
                    else
                    {
                        parsing_status = PS.CUT_TRASH;
                        //file_main.WriteLine($"\n##CUT_TRASH##");
                        //file_main.Write($"{result:X2} ");
                    }
                    // 읽은 DATA초기화, count 초기화
                    break;
                case PS.FE_SECOND: // 두번째 FE 인가? -> FE면 DATA_READ / 아니면 CUT_TRASH
                    if (result == 0xFE)
                    {
                        parsing_status = PS.DATA_READ;
                        //file_main.Write($"{result:X2} ");
                    }
                    else
                    {
                        parsing_status = PS.CUT_TRASH;
                        //file_main.WriteLine($"\n##CUT_TRASH##");
                        //file_main.Write($"{result:X2} ");
                    }
                    break;
                case PS.DATA_READ: // 257개 다 읽었는가?
                                   //file_main.Write($"{result:X2} ");
                    DATA_BUFFER[DATA_BUFFER_read_count] = result;
                    DATA_BUFFER_read_count++;
                    if (DATA_BUFFER_read_count == 334)
                    {
                        DATA_BUFFER_read_count = 0;
                        parsing_status = PS.FE_FIRST;
                        Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 334개 쌓고 왜 ??? // 여기에서 ClusteringIndex 구분해주는 함수 삽입
                        parsing_save();
                    }
                    break;
                default:
                    break;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] parsing - File save
        //------------------------------+---------------------------------------------------------------
        uint numberOfSetBits(uint i)
        {
            // C or C++: use uint32_t
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
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
                if (HUREL_PG_GUI.ViewModels.VM_MainWindow.selectedView == "SpotScanningView")
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

                        //uint error_filter = numberOfSetBits((uint)(SDATA_BUFFER[77])); // 노이즈때문에 튀는경우있어서 처리를 이렇게함
                        //if (error_filter >= 8)
                        //    error_filter = 65535;
                        //else
                        //    error_filter = 0;

                        //uint temp_trig_on_off = error_filter;
                        //if (temp_trig_on_off != trig_on_off) // 상태변화 catch를 위해[Cruxell]
                        //{
                        //    if (temp_trig_on_off == 65535)
                        //    {
                        //        current_tr++;
                        //    }
                        //    trig_on_off = temp_trig_on_off;
                        //}

                        #endregion

                        //PGStruct PG_raw_temp = new PGStruct();

                        //for (int i = 0; i < 72; i++)
                        //{
                        //    PG_raw_temp.ChannelCount[i] = SDATA_BUFFER[i];
                        //    PG_raw_temp.ChannelCount[i + 72] = SDATA_BUFFER[i + 81];
                        //}

                        //uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];

                        //PG_raw_temp.TriggerInputStartTime = Convert.ToInt32(time_count);
                        //PG_raw_temp.TriggerInputEndTime = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));

                        //PG_raw_temp.SumCounts = PG_raw_temp.ChannelCount.ToList().Sum();

                        //PG_raw.Add(PG_raw_temp);

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

                        //FPGADataModel_71Ch TempFPGAData = new FPGADataModel_71Ch();

                        //GetPGdistribution(); // Only for View  

                        //bool missingVal = true;
                        //bool corrFactorsCheck = true;

                        //double[] ChannelCount = new double[144];

                        //double[] Counts_72slit = new double[72];
                        //double[] Counts_71Slit = new double[71];

                        //double[] Real_Counts_71Slit = new double[71];

                        //double[] cnt_row1 = new double[36];
                        //double[] cnt_row2 = new double[36];
                        //double[] cnt_row3 = new double[36];
                        //double[] cnt_row4 = new double[36];

                        //double[] cnt_top = new double[36];
                        //double[] cnt_bot = new double[36];


                        //for (int i = 0; i < 72; i++)
                        //{
                        //    ChannelCount[i] = SDATA_BUFFER[i];
                        //}
                        //for (int i = 81; i < 153; i++)
                        //{
                        //    ChannelCount[i - 9] = SDATA_BUFFER[i];
                        //}

                        //if (missingVal == true)
                        //{
                        //    ChannelCount[0] = ChannelCount[1];
                        //    ChannelCount[18] = ChannelCount[19];
                        //    ChannelCount[36] = ChannelCount[37];

                        //    // for 27, 28 (26, 27 in C#) channel

                        //    //ChCounts[26] = ChCounts[8];
                        //    //ChCounts[27] = ChCounts[9];

                        //    double sum1 = 0;
                        //    double sum2 = 0;
                        //    double ratio = 0;

                        //    sum1 = 0;
                        //    sum2 = 0;
                        //    ratio = 0;

                        //    for (int k = 1; k < 18; k++)
                        //    {
                        //        sum1 += ChannelCount[k] + ChannelCount[72 + k];
                        //        sum2 += ChannelCount[18 + k] + ChannelCount[90 + k];
                        //    }
                        //    ratio = sum2 / sum1;

                        //    ChannelCount[27] = (int)Math.Round(ChannelCount[9] * ratio);
                        //    ChannelCount[26] = (int)Math.Round(ChannelCount[8] * ratio);
                        //}

                        //if (corrFactorsCheck == true)
                        //{
                        //    for (int i = 0; i < 18; i++)
                        //    {
                        //        Counts_72slit[2 * i] = ChannelCount[125 - i] * corrFactors[2][i] + ChannelCount[143 - i] * corrFactors[3][i];
                        //        Counts_72slit[2 * i + 1] = ChannelCount[89 - i] * corrFactors[0][i] + ChannelCount[107 - i] * corrFactors[1][i];

                        //        Counts_72slit[2 * i + 36] = ChannelCount[53 - i] * corrFactors[2][i + 18] + ChannelCount[71 - i] * corrFactors[3][i + 18];
                        //        Counts_72slit[2 * i + 1 + 36] = ChannelCount[17 - i] * corrFactors[0][i + 18] + ChannelCount[35 - i] * corrFactors[1][i + 18];
                        //    }
                        //    for (int i = 0; i < 71; i++)
                        //    {
                        //        Real_Counts_71Slit[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                        //    }
                        //}
                        //else // corrFactorsCheck == false
                        //{
                        //    for (int i = 0; i < 18; i++)
                        //    {
                        //        cnt_row1[i] = ChannelCount[89 - i];
                        //        cnt_row2[i] = ChannelCount[107 - i];
                        //        cnt_row3[i] = ChannelCount[125 - i];
                        //        cnt_row4[i] = ChannelCount[143 - i];

                        //        cnt_row1[i + 18] = ChannelCount[17 - i];
                        //        cnt_row2[i + 18] = ChannelCount[35 - i];
                        //        cnt_row3[i + 18] = ChannelCount[53 - i];
                        //        cnt_row4[i + 18] = ChannelCount[71 - i];
                        //    }
                        //    for (int i = 0; i < 36; i++)
                        //    {
                        //        cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                        //        cnt_top[i] = cnt_row1[i] + cnt_row2[i];
                        //    }
                        //    for (int i = 0; i < 36; i++)
                        //    {
                        //        Counts_72slit[2 * i] = cnt_bot[i];
                        //        Counts_72slit[2 * i + 1] = cnt_top[i];
                        //    }
                        //    for (int i = 0; i < 71; i++)
                        //    {
                        //        TempFPGAData.ChannelCount[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                        //    }
                        //}

                        //int Time_Start = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72])));
                        //int TriggerInputEndTime = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74])));

                        //TempFPGAData.TriggerADCHeight = Convert.ToInt32((uint)(SDATA_BUFFER[76]));

                        //TempFPGAData.StartEpochTime = MeasurementStartTime.AddMilliseconds(Time_Start / 1000);
                        //TempFPGAData.EndEpochTime = MeasurementStartTime.AddMilliseconds(TriggerInputEndTime / 1000);

                        //FPGADatas_71Ch.Add(TempFPGAData);

                        #endregion
                    }
                }
                else if (HUREL_PG_GUI.ViewModels.VM_MainWindow.selectedView == "LineScanningView")
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

                        //FPGADataModel_71Ch TempFPGAData = new FPGADataModel_71Ch();

                        //GetPGdistribution(); // Only for View  

                        //bool missingVal = true;
                        //bool corrFactorsCheck = true;

                        //double[] ChannelCount = new double[144];

                        //double[] Counts_72slit = new double[72];
                        //double[] Counts_71Slit = new double[71];

                        //double[] Real_Counts_71Slit = new double[71];

                        //double[] cnt_row1 = new double[36];
                        //double[] cnt_row2 = new double[36];
                        //double[] cnt_row3 = new double[36];
                        //double[] cnt_row4 = new double[36];

                        //double[] cnt_top = new double[36];
                        //double[] cnt_bot = new double[36];


                        //for (int i = 0; i < 72; i++)
                        //{
                        //    ChannelCount[i] = SDATA_BUFFER[i];
                        //}
                        //for (int i = 81; i < 153; i++)
                        //{
                        //    ChannelCount[i - 9] = SDATA_BUFFER[i];
                        //}

                        //if (missingVal == true)
                        //{
                        //    ChannelCount[0] = ChannelCount[1];
                        //    ChannelCount[18] = ChannelCount[19];
                        //    ChannelCount[36] = ChannelCount[37];

                        //    // for 27, 28 (26, 27 in C#) channel

                        //    //ChCounts[26] = ChCounts[8];
                        //    //ChCounts[27] = ChCounts[9];

                        //    double sum1 = 0;
                        //    double sum2 = 0;
                        //    double ratio = 0;

                        //    sum1 = 0;
                        //    sum2 = 0;
                        //    ratio = 0;

                        //    for (int k = 1; k < 18; k++)
                        //    {
                        //        sum1 += ChannelCount[k] + ChannelCount[72 + k];
                        //        sum2 += ChannelCount[18 + k] + ChannelCount[90 + k];
                        //    }
                        //    ratio = sum2 / sum1;

                        //    ChannelCount[27] = (int)Math.Round(ChannelCount[9] * ratio);
                        //    ChannelCount[26] = (int)Math.Round(ChannelCount[8] * ratio);
                        //}

                        //if (corrFactorsCheck == true)
                        //{
                        //    for (int i = 0; i < 18; i++)
                        //    {
                        //        Counts_72slit[2 * i] = ChannelCount[125 - i] * corrFactors[2][i] + ChannelCount[143 - i] * corrFactors[3][i];
                        //        Counts_72slit[2 * i + 1] = ChannelCount[89 - i] * corrFactors[0][i] + ChannelCount[107 - i] * corrFactors[1][i];

                        //        Counts_72slit[2 * i + 36] = ChannelCount[53 - i] * corrFactors[2][i + 18] + ChannelCount[71 - i] * corrFactors[3][i + 18];
                        //        Counts_72slit[2 * i + 1 + 36] = ChannelCount[17 - i] * corrFactors[0][i + 18] + ChannelCount[35 - i] * corrFactors[1][i + 18];
                        //    }
                        //    for (int i = 0; i < 71; i++)
                        //    {
                        //        Real_Counts_71Slit[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                        //    }
                        //}
                        //else // corrFactorsCheck == false
                        //{
                        //    for (int i = 0; i < 18; i++)
                        //    {
                        //        cnt_row1[i] = ChannelCount[89 - i];
                        //        cnt_row2[i] = ChannelCount[107 - i];
                        //        cnt_row3[i] = ChannelCount[125 - i];
                        //        cnt_row4[i] = ChannelCount[143 - i];

                        //        cnt_row1[i + 18] = ChannelCount[17 - i];
                        //        cnt_row2[i + 18] = ChannelCount[35 - i];
                        //        cnt_row3[i + 18] = ChannelCount[53 - i];
                        //        cnt_row4[i + 18] = ChannelCount[71 - i];
                        //    }
                        //    for (int i = 0; i < 36; i++)
                        //    {
                        //        cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                        //        cnt_top[i] = cnt_row1[i] + cnt_row2[i];
                        //    }
                        //    for (int i = 0; i < 36; i++)
                        //    {
                        //        Counts_72slit[2 * i] = cnt_bot[i];
                        //        Counts_72slit[2 * i + 1] = cnt_top[i];
                        //    }
                        //    for (int i = 0; i < 71; i++)
                        //    {
                        //        TempFPGAData.ChannelCount[i] = (Counts_72slit[i] + Counts_72slit[i + 1]) / 2;
                        //    }
                        //}

                        //int Time_Start = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[73] << 16) | ((uint)SDATA_BUFFER[72])));
                        //int TriggerInputEndTime = Convert.ToInt32(((uint)((uint)SDATA_BUFFER[75] << 16) | ((uint)SDATA_BUFFER[74])));

                        //TempFPGAData.TriggerADCHeight = Convert.ToInt32((uint)(SDATA_BUFFER[76]));

                        //TempFPGAData.StartEpochTime = MeasurementStartTime.AddMilliseconds(Time_Start / 1000);
                        //TempFPGAData.EndEpochTime = MeasurementStartTime.AddMilliseconds(TriggerInputEndTime / 1000);

                        //FPGADatas_71Ch.Add(TempFPGAData);

                        #endregion
                    }
                }
                else
                {
                    MessageBox.Show($"ViewModels.VM_MainWindow.selectedView: {HUREL_PG_GUI.ViewModels.VM_MainWindow.selectedView}");
                }
            }
        }

        private void SplitContinuousMeasuredDataToSpot()
        {
            //int NumberOfComponents = FPGADatasTemp.Count - 1;
            //if (NumberOfComponents > 7)
            //{
            //    if (isBeamOn == 0)
            //    {
            //        if (FPGADatasTemp[NumberOfComponents].TriggerInputStartTime - FPGADatasTemp[NumberOfComponents - 5].TriggerInputStartTime < 1100 & FPGADatasTemp[NumberOfComponents - 5].ChannelCountSum + FPGADatasTemp[NumberOfComponents - 4].ChannelCountSum + FPGADatasTemp[NumberOfComponents - 3].ChannelCountSum + FPGADatasTemp[NumberOfComponents - 2].ChannelCountSum + FPGADatasTemp[NumberOfComponents - 1].ChannelCountSum + FPGADatasTemp[NumberOfComponents].ChannelCountSum > 50)
            //        {
            //            BeamOnIndex = NumberOfComponents - 5;
            //            isBeamOn = 1;
            //        }
            //    }
            //    else if (isBeamOn == 1)
            //    {
            //        if (FPGADatasTemp[NumberOfComponents].ChannelCountSum < 5 | FPGADatasTemp[NumberOfComponents].TriggerInputStartTime - FPGADatasTemp[NumberOfComponents - 6].TriggerInputStartTime > 1300)
            //        {
            //            BeamOffIndex = NumberOfComponents - 1;
            //            isBeamOn = 2;

            //            ContToTrig(BeamOnIndex, BeamOffIndex);
            //        }
            //    }
            //    else if (isBeamOn == 2)
            //    {
            //        if (DelayBeamIndexCount == 5)
            //        {
            //            isBeamOn = 0;
            //            DelayBeamIndexCount = 0;
            //        }

            //        DelayBeamIndexCount = DelayBeamIndexCount + 1;
            //    }
            //}
        }
        private void ContToTrig(int BeamOnIndex, int BeamOffIndex)
        {
            //FPGADataModel TempFPGAData = new FPGADataModel();

            //TempFPGAData.TriggerInputStartTime = FPGADatasTemp[BeamOnIndex].TriggerInputStartTime;
            //TempFPGAData.TriggerInputEndTime = FPGADatasTemp[BeamOffIndex].TriggerInputEndTime;

            //TempFPGAData.StartEpochTime = MeasurementStartTime.AddMilliseconds(TempFPGAData.TriggerInputStartTime / 1000);
            //TempFPGAData.EndEpochTime = MeasurementStartTime.AddMilliseconds(TempFPGAData.TriggerInputEndTime / 1000);

            //for (int j = 0; j < 144; j++)
            //{
            //    int tempChannelCount = 0;
            //    for (int i = BeamOnIndex; i < BeamOffIndex + 1; i++)
            //    {
            //        tempChannelCount = tempChannelCount + FPGADatasTemp[i].ChannelCount[j];
            //    }
            //    TempFPGAData.ChannelCount[j] = tempChannelCount;
            //}
            //FPGADatas.Add(TempFPGAData);
        }
        private void GetPGDistribution()
        {
            SDATA_BUFFER[0] = SDATA_BUFFER[1];
            SDATA_BUFFER[18] = SDATA_BUFFER[19];
            SDATA_BUFFER[36] = SDATA_BUFFER[37];

            SDATA_BUFFER[26] = SDATA_BUFFER[25];




            //SDATA_BUFFER[26] = Convert.ToUInt16(Math.Round(Convert.ToDouble(SDATA_BUFFER[25]) + Convert.ToDouble(SDATA_BUFFER[27]) / 2));

            //SDATA_BUFFER[27] = SDATA_BUFFER[28];
            //SDATA_BUFFER[117] = SDATA_BUFFER[118];

            for (int i = 0; i < 18; i++)
            {
                chart_Slit1_pre[i] = (double)SDATA_BUFFER[98 - i] + (double)SDATA_BUFFER[116 - i];
                chart_Slit2_pre[i] = (double)SDATA_BUFFER[134 - i] + (double)SDATA_BUFFER[152 - i];

                chart_Slit1_pre[i + 18] = (double)SDATA_BUFFER[17 - i] + (double)SDATA_BUFFER[35 - i];
                chart_Slit2_pre[i + 18] = (double)SDATA_BUFFER[53 - i] + (double)SDATA_BUFFER[71 - i];

                chart_Scintillator1_pre[i] = (double)SDATA_BUFFER[98 - i];
                chart_Scintillator2_pre[i] = (double)SDATA_BUFFER[116 - i];
                chart_Scintillator3_pre[i] = (double)SDATA_BUFFER[134 - i];
                chart_Scintillator4_pre[i] = (double)SDATA_BUFFER[152 - i];

                chart_Scintillator1_pre[i + 18] = (double)SDATA_BUFFER[17 - i];
                chart_Scintillator2_pre[i + 18] = (double)SDATA_BUFFER[35 - i];
                chart_Scintillator3_pre[i + 18] = (double)SDATA_BUFFER[53 - i];
                chart_Scintillator4_pre[i + 18] = (double)SDATA_BUFFER[71 - i];
            }


            for (int i = 0; i < 36; i++)
            {
                chart_Slit1[i] = chart_Slit1[i] + chart_Slit1_pre[i];
                chart_Slit2[i] = chart_Slit2[i] + chart_Slit2_pre[i];

                chart_Scintillator1[i] = chart_Scintillator1[i] + chart_Scintillator1_pre[i];
                chart_Scintillator2[i] = chart_Scintillator2[i] + chart_Scintillator2_pre[i];
                chart_Scintillator3[i] = chart_Scintillator3[i] + chart_Scintillator3_pre[i];
                chart_Scintillator4[i] = chart_Scintillator4[i] + chart_Scintillator4_pre[i];
            }

            for (int i = 0; i < 36; i++)
            {
                chart_PGdistribution[2 * i + 1] = chart_Slit1[i];
                chart_PGdistribution[2 * i] = chart_Slit2[i];
            }

            for (int i = 0; i < 72; i++)
            {
                current_tcr += (double)SDATA_BUFFER[i];
                current_tcr += (double)SDATA_BUFFER[i + 81];
            }
            //trig_adc_a = (double)(SDATA_BUFFER[76]) / 4096.0 * 5.0;



            //for (int i = 0; i < 72; i++) // PG measurement mode: Cumulative PG distribution visualize
            //{
            //    chart_PGdistribution_sum[i] = chart_PGdistribution[i];
            //}
            for (int i = 0; i < 71; i++) // PG measurement mode: Cumulative PG distribution visualize
            {
                chart_PGdistribution_sum[i] = (chart_PGdistribution[i] + chart_PGdistribution[i + 1]) / 2;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] INI 관련
        //------------------------------+---------------------------------------------------------------
        string INI_PATH = Application.StartupPath + @"\\setting.ini";
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int sise, string filePath);

        private void WriteINI(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, INI_PATH);
        }
        private string ReadINI(string section, string key)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, INI_PATH);
            return sb.ToString();
        }

        private void Set_first_setting()
        {
            #region 처음부터 MainViewModel에서 Mode를 Continue 모드로 설정하기 때문에 필요가 없다고 생각됨
            //RB_continue.Checked = true;
            #endregion
        }

        private void Read_current_ini()
        {
            string rb = ReadINI("setting", "Mode");
            if (string.Compare("Continue", rb, true) == 0)
                countMode = CountMode.Continue;
            else if (string.Compare("TRIG1", rb, true) == 0)
                countMode = CountMode.TRIG1;
            else
                countMode = CountMode.TRIG2;

            #region Cruxell 기존 작성 코드
            //TB_0x12.Text = ReadINI("setting", "0x12");
            //TB_0x13.Text = ReadINI("setting", "0x13");
            //TB_0x14.Text = ReadINI("setting", "0x14");
            //TB_0x15.Text = ReadINI("setting", "0x15");
            //TB_0x16.Text = ReadINI("setting", "0x16");
            //TB_0x17.Text = ReadINI("setting", "0x17");
            //TB_0x18.Text = ReadINI("setting", "0x18");
            #endregion

            tb_0x12 = Convert.ToUInt16(ReadINI("setting", "0x12"));
            tb_0x13 = Convert.ToUInt16(ReadINI("setting", "0x13"));
            tb_0x14 = Convert.ToUInt16(ReadINI("setting", "0x14"));
            tb_0x15 = Convert.ToUInt16(ReadINI("setting", "0x15"));
            tb_0x16 = Convert.ToUInt16(ReadINI("setting", "0x16"));
            tb_0x17 = Convert.ToUInt16(ReadINI("setting", "0x17"));
            tb_0x18 = Convert.ToUInt16(ReadINI("setting", "0x18"));



        }

        private void Write_current_ini()
        {
            #region Cruxell 기존 작성 코드
            //WriteINI("setting", "0x12", TB_0x12.Text);
            //WriteINI("setting", "0x13", TB_0x13.Text);
            //WriteINI("setting", "0x14", TB_0x14.Text);
            //WriteINI("setting", "0x15", TB_0x15.Text);
            //WriteINI("setting", "0x16", TB_0x16.Text);
            //WriteINI("setting", "0x17", TB_0x17.Text);
            //WriteINI("setting", "0x18", TB_0x18.Text);
            #endregion

            WriteINI("setting", "0x12", Convert.ToString(tb_0x12));
            WriteINI("setting", "0x13", Convert.ToString(tb_0x13));
            WriteINI("setting", "0x14", Convert.ToString(tb_0x14));
            WriteINI("setting", "0x15", Convert.ToString(tb_0x15));
            WriteINI("setting", "0x16", Convert.ToString(tb_0x16));
            WriteINI("setting", "0x17", Convert.ToString(tb_0x17));
            WriteINI("setting", "0x18", Convert.ToString(tb_0x18));

            if (countMode == CountMode.Continue)
                WriteINI("setting", "Mode", "Continue");
            else if (countMode == CountMode.TRIG1)
                WriteINI("setting", "Mode", "TRIG1");
            else
                WriteINI("setting", "Mode", "TRIG2");
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 초기화 작업
        //------------------------------+---------------------------------------------------------------
        private void close_file()
        {
            if (file_main != null)
                file_main.Close();

            if (file_adc != null)
                file_adc.Close();
        }
        private void init_file_save()
        {
            // 기존에 열려있었다면 stream 닫기
            close_file();

            string b_name = Path.GetFileNameWithoutExtension(open_bin_path);
            string b_path = Path.GetDirectoryName(open_bin_path);

            #region Save Directory 받는 것으로 구현하기
            //string p_main = TB_data_save_dir.Text;
            #endregion

            string p_main = "";

            if (p_main.Length == 0)
                p_main = Path.Combine(b_path, b_name + "_main.txt");

            if (!Directory.Exists(Path.GetDirectoryName(p_main)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p_main));
            }
            p_main = GetUniqueFilePath(p_main);

            file_main = new StreamWriter(p_main, true);

            #region ADC 파일을 저장할지 묻는 것에 대해 구현하기
            //if (CB_save_adc.Checked) //init_file_save
            //{
            //    #region Save Directory 받는 것으로 구현하기
            //    //string p_adc = TB_trig_save_dir.Text;
            //    #endregion

            //    string p_adc = "";

            //    if (p_adc.Length == 0)
            //        p_adc = Path.Combine(b_path, b_name + "_adc.txt");

            //    if (!Directory.Exists(Path.GetDirectoryName(p_adc)))
            //    {
            //        Directory.CreateDirectory(Path.GetDirectoryName(p_adc));
            //    }
            //    p_adc = GetUniqueFilePath(p_adc);

            //    file_adc = new StreamWriter(p_adc, true);
            //}
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 종료 작업
        //------------------------------+---------------------------------------------------------------
        private void terminate_file_save()
        {
            // StreamWriter 닫기

            if (file_main != null)
                file_main.Close();

            if (file_adc != null)
                file_adc.Close();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] terminate thread
        //------------------------------+---------------------------------------------------------------
        private void terminate_thread()
        {
            if (tListen != null)
                tListen.Wait();
            if (tParsing != null)
                tParsing.Wait();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] input disable
        //------------------------------+---------------------------------------------------------------
        private void input_disable(bool state)
        {
            test_data = 0;
            parsing_status = PS.CUT_TRASH;

            #region Cruxell 주석
            //TB_r_time.Enabled = state;
            //TB_r_count.Enabled = state;
            //TB_file_name.Enabled = state;
            //TB_0x12.Enabled = state;
            //TB_0x13.Enabled = state;
            //TB_0x14.Enabled = state;
            //TB_0x15.Enabled = state;
            //TB_0x16.Enabled = state;
            //TB_0x17.Enabled = state;
            //TB_0x18.Enabled = state;
            //TB_0x19.Enabled = state;
            //TB_0x1a.Enabled = state;
            //TB_0x1b.Enabled = state;
            //TB_0x1c.Enabled = state;
            //RB_single.Enabled = state;
            //RB_coin.Enabled = state;

            //DevicesComboBox.Enabled = state;
            //EndPointsComboBox.Enabled = state;
            //PpxBox.Enabled = state;
            //QueueBox.Enabled = state;

            //PpxBox.Visible = state;
            //QueueBox.Visible = state;
            //LB_PpxBox.Visible = state;
            //LB_QueueBox.Visible = state;
            //ThroughputLabel.Visible = !state;
            //ProgressBar.Visible = !state;
            #endregion

            if (!state) // usb start
            {
                //StartBtn.Text = "Stop";
            }
            else // usb stop
            {
                //StartBtn.Text = "Start";
            }
        }
    }
}
