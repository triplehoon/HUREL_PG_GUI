using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterSpace.NMath.Core;

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
        public bool IsPgDataSet { get; private set; }

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
            ChannelCount = new int[144];
            SumCounts = 0;
            TriggerStartTime = 0;
            TriggerEndTime = 0;
            ADC = 0;
            IsPgDataSet = false;
        }

        public void SetPgData(MultiSlitPg pg)
        {
            ChannelCount = pg.ChannelCount;
            SumCounts = pg.SumCounts;
            TriggerStartTime = pg.TriggerStartTime;
            TriggerEndTime = pg.TriggerEndTime;
            ADC = pg.ADC;
        }
        public enum NccBeamState
        {
            Tuning = 0,
            Normal = 1, 
            Resume = 2, 
            Unknown = 3
        }
        public void ChangeNccBeamState(NccBeamState state)
        {
            BeamState = state;
        }
    }

    public class NccLayer : Layer
    {
        private static double ToleranceDistBetweenPlanAndLogSpot = 3;
        public NccLayer(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y, NccPlan plan)
        {
            logSpots = new List<NccLogSpot>();
            if (!LoadLogFile(recordFileDir, SpecifFileDir, coeff_x, coeff_y))
            {
                IsLayerValid = false;
            };
            (List<NccPlanSpot> planSpots, double planLayerEnergy) = plan.GetPlanSpotsByLayerNumber(LayerNumber);
            spots = new List<NccSpot>();

            foreach (NccLogSpot logSpot in logSpots)
            {
                foreach (NccPlanSpot planSpot in planSpots)
                {
                    double distance = Math.Sqrt(Math.Pow(logSpot.XPosition - planSpot.Xposition, 2) + Math.Pow(logSpot.YPosition - planSpot.Yposition, 2));
                    if (distance <= ToleranceDistBetweenPlanAndLogSpot)
                    {
                        spots.Add(new NccSpot(planSpot, logSpot));
                        break;
                    }
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
            return true;
        }
        #endregion
    }

    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession
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
        private List<MultiSlitPg> multiSlitPgs = new List<MultiSlitPg>();
        public List<MultiSlitPg> MultiSlitPgs
        {
            get
            {
                return multiSlitPgs;
            }
            private set
            {
                multiSlitPgs = value;

            }
        }
        public string PgFileName { get; private set; }
        
        private DateTime firstLayerFirstSpotLogTime;

        public bool IsPlanLoad { get; private set; }
        public bool IsPgLoad { get; private set; }
        public bool IsConfigLogFileLoad { get; private set; }

        public bool IsGetReferenceTime { get; private set; }


        public NccSession()
        {
            PgFileName = "";
            IsPlanLoad = false;
            IsPgLoad = false;
            IsConfigLogFileLoad = false;
            IsGetReferenceTime = false;            
        }


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
                    IsPlanLoad = false;
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

            Layers.Add(loadedLayer);
            Layers.Sort(SortLayer);
            


            return true;
        }
        static private int SortLayer(NccLayer layer1, NccLayer layer2)
        {
            if (layer1.PartNumber < layer2.PartNumber)
            {
                return -1;
            }
            if (layer1.PartNumber > layer2.PartNumber)
            {
                return 1;
            }

            if (layer1.LayerNumber < layer2.LayerNumber)
            {
                return -1;
            }
            if (layer1.LayerNumber > layer2.LayerNumber)
            {
                return 1;
            }

            if (layer1.NccBeamState < layer2.NccBeamState)
            {
                return -1;
            }
            if (layer1.NccBeamState > layer2.NccBeamState)
            {
                return 1;
            }

            if (layer1.BeamStateNumber < layer2.BeamStateNumber)
            {
                return -1;
            }
            if (layer1.BeamStateNumber > layer2.BeamStateNumber)
            {
                return 1;
            }
            return 0;

        }
        public bool LoadPgFile(string pgDir)
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
            multiSlitPgs = MultiSlitPg.LoadbinaryFile(pgDir);

            if (multiSlitPgs.Count != 0)
            {
                PgFileName = pgDir;
                IsPgLoad = true;
                return true;
            }
            else
            {
                IsPgLoad = false;
                return false;
            }
        }

        #region private functions
     
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
                        if (tempString.Length != 5 || tempString[0] != "Layer")
                        {
                            throw new Exception("Not a 3D pld");
                        }
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

  
    public record NccLogSpot(int LayerNumber = -1, string LayerID = "", double XPosition = 0, double YPosition = 0,
                             NccSpot.NccBeamState State = NccSpot.NccBeamState.Unknown, DateTime StartTime = new DateTime(), DateTime EndTime = new DateTime());

    public record NccPlanSpot(int LayerNumber = -1, double LayerEnergy = 0, double LayerMU = 0, int LayerSpotCount = 0,
                              double Xposition = 0, double Yposition = 0, double Zposition = 0, double MonitoringUnit = 0);

    public record MultiSlitPg(int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0)
    {        
        public static List<MultiSlitPg> LoadbinaryFile(string pgDir)
        {
            List<MultiSlitPg> spots = new List<MultiSlitPg>();
            if (pgDir != null && pgDir != "")
            {
                string PGDir = pgDir!;
                if(!Path.IsPathFullyQualified(PGDir))
                {
                    return new List<MultiSlitPg>();
                }
                using BinaryReader br = new BinaryReader(File.Open(PGDir, FileMode.Open));
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

                                MultiSlitPg pgSpot = new MultiSlitPg(ChannelCount, SumCounts, TriggerStartTime, TriggerEndTime, ADC);
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
            return spots;
        }
        public static MultiSlitPg MergePgs(List<MultiSlitPg> list)
        {
            if (list.Count == 0)
            {
                return new MultiSlitPg(new int[0], 0, 0, 0, 0, 0);
            }
            int[] channelCount = list[0].ChannelCount;
            int sumCounts = list[0].SumCounts;
            int triggerStartTime = list[0].TriggerStartTime;
            int triggerEndTime = list[list.Count].TriggerEndTime;
            double adc = list[0].ADC;
            int tick = list[0].Tick;
            for(int i = 1; i < list.Count; ++i)
            {
                
                for (int j = 0; j < channelCount.Length; ++j)
                {
                    channelCount[j] += list[i].ChannelCount[j];
                }
                sumCounts += list[i].SumCounts;
            }

            return new MultiSlitPg(channelCount, sumCounts, triggerStartTime, triggerEndTime, adc, tick);
        }
        public double GetRange(double refRangePos)
        {

            int[] pgDistribution = ChannelCount;
            
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