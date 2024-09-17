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
    private static void TestFpgaDaq()
    {
        CruxellWrapper.StartFpgaDaq();
        // wait for 30 seconds
        System.Threading.Thread.Sleep(30000);
        CruxellWrapper.StopFpgaDaq();
    }
    static void Main(string[] args)
    {
        TestCruxellLoading();
        TestDeviceLoading();
        TestFpgaDaq();
    }
}
