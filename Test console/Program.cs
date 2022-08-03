// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;



NccSession session = new NccSession();

//Test for SingleE160MeV_multiPos+1000spots ....

string pldFilePath = @"E:\OneDrive - 한양대학교\01.Hurel\01.현재작업\20220531 PG GUI\검증데이터\data_220312_NCC\data_raw\plan\PMMA\Sphere1Gy\3DplotSphere_1Gy1A_RT.pld";
string logDir = @"E:\OneDrive - 한양대학교\01.Hurel\01.현재작업\20220531 PG GUI\검증데이터\data_220312_NCC\data_raw\log";
string logMainName = "20220312_110258_671";

List<string> logFiles = Directory.GetFiles(logDir).ToList();

List<string> selectedLogFiles = logFiles.FindAll(x =>  x.Contains(logMainName) && x.Contains("record"));
string? selectedConfigFile = logFiles.Find(x => x.Contains(logMainName) && x.Contains("config"));
if (selectedConfigFile == null)
{
    Console.WriteLine("Cannot find config file");
    return;
}
else
{
    session.LoadConfigLogFile(selectedConfigFile);
}
session.LoadPlanFile(pldFilePath);

foreach (string logFile in selectedLogFiles)
{
    string specifFile = logFile.Replace("record", "specif");
    session.LoadRecordSpecifLogFile(logFile, specifFile);
}

foreach (var layer in session.Layers)
{
    Console.WriteLine($"{layer.NccBeamState}, {layer.LayerId}");
}

Console.WriteLine("done");
