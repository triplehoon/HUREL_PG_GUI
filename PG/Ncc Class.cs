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
        public bool LoadPGFile(string pgDir)
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
            pgData = new NccMultislitPg(pgDir);

            IsPGLoad = true;

            return true;
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

    public class NccMultislitPg
    {
        public string? PGDir { get; private set; }

        private List<PgSpot> spots = new List<PgSpot>();
        public NccMultislitPg(string pgDir)
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

    public record NccPlanSpot(int LayerNumber = -1, double LayerEnergy = 0, double LayerMU = 0, int LayerSpotCount = 0,
                              double Xposition = 0, double Yposition = 0, double Zposition = 0, double MonitoringUnit = 0);

    public record PgSpot(int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0);





}