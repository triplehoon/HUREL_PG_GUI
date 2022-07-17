// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;

// Verification with Box 2Gy file
NccSession session = new NccSession();

#region Plan File Name definition
string planFileFolder = @"Test Files\";
string planFileName = @"3DplotMultiSBox2Gy.pld";
string planFileDir = string.Concat(planFileFolder, planFileName);
#endregion
#region Log File Name definition
string logFileFolder = @"Test Files\";
string configLogFileName = @"20210327_212007_821.idt_config.csv";
string recordLogFileName1 = @"20210327_212007_821.map_record_0007.xdr";
string specifLogFileName1 = @"20210327_212007_821.map_specif_0007.xdr";

string recordLogFileName2 = @"20210327_212007_821.map_record_0002.xdr";
string specifLogFileName2 = @"20210327_212007_821.map_specif_0002.xdr";

string recordLogFileName3 = @"20210327_212007_821.map_record_0003.xdr";
string specifLogFileName3 = @"20210327_212007_821.map_specif_0003.xdr";

string recordLogFileName4 = @"20210327_212007_821.map_record_0001_tuning_01.xdr";
string specifLogFileName4 = @"20210327_212007_821.map_specif_0001_tuning_01.xdr";

string recordLogFileName5 = @"20210327_212007_821.map_record_0001.xdr";
string specifLogFileName5 = @"20210327_212007_821.map_specif_0001.xdr";

string configLogFile = string.Concat(logFileFolder, configLogFileName);
string recordLogFile1 = string.Concat(logFileFolder, recordLogFileName1);
string specifLogFile1 = string.Concat(logFileFolder, specifLogFileName1);

string recordLogFile2 = string.Concat(logFileFolder, recordLogFileName2);
string specifLogFile2 = string.Concat(logFileFolder, specifLogFileName2);

string recordLogFile3 = string.Concat(logFileFolder, recordLogFileName3);
string specifLogFile3 = string.Concat(logFileFolder, specifLogFileName3);

string recordLogFile4 = string.Concat(logFileFolder, recordLogFileName4);
string specifLogFile4 = string.Concat(logFileFolder, specifLogFileName4);

string recordLogFile5 = string.Concat(logFileFolder, recordLogFileName5);
string specifLogFile5 = string.Concat(logFileFolder, specifLogFileName5);
#endregion
#region PG file definition
string pgFileFolder = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료\03. PG\";
string pgFileName = @"Box2Gy_QuadLocalShift_1.bin";
string pgFileDir = string.Concat(pgFileFolder, pgFileName);
#endregion


bool flag_plan = session.LoadPlanFile(planFileDir);

bool flag_config = session.LoadConfigLogFile(configLogFile);
bool flag_recordspecif1 = session.LoadRecordSpecifLogFile(recordLogFile1, specifLogFile1);
bool flag_recordspecif2 = session.LoadRecordSpecifLogFile(recordLogFile2, specifLogFile2);
bool flag_recordspecif3 = session.LoadRecordSpecifLogFile(recordLogFile3, specifLogFile3);
bool flag_recordspecif4 = session.LoadRecordSpecifLogFile(recordLogFile4, specifLogFile4);
bool flag_recordspecif5 = session.LoadRecordSpecifLogFile(recordLogFile5, specifLogFile5);
bool flag_recordspecif6 = session.LoadRecordSpecifLogFile(recordLogFile1, specifLogFile1);

bool flag_pgFile = session.LoadPGFile(pgFileDir);

foreach (NccLayer layer in session.Layers)
{
    Console.WriteLine($"Layer: {layer.LayerNumber}, Energy: {layer.LayerEnergy}, Spot: {layer.Spots.Count}");
}
//Console.WriteLine($"FPGA data lines: {session.PGspots.Count()}");
Console.WriteLine($"FPGA data lines: {session.MultislitPgData.GetPGSpots().Count}");

