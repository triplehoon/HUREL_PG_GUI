using HUREL.PG;
using HUREL.PG.Ncc;
using PG.Fpga;
using System.Runtime.CompilerServices;




class TestClass
{
    private static void TestCruxellLoading()
    {
        CruxellWrapper.PrintSettingValues();        
    }
    private static void TestDeviceLoading()
    {
        CruxellWrapper.PrintDeviceList();
    }
    static void Main(string[] args)
    {
        TestCruxellLoading();
        TestDeviceLoading();
    }
}
