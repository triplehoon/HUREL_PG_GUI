using CenterSpace.NMath.Core;
using System.Diagnostics;
using System.Text;

namespace HUREL.PG.Ncc
{
    public class NccSpot : Spot
    {
        public DateTime BeamStartTime { get; private set; }
        public DateTime BeamEndTime { get; private set; }
        public NccBeamState BeamState { get; private set; }
        public int LayerNumber { get; private set; }
        public string LayerId { get; private set; }
        public double XPosition { get; private set; }
        public double YPosition { get; private set; }
        public int LogTick { get; private set; }

        // int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0);

        public int[] ChannelCount { get; private set; }
        public int SumCounts { get; private set; }
        public int TriggerStartTime { get; private set; }
        public int TriggerEndTime { get; private set; }
        public double ADC { get; private set; }

        //static public List<NccSpot> SortByBeamTime(List<NccSpot> spots)
        //{
        //    spots.OrderBy(x => x.BeamStartTime);
        //    return spots;
        //}
        public void setLogTick(int tick)
        {
            this.LogTick = tick;
        }

        private NccPlanSpot planSpot;
        public NccPlanSpot PlanSpot
        {
            get
            {
                if (planSpot == null) // case: log-plan spot distance is not matched within ToleranceDistBetweenPlanAndLogSpot cm
                {
                    return new NccPlanSpot();
                }
                else
                {
                    return planSpot;
                }

                //if (BeamState == NccBeamState.Tuning)
                //{
                //    Debug.Assert(true, "Tuning beam has no Plan info");
                //    return new NccPlanSpot();
                //}
                //else
                //{
                //    return planSpot;
                //}
            }
            private set
            {
                planSpot = value;
            }
        }
        public NccSpot(NccSpot PlanLog)
        {
            planSpot = PlanLog.planSpot;

            BeamStartTime = PlanLog.BeamStartTime;
            BeamEndTime = PlanLog.BeamEndTime;
            BeamState = PlanLog.BeamState;
            LayerNumber = PlanLog.LayerNumber;
            LayerId = PlanLog.LayerId;
            XPosition = PlanLog.XPosition;
            YPosition = PlanLog.YPosition;
            LogTick = PlanLog.LogTick;

            ChannelCount = new int[144];
            for (int i = 0; i < 144; i++)
            {
                ChannelCount[i] = 1;
            }

            SumCounts = 0;
            TriggerStartTime = 0;
            TriggerEndTime = 0;
            ADC = 0;
        }
        public NccSpot(NccSpot PlanLog, PgSpot PG)
        {
            planSpot = PlanLog.planSpot;

            BeamStartTime = PlanLog.BeamStartTime;
            BeamEndTime = PlanLog.BeamEndTime;
            BeamState = PlanLog.BeamState;
            LayerNumber = PlanLog.LayerNumber;
            LayerId = PlanLog.LayerId;
            XPosition = PlanLog.XPosition;
            YPosition = PlanLog.YPosition;
            LogTick = PlanLog.LogTick;

            ChannelCount = PG.ChannelCount;
            SumCounts = PG.SumCounts;
            TriggerStartTime = PG.TriggerStartTime;
            TriggerEndTime = PG.TriggerEndTime;
            ADC = PG.ADC;
        }
        public NccSpot(NccPlanSpot plan, NccLogSpot log)
        {
            //if (log.State == NccBeamState.Tuning)
            //{
            //    int a = 1;
            //}

            planSpot = plan;

            BeamStartTime = log.StartTime;
            BeamEndTime = log.EndTime;
            BeamState = log.State;
            LayerNumber = log.LayerNumber;
            LayerId = log.LayerID;
            XPosition = log.XPosition;
            YPosition = log.YPosition;
        }

        public NccSpot(NccLogSpot log, int layerNumber, double layerEnergy)
        {
            planSpot = new NccPlanSpot(layerNumber, layerEnergy, 0, 0, 0, 0, 0, 0);
            //planSpot = null;

            BeamStartTime = log.StartTime;
            BeamEndTime = log.EndTime;
            BeamState = log.State;
            LayerNumber = log.LayerNumber;
            LayerId = log.LayerID;
            XPosition = log.XPosition;
            YPosition = log.YPosition;
        }

        //public NccSpot(PgSpot PG, NccSpot PlanLog)
        //{
        //    BeamStartTime = PlanLog.BeamStartTime;
        //    BeamEndTime = PlanLog.BeamEndTime;
        //    BeamState = PlanLog.BeamState;
        //    LayerNumber = PlanLog.LayerNumber;
        //}

        public enum NccBeamState
        {
            Normal, Tuning, Resume, Unknown
        }
        public void ChangeNccBeamState(NccBeamState state)
        {
            BeamState = state;
        }
    }

    public class NccLayer : Layer
    {
        //private static double ToleranceDistBetweenPlanAndLogSpot = 3; // GUI
        private static double ToleranceDistBetweenPlanAndLogSpot = 5; // MATLAB
        //private bool isGetRefTime = false;
        //private long refTimeFirstTuning = 0;




        public NccLayer(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y, NccPlan plan)
        {
            logSpots = new List<NccLogSpot>();
            if (LoadLogFile_JJR(recordFileDir, SpecifFileDir, coeff_x, coeff_y))
            {
                IsLayerValid = false;
            };
            (List<NccPlanSpot> planSpots, double planLayerEnergy) = plan.GetPlanSpotsByLayerNumber(LayerNumber);
            spots = new List<NccSpot>();

            foreach (NccLogSpot logSpot in logSpots)
            {
                bool isMatched = false;

                foreach (NccPlanSpot planSpot in planSpots)
                {
                    double distance = Math.Sqrt(Math.Pow(logSpot.XPosition - planSpot.Xposition, 2) + Math.Pow(logSpot.YPosition - planSpot.Yposition, 2));
                    if (distance <= ToleranceDistBetweenPlanAndLogSpot)
                    {
                        spots.Add(new NccSpot(planSpot, logSpot));
                        isMatched = true;
                        break;
                    }
                }

                if (isMatched == false)
                {
                    spots.Add(new NccSpot(logSpot, LayerNumber, planLayerEnergy));
                }
            }
            LayerEnergy = planLayerEnergy;

            IsLayerValid = true;
        }
        #region Properties
        private List<NccSpot> spots;
        public List<NccSpot> Spots
        {
            get { return spots; }
        }

        private List<NccLogSpot> logSpots;
        public List<NccLogSpot> LogSpots
        {
            get { return logSpots; }
        }

        public int BeamStateNumber { get; private set; } // not appropriate when part exist
        // (Example) Normal beam: Layer 0, divided into part(1 ~ 3), tunned twice, pause exist
        //   Log file Name (descending time): 
        //      0000_part_01_tuning_01.xdr  -> LayerNumber: 0 / partNumber: 1 / NccBeamState: Tuning / BeamStateNumber: 1
        //      0000_part_01_tuning_02.xdr  -> LayerNumber: 0 / partNumber: 1 / NccBeamState: Tuning / BeamStateNumber: 2
        //      0000_part_01.xdr            -> LayerNumber: 0 / partNumber: 1 / NccBeamState: Normal / BeamStateNumber: 1
        //      0000_part_01_resume_01.xdr  -> LayerNumber: 0 / partNumber: 1 / NccBeamState: Resume / BeamStateNumber: 1 ***
        //      0000_part_01_resume_02.xdr  -> LayerNumber: 0 / partNumber: 1 / NccBeamState: Resume / BeamStateNumber: 2
        //      0000_part_02.xdr            -> LayerNumber: 0 / partNumber: 2 / NccBeamState: Normal / BeamStateNumber: 2
        //      0000_part_03.xdr            -> LayerNumber: 0 / partNumber: 3 / NccBeamState: Normal / BeamStateNumber: 3
        //      0000_part_03_resume_01.xdr  -> LayerNumber: 0 / partNumber: 3 / NccBeamState: Resume / BeamStateNumber: 1 ***
        public NccSpot.NccBeamState NccBeamState { get; private set; }
        public bool IsLayerValid { get; private set; }
        public int PartNumber { get; private set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LogDir"></param>
        /// <returns>isValid, layerNumber, partNumber, beamStateNumber, layerId, beamState</returns>
        public static (bool, int, int, int, string, NccSpot.NccBeamState) GetInfoFromLogFileName(string LogDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(LogDir);

            if (Path.GetExtension(LogDir) != ".xdr")
            {
                return (false, 0, 0, 0, "", NccSpot.NccBeamState.Unknown);
            }
            // Return
            int layerNumber = Convert.ToInt32(fileName.Split('_')[4]);
            int partNumber = 1;
            string layerId;
            int beamStateNumber = 1;
            NccSpot.NccBeamState nccBeamState = NccSpot.NccBeamState.Unknown;

            if (fileName.Contains("_part_"))
            {
                // 0: 20220601(date) && 1: 182105(time) && 2: 461(usec).map && 3: record && 4: LayerNumber(xxxx) && 5: "part" && 6: PartNumber(xx) && 7: "tuning" or "resume" && 8: TuningNumber or ResumeNumber

                partNumber = Convert.ToInt32(fileName.Split('_')[6]);

                if (fileName.Contains("tuning"))
                {
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString() + "_tuning_" + beamStateNumber.ToString();
                    nccBeamState = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString() + "_tuning_" + beamStateNumber.ToString();
                    nccBeamState = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString();
                    nccBeamState = NccSpot.NccBeamState.Normal;
                }

            }
            else
            {
                if (fileName.Contains("tuning"))
                {
                    layerId = layerNumber.ToString() + "_tuning_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    nccBeamState = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    layerId = layerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    nccBeamState = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = Convert.ToString(Convert.ToInt32(fileName.Split('_')[4]));
                    nccBeamState = NccSpot.NccBeamState.Normal;
                }
            }

            return (true, layerNumber, partNumber, beamStateNumber, layerId, nccBeamState);
        }
        #region private functions
        private bool LoadLogFile(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y)
        {

            bool isValidId = false;
            int layerNumber;
            int partNumber;
            int beamStateNumber;
            string layerId;
            NccSpot.NccBeamState state;

            (isValidId, layerNumber, partNumber, beamStateNumber, layerId, state) = GetInfoFromLogFileName(recordFileDir);

            if (!isValidId)
            {
                return false;
            }

            LayerNumber = layerNumber;
            PartNumber = partNumber;
            BeamStateNumber = beamStateNumber;
            LayerId = layerId;
            NccBeamState = state;

            Stream xdrConverter_speicf = File.Open(SpecifFileDir, FileMode.Open);
            var data_speicf = new XdrConverter_Specific(xdrConverter_speicf);

            Stream xdrConverter_record = File.Open(recordFileDir, FileMode.Open);
            var data_record = new XdrConverter_Record(xdrConverter_record);

            #region Add logSpots
            if (data_record.ErrorCheck == false)
            {
                List<float> xPositions = new List<float>();
                List<float> yPositions = new List<float>();

                List<Int64> epochTime = new List<Int64>();

                bool spotContinue = false;

                foreach (var elementData in data_record.elementData)
                {
                    if (spotContinue && (elementData.axisDataxPosition == -10000 && elementData.axisDatayPosition == -10000))
                    {
                        double tempxPosition;
                        double tempyPosition;
                        int templayerNumber;
                        DateTime tempstartEpochTime;
                        DateTime tempendEpochTime;
                        float[] exceptPosition = { -10000 };
                        xPositions = xPositions.Except(exceptPosition).ToList();
                        yPositions = yPositions.Except(exceptPosition).ToList();

                        if (xPositions.Count() == 0)
                        {
                            tempxPosition = -10000;
                        }
                        else
                        {
                            tempxPosition = xPositions.Average();
                        }
                        if (yPositions.Count() == 0)
                        {
                            tempyPosition = -10000;
                        }
                        else
                        {
                            tempyPosition = yPositions.Average();
                        }
                        tempstartEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.First())).UtcDateTime;
                        tempendEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.Last())).UtcDateTime;

                        templayerNumber = layerNumber;

                        xPositions.Clear();
                        yPositions.Clear();
                        epochTime.Clear();

                        //if (isGetRefTime == false && layerNumber == 0 && state == NccSpot.NccBeamState.Tuning)
                        //{
                        //    refTimeFirstTuning = tempstartEpochTime.Ticks;
                        //    isGetRefTime = true;
                        //}

                        //int logTick = Convert.ToInt32((tempstartEpochTime.Ticks - refTimeFirstTuning) / 10);
                        logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * coeff_y), ((tempxPosition - data_speicf.icxOffset) * coeff_x), state, tempstartEpochTime, tempendEpochTime));
                        spotContinue = false;
                    }

                    if (!spotContinue && (elementData.axisDataxPosition != -10000 || elementData.axisDatayPosition != -10000))
                    {
                        spotContinue = true;
                    }

                    if (spotContinue)
                    {
                        xPositions.Add(elementData.axisDataxPosition);
                        yPositions.Add(elementData.axisDatayPosition);
                        epochTime.Add(XdrConverter_Record.XdrRead.ToLong(elementData.epochTimeData, elementData.nrOfMicrosecsData));
                    }
                }

                if (epochTime.Count != 0)
                {
                    double tempxPosition;
                    double tempyPosition;
                    int templayerNumber;
                    DateTime tempstartEpochTime;
                    DateTime tempendEpochTime;
                    float[] exceptPosition = { -10000 };
                    xPositions = xPositions.Except(exceptPosition).ToList();
                    yPositions = yPositions.Except(exceptPosition).ToList();

                    if (xPositions.Count() == 0)
                    {
                        tempxPosition = -10000;
                    }
                    else
                    {
                        tempxPosition = xPositions.Average();
                    }
                    if (yPositions.Count() == 0)
                    {
                        tempyPosition = -10000;
                    }
                    else
                    {
                        tempyPosition = yPositions.Average();
                    }
                    tempstartEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.First())).UtcDateTime;
                    tempendEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.Last())).UtcDateTime;

                    templayerNumber = layerNumber;

                    xPositions.Clear();
                    yPositions.Clear();
                    epochTime.Clear();

                    //if (isGetRefTime == false && layerNumber == 0 && state == NccSpot.NccBeamState.Tuning)
                    //{
                    //    refTimeFirstTuning = tempstartEpochTime.Ticks;
                    //    isGetRefTime = true;
                    //}

                    //int logTick = Convert.ToInt32((tempstartEpochTime.Ticks - refTimeFirstTuning) / 10);
                    logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * coeff_y), ((tempxPosition - data_speicf.icxOffset) * coeff_x), state, tempstartEpochTime, tempendEpochTime));

                    spotContinue = false;
                }
            }
            #endregion           
            return true;
        }


        private bool LoadLogFile_JJR(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y)
        {

            bool isValidId = false;
            int layerNumber;
            int partNumber;
            int beamStateNumber;
            string layerId;
            NccSpot.NccBeamState state;

            (isValidId, layerNumber, partNumber, beamStateNumber, layerId, state) = GetInfoFromLogFileName(recordFileDir);

            if (!isValidId)
            {
                return false;
            }

            LayerNumber = layerNumber;
            PartNumber = partNumber;
            BeamStateNumber = beamStateNumber;
            LayerId = layerId;
            NccBeamState = state;

            Stream xdrConverter_speicf = File.Open(SpecifFileDir, FileMode.Open);
            var data_speicf = new XdrConverter_Specific(xdrConverter_speicf);

            Stream xdrConverter_record = File.Open(recordFileDir, FileMode.Open);
            var data_record = new XdrConverter_Record(xdrConverter_record);

            #region Add logSpots
            if (data_record.ErrorCheck == false)
            {
                List<float> xPositions = new List<float>();
                List<float> yPositions = new List<float>();

                List<Int64> epochTime = new List<Int64>();

                bool isBeamOn = false;

                foreach (var elementData in data_record.elementData)
                {
                    if (elementData.axisDataxPosition != -10000 | elementData.axisDataxPosition != -10000)
                    {
                        if (elementData.axisDataxPosition != -10000)
                        {
                            xPositions.Add(elementData.axisDataxPosition);
                        }
                        if (elementData.axisDatayPosition != -10000)
                        {
                            yPositions.Add(elementData.axisDatayPosition);
                        }
                        epochTime.Add(XdrConverter_Record.XdrRead.ToLong(elementData.epochTimeData, elementData.nrOfMicrosecsData));
                        isBeamOn = true;
                    }

                    if (isBeamOn == true && elementData.axisDataxPosition == -10000 && elementData.axisDataxPosition == -10000)
                    {
                        double tempXpos;
                        double tempYpos;
                        int tempLayerNumber;
                        DateTime tempStartEpochTime;
                        DateTime tempEndEpochTime;

                        // 1. X check
                        if (xPositions.Count() == 0)
                        {
                            Debug.Assert(true, $"No X position in the {recordFileDir}");
                            tempXpos = -10000;
                        }
                        else
                        {
                            tempXpos = xPositions.Average();
                        }

                        // 2. Y check
                        if (yPositions.Count() == 0)
                        {
                            Debug.Assert(true, $"No Y position in the {recordFileDir}");
                            tempYpos = -10000;
                        }
                        else
                        {
                            tempYpos = yPositions.Average();
                        }

                        // 3. Add log spot information
                        tempStartEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.First())).UtcDateTime;
                        tempEndEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.Last())).UtcDateTime;

                        tempLayerNumber = layerNumber;

                        xPositions.Clear();
                        yPositions.Clear();
                        epochTime.Clear();

                        logSpots.Add(new NccLogSpot(tempLayerNumber, layerId, ((tempYpos - data_speicf.icyOffset) * coeff_y), ((tempXpos - data_speicf.icxOffset) * coeff_x), state, tempStartEpochTime, tempEndEpochTime));
                        isBeamOn = false;
                    }

                    //if (epochTime.Count != 0)
                    //{
                    //    double tempxPosition;
                    //    double tempyPosition;
                    //    int templayerNumber;
                    //    DateTime tempstartEpochTime;
                    //    DateTime tempendEpochTime;
                    //    float[] exceptPosition = { -10000 };
                    //    xPositions = xPositions.Except(exceptPosition).ToList();
                    //    yPositions = yPositions.Except(exceptPosition).ToList();

                    //    if (xPositions.Count() == 0)
                    //    {
                    //        tempxPosition = -10000;
                    //    }
                    //    else
                    //    {
                    //        tempxPosition = xPositions.Average();
                    //    }
                    //    if (yPositions.Count() == 0)
                    //    {
                    //        tempyPosition = -10000;
                    //    }
                    //    else
                    //    {
                    //        tempyPosition = yPositions.Average();
                    //    }
                    //    tempstartEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.First())).UtcDateTime;
                    //    tempendEpochTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(epochTime.Last())).UtcDateTime;

                    //    templayerNumber = layerNumber;

                    //    xPositions.Clear();
                    //    yPositions.Clear();
                    //    epochTime.Clear();

                    //    //if (isGetRefTime == false && layerNumber == 0 && state == NccSpot.NccBeamState.Tuning)
                    //    //{
                    //    //    refTimeFirstTuning = tempstartEpochTime.Ticks;
                    //    //    isGetRefTime = true;
                    //    //}

                    //    //int logTick = Convert.ToInt32((tempstartEpochTime.Ticks - refTimeFirstTuning) / 10);
                    //    logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * coeff_y), ((tempxPosition - data_speicf.icxOffset) * coeff_x), state, tempstartEpochTime, tempendEpochTime));

                    //    spotContinue = false;
                    //}
                }
            }
            #endregion           
            return true;
        }




        #endregion
    }

    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession : Session
    {
        public NccSession()
        {

        }

        private static string debugDirectory = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\06. sameResultTest\Debug\";

        private List<GapPeakAndRange> gapPeakAndRange = new List<GapPeakAndRange>();

        private List<NccLayer> layers = new List<NccLayer>();
        public List<NccLayer> Layers
        {
            get
            {
                return layers;
            }
        }

        private List<NccSpot> nCCSpots = new List<NccSpot>();
        public List<NccSpot> NCCSpots
        {
            get
            {
                return nCCSpots;
            }
        }

        private List<SpotMap> spotMap = new List<SpotMap>();
        public List<SpotMap> SpotMap
        {
            get
            {
                return spotMap;
            }
        }

        private List<BeamRangeMap> beamRangeMap = new List<BeamRangeMap>();
        public List<BeamRangeMap> BeamRangeMap
        {
            get
            {
                return beamRangeMap;
            }
        }

        private NccPlan plan = new NccPlan("", "", true);


        private NccMultislitPg? pgData = null;
        public NccMultislitPg? MultislitPgData
        {
            get
            {
                return pgData;
            }
        }


        private DateTime firstLayerFirstSpotLogTime;

        public bool IsPlanLoad { get; private set; }
        public bool IsPGLoad { get; private set; }
        public bool IsConfigLogFileLoad { get; private set; }

        public bool IsGetReferenceTime { get; private set; }


        private NccLogParameter logParameter = new NccLogParameter();
        /// <summary>
        /// Load plan file
        /// if plan is loaded return true
        /// And Make layers with plan file
        /// </summary>
        /// <param name="planFileDir"></param>
        /// <returns></returns>
        public bool LoadPlanFile(string plan3DFileDir, string plan2DFileDir, bool flagFlatten, bool flagDebug)
        {
            if (plan3DFileDir.EndsWith("pld") || plan3DFileDir.EndsWith("txt") || plan2DFileDir.EndsWith("txt") || plan2DFileDir.EndsWith("txt"))
            {
                if (plan2DFileDir.EndsWith("pld") || plan2DFileDir.EndsWith("txt"))
                {
                    try
                    {
                        plan = new NccPlan(plan3DFileDir, plan2DFileDir, flagFlatten);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return false;
                    }

                    IsPlanLoad = true;

                    if (flagDebug)
                    {
                        string DebugFileName = @"1_Debug_Plan_flatten.csv";
                        string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);

                        using (StreamWriter file = new StreamWriter(DebugPath_excel))
                        {
                            List<NccPlanSpot> debug_Plan = plan.GetPlanSpots();
                            for (int i = 0; i < debug_Plan.Count; i++)
                            {
                                file.WriteLine($"{debug_Plan[i].LayerNumber}, {debug_Plan[i].LayerEnergy}, {debug_Plan[i].Xposition}, {debug_Plan[i].Yposition}, {debug_Plan[i].Zposition}, {debug_Plan[i].MonitoringUnit}");
                            }
                        }
                    }

                    return true;
                }
            }
            else if (plan3DFileDir == "")
            {
                IsPlanLoad = false;
            }
            else
            {
                IsPlanLoad = false;
            }

            return false;
        }
        public bool LoadConfigLogFile(string configFileDir)
        {
            // Log Config File Example
            // # SAD - M-id 21900
            // GTR3-PBS;SAD;DOUBLE;2;A;;;SAD parameter (X, Y);1915.8, 2300.2
            // GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X, Y);1148.16, 1202.49
            try
            {
                if (configFileDir.Contains(".idt_config.csv"))
                {
                    double sad_x, sad_y;
                    double distICtoIso_x, distICtoIso_y;

                    StreamReader sr = new StreamReader(configFileDir);
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (line.Contains("# SAD - M-id 21900"))
                            {
                                line = sr.ReadLine(); // "GTR3-PBS;SAD;DOUBLE;2;A;;;SAD parameter (X,Y);1915.8,2300.2"
                                if (!string.IsNullOrEmpty(line))
                                {
                                    string compareString = "GTR3-PBS;SAD;DOUBLE;2;A;;;SAD parameter (X,Y);";
                                    if (line.Contains(compareString))
                                    {
                                        int length = compareString.Length;
                                        string splitStr = line.Substring(length);
                                        sad_x = Convert.ToDouble(splitStr.Split(",")[0]);
                                        sad_y = Convert.ToDouble(splitStr.Split(",")[1]);

                                        line = sr.ReadLine();   // "GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X,Y);1148.16,1202.49"
                                        if (!string.IsNullOrEmpty(line))
                                        {
                                            compareString = "GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X,Y);";
                                            if (line.Contains(compareString))
                                            {
                                                length = compareString.Length;
                                                splitStr = line.Substring(length);
                                                distICtoIso_x = Convert.ToDouble(splitStr.Split(",")[0]);
                                                distICtoIso_y = Convert.ToDouble(splitStr.Split(",")[1]);

                                                logParameter.coeff_x = sad_x / (sad_x - distICtoIso_x);
                                                logParameter.coeff_y = sad_y / (sad_y - distICtoIso_y);
                                                IsConfigLogFileLoad = true;

                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return false;
        }
        public bool LoadRecordSpecifLogFile(string recordFileDir, string specifFileDir)
        {
            #region Check loaded log files whether valid or invalid

            if (!File.Exists(recordFileDir))
            {
                Debug.WriteLine($"Log(record) file doesn't exist");
                return false;
            }

            if (!File.Exists(specifFileDir))
            {
                Debug.WriteLine($"Log(specif) file doesn't exist");
                return false;
            }

            if (IsPlanLoad == false)
            {
                Debug.WriteLine($"Plan is not loaded");
                return false;
            }

            if (IsConfigLogFileLoad == false)
            {
                Debug.WriteLine($"Log(config) is not loaded");
                return false;
            }

            if (!recordFileDir.Contains("map_record_"))
            {
                Debug.WriteLine($"Log(record) is not invalid");
                return false;
            }

            if (!specifFileDir.Contains("map_specif_"))
            {
                Debug.WriteLine($"Log(specif) is not invalid");
                return false;
            }
            bool chkLayerFileValid;
            string recordLayerId;
            string specifLayerId;
            (chkLayerFileValid, _, _, _, recordLayerId, _) = NccLayer.GetInfoFromLogFileName(recordFileDir);
            if (!chkLayerFileValid)
            {
                Debug.Assert(!chkLayerFileValid, "record file is not valid");
                return false;
            }
            (chkLayerFileValid, _, _, _, specifLayerId, _) = NccLayer.GetInfoFromLogFileName(specifFileDir);
            if (!chkLayerFileValid)
            {
                Debug.Assert(!chkLayerFileValid, "record file is not valid");
                return false;
            }

            if (recordLayerId != specifLayerId)
            {
                Debug.WriteLine($"LayerId(record) != LayerId(specif)");
                return false;
            }

            foreach (var chklayer in layers)
            {
                (_, _, _, _, string layerID, _) = NccLayer.GetInfoFromLogFileName(recordLayerId);
                if (chklayer.LayerId == layerID)
                {
                    Debug.WriteLine("Layer file is already loaded");
                    return true;
                }
            }
            #endregion

            NccLayer loadedLayer = new NccLayer(recordFileDir, specifFileDir, logParameter.coeff_x, logParameter.coeff_y, plan);
            //NccSpot.NccBeamState state = nccLayer.GetSingleLogInfo();
            NccSpot.NccBeamState state = loadedLayer.NccBeamState;

            List<int> LayerNumbers = new List<int>(layers.Count);
            foreach (NccLayer chklayer in layers)
            {
                LayerNumbers.Add(chklayer.LayerNumber);
            }

            if (layers.Count == 0)
            {
                layers.Add(loadedLayer);
            }
            else
            {
                int insertIndex = 0;
                if (LayerNumbers.Any(x => x == loadedLayer.LayerNumber))
                {
                    insertIndex = LayerNumbers.FindIndex(x => x == loadedLayer.LayerNumber);

                    List<NccSpot> nccSpot = layers[insertIndex].Spots;
                    nccSpot.AddRange(loadedLayer.Spots);
                    nccSpot.OrderBy(x => x.BeamStartTime);
                }
                else
                {
                    insertIndex = LayerNumbers.Where(x => x < loadedLayer.LayerNumber).Count();
                    layers.Insert(insertIndex, loadedLayer);
                }
            }

            return true;
        }
        public bool LoadPGFile(string pgDir, bool isBrokenScin, bool flagDebug)
        {
            #region Check Valid
            if (!File.Exists(pgDir))
            {
                Debug.WriteLine($"PG file doesn't exist");
                return false;
            }

            if (!Equals(Path.GetExtension(pgDir), ".bin"))
            {
                Debug.WriteLine($"Invalid file extension");
                return false;
            }
            #endregion
            pgData = new NccMultislitPg(pgDir, isBrokenScin, flagDebug);

            IsPGLoad = true;

            return true;
        }
        public bool PostProcessing_NCC(bool DebugPGsplit)
        {
            // Output: (1) spot map, (2) beam range map

            // Algorithm
            // 1. continuously measured PG distribution -> PG distribution of spot
            //      input: PGraw, width, ULD, LLD   // output: PGspot
            // 2. data merge (PlanLog + PG)
            //      input: PlanLog, PGspot          // output: SpotNCC
            // 3. get spot map
            //      input: SpotNCC                  // output: SpotMap
            // 4. get beam range map
            //      input: SpotNCC                  // output: BeamRangeMap

            #region Debug (is broken scintillaotr?)
            //var pg_raw = MultislitPgData.GetPGSpots();
            //int[] pgDist = new int[144];
            //for (int ch = 0; ch < 144; ch++)
            //{
            //    int tempSum = 0;
            //    for (int index_line = 0; index_line < pg_raw.Count; index_line++)
            //    {
            //        tempSum += pg_raw[index_line].ChannelCount[ch];
            //    }
            //    pgDist[ch] = tempSum;
            //    Console.WriteLine($"{ch}, {tempSum}");
            //}
            #endregion

            //List<PgSpot> pgSpots = splitDataIntoSpot(MultislitPgData.GetPGSpots(), 5, 40, 30, "SP34"); // SP34 사용한 케이스
            List<PgSpot> pgSpots = splitDataIntoSpot(MultislitPgData.GetPGSpots(), 4, 20, 10, "PMMA", DebugPGsplit); // PMMA 사용한 케이스
           
            bool flagDebugMergedData = true;
            nCCSpots = mergeNCCSpotData(pgSpots, layers, flagDebugMergedData);

            bool flagDebug_pgSpots = true;
            if (flagDebug_pgSpots)
            {
                List<double[]> pgSpots71Debug = new List<double[]>();
                List<double[]> pgSpots72Debug = new List<double[]>();
                foreach (NccSpot spot in nCCSpots)
                {
                    (double[] pgSpots71, double[] pgSpots72) = convert71from144(spot.ChannelCount, "220312");
                    pgSpots71Debug.Add(pgSpots71);
                    pgSpots72Debug.Add(pgSpots72);
                }

                string DebugFileName1 = @"7_Debug_PGdist72.csv";
                string DebugPath_excel1 = string.Concat(debugDirectory, DebugFileName1);
                using (StreamWriter file = new StreamWriter(DebugPath_excel1))
                {
                    file.Write($"#, ");
                    for (int ch = 0; ch < 72; ch++)
                    {
                        file.Write($"{ch + 1}, ");
                    }
                    file.WriteLine("");

                    int line = 1;
                    foreach (double[] counts in pgSpots72Debug)
                    {
                        file.Write($"{line}, ");
                        for (int ch = 0; ch < 72; ch++)
                        {
                            file.Write($"{counts[ch]}, ");
                        }
                        file.WriteLine("");
                        line++;
                    }
                }

                string DebugFileName2 = @"8_Debug_PGdist71.csv";
                string DebugPath_excel2 = string.Concat(debugDirectory, DebugFileName2);
                using (StreamWriter file = new StreamWriter(DebugPath_excel2))
                {
                    file.Write($"#, ");
                    for (int ch = 0; ch < 71; ch++)
                    {
                        file.Write($"{ch + 1}, ");
                    }
                    file.WriteLine("");

                    int line = 1;
                    foreach (double[] counts in pgSpots71Debug)
                    {
                        file.Write($"{line}, ");
                        for (int ch = 0; ch < 71; ch++)
                        {
                            file.Write($"{counts[ch]}, ");
                        }
                        file.WriteLine("");
                        line++;
                    }
                }
            }

            //int selectedLayer = 0;
            //getSpotMap(nCCSpots, selectedLayer);

            int startLayer = 0;
            int endLayer = nCCSpots.Last().LayerNumber;
            double sigma = 7;
            double cutoffMU = 1;
            getBeamRangeMap(nCCSpots, sigma, startLayer, endLayer, cutoffMU);




            return true;
        }
        public bool LoadPeakAndGapRange(string fileDir)
        {
            using (FileStream fs = new FileStream(fileDir, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                {
                    string lines = null;
                    string[] tempString = null;

                    while ((lines = sr.ReadLine()) != null)
                    {
                        tempString = lines.Split("\t");

                        double energy = Convert.ToDouble(tempString[0]);
                        double gapValue = Convert.ToDouble(tempString[1]);
                        double unknownValue = Convert.ToDouble(tempString[2]);
                        gapPeakAndRange.Add(new GapPeakAndRange(energy, gapValue, unknownValue));
                    }
                }
            }

            if (gapPeakAndRange == null || gapPeakAndRange.Count == 0)
            {
                return false;
            }

            return true;
        }

        #region private functions


        private List<PgSpot> splitDataIntoSpot(List<PgSpot> PG_raw, int width, int ULD, int LLD, string phantom, bool flagDebug)
        {
            // Return data
            List<PgSpot> PG_144 = new List<PgSpot>();

            bool isSpot = false;
            int dataCount = PG_raw.Count();

            int index_SpotStart = 0;
            int index_SpotEnd = 0;

            if (phantom == "SP34") // recording interval = 200 us (2021-03-27 measured data)
            {
                for (int index = width - 1; index < dataCount; index++)
                {
                    int sum_Count = 0;
                    for (int x = 0; x < width; x++)
                    {
                        sum_Count += PG_raw[index - width + x + 1].SumCounts;
                    }

                    if (isSpot == false)
                    {
                        if (sum_Count > ULD)
                        {
                            isSpot = true;
                            index_SpotStart = index;
                            index += width;
                        }
                    }
                    else
                    {
                        if (sum_Count < LLD)
                        {
                            isSpot = false;
                            index_SpotEnd = index;
                            index += width;

                            // =============== PgSpot definition =============== //
                            // int[] ChannelCount       & int SumCounts
                            // int TriggerStartTime = 0 & int TriggerEndTime = 0
                            // double ADC = 0           & int Tick = 0

                            int[] channelCount = new int[144];
                            for (int ch = 0; ch < 144; ch++)
                            {
                                int chSum = 0;
                                for (int idx = index_SpotStart; idx < index_SpotEnd; idx++)
                                {
                                    chSum += PG_raw[idx].ChannelCount[ch];
                                }
                                channelCount[ch] = chSum;
                            }

                            int sumCounts = channelCount.Sum();

                            int triggerStartTime = PG_raw[index_SpotStart].TriggerStartTime;
                            int triggerEndTime = PG_raw[index_SpotEnd].TriggerStartTime;

                            PG_144.Add(new PgSpot(channelCount, sumCounts, triggerStartTime, triggerEndTime, 0, 0));
                        }
                    }
                }
            }
            else // phantom == "PMMA", recording interval = 100 us
            {
                // New version (after 2022-08-15 21:08)
                for (int index = width - 1; index < dataCount; index++)
                {
                    int sum_Count = PG_raw[index].SumCounts + PG_raw[index - 1].SumCounts + PG_raw[index - 2].SumCounts + PG_raw[index - 3].SumCounts;
                    //for (int i = 0; i < width; i++)
                    //{
                    //    sum_Count += PG_raw[index - width + i + 1].SumCounts;
                    //}

                    if (isSpot == false)
                    {
                        if (sum_Count > ULD)
                        {
                            isSpot = true;
                            index_SpotStart = index - width;
                            index += width;
                        }
                    }
                    else
                    {
                        if (sum_Count < LLD)
                        {
                            isSpot = false;
                            index_SpotEnd = index;
                            index += width;

                            // =============== PgSpot definition =============== //
                            // int[] ChannelCount       & int SumCounts
                            // int TriggerStartTime = 0 & int TriggerEndTime = 0
                            // double ADC = 0           & int Tick = 0

                            int[] channelCount = new int[144];
                            for (int ch = 0; ch < 144; ch++)
                            {
                                int chSum = 0;
                                for (int idx = index_SpotStart; idx < index_SpotEnd; idx++)
                                {
                                    chSum += PG_raw[idx].ChannelCount[ch];
                                }
                                channelCount[ch] = chSum;
                            }

                            int sumCounts = channelCount.Sum();

                            int triggerStartTime = PG_raw[index_SpotStart].TriggerStartTime;
                            int triggerEndTime = PG_raw[index_SpotEnd].TriggerStartTime;

                            PG_144.Add(new PgSpot(channelCount, sumCounts, triggerStartTime, triggerEndTime, 0, 0));
                        }
                    }
                }

                #region before 2022-08-15 21:07
                //for (int index = width - 1; index < dataCount; index++)
                //{
                //    int sum_Count = 0;
                //    for (int x = 0; x < width; x++)
                //    {
                //        sum_Count += PG_raw[index - width + x + 1].SumCounts;
                //    }

                //    if (isSpot == false)
                //    {
                //        if (sum_Count > ULD)
                //        {
                //            isSpot = true;
                //            index_SpotStart = index;
                //            index += 15;
                //        }
                //    }
                //    else
                //    {
                //        //if (sum_Count < LLD)
                //        if (PG_raw[index].SumCounts < LLD)
                //        {
                //            isSpot = false;
                //            index_SpotEnd = index;
                //            index += 15;

                //            // =============== PgSpot definition =============== //
                //            // int[] ChannelCount       & int SumCounts
                //            // int TriggerStartTime = 0 & int TriggerEndTime = 0
                //            // double ADC = 0           & int Tick = 0

                //            int[] channelCount = new int[144];
                //            for (int ch = 0; ch < 144; ch++)
                //            {
                //                int chSum = 0;
                //                for (int idx = index_SpotStart; idx < index_SpotEnd; idx++)
                //                {
                //                    chSum += PG_raw[idx].ChannelCount[ch];
                //                }
                //                channelCount[ch] = chSum;
                //            }

                //            int sumCounts = channelCount.Sum();

                //            int triggerStartTime = PG_raw[index_SpotStart].TriggerStartTime;
                //            int triggerEndTime = PG_raw[index_SpotEnd].TriggerStartTime;

                //            PG_144.Add(new PgSpot(channelCount, sumCounts, triggerStartTime, triggerEndTime, 0, 0));
                //        }
                //    }
                //}
                #endregion
            }

            #region Debug (.csv)   

            if (flagDebug == true)
            {
                string DebugFileName = @"3_Debug_SplitIntoSpots.csv";
                string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                using (StreamWriter file = new StreamWriter(DebugPath_excel))
                {
                    file.Write($"SpotNum, StartTime, EndTime, Duration, TimeGap, SumCnt");
                    for (int i = 0; i < 144; i++)
                    {
                        file.Write($", Ch {i + 1}");
                    }
                    file.WriteLine("");

                    for (int i = 0; i < PG_144.Count(); i++)
                    {
                        file.Write($"{i + 1}, ");
                        file.Write($"{PG_144[i].TriggerStartTime}, ");
                        file.Write($"{PG_144[i].TriggerEndTime}, ");
                        file.Write($"{(PG_144[i].TriggerEndTime - PG_144[i].TriggerStartTime)}, ");

                        if (i == 0)
                        {
                            file.Write($"0, ");
                        }
                        else
                        {
                            file.Write($"{(PG_144[i].TriggerStartTime - PG_144[i - 1].TriggerStartTime)}, ");
                        }

                        file.Write($"{PG_144[i].SumCounts}, ");
                        for (int ch = 0; ch < 143; ch++)
                        {
                            file.Write($"{PG_144[i].ChannelCount[ch]}, ");
                        }
                        file.WriteLine($"{PG_144[i].ChannelCount[143]}");
                    }
                }
            }

            #endregion

            return PG_144;
        }
        private List<NccSpot> mergeNCCSpotData(List<PgSpot> pgSpots, List<NccLayer> layers, bool flagDebug)
        {
            // Return data
            List<NccSpot> nccSpots = new List<NccSpot>();

            // 1. Set parameter
            // 2. Get reference time (PlanLog)
            // 3. Get reference time (PG)
            // 4. Merge data

            #region 1. Set parameter
            int refTimePG = 0;
            long refTimePlanLog = 0;
            int numOfCompareSpot = 10;
            #endregion

            #region 2. Get reference time (PlanLog)
            List<NccSpot> PlanLog = new List<NccSpot>();
            List<NccSpot> PlanLogTemp = new List<NccSpot>();

            foreach (NccLayer layer in layers)
            {
                foreach (NccSpot spot in layer.Spots)
                {
                    PlanLogTemp.Add(spot);
                }
            }

            PlanLog = PlanLogTemp.OrderBy(x => x.BeamStartTime).ToList();

            bool getRefTimePlanLog = false;
            //long refTimePlanLogTemp = PlanLog[0].BeamStartTime.Ticks;

            for (int i = 0; i < PlanLog.Count - numOfCompareSpot; i++)
            {
                if ((PlanLog[i + numOfCompareSpot].BeamStartTime.Ticks - PlanLog[i].BeamStartTime.Ticks) / 10 < 0.5E6)
                {
                    getRefTimePlanLog = true;
                    refTimePlanLog = PlanLog[i].BeamStartTime.Ticks;
                    break;
                }
            }

            for (int i = 0; i < PlanLog.Count; i++)
            {
                int GapBetweenFirstSpot = Convert.ToInt32((PlanLog[i].BeamStartTime.Ticks - refTimePlanLog) / 10);
                PlanLog[i].setLogTick(GapBetweenFirstSpot);
            }

            if (getRefTimePlanLog == false)
            {
                refTimePlanLog = 0;
                Debug.Assert(true, $"Can't get reference time of PlanLog");
                return nccSpots;
            }

            #endregion

            #region 3. Get reference time (PG)
            bool getRefTimePG = false;
            for (int i = 0; i < pgSpots.Count - numOfCompareSpot; i++)
            {
                if (pgSpots[i + numOfCompareSpot].TriggerStartTime - pgSpots[i].TriggerStartTime < 0.5E6)
                {
                    getRefTimePG = true;
                    refTimePG = pgSpots[i].TriggerStartTime;
                    break;
                }
            }

            if (getRefTimePG == false)
            {
                refTimePG = 0;
                Debug.Assert(true, $"Can't get reference time of PG");
                return nccSpots;
            }

            List<PgSpot> pgSpots_withTick = new List<PgSpot>();
            foreach (PgSpot spot in pgSpots)
            {
                int pgTick = spot.TriggerStartTime - refTimePG;
                pgSpots_withTick.Add(new PgSpot(spot.ChannelCount, spot.SumCounts, spot.TriggerStartTime, spot.TriggerEndTime, spot.ADC, pgTick));
            }
            #endregion

            #region 4. Merge data (pgSpots_withTick, PlanLog)
            int numOfCompareSpots = 10;
            int timeMargin = 100000;    // 100 ms

            int numOfPGSpots = pgSpots_withTick.Count;

            int index_PG = 0;
            int index_PlanLog = 0;

            while (index_PG < pgSpots_withTick.Count && index_PlanLog < PlanLog.Count)
            {
                if (Math.Abs(pgSpots_withTick[index_PG].Tick - PlanLog[index_PlanLog].LogTick) < timeMargin)
                {
                    nccSpots.Add(new NccSpot(PlanLog[index_PlanLog], pgSpots_withTick[index_PG]));

                    index_PG++;
                    index_PlanLog++;
                }
                else
                {
                    bool isSpotMatched = false;
                    for (int index_PG_temp = index_PG; index_PG_temp < Math.Min(index_PG + numOfCompareSpots, numOfPGSpots); index_PG_temp++)
                    {
                        if (Math.Abs(pgSpots_withTick[index_PG_temp].Tick - PlanLog[index_PlanLog].LogTick) < timeMargin)
                        {
                            nccSpots.Add(new NccSpot(PlanLog[index_PlanLog], pgSpots_withTick[index_PG]));

                            index_PG = index_PG_temp + 1;
                            index_PlanLog++;

                            isSpotMatched = true;
                            break;
                        }
                    }

                    if (isSpotMatched == false)
                    {
                        Debug.Assert(true, $"log spot number {index_PlanLog} was not matched with PG trigger");

                        nccSpots.Add(new NccSpot(PlanLog[index_PlanLog]));
                        index_PlanLog++;

                        if (index_PG + numOfCompareSpots > numOfPGSpots)
                        {
                            break;
                        }
                    }
                }
            }
            #endregion

            #region 5. Debug (using flag)

            // data variable name: nccSpots

            if (flagDebug) // Excel debugging
            {
                string DebugFileName = @"4_Debug_MergedData.csv";
                string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                using (StreamWriter file = new StreamWriter(DebugPath_excel))
                {
                    file.Write($"Spot #, StartTime, EndTime, Layer #, Tuning #, Resume #, Part #, X, Y, Layer #, Layer E, X, Y, Z, MU");
                    for (int i = 0; i < 144; i++)
                    {
                        file.Write($", ch {i + 1}");
                    }
                    file.WriteLine();

                    var refTime = nccSpots[0].BeamStartTime;
                    for (int i = 0; i < nccSpots.Count; i++)
                    {
                        var data = nccSpots[i];

                        double L_startTime = (data.BeamStartTime - refTime).Ticks;
                        double L_endTime = (data.BeamEndTime - refTime).Ticks;
                        //double L_startTime = (planLog[i].BeamStartTime - refTime).Ticks / 10000000;
                        //double L_endTime = (planLog[i].BeamEndTime - refTime).Ticks / 10000000;
                        var L_layerNumber = data.LayerNumber + 1;

                        int L_tuningNum = 0;
                        if (data.BeamState == NccSpot.NccBeamState.Tuning)
                        {
                            L_tuningNum = Convert.ToInt32(data.LayerId.Split("_").Last());
                        }

                        int L_resumeNum = 0;
                        if (data.BeamState == NccSpot.NccBeamState.Resume)
                        {
                            L_tuningNum = Convert.ToInt32(data.LayerId.Split("_").Last());
                        }

                        int L_partNum = 0;
                        var L_Xpos = data.XPosition;
                        var L_Ypos = data.YPosition;

                        var P_LayerNumber = data.PlanSpot.LayerNumber + 1;
                        var P_LayerEnergy = data.PlanSpot.LayerEnergy;
                        var P_Xpos = data.PlanSpot.Xposition;
                        var P_Ypos = data.PlanSpot.Yposition;
                        var P_Zpos = data.PlanSpot.Zposition;
                        var P_MU = data.PlanSpot.MonitoringUnit;

                        file.Write($"{i + 1}, {L_startTime}, {L_endTime}, {L_layerNumber}, {L_tuningNum}, {L_resumeNum}, {L_partNum}, {L_Xpos}, {L_Ypos}, {P_LayerNumber}, {P_LayerEnergy}, {P_Xpos}, {P_Ypos}, {P_Zpos}, {P_MU}");
                        for (int ch = 0; ch < 144; ch++)
                        {
                            file.Write($", {data.ChannelCount[ch]}");

                        }
                        file.WriteLine($", {data.TriggerStartTime}, {data.TriggerEndTime}");
                    }
                }
            }

            #endregion

            return nccSpots;
        }

        private void getSpotMap(List<NccSpot> nCCSpots, int selectedLayer)
        {
            // Return
            List<SpotMap> spotMap = new List<SpotMap>();

            List<NccSpot> spots = (from spot in nCCSpots
                                   where spot.LayerNumber == selectedLayer
                                   where spot.BeamState != NccSpot.NccBeamState.Tuning
                                   select spot).ToList();
            int spotCounts = spots.Count;

            #region get gaussian weight map (confirmed) - output: [gaussianWeigtMap]

            List<double[]> gaussianWeightMap = new List<double[]>();
            for (int spotIndex = 0; spotIndex < spotCounts; spotIndex++)
            {
                double[] distance = new double[spotCounts];

                for (int compareSpotindex = 0; compareSpotindex < spotCounts; compareSpotindex++)
                {
                    double Xdifference = spots[spotIndex].XPosition - spots[compareSpotindex].XPosition;
                    double Ydifference = spots[spotIndex].YPosition - spots[compareSpotindex].YPosition;

                    distance[compareSpotindex] = Math.Sqrt(Math.Pow(Xdifference, 2) + Math.Pow(Ydifference, 2));
                }

                double aggregateSigma = 7.8;

                double[] gaussianWeightMapTemp = new double[spotCounts];
                for (int compareSpotindex = 0; compareSpotindex < spotCounts; compareSpotindex++)
                {
                    gaussianWeightMapTemp[compareSpotindex] = Math.Exp(-0.5 * Math.Pow(distance[compareSpotindex] / aggregateSigma, 2));
                }

                gaussianWeightMap.Add(gaussianWeightMapTemp);
            }

            #endregion

            #region get gap between peak and range (confirmed) - output: [gap]
            double layerEnergy = spots.Last().PlanSpot.LayerEnergy;

            int index = gapPeakAndRange.FindIndex(gapList => gapList.energy >= layerEnergy);

            double gapInterpolation = (gapPeakAndRange[index].GapValue - gapPeakAndRange[index - 1].GapValue) / (gapPeakAndRange[index].energy - gapPeakAndRange[index - 1].energy) * (layerEnergy - gapPeakAndRange[index - 1].energy);
            double gap = gapPeakAndRange[index - 1].GapValue + gapInterpolation;
            #endregion

            #region get range, range difference () - output: [spotRange], [spotRangeDifference]

            double[] spotRange = new double[spotCounts];
            double[] spotRangeDifference = new double[spotCounts];

            for (int i = 0; i < spotCounts; i++)
            {
                //i = 9;

                double[] aggregatedPGdistribution = new double[144];

                for (int j = 0; j < 144; j++)
                {
                    double chCounts = 0;

                    for (int k = 0; k < spotCounts; k++)
                    {
                        if (gaussianWeightMap[i][k] > 0.001)
                        {
                            chCounts += gaussianWeightMap[i][k] * spots[k].ChannelCount[j];
                        }
                    }

                    aggregatedPGdistribution[j] = chCounts;
                }

                double isoDepth = 110; // mm unit

                bool is144ChCount = false;
                spotRange[i] = getRange_ver4p0(aggregatedPGdistribution, spots[i].PlanSpot.Zposition, is144ChCount);
                spotRangeDifference[i] = spotRange[i] - (spots[i].PlanSpot.Zposition + gap);

                //Console.WriteLine($"{spots[i].XPosition}, {spots[i].YPosition}, {spots[i].PlanSpot.Zposition}, {spotRange[i]}, {spotRangeDifference[i]}");
                Console.WriteLine($"{i}, {-spots[i].YPosition}, {spots[i].XPosition}, {spots[i].PlanSpot.Zposition}, {spotRangeDifference[i]}");
            }

            #endregion

            #region get spot map () - output: [spotMap]

            spotMap = new List<SpotMap>();
            for (int spotIndex = 0; spotIndex < spotCounts; spotIndex++)
            {
                spotMap.Add(new SpotMap(-spots[spotIndex].YPosition, spots[spotIndex].XPosition, spots[spotIndex].PlanSpot.MonitoringUnit, spotRangeDifference[spotIndex]));
            }

            #endregion

        }

        private void getBeamRangeMap(List<NccSpot> nCCSpots, double sigma, int startLayer, int endLayer, double cutoffMU)
        {
            #region 1. Grid setting - output: [xPos], [yPos], [gridPitch]

            // should be modified to be changeable at the View
            double gridXMin = -60;
            double gridXMax = 60;
            double gridYMin = -60;
            double gridYMax = 60;
            double gridPitch = 5;

            int numOfXGrid = (int)Math.Ceiling(((gridXMax - gridXMin) / gridPitch) + 1);
            int numOfYGrid = (int)Math.Ceiling(((gridYMax - gridYMin) / gridPitch) + 1);

            double[] xPos = new double[numOfXGrid];
            double[] yPos = new double[numOfYGrid];

            for (int i = 0; i < numOfXGrid; i++)
            {
                xPos[i] = gridXMin + gridPitch * i;
            }
            for (int i = 0; i < numOfYGrid; i++)
            {
                yPos[i] = gridYMin + gridPitch * i;
            }


            double[] xgrid_mm = new double[71];
            for (int i = 0; i < 71; i++)
            {
                xgrid_mm[i] = -105.3220 + 3.0092 * i; // -> static
            }

            double cameraSetupPosition = 12; // depends on camera setup position -> should be modified to be changeable at the View
            double[] xgrid_iso_mm = new double[71];
            for (int i = 0; i < 71; i++)
            {
                xgrid_iso_mm[i] = xgrid_mm[i] + cameraSetupPosition;
            }

            int direction = 0;
            double rangeRatio_mat2water = 1 / 1.17;

            #endregion

            #region 2. get shifted distribution of each grid

            double[,] mapDiff = new double[xPos.Length, yPos.Length];
            double[,] mapMU = new double[xPos.Length, yPos.Length];
            double[,,] map_PGdist = new double[xPos.Length, yPos.Length, 71];

            List<NccSpot> totalSpots = (from spot in nCCSpots
                                            //where spot.BeamState != NccSpot.NccBeamState.Tuning
                                        select spot).ToList();
            List<NccSpot> selectedSpots = (from spot in nCCSpots
                                           where spot.LayerNumber >= startLayer
                                           where spot.LayerNumber <= endLayer
                                           //where spot.BeamState != NccSpot.NccBeamState.Tuning
                                           select spot).ToList();

            bool debugFlag_flagGrid = true;
            bool[,] flagGrid = getFlagGrid(xPos, yPos, gridPitch, totalSpots, debugFlag_flagGrid);

            #region 2-2. get range difference and MU of each grid - output: [mapDiff], [mapMU]

            List<double[]> shiftedPGDistSum_DebugList = new List<double[]>(); // for debug (Jaerin)

            for (int i = 0; i < xPos.Length; i++)
            {
                for (int j = 0; j < yPos.Length; j++)
                {
                    double[] shiftedPGDistSum = new double[71];

                    if (flagGrid[i, j] == true)
                    {
                        shiftedPGDistSum = new double[71];

                        for (int k = 0; k < selectedSpots.Count; k++)
                        {
                            double spotXpos = selectedSpots[k].XPosition;
                            double spotYpos = selectedSpots[k].YPosition;

                            double xDifference = xPos[i] - spotXpos;
                            double yDifference = yPos[j] - spotYpos;

                            double distance = Math.Sqrt(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2));

                            if (distance <= 3 * sigma)
                            {
                                double spotEnergy = selectedSpots[k].PlanSpot.LayerEnergy;
                                double gap = calcGap(gapPeakAndRange, spotEnergy, rangeRatio_mat2water);

                                (double[] pgDist71, double[] pgDist72) = convert71from144(selectedSpots[k].ChannelCount, "220312");

                                double[] shiftedXgrid = new double[71];
                                double[] shiftedPGDist = new double[71];
                                double zPos = selectedSpots[k].PlanSpot.Zposition;
                                (shiftedXgrid, shiftedPGDist) = getShiftedData(xgrid_iso_mm, zPos, gap, direction, pgDist71);

                                double weight = Math.Exp(-0.5 * Math.Pow(distance / sigma, 2));

                                for (int ch = 0; ch < 71; ch++)
                                {
                                    shiftedPGDist[ch] = shiftedPGDist[ch] * weight;
                                }
                                //shiftedPGDist = shiftedPGDist.Select(x => x * weight).ToArray();

                                for (int ch = 0; ch < 71; ch++)
                                {
                                    shiftedPGDistSum[ch] = shiftedPGDistSum[ch] + shiftedPGDist[ch];
                                }

                                mapMU[i, j] = mapMU[i, j] + weight * selectedSpots[k].PlanSpot.MonitoringUnit;

                                #region


                                //double[] xgridCorrected = getCorrectedXgrid(xgrid_iso_mm, selectedSpots[k].PlanSpot.Zposition, gap, direction);
                                //double[] PGdistShifted = getShiftedPGdist(xgridCorrected, pgDist71, xgrid_iso_mm);

                                //double weight = Math.Exp(-0.5 * Math.Pow(distance / sigma, 2));
                                //PGdist_Shifted = PGdist_Shifted.Select(x => x * weight).ToArray();

                                //for (int p = 0; p < 71; p++)
                                //{
                                //    PGdistSum_Shifted[p] = PGdistSum_Shifted[p] + PGdist_Shifted[p];
                                //}

                                //mapMU[i, j] = mapMU[i, j] + weight * selectedSpots[k].PlanSpot.MonitoringUnit;

                                #region

                                //#region 2-2-1. gapPeakandRange - output: [gap]



                                //int index = gapPeakAndRange.FindIndex(gapList => gapList.energy >= spotEnergy);
                                //double gapInterpolation = (gapPeakAndRange[index].GapValue - gapPeakAndRange[index - 1].GapValue) / (gapPeakAndRange[index].energy - gapPeakAndRange[index - 1].energy) * (spotEnergy - gapPeakAndRange[index - 1].energy);
                                //double gap = gapPeakAndRange[index - 1].GapValue + gapInterpolation;


                                //#endregion

                                //#region 2-2-2. ch 144 -> ch 71 of selectedSpots[k] - output: [counts71Ch]

                                //double[] counts71Ch = convert71from144(selectedSpots[k].ChannelCount);

                                //#endregion

                                //#region 2-2-3. get Shifted distribution according to (plannedZposition + gap) - output: [shiftedPGdistributionTemp]

                                //double[] shiftedPGdistributionTemp = new double[71];
                                //double[] xgridTemp = xgrid_iso_mm.Select(x => x - (selectedSpots[k].PlanSpot.Zposition + gap)).ToArray();

                                //if (selectedSpots[k].PlanSpot.Zposition + gap >= 0)
                                //{
                                //    int interpIndex = xgridTemp.ToList().FindIndex(a => a >= xgrid_iso_mm[0] && a <= xgrid_iso_mm[1]); // 옮겨진 X 축에서 -105 ~ -102 사이에 오는 index
                                //    double left = xgridTemp[interpIndex] - xgrid_iso_mm[0];  // 내분점 left 길이 (index 기준)
                                //    double right = xgrid_iso_mm[1] - xgridTemp[interpIndex]; // 내분점 right 길이

                                //    for (int ii = 0; ii < 71 - interpIndex - 1; ii++)
                                //    {
                                //        shiftedPGdistributionTemp[ii] = (right * counts71Ch[interpIndex + ii] + left * counts71Ch[interpIndex + ii - 1]) / 3; // 내분, [index] 기준에서 [index-1] 기준으로 옮겨옴에 주의
                                //    }
                                //    for (int ii = 71 - interpIndex - 1; ii < 71; ii++)
                                //    {
                                //        shiftedPGdistributionTemp[ii] = shiftedPGdistributionTemp[71 - interpIndex - 1 - 1];
                                //    }
                                //}
                                //else // 빼주는 값이 0보다 작은 경우(빔이 0보다 얕게 들어감, 축이 오른쪽으로 이동하는 경우)
                                //{
                                //    int interpIndex = xgridTemp.ToList().FindIndex(a => a >= xgrid_iso_mm[69] && a <= xgrid_iso_mm[70]);
                                //    double left = xgridTemp[index] - xgrid_iso_mm[69];
                                //    double right = xgrid_iso_mm[70] - xgridTemp[index];

                                //    for (int ii = 0; ii < interpIndex + 1; ii++)
                                //    {
                                //        shiftedPGdistributionTemp[70 - interpIndex + ii] = (right * counts71Ch[ii + 1] + left * counts71Ch[ii]) / 3;
                                //    }

                                //    if (interpIndex < 69)
                                //    {
                                //        for (int ii = 0; ii < 70 - interpIndex; ii++)
                                //        {
                                //            shiftedPGdistributionTemp[ii] = shiftedPGdistributionTemp[70 - interpIndex];
                                //        }
                                //    }
                                //}

                                //#endregion

                                //#region 2-2-4. apply distance weight, get shifted PG distribution

                                //double weight = Math.Exp(-0.5 * Math.Pow(distance / sigma, 2));
                                //shiftedPGdistributionTemp = shiftedPGdistributionTemp.Select(x => x * weight).ToArray();

                                //for (int p = 0; p < 71; p++)
                                //{
                                //    shiftedPGdistribution[p] = shiftedPGdistribution[p] + shiftedPGdistributionTemp[p];
                                //}                                

                                //mapMU[i, j] = mapMU[i, j] + weight * selectedSpots[k].PlanSpot.MonitoringUnit;

                                //#endregion
                                #endregion
                                #endregion
                            }
                        }

                        shiftedPGDistSum_DebugList.Add(shiftedPGDistSum); // for debug (Jaerin)
                        mapDiff[i, j] = getRange_ver4p3(xgrid_iso_mm, shiftedPGDistSum, 0, 0);

                    }
                    else
                    {
                        for (int ch = 0; ch < 71; ch++)
                        {
                            shiftedPGDistSum[ch] = 0;
                        }
                        shiftedPGDistSum_DebugList.Add(shiftedPGDistSum);
                    }
                }

                bool flagDebug_GridMU = true;
                if (flagDebug_GridMU == true)
                {
                    string DebugFileName = @"6_Debug_GridMU.csv";
                    string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                    using (StreamWriter file = new StreamWriter(DebugPath_excel))
                    {
                        int xGrid = flagGrid.GetLength(0);
                        int yGrid = flagGrid.GetLength(1);

                        for (int x = 0; x < xGrid; x++)
                        {
                            for (int y = 0; y < yGrid - 1; y++)
                            {
                                file.Write($"{mapMU[x, y]}, ");
                            }
                            file.WriteLine($"{mapMU[x, yGrid - 1]}");
                        }
                    }
                }

                bool flagDebug_GridRange = true;
                if (flagDebug_GridRange == true)
                {
                    string DebugFileName = @"10_Debug_GridRangeDiffrence.csv";
                    string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                    using (StreamWriter file = new StreamWriter(DebugPath_excel))
                    {
                        int xGrid = flagGrid.GetLength(0);
                        int yGrid = flagGrid.GetLength(1);

                        for (int x = 0; x < xGrid; x++)
                        {
                            for (int y = 0; y < yGrid - 1; y++)
                            {
                                file.Write($"{mapDiff[x, y]}, ");
                            }
                            file.WriteLine($"{mapDiff[x, yGrid - 1]}");
                        }
                    }
                }
            }

            bool flagDebug_shiftedPGdist71 = true;
            if (flagDebug_shiftedPGdist71 == true)
            {
                string DebugFileName = @"9_Debug_shiftedPGdist71.csv";
                string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                using (StreamWriter file = new StreamWriter(DebugPath_excel))
                {
                    int xGrid = flagGrid.GetLength(0);
                    int yGrid = flagGrid.GetLength(1);

                    int index = 0;

                    for (int x = 0; x < xGrid; x++)
                    {
                        for (int y = 0; y < yGrid; y++)
                        {
                            file.Write($"{x + 1}, {y + 1}");

                            for (int ch = 0; ch < 71; ch++)
                            {
                                file.Write($", {shiftedPGDistSum_DebugList[index][ch]}");
                            }
                            file.WriteLine($"");
                            index++;
                        }
                    }
                }
            }





            #endregion

            #region 2-3. get beam range map - output: [beamRangeMap]

            for (int i = 0; i < xPos.Length; i++)
            {
                for (int j = 0; j < yPos.Length; j++)
                {
                    if (mapMU[i, j] > cutoffMU)
                    {
                        beamRangeMap.Add(new BeamRangeMap(-yPos[j], xPos[i], mapMU[i, j], mapDiff[i, j]));
                    }
                    else
                    {
                        beamRangeMap.Add(new BeamRangeMap(-yPos[j], xPos[i], 0, mapDiff[i, j]));
                    }
                }
            }
            beamRangeMap.Add(new BeamRangeMap(-10000, -10000, 20, 0));

            #region Debug

            for (int i = 0; i < beamRangeMap.Count; i++)
            {
                Console.WriteLine($"{beamRangeMap[i].X}, {beamRangeMap[i].Y}, {beamRangeMap[i].RangeDifference}, {beamRangeMap[i].MU}");
            }

            #endregion

            #endregion

            #endregion

            int a = 1;
        }

        private (double[], double[]) getShiftedData(double[] xgrid_iso_mm, double Zpos, double gap, int direction, double[] pgDist71)
        {
            // 1. Get Shifted X axis
            double[] shiftedXgrid = new double[71]; // Return (1)

            if (direction == 1)
            {
                shiftedXgrid = xgrid_iso_mm.Select(x => x - (Zpos + gap)).ToArray();
            }
            else // direction == 0
            {
                shiftedXgrid = xgrid_iso_mm.Select(x => x + (Zpos + gap)).ToArray();
            }

            // 2. Get Shifted PG distribution
            double[] shiftedPGDist = new double[71]; // Return (2)

            double xInterval = xgrid_iso_mm[1] - xgrid_iso_mm[0]; // 여기까지 맞음
            double shiftedX = shiftedXgrid[0] - xgrid_iso_mm[0];

            if (shiftedX >= 0)
            {
                //                                                                                                <index>
                // Shifted X axis :                   a[0]       a[1]       a[2]       a[3]       a[4]       ...   a[68]       a[69]       a[70]
                // xgrid_iso_mm   : b[0]       b[1]       b[2]    |  b[3]       ...    b[67]      b[68]      b[69]  |    b[70]
                // PG 71ch dist   : c[0]       c[1]       c[2]    |  c[3]       ...    c[67]      c[68]      c[69]  |    c[70]
                //                                                                                             |left|right|
                // Shifted PG dist:                   d[0]       d[1]       d[2]       d[3]       d[4]       ...    d[68]      d[69]       d[70]

                int idx = xgrid_iso_mm.ToList().FindIndex(x => x >= shiftedXgrid[0] && x <= shiftedXgrid[1]);
                double left = xgrid_iso_mm[idx] - shiftedXgrid[0];
                double right = shiftedXgrid[1] - xgrid_iso_mm[idx];
                for (int i = 0; i < 71 - idx; i++)
                {
                    shiftedPGDist[idx + i] = (right * pgDist71[i] + left * pgDist71[i + 1]) / xInterval;
                }
                for (int i = 0; i < idx; i++)
                {
                    shiftedPGDist[i] = shiftedPGDist[idx];
                }
            }
            else
            {
                //                                      <index>
                // xgridCorrected : a[0]       a[1]       a[2]       a[3]       a[4]       ...   a[69]       a[70]
                // xgrid_iso_mm   :                   b[0]  |    b[1]       b[2]       b[3]       ...    b[68]      b[69]       b[70]
                // PG 71ch dist   :                   c[0]  |    c[1]       c[2]       c[3]       ...    c[68]      c[69]       c[70]
                //                                     |left|right|
                // Shifted PG dist: d[0]       d[1]       d[2]       d[3]       ...    d[68]      d[69]       d[70]

                int idx = shiftedXgrid.ToList().FindIndex(x => x >= xgrid_iso_mm[0] && x <= xgrid_iso_mm[1]);
                double left = shiftedXgrid[idx] - xgrid_iso_mm[0];
                double right = xgrid_iso_mm[1] - shiftedXgrid[idx];
                for (int i = 0; i < 71 - idx; i++)
                {
                    shiftedPGDist[i] = (right * pgDist71[i + idx] + left * pgDist71[i + idx - 1]) / xInterval;
                }
                for (int i = 71 - idx; i < 71; i++)
                {
                    shiftedPGDist[i] = shiftedPGDist[71 - idx - 1];
                }
            }

            return (shiftedXgrid, shiftedPGDist);
        }
        private double[] getCorrectedXgrid(double[] xgrid_iso_mm, double Zpos, double gap, int direction)
        {
            double[] xgrid_temp = new double[71];

            if (direction == 1)
            {
                xgrid_temp = xgrid_iso_mm.Select(x => x - (Zpos + gap)).ToArray();
            }
            else // direction == 0
            {
                xgrid_temp = xgrid_iso_mm.Select(x => x + (Zpos + gap)).ToArray();
            }

            return xgrid_temp;
        }

        private double interp1(double[] X, double[] Y, double Xq)
        {
            double Yq; // return

            // data
            // idx:   0    1    2         3    4    5    6
            //   X:  x1   x2   x3  (Xq)  x4   x5   x6   x7 ...
            //   Y:  y1   y2   y3  (Yq)  y4   y5   y6   y7 ...

            // return
            // Yq = y3       + (y4     - y3)       * ((Xq - x3)       / (x4     - x3))
            //    = Y[idx-1] + (Y[idx] - Y[idx-1]) * ((Xq - X[idx-1]) / (X[idx] - X[idx-1]))

            int idx = X.ToList().FindIndex(x => x >= Xq); // x4
            if (idx != -1)
            {
                Yq = Y[idx - 1] + (Y[idx] - Y[idx - 1]) * ((Xq - X[idx - 1]) / (X[idx] - X[idx - 1]));
            }
            else if (idx == 0)
            {
                Debug.Assert(true, $"spot energy is too low compared to gapPeakAndRange.txt list");
                Yq = Y[0];
            }
            else // idx == -1
            {
                Debug.Assert(true, $"spot energy is too high compared to gapPeakAndRange.txt list");
                Yq = 0;
            }

            return Yq;
        }

        private double calcGap(List<GapPeakAndRange> gapPeakAndRange, double spotEnergy, double rangeRatio_mat2water)
        {
            double[] X = (from data in gapPeakAndRange
                          select data.energy).ToArray();
            double[] Y = (from data in gapPeakAndRange
                          select data.GapValue).ToArray();

            double gap = interp1(X, Y, spotEnergy) * rangeRatio_mat2water;
            return gap;
        }

        private bool[,] getFlagGrid(double[] xPos, double[] yPos, double gridPitch, List<NccSpot> totalSpots, bool debugFlag)
        {
            bool[,] flagGrid = new bool[xPos.Length, yPos.Length];
            int numOfTotalSpots = totalSpots.Count();

            for (int x = 0; x < xPos.Length; x++)
            {
                for (int y = 0; y < yPos.Length; y++)
                {
                    for (int i = 0; i < numOfTotalSpots; i++)
                    {
                        double spotXpos = totalSpots[i].XPosition;
                        double spotYpos = totalSpots[i].YPosition;

                        bool flagX = (xPos[x] - (gridPitch / 2) < spotXpos) && (xPos[x] + (gridPitch / 2) > spotXpos);
                        bool flagY = (yPos[y] - (gridPitch / 2) < spotYpos) && (yPos[y] + (gridPitch / 2) > spotYpos);

                        if (flagX && flagY)
                        {
                            flagGrid[x, y] = true;
                            break;
                        }
                    }
                }
            }

            if (debugFlag == true)
            {
                string DebugFileName = @"5_Debug_flagGrid.csv";
                string DebugPath_excel = string.Concat(debugDirectory, DebugFileName);
                using (StreamWriter file = new StreamWriter(DebugPath_excel))
                {
                    int xGrid = flagGrid.GetLength(0);
                    int yGrid = flagGrid.GetLength(1);

                    for (int x = 0; x < xGrid; x++)
                    {
                        for (int y = 0; y < yGrid - 1; y++)
                        {
                            if (flagGrid[x, y] == true)
                            {
                                file.Write($"1, ");
                            }
                            else // (flagGrid[x, y] == false)
                            {
                                file.Write($"0, ");
                            }
                        }

                        if (flagGrid[x, yGrid - 1] == true)
                        {
                            file.WriteLine($"1, ");
                        }
                        else // (flagGrid[x, yGrid - 1] == false)
                        {
                            file.WriteLine($"0, ");
                        }
                    }
                }
            }

            return flagGrid;
        }

        private (double[], double[]) convert71from144(int[] pgDist144, string missingValDate)
        {
            double[] cnt_row1 = new double[36];
            double[] cnt_row2 = new double[36];
            double[] cnt_row3 = new double[36];
            double[] cnt_row4 = new double[36];

            double[] cnt_top = new double[36];
            double[] cnt_bot = new double[36];

            double[] cnt_72ch = new double[72];

            for (int ii = 0; ii < 18; ii++)
            {
                cnt_row1[ii] = pgDist144[89 - ii];
                cnt_row2[ii] = pgDist144[107 - ii];
                cnt_row3[ii] = pgDist144[125 - ii];
                cnt_row4[ii] = pgDist144[143 - ii];

                cnt_row1[ii + 18] = pgDist144[17 - ii];
                cnt_row2[ii + 18] = pgDist144[35 - ii];
                cnt_row3[ii + 18] = pgDist144[53 - ii];
                cnt_row4[ii + 18] = pgDist144[71 - ii];
            }

            // fill missing value because of broken scintillator
            List<int[]> errorList = new List<int[]>(); // { rowNumber[0], chNumber[1], correctionMethod[2] }
            switch (missingValDate)
            {
                case "":
                    break;

                case "220312":
                    errorList.Add(new int[] { 1, 2, 1 });
                    errorList.Add(new int[] { 1, 21, 2 });
                    errorList.Add(new int[] { 1, 35, 1 });
                    errorList.Add(new int[] { 2, 11, 1 });
                    errorList.Add(new int[] { 2, 19, 2 });
                    errorList.Add(new int[] { 2, 25, 1 });
                    errorList.Add(new int[] { 2, 28, 3 });
                    errorList.Add(new int[] { 2, 29, 4 });
                    errorList.Add(new int[] { 2, 36, 1 });
                    errorList.Add(new int[] { 3, 20, 2 });
                    errorList.Add(new int[] { 3, 24, 2 });
                    errorList.Add(new int[] { 4, 10, 2 });
                    break;

                case "220528":
                    break;
            }

            List<double[]> cnt_rows = new List<double[]>();
            cnt_rows.Add(cnt_row1);
            cnt_rows.Add(cnt_row2);
            cnt_rows.Add(cnt_row3);
            cnt_rows.Add(cnt_row4);

            // 1. get error channels of top/bottom
            List<int> topErrorChTemp = new List<int>();
            List<int> botErrorChTemp = new List<int>();
            foreach (int[] error in errorList)
            {
                if (error[0] == 1 || error[0] == 2)
                {
                    topErrorChTemp.Add(error[1]);
                }
                else // error[0] == 3 or 4
                {
                    botErrorChTemp.Add(error[1]);
                }
            }            

            // 2. get error channels (unique) of top/bottom
            int[] topErrorCh = topErrorChTemp.Distinct().ToArray();
            Array.Sort(topErrorCh);
            int[] botErrorCh = botErrorChTemp.Distinct().ToArray();
            Array.Sort(botErrorCh);

            // 3. get valid channels of top/bottom
            int[] topValidCh = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
            int[] botValidCh = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
            if (topErrorCh.Length != 0)
            {
                foreach (int errorCh in topErrorCh)
                {
                    topValidCh = topValidCh.Where(ch => ch != errorCh).ToArray();
                }
            }
            if (botErrorCh.Length != 0)
            {
                foreach (int errorCh in botErrorCh)
                {
                    botValidCh = botValidCh.Where(ch => ch != errorCh).ToArray();
                }
            }

            // 4. correction of each channel counts
            // *** (1: pair, 2: mean, 3-> left, 4 -> right) ***
            foreach (int[] error in errorList)
            {
                int tarRowIndex = error[0] - 1;
                int tarChIndex = error[1] - 1;
                int corrMethod = error[2];

                if (corrMethod == 1) // pair
                {
                    int refRowIndex;
                    double tarRowValidChCountsSum;
                    double refRowValidChCountsSum;
                    double correctionRatio;

                    if (tarRowIndex == 0)
                    {
                        refRowIndex = 1;
                    }
                    else if (tarRowIndex == 1)
                    {
                        refRowIndex = 0;
                    }
                    else if (tarRowIndex == 2)
                    {
                        refRowIndex = 3;
                    }
                    else
                    {
                        refRowIndex = 2;
                    }

                    tarRowValidChCountsSum = cnt_rows[tarRowIndex].Sum();
                    foreach (int errorCh in topErrorCh)
                    {
                        int errorChIndex = errorCh - 1;
                        tarRowValidChCountsSum -= cnt_rows[tarRowIndex][errorChIndex];
                    }

                    refRowValidChCountsSum = cnt_rows[refRowIndex].Sum();
                    foreach (int errorCh in topErrorCh)
                    {
                        int errorChIndex = errorCh - 1;
                        refRowValidChCountsSum -= cnt_rows[refRowIndex][errorChIndex];
                    }

                    correctionRatio = tarRowValidChCountsSum / refRowValidChCountsSum;
                    cnt_rows[tarRowIndex][tarChIndex] = cnt_rows[refRowIndex][tarChIndex] * correctionRatio;
                }
                else if (corrMethod == 2) // mean
                {
                    cnt_rows[tarRowIndex][tarChIndex] = (cnt_rows[tarRowIndex][tarChIndex - 1] + cnt_rows[tarRowIndex][tarChIndex + 1]) / 2;
                }
                else if (corrMethod == 3) // left
                {
                    cnt_rows[tarRowIndex][tarChIndex] = cnt_rows[tarRowIndex][tarChIndex - 1];
                }
                else // (corrMethod == 4) // right
                {
                    cnt_rows[tarRowIndex][tarChIndex] = cnt_rows[tarRowIndex][tarChIndex + 1];
                }
            }

            for (int ii = 0; ii < 36; ii++)
            {
                cnt_bot[ii] = cnt_rows[2][ii] + cnt_rows[3][ii];
                cnt_top[ii] = cnt_rows[0][ii] + cnt_rows[1][ii];
            }

            for (int ii = 0; ii < 36; ii++)
            {
                cnt_72ch[2 * ii] = cnt_bot[ii];
                cnt_72ch[2 * ii + 1] = cnt_top[ii];
            }

            double[] pgDist71 = new double[71];
            for (int ii = 0; ii < 71; ii++)
            {
                pgDist71[ii] = (cnt_72ch[ii] + cnt_72ch[ii + 1]) / 2;
            }

            return (pgDist71, cnt_72ch);
        }

        private double getRange_ver4p3(double[] xGrid, double[] pgDist, double rangeTrue, int direction)
        {
            // === Return data === //
            double range = -1;

            // === Algorithm === //
            // 0. Parameter setting: (1) xgrid_diff[69]  (2) algorithm: sigma_gaussFilt, cutoffLevel, offset, pitch, minPeakDistance, scope
            // 1. distribution reconstruction: 144 -> 72 -> 71
            // 2. apply gaussian kernel to 2nd derivative of distribution
            // 3. findpeaks
            // 4. select valid findpeaks value
            // 5. find peak valley
            // 6. get range

            if (direction == 0)
            {
                Array.Reverse(pgDist);
            }

            double sigma_gaussFilt = 5;
            double cutoffLevel = 0.5;
            double offset = 0.6;
            double minPeakDistance = 10;
            double pitch = 3;

            double[] xgrid_diff = new double[69];
            for (int i = 0; i < 69; i++)
            {
                xgrid_diff[i] = xGrid[i + 1];
            }

            double[] dist_diff_unfilt = new double[69];
            for (int i = 0; i < 69; i++)
            {
                dist_diff_unfilt[i] = -(pgDist[i + 2] - pgDist[i]) / (2 * pitch);
            }

            double[] dist_diff = new double[69];
            dist_diff = imgaussfilt(dist_diff_unfilt, sigma_gaussFilt / pitch);

            (double[] pks, double[] locs) = findpeaks(dist_diff, xgrid_diff, minPeakDistance);
            (double val_pk, int loc_pk) = getValidPeakWithScope(pks, locs, rangeTrue, 30);

            if ((val_pk, loc_pk) == (-10000, -10000))
            {
                return range = -10000;
            }

            double[] cnt_71ch_2ndDer_reverse = new double[69];
            for (int i = 0; i < 69; i++)
            {
                cnt_71ch_2ndDer_reverse[i] = -dist_diff[i];
            }

            DoubleVector secondDer_reverse = new DoubleVector(cnt_71ch_2ndDer_reverse);
            PeakFinderRuleBased peakFind_reverse = new PeakFinderRuleBased(secondDer_reverse);

            peakFind_reverse.LocatePeaks();
            double[] distanceFromPeak = new double[peakFind_reverse.NumberPeaks];
            for (int i = 0; i < peakFind_reverse.NumberPeaks; i++)
            {
                distanceFromPeak[i] = loc_pk - peakFind_reverse[i].X;
            }

            double[] tempLeft = (from distance in distanceFromPeak
                                 where distance >= 2
                                 select distance).ToArray();
            double[] tempRight = (from distance in distanceFromPeak
                                  where distance <= -2
                                  select distance).ToArray();
            //double[] tempLeft = (from distance in distanceFromPeak
            //                     where distance > 0
            //                     select distance).ToArray();
            //double[] tempRight = (from distance in distanceFromPeak
            //                      where distance < 0
            //                      select distance).ToArray();

            int LeftIndex, RightIndex;

            if (tempLeft.Length != 0)
            {
                LeftIndex = loc_pk - Convert.ToInt32(tempLeft.Last()); // 수정 2022-01-09 23:06
            }
            else
            {
                LeftIndex = 0;
            }

            if (tempRight.Length != 0)
            {
                RightIndex = loc_pk - Convert.ToInt32(tempRight[0]); // 수정 2022-01-09 23:06
            }
            else
            {
                RightIndex = 68;
            }

            double minValue_left = dist_diff[LeftIndex];
            double minValue_right = dist_diff[RightIndex];

            double bottomLevel = Math.Max(minValue_left, minValue_right);

            double baseline = bottomLevel + cutoffLevel * (val_pk - bottomLevel);

            #endregion

            #region 6. get range

            double sig_MR = new double();
            double sig_M = new double();

            if (RightIndex - LeftIndex <= 4)
            {
                double chkX = loc_pk;
            }

            for (int i = LeftIndex; i < RightIndex + 1; i++)
            {
                if (dist_diff[i] - baseline > 0)
                {
                    sig_MR += (dist_diff[i] - baseline) * xgrid_diff[i];
                    sig_M += dist_diff[i] - baseline;
                }
            }

            range = (sig_MR / sig_M) + offset;

            #endregion

            return range;
        }

        private (double[], double[]) findpeaks(double[] dist_diff, double[] xgrid_diff, double minPeakDistance)
        {
            // modify later https://www.cnblogs.com/sowhat4999/p/7050697.html
            DoubleVector secondDer = new DoubleVector(dist_diff);
            PeakFinderRuleBased peakFind = new PeakFinderRuleBased(secondDer);

            peakFind.LocatePeaks();
            peakFind.ApplySortOrder(PeakFinderRuleBased.PeakSortOrder.Descending);

            List<Extrema> FoundPeaks = peakFind.GetAllPeaks();

            int index = 0;
            if (FoundPeaks.Count > 0)
            {
                while (true)
                {
                    if (index < FoundPeaks.Count())
                    {
                        FoundPeaks.RemoveAll(x => Math.Abs(x.X - FoundPeaks[index].X) < minPeakDistance && x.X != FoundPeaks[index].X);

                        if (index < FoundPeaks.Count())
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Trace.WriteLine("Error: [getRange_ver4px - findpeaks]");
            }

            var locs = (from Pks in FoundPeaks
                        select Pks.X).ToArray();
            var pks = (from Pks in FoundPeaks
                       select Pks.Y).ToArray();

            return (pks, locs);
        }

        private (double, int) getValidPeakWithScope(double[] pks, double[] locs, double rangeTrue, double scope)
        {
            List<int> indexList = new List<int>();
            List<double> peaksList = new List<double>();
            List<double> locsList = new List<double>();
            int validIndex = 0;

            foreach (var loc in locs)
            {
                if (3 * (loc - 34) > rangeTrue - scope && 3 * (loc - 34) < rangeTrue + scope)
                {
                    indexList.Add(validIndex);
                    peaksList.Add(pks[validIndex]);
                    locsList.Add(locs[validIndex]);
                }
                validIndex++;
            }

            //double val_peak = -10000;
            //int loc_peak = -10000;
            double val_peak;
            int loc_peak;

            if (peaksList.Count() != 0)
            {
                val_peak = peaksList.Max();
                loc_peak = (int)locsList[peaksList.IndexOf(val_peak)];

                return (val_peak, loc_peak);
            }
            else
            {
                return (-10000, -10000);
            }
        }

        private double getRange_ver4p0(double[] pgDistribution, double refRangePos, bool is144ChCount)
        {
            // === Return data === //
            double range = -1;

            // === Algorithm === //
            // 0. Parameter setting: (1) xgrid_diff[69]  (2) algorithm: sigma_gaussFilt, cutoffLevel, offset, pitch, minPeakDistance, scope
            // 1. distribution reconstruction: 144 -> 72 -> 71
            // 2. apply gaussian kernel to 2nd derivative of distribution
            // 3. findpeaks
            // 4. select valid findpeaks value
            // 5. find peak valley
            // 6. get range

            #region 0. Parameter setting

            double[] xgrid_diff = new double[69];
            for (int i = 0; i < 69; i++)
            {
                xgrid_diff[i] = -102 + 3 * i;
            }

            double sigma_gaussFilt = 5;
            double cutoffLevel = 0.5;
            double offset = 0;
            double pitch = 3;
            double minPeakDistance = 10;
            double scope = 30;

            #endregion

            #region 1. PG distribution reconstruction: 144 -> 72 -> 71

            double[] cnt_71ch = new double[71];

            if (is144ChCount == false)
            {
                cnt_71ch = getPGdist(pgDistribution);
            }
            else // is144ChCount == true
            {
                cnt_71ch = pgDistribution;
            }


            #region Debug (ch counts)

            //for (int i = 0; i < 36; i++)
            //{
            //    Console.WriteLine($"{cnt_row1[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 36; i++)
            //{
            //    Console.WriteLine($"{cnt_row2[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 36; i++)
            //{
            //    Console.WriteLine($"{cnt_row3[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 36; i++)
            //{
            //    Console.WriteLine($"{cnt_row4[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 144; i++)
            //{
            //    Console.WriteLine($"{pgDistribution[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 72; i++)
            //{
            //    Console.WriteLine($"{cnt_72ch[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            //for (int i = 0; i < 71; i++)
            //{
            //    Console.WriteLine($"{cnt_71ch[i]}"); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //}

            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("");

            #endregion

            #endregion

            #region 2. Apply gaussian kernel to 2nd derivative of 71 ch PG distribution

            double[] cnt_71ch_2ndDer = new double[69];
            for (int i = 0; i < 69; i++)
            {
                cnt_71ch_2ndDer[i] = -(cnt_71ch[i + 2] - cnt_71ch[i]) / (2 * pitch);
            }

            double[] cnt_71ch_2ndDer_gaussFilt = new double[69];

            #region 2-1. Generate Gaussian kernel

            double[] hcol = new double[9];
            double hcolSum = 0;

            double sigmaValue = sigma_gaussFilt / pitch;
            //double sigmaValue = sigma_gaussFilt;

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigmaValue)) * Math.Exp(-(Math.Pow(4 - i, 2) / (2 * (Math.Pow(sigmaValue, 2)))));
                hcolSum += hcol[i];
            }

            double normalizationFactor = 1 / hcolSum; // 수정 가능

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = hcol[i] * normalizationFactor;
            }

            #endregion

            #region 2-2. Apply Gaussian kernel to cnt_71ch_2ndDer

            List<double> preConv_dist_unfilt = new List<double>();
            double dist_diff_temp = 0;

            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[0]);
            }
            for (int i = 0; i < 69; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[68]);
            }

            for (int i = 0; i < 69; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dist_diff_temp += (preConv_dist_unfilt[i + j] * hcol[j]);
                }
                cnt_71ch_2ndDer_gaussFilt[i] = dist_diff_temp;
                dist_diff_temp = 0;
            }

            //Console.WriteLine(""); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Console.WriteLine("");
            //Console.WriteLine("");
            //for (int i = 0; i < 69; i++)
            //{
            //    Console.WriteLine($"{cnt_71ch_2ndDer_gaussFilt[i]}");
            //}


            #endregion

            #endregion

            #region 3. findpeaks
            // modify later to https://www.cnblogs.com/sowhat4999/p/7050697.html
            DoubleVector secondDer = new DoubleVector(cnt_71ch_2ndDer_gaussFilt);
            PeakFinderRuleBased peakFind = new PeakFinderRuleBased(secondDer);

            peakFind.LocatePeaks();
            peakFind.ApplySortOrder(PeakFinderRuleBased.PeakSortOrder.Descending);

            List<Extrema> FoundPeaks = peakFind.GetAllPeaks();

            int index = 0;
            if (FoundPeaks.Count > 0)
            {
                while (true)
                {
                    if (index < FoundPeaks.Count())
                    {
                        FoundPeaks.RemoveAll(x => Math.Abs(x.X - FoundPeaks[index].X) < minPeakDistance && x.X != FoundPeaks[index].X);

                        #region Debugging
                        //Trace.WriteLine("");
                        //Trace.WriteLine("삭제 중"); int j = 1;
                        //foreach (var kk in FoundPeaks)
                        //{
                        //    Trace.WriteLine($"{j}. {kk.X}, {kk.Y}");
                        //    j++;
                        //}
                        #endregion

                        if (index < FoundPeaks.Count())
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Trace.WriteLine("Error: [getRange_ver4px - findpeaks]");
            }

            var locs = (from Pks in FoundPeaks
                        select Pks.X).ToArray();
            var pks = (from Pks in FoundPeaks
                       select Pks.Y).ToArray();

            #endregion

            #region 4. select valid findpeaks value

            List<int> indexList = new List<int>();
            List<double> peaksList = new List<double>();
            List<double> locsList = new List<double>();
            int validIndex = 0;

            foreach (var loc in locs)
            {
                if (3 * (loc - 34) > refRangePos - scope && 3 * (loc - 34) < refRangePos + scope)
                {
                    indexList.Add(validIndex);
                    peaksList.Add(pks[validIndex]);
                    locsList.Add(locs[validIndex]);
                }
                validIndex++;
            }

            double val_peak = -10000;
            int loc_peak = -10000;

            if (peaksList.Count() != 0)
            {
                val_peak = peaksList.Max();
                loc_peak = (int)locsList[peaksList.IndexOf(val_peak)];
            }
            else
            {
                return -10000; // range
            }

            #endregion

            #region 5. find peak valley



            double[] cnt_71ch_2ndDer_reverse = new double[69];
            for (int i = 0; i < 69; i++)
            {
                cnt_71ch_2ndDer_reverse[i] = -cnt_71ch_2ndDer[i];
            }

            DoubleVector secondDer_reverse = new DoubleVector(cnt_71ch_2ndDer_reverse);
            PeakFinderRuleBased peakFind_reverse = new PeakFinderRuleBased(secondDer_reverse);

            peakFind_reverse.LocatePeaks();
            double[] distanceFromPeak = new double[peakFind_reverse.NumberPeaks];
            for (int i = 0; i < peakFind_reverse.NumberPeaks; i++)
            {
                distanceFromPeak[i] = loc_peak - peakFind_reverse[i].X;
            }

            double[] tempLeft = (from distance in distanceFromPeak
                                 where distance >= 2
                                 select distance).ToArray();
            double[] tempRight = (from distance in distanceFromPeak
                                  where distance <= -2
                                  select distance).ToArray();
            //double[] tempLeft = (from distance in distanceFromPeak
            //                     where distance > 0
            //                     select distance).ToArray();
            //double[] tempRight = (from distance in distanceFromPeak
            //                      where distance < 0
            //                      select distance).ToArray();

            int LeftIndex, RightIndex;

            if (tempLeft.Length != 0)
            {
                LeftIndex = loc_peak - Convert.ToInt32(tempLeft.Last()); // 수정 2022-01-09 23:06
            }
            else
            {
                LeftIndex = 0;
            }

            if (tempRight.Length != 0)
            {
                RightIndex = loc_peak - Convert.ToInt32(tempRight[0]); // 수정 2022-01-09 23:06
            }
            else
            {
                RightIndex = 68;
            }

            double minValue_left = cnt_71ch_2ndDer_gaussFilt[LeftIndex];
            double minValue_right = cnt_71ch_2ndDer_gaussFilt[RightIndex];

            double bottomLevel = Math.Max(minValue_left, minValue_right);

            double baseline = bottomLevel + cutoffLevel * (val_peak - bottomLevel);

            #endregion

            #region 6. get range

            double sig_MR = new double();
            double sig_M = new double();

            if (RightIndex - LeftIndex <= 4)
            {
                double chkX = loc_peak;
            }

            for (int i = LeftIndex; i < RightIndex + 1; i++)
            {
                if (cnt_71ch_2ndDer_gaussFilt[i] - baseline > 0)
                {
                    sig_MR += (cnt_71ch_2ndDer_gaussFilt[i] - baseline) * xgrid_diff[i];
                    sig_M += cnt_71ch_2ndDer_gaussFilt[i] - baseline;
                }
            }

            range = (sig_MR / sig_M) + offset;

            #endregion

            return range;
        }

        private double[] imgaussfilt(double[] dist_diff_unfilt, double sigmaValue)
        {
            double[] dist_diff = new double[69]; // Return
            List<double> preConv_dist_unfilt = new List<double>();

            #region Gaussian Kernel 생성

            double[] hcol = new double[9];
            double hcolSum = 0;

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigmaValue)) * Math.Exp(-(Math.Pow(4 - i, 2) / (2 * (Math.Pow(sigmaValue, 2)))));
                hcolSum += hcol[i];
            }

            double normalizationFactor = 1 / hcolSum; // 수정 가능

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = hcol[i] * normalizationFactor;
            }

            #endregion

            #region Gaussian Kernel 적용 (double[69] dist_diff)

            double dist_diff_temp = 0;

            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(dist_diff_unfilt[0]);
            }
            for (int i = 0; i < 69; i++)
            {
                preConv_dist_unfilt.Add(dist_diff_unfilt[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(dist_diff_unfilt[68]);
            }

            for (int i = 0; i < 69; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dist_diff_temp += (preConv_dist_unfilt[i + j] * hcol[j]);
                }
                dist_diff[i] = dist_diff_temp;

                dist_diff_temp = 0;
            }

            #endregion

            return dist_diff;
        }

        private double[] getPGdist(double[] pg144ch)
        {
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            double[] cnt_row1 = new double[36];
            double[] cnt_row2 = new double[36];
            double[] cnt_row3 = new double[36];
            double[] cnt_row4 = new double[36];

            double[] cnt_top = new double[36];
            double[] cnt_bot = new double[36];

            double[] cnt_72ch = new double[72];


            for (int i = 0; i < 18; i++)
            {
                cnt_row1[i] = pg144ch[89 - i];
                cnt_row2[i] = pg144ch[107 - i];
                cnt_row3[i] = pg144ch[125 - i];
                cnt_row4[i] = pg144ch[143 - i];

                cnt_row1[i + 18] = pg144ch[17 - i];
                cnt_row2[i + 18] = pg144ch[35 - i];
                cnt_row3[i + 18] = pg144ch[53 - i];
                cnt_row4[i + 18] = pg144ch[71 - i];
            }

            for (int i = 0; i < 36; i++)
            {
                cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                cnt_top[i] = cnt_row1[i] + cnt_row2[i];
            }

            for (int i = 0; i < 36; i++)
            {
                cnt_72ch[2 * i] = cnt_bot[i];
                cnt_72ch[2 * i + 1] = cnt_top[i];
            }


            double[] pg71ch = new double[71];
            for (int i = 0; i < 71; i++)
            {
                pg71ch[i] = (cnt_72ch[i] + cnt_72ch[i + 1]) / 2;
            }

            return pg71ch;
        }


        private struct NccLogParameter
        {
            public double coeff_x;
            public double coeff_y;
            public DateTime TimeREF; // 나중에 쓰일것
        }
    }

    public class NccPlan
    {
        public string? PlanFile { get; private set; }
        public double TotalPlanMonitoringUnit { get; }
        public int TotalPlanLayer { get; }

        private List<NccPlanSpot> spots = new List<NccPlanSpot>();
        public NccPlan(string plan3DFileDir, string plan2DFileDir, bool flagFlatten)
        {
            // 1. Load 2D plan file
            double totalMU = 0;
            double cumMeterSetWeight = 0;

            if (plan2DFileDir != null & plan2DFileDir != "")
            {
                using (FileStream fs = new FileStream(plan2DFileDir!, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        string lines = new string("");
                        string[] tempString = new string[0];

                        tempString = sr.ReadLine()!.Split(",");

                        totalMU = Convert.ToDouble(tempString[7]);
                        cumMeterSetWeight = Convert.ToDouble(tempString[8]);
                    }
                }
            }

            // 2. Load 3D plan file
            if (plan3DFileDir != null & plan3DFileDir != "")
            {
                PlanFile = plan3DFileDir;

                using (FileStream fs = new FileStream(plan3DFileDir!, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        string lines = new string("");
                        string[] tempString = new string[0];

                        int TempLayerNumber = 0;

                        tempString = sr.ReadLine()!.Split(",");

                        double LayerEnergy = Convert.ToDouble(tempString[2]);
                        double LayerMU = Convert.ToDouble(tempString[3]);
                        int LayerSpotCount = Convert.ToInt32(tempString[4]) / 2;

                        while ((lines = sr.ReadLine()!) != null)
                        {
                            NccPlanSpot tempPlanSpot = new NccPlanSpot();

                            if (lines.Contains("Layer"))
                            {
                                tempString = lines.Split(",");

                                TempLayerNumber += 1;

                                LayerEnergy = Convert.ToDouble(tempString[2]);
                                LayerMU = Convert.ToDouble(tempString[3]);
                                LayerSpotCount = Convert.ToInt32(tempString[4]) / 2;
                            }
                            else
                            {
                                tempString = lines.Split("\t");
                                spots.Add(new NccPlanSpot(TempLayerNumber, LayerEnergy, LayerMU, LayerSpotCount, Convert.ToDouble(tempString[0]),
                                              Convert.ToDouble(tempString[1]), Convert.ToDouble(tempString[2]), Convert.ToDouble(tempString[3]) * totalMU / cumMeterSetWeight));
                            }
                        }
                    }
                }

                List<NccPlanSpot> spotsTemp = new List<NccPlanSpot>();
                if (flagFlatten == true)
                {
                    int LayerCount = spots.Last().LayerNumber;
                    int spotIndex = 0;

                    for (int i = 0; i < LayerCount + 1; i++)
                    {
                        var zPosInLayer = (from spot in spots
                                           where spot.LayerNumber == i
                                           select spot.Zposition).ToList();

                        var zPosAverage = zPosInLayer.Average();
                        var spotCounts = zPosInLayer.Count();

                        var ss = (from spot in spots
                                  where spot.LayerNumber == i
                                  select spot).ToList();

                        for (int j = 0; j < spotCounts; j++)
                        {
                            int layerNumber = spots[spotIndex].LayerNumber;
                            double layerEnergy = spots[spotIndex].LayerEnergy;
                            double layerMU = spots[spotIndex].LayerMU;
                            int layerSpotCount = spots[spotIndex].LayerSpotCount;
                            double xPosition = spots[spotIndex].Xposition;
                            double yPosition = spots[spotIndex].Yposition;
                            double zPosition = zPosAverage; ///
                            double MonitoringUnit = spots[spotIndex].MonitoringUnit;

                            spotsTemp.Add(new NccPlanSpot(layerNumber, layerEnergy, layerMU, layerSpotCount, xPosition, yPosition, zPosition, MonitoringUnit));
                            spotIndex++;
                        }

                    }
                    spots.Clear();
                    spots = spotsTemp;
                }

                TotalPlanMonitoringUnit = 0;
                foreach (NccPlanSpot spot in spots)
                {
                    TotalPlanMonitoringUnit += spot.MonitoringUnit;
                }

                TotalPlanLayer = 0;
                TotalPlanLayer = spots.Last().LayerNumber;
            }
        }
        public (List<NccPlanSpot>, double layerEnergy) GetPlanSpotsByLayerNumber(int layerNumber)
        {
            List<NccPlanSpot> planSpotSingleLayer = (from planSpots in spots
                                                     where planSpots.LayerNumber == layerNumber
                                                     select planSpots).ToList();
            double layerEnergy = planSpotSingleLayer.Last().LayerEnergy;

            return (planSpotSingleLayer, layerEnergy);
        }

        public List<NccPlanSpot> GetPlanSpots()
        {
            return spots;
        }
    }

    public class NccMultislitPg
    {
        public string? PGDir { get; private set; }

        private List<PgSpot> spots = new List<PgSpot>();
        public NccMultislitPg(string pgDir, bool isBroken, bool flagDebug)
        {
            if (pgDir != null & pgDir != "")
            {
                PGDir = pgDir;

                using BinaryReader br = new BinaryReader(File.Open(pgDir, FileMode.Open));
                {
                    long length = br.BaseStream.Length;
                    byte[] buffer = new byte[1024 * 1024 * 1000];
                    buffer = br.ReadBytes(Convert.ToInt32(length));

                    byte[] DATA_BUFFER = new byte[334];
                    ushort[] SDATA_BUFFER = new ushort[167];

                    int CurrentPos = 0;

                    while (CurrentPos < length)
                    {
                        if (buffer[CurrentPos] == 0xFE)
                        {
                            CurrentPos += 1;

                            if (buffer[CurrentPos] == 0xFE)
                            {
                                int[] ChannelCount = new int[144];
                                int TriggerStartTime = new int();
                                int TriggerEndTime = new int();
                                double ADC = new double();

                                int SumCounts = new int();

                                for (int i = 0; i < 334; i++)
                                {
                                    CurrentPos += 1;
                                    DATA_BUFFER[i] = buffer[CurrentPos];
                                }
                                Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length);

                                for (int ch = 0; ch < 72; ch++)
                                {
                                    ChannelCount[ch] = SDATA_BUFFER[ch];
                                }
                                for (int ch = 81; ch < 153; ch++)
                                {
                                    ChannelCount[ch - 9] = SDATA_BUFFER[ch];
                                }
                                uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];

                                TriggerStartTime = Convert.ToInt32(time_count);
                                TriggerEndTime = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));
                                ADC = (double)SDATA_BUFFER[76] / 4096.0 * 5.0;

                                SumCounts = ChannelCount.ToList().Sum();

                                if (isBroken == true)
                                {
                                    ChannelCount[0] = ChannelCount[1];
                                    ChannelCount[18] = ChannelCount[19];
                                    ChannelCount[36] = ChannelCount[37];

                                    // 27 

                                    ChannelCount[26] = ChannelCount[25];
                                    ChannelCount[27] = ChannelCount[28];

                                    ChannelCount[45] = ChannelCount[44];

                                    ChannelCount[63] = ChannelCount[62];

                                    ChannelCount[41] = ChannelCount[40];

                                    //double temp1 = (ChannelCount[25] + ChannelCount[27]) / 2;
                                    //ChannelCount[26] = (int)Math.Truncate(temp1);

                                    //double temp2 = (ChannelCount[30] + ChannelCount[28]) / 2;
                                    //ChannelCount[29] = (int)Math.Truncate(temp2);

                                    //ChannelCount[29] = ChannelCount[]
                                }

                                PgSpot pgSpot = new PgSpot(ChannelCount, SumCounts, TriggerStartTime, TriggerEndTime, ADC);
                                spots.Add(pgSpot);
                            }
                            else
                            {
                                CurrentPos += 1;
                            }
                        }
                        else
                        {
                            CurrentPos += 1;
                        }
                    }
                }
            }
        }

        public List<PgSpot> GetPGSpots()
        {
            return spots;
        }

        private struct PGstruct // Tick?
        {
            public int[] ChannelCount;
            public int TriggerStartTime;
            public int TriggerEndTime;
            public double ADC;

            public int Tick;
        }
    }

    public record NccLogSpot(int LayerNumber = -1, string LayerID = "", double XPosition = 0, double YPosition = 0,
                             NccSpot.NccBeamState State = NccSpot.NccBeamState.Unknown, DateTime StartTime = new DateTime(), DateTime EndTime = new DateTime());
    public record NccLogSpotTick(int LayerNumber = -1, string LayerID = "", double XPosition = 0, double YPosition = 0,
                                NccSpot.NccBeamState State = NccSpot.NccBeamState.Unknown, DateTime StartTime = new DateTime(), DateTime EndTime = new DateTime(), int Tick = 0);

    public record NccPlanSpot(int LayerNumber = -1, double LayerEnergy = 0, double LayerMU = 0, int LayerSpotCount = 0,
                              double Xposition = 0, double Yposition = 0, double Zposition = 0, double MonitoringUnit = 0);

    public record PgSpot(int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0);

    public record SpotMap(double X, double Y, double MU, double RangeDifference);
    public record BeamRangeMap(double X, double Y, double MU, double RangeDifference);
    public record GapPeakAndRange(double energy, double GapValue, double unknownValue);



    public class FunctionTestClass
    {
        public double getRange(int[] pgDistribution, double refRangePos)
        {
            // === Return data === //
            double range = -1;

            // === Algorithm === //
            // 0. Parameter setting: (1) xgrid_diff[69]  (2) algorithm: sigma_gaussFilt, cutoffLevel, offset, pitch, minPeakDistance, scope
            // 1. distribution reconstruction: 144 -> 72 -> 71
            // 2. apply gaussian kernel to 2nd derivative of distribution
            // 3. findpeaks
            // 4. select valid findpeaks value
            // 5. find peak valley
            // 6. get range

            #region 0. Parameter setting

            double[] xgrid_diff = new double[69];
            for (int i = 0; i < 69; i++)
            {
                xgrid_diff[i] = -102 + 3 * i;
            }

            double sigma_gaussFilt = 5;
            double cutoffLevel = 0.5;
            double offset = 0;
            double pitch = 3;
            double minPeakDistance = 10;
            double scope = 30;

            #endregion

            #region 1. PG distribution reconstruction: 144 -> 72 -> 71

            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            // 1-1. missing scintillator correction








            int[] cnt_row1 = new int[36];
            int[] cnt_row2 = new int[36];
            int[] cnt_row3 = new int[36];
            int[] cnt_row4 = new int[36];

            int[] cnt_top = new int[36];
            int[] cnt_bot = new int[36];

            int[] cnt_72ch = new int[72];
            double[] cnt_71ch = new double[71];

            for (int i = 0; i < 18; i++)
            {
                cnt_row1[i] = pgDistribution[89 - i];
                cnt_row2[i] = pgDistribution[107 - i];
                cnt_row3[i] = pgDistribution[125 - i];
                cnt_row4[i] = pgDistribution[143 - i];

                cnt_row1[i + 18] = pgDistribution[17 - i];
                cnt_row2[i + 18] = pgDistribution[35 - i];
                cnt_row3[i + 18] = pgDistribution[53 - i];
                cnt_row4[i + 18] = pgDistribution[71 - i];
            }

            for (int i = 0; i < 36; i++)
            {
                cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                cnt_top[i] = cnt_row1[i] + cnt_row2[i];
            }

            for (int i = 0; i < 36; i++)
            {
                cnt_72ch[2 * i] = cnt_bot[i];
                cnt_72ch[2 * i + 1] = cnt_top[i];
            }

            for (int i = 0; i < 71; i++)
            {
                cnt_71ch[i] = (cnt_72ch[i] + cnt_72ch[i + 1]) / 2;
                //Console.WriteLine($"{cnt_71ch[i]}");
            }

            #endregion

            #region 2. Apply gaussian kernel to 2nd derivative of 71 ch PG distribution

            double[] cnt_71ch_2ndDer = new double[69];
            for (int i = 0; i < 69; i++)
            {
                cnt_71ch_2ndDer[i] = -(cnt_71ch[i + 2] - cnt_71ch[i]) / (2 * pitch);
            }

            double[] cnt_71ch_2ndDer_gaussFilt = new double[69];
            #region 1. Generate Gaussian kernel

            double[] hcol = new double[9];
            double hcolSum = 0;

            double sigmaValue = sigma_gaussFilt / pitch;

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigmaValue)) * Math.Exp(-(Math.Pow(4 - i, 2) / (2 * (Math.Pow(sigmaValue, 2)))));
                hcolSum += hcol[i];
            }

            double normalizationFactor = 1 / hcolSum; // 수정 가능

            for (int i = 0; i < 9; i++)
            {
                hcol[i] = hcol[i] * normalizationFactor;
            }

            #endregion
            #region 2. Apply Gaussian kernel to cnt_71ch_2ndDer

            List<double> preConv_dist_unfilt = new List<double>();
            double dist_diff_temp = 0;

            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[0]);
            }
            for (int i = 0; i < 69; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                preConv_dist_unfilt.Add(cnt_71ch_2ndDer[68]);
            }

            for (int i = 0; i < 69; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dist_diff_temp += (preConv_dist_unfilt[i + j] * hcol[j]);
                }
                cnt_71ch_2ndDer_gaussFilt[i] = dist_diff_temp;
                dist_diff_temp = 0;
            }

            #endregion

            #endregion

            #region 3. findpeaks
            // modify later to https://www.cnblogs.com/sowhat4999/p/7050697.html
            DoubleVector secondDer = new DoubleVector(cnt_71ch_2ndDer_gaussFilt);
            PeakFinderRuleBased peakFind = new PeakFinderRuleBased(secondDer);

            peakFind.LocatePeaks();
            peakFind.ApplySortOrder(PeakFinderRuleBased.PeakSortOrder.Descending);

            List<Extrema> FoundPeaks = peakFind.GetAllPeaks();

            int index = 0;
            if (FoundPeaks.Count > 0)
            {
                while (true)
                {
                    if (index < FoundPeaks.Count())
                    {
                        FoundPeaks.RemoveAll(x => Math.Abs(x.X - FoundPeaks[index].X) < minPeakDistance && x.X != FoundPeaks[index].X);

                        #region Debugging
                        //Trace.WriteLine("");
                        //Trace.WriteLine("삭제 중"); int j = 1;
                        //foreach (var kk in FoundPeaks)
                        //{
                        //    Trace.WriteLine($"{j}. {kk.X}, {kk.Y}");
                        //    j++;
                        //}
                        #endregion

                        if (index < FoundPeaks.Count())
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Trace.WriteLine("Error: [getRange_ver4px - findpeaks]");
            }

            var locs = (from Pks in FoundPeaks
                        select Pks.X).ToArray();
            var pks = (from Pks in FoundPeaks
                       select Pks.Y).ToArray();

            #endregion

            #region 4. select valid findpeaks value

            List<int> indexList = new List<int>();
            List<double> peaksList = new List<double>();
            List<double> locsList = new List<double>();
            int validIndex = 0;

            foreach (var loc in locs)
            {
                if (3 * (loc - 34) > refRangePos - scope && 3 * (loc - 34) < refRangePos + scope)
                {
                    indexList.Add(validIndex);
                    peaksList.Add(pks[validIndex]);
                    locsList.Add(locs[validIndex]);
                }
                validIndex++;
            }

            double val_peak = -10000;
            int loc_peak = -10000;

            if (peaksList.Count() != 0 & validIndex > 0)
            {
                val_peak = peaksList.Max();
                loc_peak = (int)locsList[peaksList.IndexOf(val_peak)];
            }
            else
            {
                return -10000; // range
            }

            #endregion

            #region 5. find peak valley

            double[] cnt_71ch_2ndDer_reverse = new double[69];
            for (int i = 0; i < 69; i++)
            {
                cnt_71ch_2ndDer_reverse[i] = -cnt_71ch_2ndDer[i];
            }

            DoubleVector secondDer_reverse = new DoubleVector(cnt_71ch_2ndDer_reverse);
            PeakFinderRuleBased peakFind_reverse = new PeakFinderRuleBased(secondDer_reverse);

            peakFind_reverse.LocatePeaks();
            double[] distanceFromPeak = new double[peakFind_reverse.NumberPeaks];
            for (int i = 0; i < peakFind_reverse.NumberPeaks; i++)
            {
                distanceFromPeak[i] = loc_peak - peakFind_reverse[i].X;
            }

            double[] tempLeft = (from distance in distanceFromPeak
                                 where distance > 0
                                 select distance).ToArray();
            double[] tempRight = (from distance in distanceFromPeak
                                  where distance < 0
                                  select distance).ToArray();

            int LeftIndex, RightIndex;

            if (tempLeft.Length != 0)
            {
                LeftIndex = loc_peak - Convert.ToInt32(tempLeft.Last()); // 수정 2022-01-09 23:06
            }
            else
            {
                LeftIndex = 0;
            }

            if (tempRight.Length != 0)
            {
                RightIndex = loc_peak - Convert.ToInt32(tempRight[0]); // 수정 2022-01-09 23:06
            }
            else
            {
                RightIndex = 68;
            }

            double minValue_left = cnt_71ch_2ndDer_gaussFilt[LeftIndex];
            double minValue_right = cnt_71ch_2ndDer_gaussFilt[RightIndex];

            double bottomLevel = Math.Max(minValue_left, minValue_right);

            double baseline = bottomLevel + cutoffLevel * (val_peak - bottomLevel);

            #endregion

            #region 6. get range

            double sig_MR = new double();
            double sig_M = new double();

            for (int i = LeftIndex; i < RightIndex + 1; i++)
            {
                if (cnt_71ch_2ndDer_gaussFilt[i] - baseline > 0)
                {
                    sig_MR += (cnt_71ch_2ndDer_gaussFilt[i] - baseline) * xgrid_diff[i];
                    sig_M += cnt_71ch_2ndDer_gaussFilt[i] - baseline;
                }
            }

            range = (sig_MR / sig_M) + offset;

            #endregion

            return range;
        }
    }
}