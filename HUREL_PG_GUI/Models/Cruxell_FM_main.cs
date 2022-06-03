using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSPGC_GUI.Model;

namespace MSPGC_GUI.Model
{
    public partial class CRUXELLMSPGC
    {
        //------------------------------+---------------------------------------------------------------
        // [변수] Material Skin에 있던 변수, WPF에서 따로 생성(by 재린)
        //------------------------------+---------------------------------------------------------------
        string LB_current_tr, LB_current_tcr, LB_average_tr, LB_average_tcr;


        public string ver_string = "Ver 21.02.23";
        double test_data = 0;
        double test_data2 = 0;
        int time_out = 0;
        BlockingCollection<byte[]> test_buffer = new BlockingCollection<byte[]>(10000);
        // byte[] test2_buffer = new byte[1024 * 1024 * 700];
        int p_head = 0; // test2_buffer의 50개중 몇번째인지
        int p_tail = 0;
        int i_head = 0; // 1메가 배열 내부의 인덱스
        int p_error = 0;
        // byte[] test_buffer = new byte[1024 * 1024 * 128];

        string open_bin_path = "";
        bool is_convert = false;
        public int is_init = 0;



        public CRUXELLMSPGC()
        {
            Trace.WriteLine("Cruxell_FM_main Initialized");

            init_data_and_buffer();
            init_USB();
        }

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 메인 폼 닫힐 때
        //------------------------------+---------------------------------------------------------------
        private void FM_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region 메인 윈도우가 닫힐 경우 이벤트를 발생할 것인지에 대한 부분
            //if (StartBtn.Text.Equals("Start") == false)
            //{
            //    start_stop_usb();
            //}


            //// Executes on clicking close button
            //bRunning = false;
            //if (usbDevices != null)
            //    usbDevices.Dispose();

            //Write_current_ini(); // 현재 설정 저장하기
            //terminate_file_save(); // 파일 저장 종료
            //terminate_thread(); // 스레드 종료
            //Trace.WriteLine("HY : EXIT");
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] START 버튼 클릭 시
        //------------------------------+---------------------------------------------------------------
        private void BT_start_Click(object sender, EventArgs e)
        {
            #region Command 함수로 대체
            //start_stop_usb();
            #endregion
        }

        private void event_mode_change(object sender, EventArgs e)
        {
            #region MainViewModel에서 모드가 바뀌는 경우 바로바로 박스 visibility 변경하도록 수정하기
            //if (RB_continue.Checked)
            //{
            //    TB_0x12.Visible = true;
            //    TB_0x13.Visible = true;
            //    TB_0x14.Visible = false;
            //    TB_0x15.Visible = false;
            //    TB_0x16.Visible = false;
            //    TB_0x17.Visible = false;
            //    TB_0x18.Visible = true;
            //}
            //else if (RB_trig1.Checked)
            //{
            //    TB_0x12.Visible = true;
            //    TB_0x13.Visible = true;
            //    TB_0x14.Visible = true;
            //    TB_0x15.Visible = true;
            //    TB_0x16.Visible = false;
            //    TB_0x17.Visible = false;
            //    TB_0x18.Visible = true;

            //}
            //else // RB_trig2
            //{
            //    TB_0x12.Visible = true;
            //    TB_0x13.Visible = false;
            //    TB_0x14.Visible = false;
            //    TB_0x15.Visible = false;
            //    TB_0x16.Visible = true;
            //    TB_0x17.Visible = true;
            //    TB_0x18.Visible = true;
            //}
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 인풋값 정수/실수만 받도록
        //------------------------------+---------------------------------------------------------------
        private void digit_keypress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }
        private void digit2_keypress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '.' || e.KeyChar == '-'))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [변수] 타이머 관련 변수
        //------------------------------+---------------------------------------------------------------
        public int timer_all_stop = 0; // 강제종료 확인 변수
        public int timer_mode = 0; // 0 : 종료 / 1 : main만 / 2 : main + sub
        System.Threading.Timer timer_main; // 누적 시간
        System.Threading.Timer timer_sub; // 개별 타이머
        public int timer_count = 0; // 누적 시간 데이터
        public int timer_count2 = 0; // 곱해서 계산해논 시간
        public int timer_sub_val = 0; // 개별 타이머 기록값
        delegate void TimerEventFiredDelegate();

        //------------------------------+---------------------------------------------------------------
        // [함수] timer_main 관련
        //------------------------------+---------------------------------------------------------------
        private void timer_main_callback(Object state)
        {
            #region 반드시 구현해야 하는 부분
            //BeginInvoke(new TimerEventFiredDelegate(timer_main_callback_work));
            #endregion
        }

        double sum_tr, sum_tcr;
        double current_tr, current_tcr, average_tr, average_tcr = 0;
        int sec_count = 0;

        private void timer_main_callback_work()
        {
            #region 반드시 구현해야 하는 부분
            //sec_count++;
            //LB_current_tr.Text = current_tr.ToString("F2");
            //LB_current_tcr.Text = current_tcr.ToString("F0");

            //LB_remain.Text = test_buffer.Count().ToString();

            //sum_tr += current_tr;
            //sum_tcr += current_tcr;

            //average_tr = sum_tr / sec_count;
            //average_tcr = sum_tcr / sec_count;

            //LB_average_tr.Text = average_tr.ToString("F2");
            //LB_average_tcr.Text = average_tcr.ToString("F0");

            //for (int i = 0; i < 119; ++i)
            //{
            //    chart2_data1[i] = chart2_data1[i + 1];
            //}
            //chart2_data1[119] = sum_tr;// trig_adc_a;


            //chart1.Series[0].Points.DataBindY(chart1_data);
            //chart2.Series[0].Points.DataBindY(chart2_data1);
            //chart3.Series[0].Points.DataBindY(chart3_data);
            //chart4.Series[0].Points.DataBindY(chart4_data);

            //current_tr = 0;
            //current_tcr = 0;

            //chart2.ChartAreas[0].AxisX.CustomLabels.Clear();
            //chart2.ChartAreas[0].AxisX.CustomLabels.Add(-4, 4, sec_count - 119 > 0 ? (sec_count - 119).ToString() : "0");
            //chart2.ChartAreas[0].AxisX.CustomLabels.Add(40 - 4, 40 + 4, sec_count - 80 > 0 ? (sec_count - 80).ToString() : "0");
            //chart2.ChartAreas[0].AxisX.CustomLabels.Add(80 - 4, 80 + 4, sec_count - 40 > 0 ? (sec_count - 40).ToString() : "0");
            //chart2.ChartAreas[0].AxisX.CustomLabels.Add(119 - 4, 119 + 4, sec_count.ToString());
            #endregion
        }

        private void timer_main_on()
        {
            // main timer on
            current_tr = 0;
            current_tcr = 0;
            average_tr = 0;
            average_tcr = 0;

            sum_tcr = 0;
            sum_tr = 0;
            sec_count = 0;

            LB_current_tr = current_tr.ToString();
            LB_current_tcr = current_tcr.ToString();
            LB_average_tr = average_tr.ToString();
            LB_average_tcr = average_tcr.ToString();

            timer_main = new System.Threading.Timer(timer_main_callback);
            timer_main.Change(0, 1000); // 대기 : 0 / 반복주기 1초

            //MeasurementStartTime = new DateTime(2021, 03, 27, 18, 02, 47, 980);
            MeasurementStartTime = DateTime.Now;
            //Debug.WriteLine(MeasurementStartTime.ToString("mm/dd/yyyy hh:mm:ss.fff"));
            //Debug.WriteLine(MeasurementStartTime);
        }

        private void timer_main_off()
        {
            // timer_main 종료
            if (timer_main != null)
            {
                //    1.대기,반복 무한대로 타이머 종료와같음
                timer_main.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                //////////// 2. 변수 초기화
                //////////timer_count = 0;
                //////////LB_r_time.Text = $"Record Time : 00:00:00";
                //////////LB_setting_time.Text = $"Setting Time : 00:00:00";
                //////////LB_read_count.Text = "Count rate : 0";
                //////////read_fefe_count = 0;
            }
        }

        #region Timer Sub 관련
        //------------------------------+---------------------------------------------------------------
        // [함수] timer_sub 관련
        //------------------------------+---------------------------------------------------------------
        //private void timer_sub_callback(Object state)
        //{
        //    BeginInvoke(new TimerEventFiredDelegate(timer_sub_callback_work));
        //}
        //private void timer_sub_callback_work()
        //{
        //    //////////// 1. 현재 sub time / count값 가져오기
        //    //////////int tb_sub_time = int.Parse(TB_r_time.Text);
        //    //////////int tb_sub_count = int.Parse(TB_r_count.Text);

        //    //////////// 2. 시간감소시키기
        //    //////////tb_sub_time--;
        //    //////////if (tb_sub_time > 0)
        //    //////////{
        //    //////////    TB_r_time.Text = tb_sub_time.ToString();
        //    //////////}
        //    //////////else if(tb_sub_time == 0)
        //    //////////{

        //    //////////}
        //    //////////else // tb_sub_time == 0
        //    //////////{
        //    //////////    if(tb_sub_count != 0)
        //    //////////    {
        //    //////////        tb_sub_time = timer_sub_val;
        //    //////////        tb_sub_count--;
        //    //////////        TB_r_count.Text = tb_sub_count.ToString();
        //    //////////        TB_r_time.Text = tb_sub_time.ToString();
        //    //////////    }
        //    //////////    else
        //    //////////    {
        //    //////////    }
        //    //////////}
        //}
        //private void timer_sub_on()
        //{
        //    // sub timer on
        //    timer_sub = new System.Threading.Timer(timer_sub_callback);
        //    timer_sub.Change(0, 1000); // 대기 : 0 / 반복주기 1초
        //}
        //private void timer_sub_off()
        //{
        //    // timer_sub 종료
        //    if (timer_sub != null)
        //    {
        //        // 1. 대기,반복 무한대로 타이머 종료와같음
        //        timer_sub.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        //        // 2. 변수 초기화
        //        timer_sub_val = 0;
        //        TB_r_time.Text = "0";
        //    }
        //}
        #endregion

        //------------------------------+---------------------------------------------------------------
        // [함수] timer usb 관련
        //------------------------------+---------------------------------------------------------------
        private void timer_start()
        {
            #region Cruxell 주석
            ////////timer_count2 = tb_0x0a * tb_0x0b;
            ////////if (timer_count2 != 0)
            ////////    LB_setting_time.Text = $"Setting Time : {(timer_count2 / 3600):D2}:{(timer_count2 % 3600 / 60):D2}:{(timer_count2 % 3600 % 60):D2}";
            ////////else
            ////////    LB_setting_time.Text = $"Setting Time : 00:00:00";
            #endregion

            timer_main_on();
        }
        private void timer_end()
        {
            timer_main_off();

            #region Cruxell 주석
            //timer_sub_off();
            //// 0. 강제종료
            //if (timer_all_stop == 1)
            //{
            //    timer_main_off();
            //    timer_sub_off();
            //    // 강제종료로 인한 변수 초기화
            //    timer_all_stop = 0;
            //    TB_r_count.Text = "0";
            //}
            //// 1. mode1로 구동중이였던 경우 main만 종료하고 끝
            //else if (timer_mode == 1)
            //{
            //    timer_main_off();
            //}
            //// 2. mode2로 구동중이였던 경우 sub만 종료 -> 상태봐서 main도 종료
            //else if (timer_mode == 2)
            //{
            //    int tb_sub_count = int.Parse(TB_r_count.Text);
            //    // count 1회 감소
            //    TB_r_count.Text = (tb_sub_count - 1).ToString();

            //    // 2-1. count가 0이 됨 = main + sub 종료                        
            //    if (tb_sub_count - 1 == 0)
            //    {
            //        timer_main_off();
            //        timer_sub_off();
            //    }
            //    // 2-2. count가 남아있다 -> sub만 종료 -> sub 값 복원 -> usb 재시작
            //    else
            //    {
            //        // sub end & sub val 복원
            //        timer_sub.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); // 대기,반복 무한대로 타이머 종료와같음
            //        TB_r_time.Text = timer_sub_val.ToString();

            //        // start_stop_usb();
            //    }
            //}
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 콤보박스들 값 변경시 이벤트들
        //------------------------------+---------------------------------------------------------------
        #region 실력이 나중에 더 된다면 그냥 back-end에다가 붙여버리기
        private void event_EndPointsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EndPointsComboBox_SelectedIndexChanged(sender, e);
        }

        private void event_DevicesComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DeviceComboBox_SelectedIndexChanged(sender, e);
        }
        #endregion


        private void ConvertThread()
        {
            using (BinaryReader b = new BinaryReader(File.Open(open_bin_path, FileMode.Open)))
            {
                long pos = 0;
                long length = b.BaseStream.Length;
                byte[] buffer = new byte[1024 * 1024 * 10];
                while (pos < length)
                {
                    int read_length = 1024 * 1024 * 10;
                    if (read_length + pos >= length)
                        read_length = (int)(length - pos);

                    buffer = b.ReadBytes(read_length);
                    for (int i = 0; i < read_length; ++i)
                        check_parsing_state(buffer[i]);
                    pos += read_length;
                }
            }
            if (file_main != null)
                file_main.Close();
            if (file_adc != null)
                file_adc.Close();
        }


        public void print_init(int mode, uint pre, uint post, uint interval_time)
        {
            // 1. Interval time과 현재 시간 출력
            switch (mode)
            {
                case 1:
                    // Continue 모드에서의 출력
                    // file_main.WriteLine($"{interval_time * 10}us, Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Continue Mode, Recording interval: {interval_time * 10}us");
                    file_main.WriteLine();
                    file_main.WriteLine($"Type    Start time(us)      End time(us)        counts on the channels(#)");
                    file_main.WriteLine();
                    break;
                case 2:
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Trigger Mode 1, Recording interval: {interval_time * 10}us, Timing margin pre = {pre}, post = {post}");
                    file_main.WriteLine();
                    file_main.WriteLine($"Number  Start time(us)      End time(us)        counts on the channels(#)");
                    file_main.WriteLine();
                    break;
                case 3:
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Trigger Mode 2, Timing margin pre = {pre}, post = {post}");
                    file_main.WriteLine();
                    file_main.WriteLine($"Number  Start time(us)      End time(us)        counts odn the channels(#)");
                    file_main.WriteLine();
                    break;
                default:
                    break;
            }
        }

        private void BT_convert_Click(object sender, EventArgs e) // -> Command로 이식하기
        {
            open_bin_path = ShowFileOpenDialog();
            if (open_bin_path.CompareTo("") == 0)
                return;
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
        }

        private void button3_Click(object sender, EventArgs e)
        {
            #region Change Directory ( Save Binary File) 이벤트 핸들러
            //// bin
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "Data|*.bin";
            //saveFileDialog1.Title = "Save Binary file";
            //saveFileDialog1.ShowDialog();

            //if (saveFileDialog1.FileName != "")
            //{
            //    TB_bin_save_dir.Text = saveFileDialog1.FileName;
            //}
            #endregion
        }

        private void button2_Click(object sender, EventArgs e)
        {
            #region Change Directory ( Save Data_main File ) 이벤트 핸들러
            //// main
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "Data|*.txt";
            //saveFileDialog1.Title = "Save data_main file";
            //saveFileDialog1.ShowDialog();

            //if (saveFileDialog1.FileName != "")
            //{
            //    TB_data_save_dir.Text = saveFileDialog1.FileName;
            //    TB_trig_save_dir.Text = Path.Combine(Path.GetDirectoryName(TB_data_save_dir.Text), Path.GetFileNameWithoutExtension(TB_data_save_dir.Text) + "_adc.txt");
            //}
            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region Change Directory ( Save Data_adc File ) 이벤트 핸들러
            //// trig
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "Data|*.txt";
            //saveFileDialog1.Title = "Save data_adc file";
            //saveFileDialog1.ShowDialog();

            //if (saveFileDialog1.FileName != "")
            //{
            //    TB_trig_save_dir.Text = saveFileDialog1.FileName;
            //}
            #endregion
        }
    }
}
