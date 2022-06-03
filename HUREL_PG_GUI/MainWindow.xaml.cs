using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HUREL_PG_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTU2MjAyQDMxMzkyZTM0MmUzMFBjTWVDd2FmeXVGejBPU0VjdTlONTVpdm1kSjJlSW0zODFjUW9PRkh6a0k9");
            InitializeComponent();
        }

        #region MainWindow가 닫힐 경우에 대한 event 작업 추가할 것
        // MainWindow가 닫힐 경우에 대한 event 추가
        //if (StartBtn.Text.Equals("Start") == false)
        //{
        //    Command_MonitoringStart();
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
}
