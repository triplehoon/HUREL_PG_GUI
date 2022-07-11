// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;

// Verification with Box 2Gy file
NccSession session = new NccSession();

string planFileFolder = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료\01. Plan\";
string planFileName = @"3DplotMultiSBox2Gy.pld";
string planFileDir = string.Concat(planFileFolder, planFileName);
bool flag_plan = session.LoadPlanFile(planFileDir);

string logFileFolder = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료\02. Log\Box2Gy_NoShift_1\";
string configLogFileName = @"20210327_183905_097.idt_config.csv";
string recordLogFileName1 = @"20210327_183905_097.map_record_0007.xdr";
string specifLogFileName1 = @"20210327_183905_097.map_specif_0007.xdr";

string recordLogFileName2 = @"20210327_183905_097.map_record_0002.xdr";
string specifLogFileName2 = @"20210327_183905_097.map_specif_0002.xdr";

string recordLogFileName3 = @"20210327_183905_097.map_record_0003.xdr";
string specifLogFileName3 = @"20210327_183905_097.map_specif_0003.xdr";

string recordLogFileName4 = @"20210327_183905_097.map_record_0001_tuning_01.xdr";
string specifLogFileName4 = @"20210327_183905_097.map_specif_0001_tuning_01.xdr";

string recordLogFileName5 = @"20210327_183905_097.map_record_0001.xdr";
string specifLogFileName5 = @"20210327_183905_097.map_specif_0001.xdr";

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

bool flag_config = session.LoadConfigLogFile(configLogFile);
bool flag_recordspecif1 = session.LoadRecordSpecifLogFile(recordLogFile1, specifLogFile1);
bool flag_recordspecif2 = session.LoadRecordSpecifLogFile(recordLogFile2, specifLogFile2);
bool flag_recordspecif3 = session.LoadRecordSpecifLogFile(recordLogFile3, specifLogFile3);
bool flag_recordspecif4 = session.LoadRecordSpecifLogFile(recordLogFile4, specifLogFile4);
bool flag_recordspecif5 = session.LoadRecordSpecifLogFile(recordLogFile5, specifLogFile5);

foreach (NccLayer layer in session.Layers)
{
    Console.WriteLine($"Layer: {layer.LayerNumber}, Energy: {layer.PlanEnergy}, Spot: {layer.GetSpot().Count}");   
}


