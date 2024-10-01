using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL_PG_GUI.ViewModels
{
    class VM_DAQsetting
    {
        static public string corrFactorsDirectory = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\00. New Version\HUREL_PG_GUI\HUREL_PG_GUI\Datas\corrFactors.txt";

        #region 측정 모드에 따른 visibility 설정(미구현)
        //if (variables_FPGA.CountMode_0x11 == 0) // Continuous mode
        //{
        //    //TB_0x12.Visible = true;
        //    //TB_0x13.Visible = true;
        //    //TB_0x14.Visible = false;
        //    //TB_0x15.Visible = false;
        //    //TB_0x16.Visible = false;
        //    //TB_0x17.Visible = false;
        //    //TB_0x18.Visible = true;
        //}
        //else if (variables_FPGA.CountMode_0x11 == 1) // TRIG 1 mode
        //{
        //    //TB_0x12.Visible = true;
        //    //TB_0x13.Visible = true;
        //    //TB_0x14.Visible = true;
        //    //TB_0x15.Visible = true;
        //    //TB_0x16.Visible = false;
        //    //TB_0x17.Visible = false;
        //    //TB_0x18.Visible = true;
        //}
        //else // TRIG 2 mode
        //{
        //    //TB_0x12.Visible = true;
        //    //TB_0x13.Visible = false;
        //    //TB_0x14.Visible = false;
        //    //TB_0x15.Visible = false;
        //    //TB_0x16.Visible = true;
        //    //TB_0x17.Visible = true;
        //    //TB_0x18.Visible = true;
        //}
        #endregion
    }
}
