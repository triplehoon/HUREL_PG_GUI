using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HUREL.PG.Ncc.NccSession;

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

        private NccPlanSpot planSpot;
        public NccPlanSpot PlanSpot
        {
            get
            {
                if (BeamState == NccBeamState.Tuning)
                {
                    Debug.Assert(true, "Tuning beam has no Plan info");
                    return new NccPlanSpot();
                }
                else
                {
                    return planSpot;
                }
            }
            private set
            {
                planSpot = value;
            }
        }
        public NccSpot(NccPlanSpot plan, NccLogSpot log)
        {
            planSpot = plan;

            BeamStartTime = log.StartTime;
            BeamEndTime = log.EndTime;
            BeamState = log.State;
            LayerNumber = log.LayerNumber;
            LayerId = log.LayerID;
            XPosition = log.XPosition;
            YPosition = log.YPosition;
        }
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
        public NccLayer(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y, NccPlan plan)
        {
            (List<NccLogSpot> logSpots, int layerNumber, string layerId, NccSpot.NccBeamState state) = LoadLogFile(recordFileDir, SpecifFileDir, coeff_x, coeff_y);
            (List<NccPlanSpot> planSpots, double planLayerEnergy) = plan.GetPlanSpotsByLayerNumber(layerNumber);
            spots = mergePlanLog(logSpots, planSpots);

            SetLayerEnergy(planLayerEnergy);
            SetLayerNumber(layerNumber);
            SetLayerId(layerId);

            BeamStateNumber = -999999; // use?
            NccBeamState = state;

            IsLayerValid = true; // purpose?
        }

        #region Properties

        private List<NccSpot> spots;

        private static double ToleranceDistBetweenPlanAndLogSpot = 3;
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
        public bool IsLayerValid { get; private set; } // purpose?

        #endregion

        public List<NccSpot> GetSpot()
        {
            return spots;
        }
        public static string GetLayerIdFromLogFileName(string LogDir)
        {
            // Return
            string LayerId = "";

            string LogName = Path.GetFileNameWithoutExtension(LogDir);
            int layerNumber = Convert.ToInt32(LogName.Split('_')[4]);

            if (LogName.Contains("record"))
            {
                int index = LogName.IndexOf("record");
                LayerId = LogName.Substring(index + "record".Length + 1);
            }
            else if (LogName.Contains("specif"))
            {
                int index = LogName.IndexOf("specif");
                LayerId = LogName.Substring(index + "specif".Length + 1);
            }
            else
            {
                Debug.Assert(true, $"Invalid Log File Name");
            }

            return LayerId;
        }

        #region private functions
        private (NccSpot.NccBeamState, int, string) getInfoFromLogFileName(string LogDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(LogDir);

            // Return
            NccSpot.NccBeamState state;
            int layerNumber = Convert.ToInt32(fileName.Split('_')[4]);
            string layerId;

            if (fileName.Contains("_part_"))
            {
                // 0: 20220601(date) && 1: 182105(time) && 2: 461(usec).map && 3: record && 4: LayerNumber(xxxx) && 5: "part" && 6: PartNumber(xx) && 7: "tuning" or "resume" && 8: TuningNumber or ResumeNumber

                int PartNumber = Convert.ToInt32(fileName.Split('_')[6]);

                if (fileName.Contains("tuning"))
                {
                    int TuningNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + TuningNumber.ToString();
                    state = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    int ResumeNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + ResumeNumber.ToString();
                    state = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString();
                    state = NccSpot.NccBeamState.Normal;
                }
            }
            else
            {
                if (fileName.Contains("tuning"))
                {
                    layerId = layerNumber.ToString() + "_tuning_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    state = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    layerId = layerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    state = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = Convert.ToString(Convert.ToInt32(fileName.Split('_')[4]));
                    state = NccSpot.NccBeamState.Normal;
                }
            }
            //layerId = getLayerIdFromLogFileName(Dir);
            return (state, layerNumber, layerId);
        }
        private (List<NccLogSpot>, int, string, NccSpot.NccBeamState) LoadLogFile(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y)
        {
            List<NccLogSpot> logSpots = new List<NccLogSpot>();

            NccSpot.NccBeamState state;
            int layerNumber;
            string layerId;

            (state, layerNumber, layerId) = getInfoFromLogFileName(recordFileDir);

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

                    logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * coeff_y), ((tempxPosition - data_speicf.icxOffset) * coeff_x), state, tempstartEpochTime, tempendEpochTime));
                    spotContinue = false;
                }
            }
            #endregion

            return (logSpots, layerNumber, layerId, state);
        }
        private List<NccSpot> mergePlanLog(List<NccLogSpot> logSpots, List<NccPlanSpot> planSpots)
        {
            List<NccSpot> nccSpot = new List<NccSpot>();
            double layerEnergy = planSpots[0].LayerEnergy;

            foreach (NccLogSpot logSpot in logSpots)
            {
                foreach (NccPlanSpot planSpot in planSpots)
                {
                    double distance = Math.Sqrt(Math.Pow(logSpot.XPosition - planSpot.Xposition, 2) + Math.Pow(logSpot.YPosition - planSpot.Yposition, 2));
                    if (distance <= ToleranceDistBetweenPlanAndLogSpot)
                    {
                        nccSpot.Add(new NccSpot(planSpot, logSpot));
                        break;
                    }
                }
            }

            return nccSpot;
        }
        #endregion
    }

    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession : Session
    {
        private List<NccLayer> layers = new List<NccLayer>();
        public List<NccLayer> Layers
        {
            get
            {
                return layers;
            }
        }

        private NccPlan plan = new NccPlan("");

        public List<PGSpot> PGspots = new List<PGSpot>(); ///////////////////// temp



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
        public bool LoadPlanFile(string planFileDir)
        {
            if (planFileDir.EndsWith("pld") || planFileDir.EndsWith("txt"))
            {
                try
                {
                    plan = new NccPlan(planFileDir);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }

                IsPlanLoad = true;

                return true;
            }
            else if (planFileDir == "")
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
        public bool LoadRecordSpecifLogFile(string recordFileDir, string SpecifFileDir)
        {
            #region Check loaded log files whether valid or invalid

            if (!File.Exists(recordFileDir))
            {
                Debug.Assert(true, $"Log(record) file doesn't exist");
                return false;
            }

            if (!File.Exists(SpecifFileDir))
            {
                Debug.Assert(true, $"Log(specif) file doesn't exist");
                return false;
            }

            if (IsPlanLoad == false)
            {
                Debug.Assert(true, $"Plan is not loaded");
                return false;
            }

            if (IsConfigLogFileLoad == false)
            {
                Debug.Assert(true, $"Log(config) is not loaded");
                return false;
            }

            if (!recordFileDir.Contains("map_record_"))
            {
                Debug.Assert(true, $"Log(record) is not invalid");
                return false;
            }

            if (!SpecifFileDir.Contains("map_specif_"))
            {
                Debug.Assert(true, $"Log(specif) is not invalid");
                return false;
            }

            if (NccLayer.GetLayerIdFromLogFileName(recordFileDir) != NccLayer.GetLayerIdFromLogFileName(SpecifFileDir))
            {
                Debug.Assert(true, $"LayerId(record) != LayerId(specif)");
                return false;
            }

            foreach (var chklayer in layers)
            {
                (var beamState, var layerNumber, var layerId) = getInfoFromLogFileName(recordFileDir);
                if (chklayer.LayerId == layerId)
                {
                    Debug.WriteLine("Layer file is already loaded");
                    return true;
                }
            }
            #endregion

            NccLayer layer = new NccLayer(recordFileDir, SpecifFileDir, logParameter.coeff_x, logParameter.coeff_y, plan);
            layers = insertLayer(layers, layer);

            return true;
        }

        public bool LoadPGFile(string pgDir)
        {
            #region Check Valid
            if (!File.Exists(pgDir))
            {
                Debug.Assert(true, $"PG file doesn't exist");
                return false;
            }

            if (!Equals(Path.GetExtension(pgDir), ".bin"))
            {
                Debug.Assert(true, $"Invalid file extension");
                return false;
            }
            #endregion

            PG pg = new PG(pgDir);
            PGspots = pg.GetPGSpots();

            IsPGLoad = true;

            return true;
        }


        #region private functions
        private List<NccLayer> insertLayer(List<NccLayer> layers, NccLayer nccLayer)
        {
            //NccSpot.NccBeamState state = nccLayer.GetSingleLogInfo();
            NccSpot.NccBeamState state = nccLayer.NccBeamState;

            List<int> LayerNumbers = new List<int>(layers.Count);
            foreach (NccLayer layer in layers)
            {
                LayerNumbers.Add(layer.LayerNumber);
            }

            if (layers.Count == 0)
            {
                layers.Add(nccLayer);
            }
            else
            {
                int insertIndex = 0;
                if (LayerNumbers.Any(x => x == nccLayer.LayerNumber))
                {
                    insertIndex = LayerNumbers.FindIndex(x => x == nccLayer.LayerNumber);

                    List<NccSpot> nccSpot = layers[insertIndex].GetSpot();
                    nccSpot.AddRange(nccLayer.GetSpot());
                    nccSpot.OrderBy(x => x.BeamStartTime);
                }
                else
                {
                    insertIndex = LayerNumbers.Where(x => x < nccLayer.LayerNumber).Count();
                    layers.Insert(insertIndex, nccLayer);
                }
            }

            return layers;
        }
        private (NccSpot.NccBeamState, int, string) getInfoFromLogFileName(string LogDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(LogDir);

            // Return
            NccSpot.NccBeamState state;
            int layerNumber = Convert.ToInt32(fileName.Split('_')[4]);
            string layerId;

            if (fileName.Contains("_part_"))
            {
                // 0: 20220601(date) && 1: 182105(time) && 2: 461(usec).map && 3: record && 4: LayerNumber(xxxx) && 5: "part" && 6: PartNumber(xx) && 7: "tuning" or "resume" && 8: TuningNumber or ResumeNumber

                int PartNumber = Convert.ToInt32(fileName.Split('_')[6]);

                if (fileName.Contains("tuning"))
                {
                    int TuningNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + TuningNumber.ToString();
                    state = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    int ResumeNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + ResumeNumber.ToString();
                    state = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = layerNumber.ToString() + "_part_" + PartNumber.ToString();
                    state = NccSpot.NccBeamState.Normal;
                }
            }
            else
            {
                if (fileName.Contains("tuning"))
                {
                    layerId = layerNumber.ToString() + "_tuning_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    state = NccSpot.NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    layerId = layerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    state = NccSpot.NccBeamState.Resume;
                }
                else
                {
                    layerId = Convert.ToString(Convert.ToInt32(fileName.Split('_')[4]));
                    state = NccSpot.NccBeamState.Normal;
                }
            }
            //layerId = getLayerIdFromLogFileName(Dir);
            return (state, layerNumber, layerId);
        }
        #endregion

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
        public NccPlan(string planFile)
        {
            if (planFile != null & planFile != "")
            {
                PlanFile = planFile;

                using (FileStream fs = new FileStream(planFile!, FileMode.Open))
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
                                                         Convert.ToDouble(tempString[1]), Convert.ToDouble(tempString[2]), Convert.ToDouble(tempString[3])));
                            }
                        }
                    }
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
    }

    public class PG
    {
        public string? PGDir { get; private set; }

        private List<PGSpot> spots = new List<PGSpot>();
        public PG(string pgDir)
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

                                PGSpot pgSpot = new PGSpot(ChannelCount, SumCounts, TriggerStartTime, TriggerEndTime, ADC);
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

        public List<PGSpot> GetPGSpots()
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

    public record NccPlanSpot(int LayerNumber = -1, double LayerEnergy = 0, double LayerMU = 0, int LayerSpotCount = 0,
                              double Xposition = 0, double Yposition = 0, double Zposition = 0, double MonitoringUnit = 0);

    public record PGSpot(int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0);




}