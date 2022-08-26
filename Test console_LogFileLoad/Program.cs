// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.Ncc;
using System.Text;

// Verification with Box 2Gy file
NccSession session = new NccSession();



int expCaseNumber = 5;
# region Exp List (20220312)
// Exp list
// 1   PMMA_SingleE130MeV_500spots_1MU
// 2   PMMA_NoShift1_Sph_1Gy
// 3   PMMA_NoShift1_Sph_2Gy
// 4   PMMA_NoShift1_Box_1Gy
// 5   PMMA_NoShift1_Box_2Gy(Default)
// 6   PMMA_Global_01mm_Sph_1Gy
// 7   PMMA_Global_01mm_Sph_2Gy
// 8   PMMA_Global_01mm_Box_1Gy
// 9   PMMA_Global_01mm_Box_2Gy
// 10  PMMA_Global_02mm_Sph_1Gy
// 11  PMMA_Global_02mm_Sph_2Gy
// 12  PMMA_Global_02mm_Box_1Gy
// 13  PMMA_Global_02mm_Box_2Gy
// 14  PMMA_Global_03mm_Sph_1Gy
// 15  PMMA_Global_03mm_Sph_2Gy
// 16  PMMA_Global_03mm_Box_1Gy
// 17  PMMA_Global_03mm_Box_2Gy
// 18  PMMA_Global_04mm_Sph_1Gy
// 19  PMMA_Global_04mm_Sph_2Gy
// 20  PMMA_Global_04mm_Box_1Gy
// 21  PMMA_Global_04mm_Box_2Gy
// 22  PMMA_Global_05mm_Sph_1Gy
// 23  PMMA_Global_05mm_Sph_2Gy
// 24  PMMA_Global_05mm_Box_1Gy
// 25  PMMA_Global_05mm_Box_2Gy
// 26  PMMA_Global_08mm_Sph_1Gy
// 27  PMMA_Global_08mm_Sph_2Gy
// 28  PMMA_Global_08mm_Box_1Gy
// 29  PMMA_Global_08mm_Box_2Gy
// 30  PMMA_Global_10mm_Sph_1Gy
// 31  PMMA_Global_10mm_Sph_2Gy
// 32  PMMA_Global_10mm_Box_1Gy
// 33  PMMA_Global_10mm_Box_2Gy
// 34  PMMA_SingleLocal_01mm_Sph_1Gy
// 35  PMMA_SingleLocal_01mm_Sph_2Gy
// 36  PMMA_SingleLocal_01mm_Box_1Gy
// 37  PMMA_SingleLocal_01mm_Box_2Gy
// 38  PMMA_SingleLocal_02mm_Sph_1Gy
// 39  PMMA_SingleLocal_02mm_Sph_2Gy
// 40  PMMA_SingleLocal_02mm_Box_1Gy
// 41  PMMA_SingleLocal_02mm_Box_2Gy
// 42  PMMA_SingleLocal_03mm_Sph_1Gy
// 43  PMMA_SingleLocal_03mm_Sph_2Gy
// 44  PMMA_SingleLocal_03mm_Box_1Gy
// 45  PMMA_SingleLocal_03mm_Box_2Gy
// 46  PMMA_SingleLocal_04mm_Sph_1Gy
// 47  PMMA_SingleLocal_04mm_Sph_2Gy
// 48  PMMA_SingleLocal_04mm_Box_1Gy
// 49  PMMA_SingleLocal_04mm_Box_2Gy
// 50  PMMA_SingleLocal_05mm_Sph_1Gy
// 51  PMMA_SingleLocal_05mm_Sph_2Gy
// 52  PMMA_SingleLocal_05mm_Box_1Gy
// 53  PMMA_SingleLocal_05mm_Box_2Gy
// 54  PMMA_SingleLocal_08mm_Sph_1Gy
// 55  PMMA_SingleLocal_08mm_Sph_2Gy
// 56  PMMA_SingleLocal_08mm_Box_1Gy
// 57  PMMA_SingleLocal_08mm_Box_2Gy
// 58  PMMA_SingleLocal_10mm_Sph_1Gy
// 59  PMMA_SingleLocal_10mm_Sph_2Gy
// 60  PMMA_SingleLocal_10mm_Box_1Gy
// 61  PMMA_SingleLocal_10mm_Box_2Gy
// 62  PMMA_MultiE_5000spots_0p1MU
// 63  PMMA_MultiE_1000spots_0p1MU
// 64  PMMA_SingleE160MeV_multiPos_1000spots_0p1MU
// 65  PMMA_SingleE150MeV_multiPos_1000spots_0p1MU
// 66  PMMA_SingleE140MeV_multiPos_1000spots_0p1MU
// 67  PMMA_SingleE130MeV_multiPos_1000spots_0p1MU
// 68  PMMA_SingleE120MeV_multiPos_1000spots_0p1MU
// 69  PMMA_SingleE110MeV_multiPos_1000spots_0p1MU
// 70  PMMA_SingleE100MeV_multiPos_1000spots_0p1MU
// 71  PMMA_NoShift2_Sph_1Gy
// 72  PMMA_NoShift2_Sph_2Gy
// 73  PMMA_NoShift2_Box_1Gy
// 74  PMMA_NoShift2_Box_2Gy
// 75  PMMA_MultiLocal_04thSlab_01 & 02mm_Sph_1Gy
// 76  PMMA_MultiLocal_04thSlab_01 & 02mm_Sph_2Gy
// 77  PMMA_MultiLocal_04thSlab_01 & 02mm_Box_1Gy
// 78  PMMA_MultiLocal_04thSlab_01 & 02mm_Box_2Gy
// 79  PMMA_MultiLocal_04thSlab_03 & 04mm_Sph_1Gy
// 80  PMMA_MultiLocal_04thSlab_03 & 04mm_Sph_2Gy
// 81  PMMA_MultiLocal_04thSlab_03 & 04mm_Box_1Gy
// 82  PMMA_MultiLocal_04thSlab_03 & 04mm_Box_2Gy
// 83  PMMA_MultiLocal_04thSlab_05 & 10mm_Sph_1Gy
// 84  PMMA_MultiLocal_04thSlab_05 & 10mm_Sph_2Gy
// 85  PMMA_MultiLocal_04thSlab_05 & 10mm_Box_1Gy
// 86  PMMA_MultiLocal_04thSlab_05 & 10mm_Box_2Gy
// 87  PMMA_MultiLocal_11thSlab_01 & 02mm_Sph_1Gy
// 88  PMMA_MultiLocal_11thSlab_01 & 02mm_Sph_2Gy
// 89  PMMA_MultiLocal_11thSlab_01 & 02mm_Box_1Gy
// 90  PMMA_MultiLocal_11thSlab_01 & 02mm_Box_2Gy
// 91  PMMA_MultiLocal_11thSlab_05 & 10mm_Sph_1Gy
// 92  PMMA_MultiLocal_11thSlab_05 & 10mm_Sph_2Gy
// 93  PMMA_MultiLocal_11thSlab_05 & 10mm_Box_1Gy
// 94  PMMA_MultiLocal_11thSlab_05 & 10mm_Box_2Gy
// 95  RandoMale_1ALT_NoShift
// 96  RandoMale_1ALT_Global_01mm
// 97  RandoMale_1ALT_Global_02mm
// 98  RandoMale_1ALT_Global_04mm
// 99  RandoMale_1ALT_Global_05mm
// 100 RandoMale_1ALT_Global_07mm
// 101 RandoMale_1ALT_Global_10mm
// 102 RandoMale_1ALT_Local_01mm
// 103 RandoMale_1ALT_Local_02mm
// 104 RandoMale_1ALT_Local_03mm
// 105 RandoMale_1ALT_Local_04mm
// 106 RandoMale_1ALT_Local_05mm
// 107 RandoMale_1ALT_Local_07mm
// 108 RandoMale_1ALT_Local_10mm
// 109 RandoMale_1ALT_Global_03mm
#endregion

string debugDirectory = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\06. sameResultTest\Debug\";
string expListExcel = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\06. sameResultTest\ExperimentList\ExpList_220312.csv";
List<expInfo> expInfoList = new List<expInfo>();
using (FileStream fs = new FileStream(expListExcel, FileMode.Open))
{
    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
    {
        string line;
        string[] tempString = new string[6];

        expInfo emptySingleExp = new expInfo();
        expInfoList.Add(emptySingleExp);

        while ((line = sr.ReadLine()!) != null)
        {
            tempString = line.Split(",");

            expInfo singleExp = new expInfo();

            singleExp.numCase = Convert.ToInt32(tempString[0]);
            singleExp.phantomName = tempString[1];
            singleExp.pgFileName = tempString[2];
            singleExp.pldFileName = tempString[3];
            singleExp.spot3DFileName = tempString[4];
            singleExp.logFileName = tempString[5];

            expInfoList.Add(singleExp);
        }
    }
}

string mainFolder = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\06. sameResultTest\data_sorted\";
string folderPG = @"pg_bin\";
string folder2DPLD = @"pld2d\";
string folder3DPLD = @"pld3d\";
string folderLog = @"log\";

string folder_PG = string.Concat(mainFolder, folderPG);
string folder_2DPLD = string.Concat(mainFolder, folder2DPLD);
string folder_3DPLD = string.Concat(mainFolder, folder3DPLD);
string folder_Log = string.Concat(mainFolder, folderLog);


// ======================================= //
// ========== 1. Load plan file ========== //
// ======================================= //
List<string> fileList2DPLD = Directory.GetFiles(folder_2DPLD).ToList();
string plan2DFileDir = (from file in fileList2DPLD
                        where file.Contains(expInfoList[expCaseNumber].pldFileName)
                        select file).ToList()[0];

List<string> fileList3DPLD = Directory.GetFiles(folder_3DPLD).ToList();
string plan3DFileDir = (from file in fileList3DPLD
                        where file.Contains(expInfoList[expCaseNumber].spot3DFileName)
                        select file).ToList()[0];
bool flagFlatten = true;
bool flagDebug_Plan = true;
session.LoadPlanFile(plan3DFileDir, plan2DFileDir, flagFlatten, flagDebug_Plan);


// ======================================= //
// ========== 2. Load log files ========== //
// ======================================= //
List<string> fileList = Directory.GetFiles(mainFolder).ToList();

List<string> expLogList = Directory.GetFiles(folder_Log).ToList();
string configLogFileDir = (from file in expLogList
                    where file.Contains(expInfoList[expCaseNumber].logFileName)
                    where file.Contains("idt_config")
                    select file).ToList()[0];
session.LoadConfigLogFile(configLogFileDir);

List<string> recordLogFilesList = (from file in expLogList
                        where file.Contains(expInfoList[expCaseNumber].logFileName)
                        where file.Contains("map_record_")
                        select file).ToList();
foreach (string recordDir in recordLogFilesList)
{
    string specifDir = recordDir.Replace("record", "specif");    
    session.LoadRecordSpecifLogFile(recordDir, specifDir);
}

bool flagDebug_Log = true;
if (flagDebug_Log) // Excel debugging
{
    List<NccSpot> planLog = new List<NccSpot>();
    List<NccSpot> planLogTemp = new List<NccSpot>();

    foreach (NccLayer layer in session.Layers)
    {
        foreach (NccSpot spot in layer.Spots)
        {
            planLogTemp.Add(spot);
        }
    }
    planLog = planLogTemp.OrderBy(x => x.BeamStartTime).ToList();

    string DebugFileName = @"2_Debug_PlanLog.csv";
    string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
    using (StreamWriter file = new StreamWriter(DebugPath_excel))
    {
        var refTime = planLog[0].BeamStartTime;

        file.WriteLine($"Spot #, StartTime, EndTime, Layer #, Tuning #, Resume #, Part #, X, Y, Layer #, Layer E, X, Y, Z, MU");

        for (int i = 0; i < planLog.Count; i++)
        {
            double L_startTime = (planLog[i].BeamStartTime - refTime).Ticks;            
            double L_endTime = (planLog[i].BeamEndTime - refTime).Ticks;
            //double L_startTime = (planLog[i].BeamStartTime - refTime).Ticks / 10000000;
            //double L_endTime = (planLog[i].BeamEndTime - refTime).Ticks / 10000000;
            var L_layerNumber = planLog[i].LayerNumber + 1;

            int L_tuningNum = 0;
            if (planLog[i].BeamState == NccSpot.NccBeamState.Tuning)
            {
                L_tuningNum = Convert.ToInt32(planLog[i].LayerId.Split("_").Last());
            }

            int L_resumeNum = 0;
            if (planLog[i].BeamState == NccSpot.NccBeamState.Resume)
            {
                L_tuningNum = Convert.ToInt32(planLog[i].LayerId.Split("_").Last());
            }
                        
            int L_partNum = 0;            
            var L_Xpos = planLog[i].XPosition;
            var L_Ypos = planLog[i].YPosition;

            var P_LayerNumber = planLog[i].PlanSpot.LayerNumber + 1;
            var P_LayerEnergy = planLog[i].PlanSpot.LayerEnergy;
            var P_Xpos = planLog[i].PlanSpot.Xposition;
            var P_Ypos = planLog[i].PlanSpot.Yposition;
            var P_Zpos = planLog[i].PlanSpot.Zposition;
            var P_MU = planLog[i].PlanSpot.MonitoringUnit;

            file.WriteLine($"{i + 1}, {L_startTime}, {L_endTime}, {L_layerNumber}, {L_tuningNum}, {L_resumeNum}, {L_partNum}, {L_Xpos}, {L_Ypos}, {P_LayerNumber}, {P_LayerEnergy}, {P_Xpos}, {P_Ypos}, {P_Zpos}, {P_MU}");            
        }
    }
}


// ===================================== //
// ========== 3. Load PG file ========== //
// ===================================== //
List<string> pgFileListDir = Directory.GetFiles(folder_PG).ToList();
string pgFileDir = (from file in pgFileListDir
                    where file.Contains(expInfoList[expCaseNumber].pgFileName)
                    select file).ToList()[0];
bool flagBrokenScin = false;
bool flagDebug_PG = true;
session.LoadPGFile(pgFileDir, flagBrokenScin, flagDebug_PG);


// ======================================== //
// ========== 4. Post-processing ========== //
// ======================================== //
string peakAndGapRangeFileDir = @"\\166.104.155.16\HUREL_Data\99.임시보관자료\정재린_임시\GUI Data\gapPeakAndRange.txt";
session.LoadPeakAndGapRange(peakAndGapRangeFileDir);

bool flagDebug_PGsplit = true;
session.PostProcessing_NCC(flagDebug_PGsplit);




public class expInfo
{
    public int numCase;
    public string phantomName;
    public string pgFileName;
    public string pldFileName;
    public string spot3DFileName;
    public string logFileName;
    //public string startTime;
}