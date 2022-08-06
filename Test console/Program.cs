// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.MultiSlit;
using HUREL.PG.Ncc;

MultislitControl.InitiateNcc();


MultislitControl.CurrentSession.LoadPlanFile(@"D:\3DplotBox_1Gy1A_RT.pld");

Task test = MultislitControl.MonitoringRunFtpAndFpgaLoop();

while (Console.ReadKey().Key != ConsoleKey.Escape)
{
    Console.WriteLine($"{MultislitControl.CurrentSession.Layers.Count}: LayerCount");
    Console.WriteLine($"{MultislitControl.CurrentSession.PgRawData.Count}: PgCount");
    int lastLayerIndex = MultislitControl.CurrentSession.Layers.Count;
    if (lastLayerIndex <= 0)
    {
        continue;
    }
    NccLayer layer = MultislitControl.CurrentSession.Layers[lastLayerIndex - 1];
    List<SpotMap> stopMap = layer.GetSpotMap();
    Console.WriteLine("---------------------------------------------------------------");
    foreach (SpotMap map in stopMap)
    {
        Console.WriteLine($"{map.X}, {map.Y}, ({map.RangeDifference}, {map.MU})");
    }
    Thread.Sleep(1);
}
await MultislitControl.StopMonitoringRunFtpAndFpgaLoop();

await test;

Thread.Sleep(5000);
Console.ReadLine();