using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace HUREL_PG_GUI.Views
{
    /// <summary>
    /// Window_SpotScanningView_PostProcessing.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window_SpotScanningView_PostProcessing : Window
    {
        public Window_SpotScanningView_PostProcessing()
        {
            InitializeComponent();
        }

        #region WPF에서 닫힌 Window 다시 사용하기(Window 닫기 대신 감추기)
        // 출처: https://blog.naver.com/forour/30123588162
        bool? private_dialog_result;
        delegate void FHideWindow();

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            private_dialog_result = DialogResult;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new FHideWindow(_HideThisWindow));
        }

        private void _HideThisWindow()
        {
            this.Hide();
            (typeof(Window)).GetField("_isClosing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, false);
            (typeof(Window)).GetField("_dialogResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, private_dialog_result);
            private_dialog_result = null;
        }
        #endregion


    }
}
