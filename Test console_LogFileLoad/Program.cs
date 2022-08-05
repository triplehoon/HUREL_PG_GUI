// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;

// Verification with Box 2Gy file
NccSession session = new NccSession();


// 0. Set root folder
string rootFolder = @"\\166.104.155.16\HUREL_Data\99.임시보관자료\정재린_임시\GUI Data\";
string caseNumber = @"case5\";

string mainFolder = string.Concat(rootFolder, caseNumber);
List<string> fileList = Directory.GetFiles(mainFolder).ToList();


// ======================================= //
// ========== 1. Load plan file ========== //
// ======================================= //
string planFileDir = (from file in fileList
                      where file.Contains("pld")
                      select file).ToList()[0];
session.LoadPlanFile(planFileDir);


// ======================================= //
// ========== 2. Load log files ========== //
// ======================================= //
string configLogFileDir = (from file in fileList
                           where file.Contains("config")
                           select file).ToList()[0];
session.LoadConfigLogFile(configLogFileDir);

List<string> recordLogFilesList = (from file in fileList
                                   where file.Contains("record")
                                   where file.Contains("xdr")
                                   select file).ToList();
foreach (string recordDir in recordLogFilesList)
{
    string specifDir = recordDir.Replace("record", "specif");
    session.LoadRecordSpecifLogFile(recordDir, specifDir);
}


// ===================================== //
// ========== 3. Load PG file ========== //
// ===================================== //
string pgFileDir = (from file in fileList
                    where file.Contains("bin")
                    select file).ToList()[0];
session.LoadPGFile(pgFileDir, true);

//int[] pgCounts_144ch = new int[144];
//for (int ch = 0; ch < 144; ch++)
//{
//    int tempSum = 0;
//    for (int idx_line = 0; idx_line < PG_raw.Count; idx_line++)
//    {
//        tempSum += PG_raw[idx_line].ChannelCount[ch];
//    }
//    pgCounts_144ch[ch] = tempSum;
//    // Console.WriteLine($"{ch}, {tempSum}");
//}

// ======================================== //
// ========== 4. Post-processing ========== //
// ======================================== //
string peakAndGapRangeFileName = @"gapPeakAndRange.txt";
string peakAndGapRangeFileDir = string.Concat(rootFolder, peakAndGapRangeFileName);
session.LoadPeakAndGapRange(peakAndGapRangeFileDir);

session.PostProcessing_NCC();



// ================================ //
// ========== To-do list ========== //
// ================================ //
// a. gapPeakAndRange load, data save in NccSession (constructor)
// b. getSpotMap write, data save in NccSession
// c. getBeamRangeMap write, data save in NccSession






int a = 1;
















//string pgPath = @"\\166.104.155.16\HUREL_Data\99.임시보관자료\정재린_임시\";
////string pgFile = "data.bin";      // SP34, isocenter depth =  70 mm,  95.09 MeV, 10000 spots, 0.1 MU
////string pgFile = "data (2).bin";  // SP34, isocenter depth = 110 mm, 122.6  MeV, 10000 spots, 0.1 MU
//string pgFile = "data (4).bin";  // SP34, isocenter depth = 150 mm, 146.45 MeV, 10000 spots, 0.1 MU
////string pgFile = "data (5).bin";  // SP34, isocenter depth = 230 mm, 186.3  MeV, 10000 spots, 0.1 MU

////string pgPath = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료\03. PG\";
////string pgFile = "Box2Gy_QuadLocalShift_1.bin";
////string pgPath = @"\\166.104.155.16\HUREL_Data\99.임시보관자료\구영모_임시\GUI검증데이터\data_220312_NCC\data_raw\pg\data\";
////string pgFile = "data.bin";    // 130 MeV, 500 spots, 1 MU
////string pgFile = "data (64).bin";  // 160 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (65).bin";  // 150 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (66).bin";  // 140 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (67).bin";  // 130 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (68).bin";  // 120 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (69).bin";  // 110 MeV, 1000 spots, 0.1 MU
////string pgFile = "data (70).bin";  // 100 MeV, 1000 spots, 0.1 MU
//string pgDir = string.Concat(pgPath, pgFile);
//bool flag_PG = session.LoadPGFile(pgDir);

//var PG_raw = session.MultislitPgData.GetPGSpots();
//bool flag_PostProcessing = session.PostProcessing_NCC();
//int[] pgCounts_144ch = new int[144];
//for (int ch = 0; ch < 144; ch++)
//{
//    int tempSum = 0;
//    for (int idx_line = 0; idx_line < PG_raw.Count; idx_line++)
//    {
//        tempSum += PG_raw[idx_line].ChannelCount[ch];
//    }
//    pgCounts_144ch[ch] = tempSum;
//    // Console.WriteLine($"{ch}, {tempSum}");
//}

//double isocenterDepth = 230;
//FunctionTestClass testClass = new FunctionTestClass();
//double range = isocenterDepth + testClass.getRange(pgCounts_144ch, 0);


//foreach (NccLayer layer in session.Layers)
//{
//    Console.WriteLine($"Layer: {layer.LayerNumber}, Energy: {layer.LayerEnergy}, Spot: {layer.Spots.Count}");
//}
////Console.WriteLine($"FPGA data lines: {session.PGspots.Count()}");

