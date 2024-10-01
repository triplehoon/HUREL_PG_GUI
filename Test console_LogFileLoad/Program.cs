// See https://aka.ms/new-console-template for more information
using HUREL.PG;
using HUREL.PG.NccHelper;



NccSession session = new NccSession();

//Test for SingleE160MeV_multiPos+1000spots ....

string pldFilePath = @"E:\OneDrive - 한양대학교\01.Hurel\01.현재작업\20220531 PG GUI\검증데이터\data_220312_NCC\data_raw\plan\PMMA\Sphere1Gy\3DplotSphere_1Gy1A_RT.pld";
string logDir = @"E:\OneDrive - 한양대학교\01.Hurel\01.현재작업\20220531 PG GUI\검증데이터\data_220312_NCC\data_raw\log";
string logMainName = "20220312_110258_671";

string binaryFilePath = @"E:\OneDrive - 한양대학교\01.Hurel\01.현재작업\20220531 PG GUI\검증데이터\data_220312_NCC\data_sorted\pg_bin\PMMA_NoShift1_Sph_1Gy.bin";

List<string> logFiles = Directory.GetFiles(logDir).ToList();

List<string> selectedLogFiles = logFiles.FindAll(x => x.Contains(logMainName) && x.Contains("record"));
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
int spotCounts = 0;
foreach (string logFile in selectedLogFiles)
{
    string specifFile = logFile.Replace("record", "specif");
    session.LoadRecordSpecifLogFile(logFile, specifFile);

}

foreach (var layer in session.Layers)
{
    Console.WriteLine($"{layer.NccBeamState}, {layer.LayerId}");
    spotCounts += layer.Spots.Count;
}


session.LoadPgFile(binaryFilePath);

Console.WriteLine($"{session.PgSpots.Count}");
var spotMap = session.Layers[5].GetSpotMap();

Console.WriteLine("done");
//List<NccSpot> mergeNCCSpotData(List<MultiSlitPg> pgSpots, List<NccLayer> layers)
//{
//    // Return data
//    List<NccSpot> nccSpots = new List<NccSpot>();

//    // 1. Set parameter
//    // 2. Get reference time (PlanLog)
//    // 3. Get reference time (PG)
//    // 4. Merge data

//    #region 1. Set parameter
//    int refTimePG = 0;
//    int refTimePlanLog = 0;
//    int numOfCompareSpot = 10;
//    #endregion

//    #region 2. Get reference time (PlanLog)
//    List<NccSpot> PlanLog = new List<NccSpot>();
//    foreach (NccLayer layer in layers)
//    {
//        foreach (NccSpot spot in layer.Spots)
//        {
//            PlanLog.Add(spot);
//        }
//    }

//    bool getRefTimePlanLog = false;
//    logDir refTimePlanLogTemp = PlanLog[0].BeamStartTime.Ticks;
//    for (int i = 0; i < PlanLog.Count - numOfCompareSpot; i++)
//    {
//        if (PlanLog[i + numOfCompareSpot].TriggerStartTime - PlanLog[i].TriggerStartTime < 0.5E6)
//        {
//            getRefTimePlanLog = true;
//            refTimePlanLog = PlanLog[i].TriggerStartTime;
//            break;
//        }
//    }

//    if (getRefTimePlanLog == false)
//    {
//        refTimePlanLog = 0;
//        return nccSpots;
//    }

//    for (int i = 0; i < PlanLog.Count; i++)
//    {
//        PlanLog[i].SetLogTick(PlanLog[i].TriggerStartTime - refTimePlanLog);
//    }
//    #endregion

//    #region 3. Get reference time (PG)
//    bool getRefTimePG = false;
//    for (int i = 0; i < pgSpots.Count - numOfCompareSpot; i++)
//    {
//        if (pgSpots[i + numOfCompareSpot].TriggerStartTime - pgSpots[i].TriggerStartTime < 0.5E6)
//        {
//            getRefTimePG = true;
//            refTimePG = pgSpots[i].TriggerStartTime;
//            break;
//        }
//    }

//    if (getRefTimePG == false)
//    {
//        refTimePG = 0;
//        return nccSpots;
//    }

//    List<MultiSlitPg> pgSpots_withTick = new List<MultiSlitPg>();
//    foreach (MultiSlitPg spot in pgSpots)
//    {
//        int pgTick = spot.TriggerStartTime - refTimePG;
//        pgSpots_withTick.Add(new MultiSlitPg(spot.ChannelCount, spot.SumCounts, spot.TriggerStartTime, spot.TriggerEndTime, spot.ADC, pgTick));
//    }
//    #endregion

//    #region 4. Merge data (pgSpots_withTick, PlanLog)
//    int numOfCompareSpots = 10;
//    int timeMargin = 100000;    // 100 ms

//    int numOfPGSpots = pgSpots_withTick.Count;

//    int index_PG = 0;
//    int index_PlanLog = 0;

//    while (index_PG < pgSpots_withTick.Count && index_PlanLog < PlanLog.Count)
//    {
//        if (Math.Abs(pgSpots_withTick[index_PG].Tick - PlanLog[index_PlanLog].LogTick) < timeMargin)
//        {
//            PlanLog[index_PlanLog].SetPgData(pgSpots_withTick[index_PG]);
//            nccSpots.Add(PlanLog[index_PlanLog]);
//            index_PG++;
//            index_PlanLog++;
//        }
//        else
//        {
//            bool isSpotMatched = false;
//            for (int index_PG_temp = index_PG; index_PG_temp < Math.Min(index_PG + numOfCompareSpots, numOfPGSpots); index_PG_temp++)
//            {
//                if (Math.Abs(pgSpots_withTick[index_PG_temp].Tick - PlanLog[index_PlanLog].LogTick) < timeMargin)
//                {
//                    PlanLog[index_PlanLog].SetPgData(pgSpots_withTick[index_PG]);
//                    nccSpots.Add(PlanLog[index_PlanLog]);

//                    index_PG = index_PG_temp + 1;
//                    index_PlanLog++;

//                    isSpotMatched = true;
//                    break;
//                }
//            }

//            if (isSpotMatched == false)
//            {

//                nccSpots.Add(PlanLog[index_PlanLog]);


//                index_PlanLog++;

//                if (index_PG + numOfCompareSpots > numOfPGSpots)
//                {
//                    break;
//                }
//            }
//        }
//    }
//    #endregion

//    return nccSpots;
//}
