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
        public NccSpot spot { get; }

        // LogData.Add(new LogStruct_NCC(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * logParameter.coeff_y), ((tempxPosition - data_speicf.icxOffset) * logParameter.coeff_x), state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));        
        public NccBeamState BeamState { get; private set; }
        public DateTime BeamStartTime { get; private set; }
        public DateTime BeamEndTime { get; private set; } 

        // Jaerin Add (Temp)
        public double XPosition { get; private set; }
        public double YPosition { get; private set; }
        public int LayerNumber { get; private set; }
        public string LayerId { get; private set; }
        

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

         
        public NccSpot()
        {
            
        }
        // PG add

        public NccSpot(NccPlanSpot plan, LogStruct_NCC log) //
        {
            spot = new NccSpot();

            spot.planSpot = plan;

            spot.BeamStartTime = log.StartTime;
            spot.BeamEndTime = log.EndTime;
            spot.BeamState = log.State;
            spot.LayerNumber = log.LayerNumber;
            spot.LayerId = log.LayerID;
            spot.XPosition = log.XPosition;
            spot.YPosition = log.YPosition;
        }
        //public NccSpot(NccPlanSpot plan)
        //{

        //}
        public NccSpot(string logLayerId)
        {
            //LogLayerId = logLayerId;
        }
        public enum NccBeamState
        {
            Normal, Tuning, Resume, Unknown
        }        
        public void ChangNccBeamState(NccBeamState state)
        {
            //State = state;
        }
    }




    public class NccLayer : Layer
    {
        private List<NccSpot> spots;
        public NccLayer(List<NccSpot> spot, int layerNumber, double planEnergy)
        {
            if (spot.Count != 0)
            {
                spots = spot;
                setLayerProperty(layerNumber, planEnergy);
            }

        }

        //public bool LogInfo(XdrDataRecorderRpcLayerConverter convertInfo)
        public bool LogInfo(XdrConverter_Record convertInfo)
        {
            if (convertInfo.ErrorCheck)
            {
                return false;
            }
            
            return true;
        }

        public NccSpot.NccBeamState GetSingleLogInfo(NccLayer nccLayer)
        {
            if (spots.Count != 0)
            {
                return spots[0].BeamState;
            }
            else
            {
                return NccSpot.NccBeamState.Unknown;
            }
        }

        public List<NccSpot> GetSpot()
        {
            return spots;
        }

        private int setLayerNumber(int layerNumber)
        {
            return layerNumber;
        }
    }
    
    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession:Session
    {
        static double ToleranceDistBetweenPlanAndLogSpot = 3;
        static void ChangetestValue(int value)
        {
            ToleranceDistBetweenPlanAndLogSpot = value;
        }


        private List<NccLayer> layers = new List<NccLayer>(); //////////////

        private NccPlan plan = new NccPlan("");

        private DateTime firstLayerFirstSpotLogTime;

        public string? LogFile { get; private set; } // added by jaerin

        public bool IsPlanLoad { get; private set; }

        public bool IsConfigLogFileLoad { get; private set; }

        public bool IsGetReferenceTime { get; private set; }
                
        public record LogStruct_NCC(int LayerNumber, string LayerID, double XPosition, double YPosition,
                                          NccSpot.NccBeamState State, DateTime StartTime, DateTime EndTime);

        private LogParameter logParameter = new LogParameter();


        
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
                NccPlan tempPlan = new NccPlan(planFileDir);
                plan = tempPlan;
                IsPlanLoad = true;

                return true;
            }
            else if (planFileDir == "")
            {
                //MessageBox.Show($"Plan_NCC File Load Canceled", $"Plan_NCC File Load Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //MessageBox.Show($"InValid Data Extension", $"Plan_NCC File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            

            return false;
        }

        public bool LoadConfigLogFile(string configFileDir)
        {
            // Log Config File Example
            // # SAD - M-id 21900
            // GTR3-PBS;SAD;DOUBLE;2;A;;;SAD parameter (X, Y);1915.8, 2300.2
            // GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X, Y);1148.16, 1202.49

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

            if (getLayerIdFromLogFileName(recordFileDir) != getLayerIdFromLogFileName(SpecifFileDir))
            {
                Debug.Assert(true, $"LayerId(record) != LayerId(specif)");
                return false;
            }

            #endregion

            (List<LogStruct_NCC> logSpots, int logLayerNumber, NccSpot.NccBeamState state) = getLogSpotData(recordFileDir, SpecifFileDir);
            (List<NccPlanSpot> planSpots, double planLayerEnergy) = plan.GetPlanSpotsByLayerNumber(logLayerNumber);
            List<NccSpot> nccSpot = MergePlanLog(logSpots, planSpots);

            NccLayer layer = new NccLayer(nccSpot, logLayerNumber, planLayerEnergy);
            layers = InsertSpotData(layers, layer);

            return true;
        }

        public List<NccLayer> GetLayerInfo()
        {
            return layers;
        }



        private List<NccSpot> MergePlanLog(List<LogStruct_NCC> logSpots, List<NccPlanSpot> planSpots)
        {
            List<NccSpot> nccSpot = new List<NccSpot>();
            double layerEnergy = planSpots[0].LayerEnergy;

            foreach (LogStruct_NCC logSpot in logSpots)
            {
                foreach (NccPlanSpot planSpot in planSpots)
                {
                    double distance = Math.Sqrt(Math.Pow(logSpot.XPosition - planSpot.Xposition, 2) + Math.Pow(logSpot.YPosition - planSpot.Yposition, 2));
                    if (distance <= ToleranceDistBetweenPlanAndLogSpot)
                    {
                        nccSpot.Add((new NccSpot(planSpot, logSpot)).spot);
                        break;
                    }
                }
            }

            return nccSpot;
        }

        private List<NccLayer> InsertSpotData(List<NccLayer> layers, NccLayer nccLayer)
        {
            NccSpot.NccBeamState state = nccLayer.GetSingleLogInfo(nccLayer);

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

        private (List<LogStruct_NCC>, int, NccSpot.NccBeamState) getLogSpotData(string recordFileDir, string SpecifFileDir)
        {
            // Return
            List<LogStruct_NCC> logSpots = new List<LogStruct_NCC>();

            NccSpot.NccBeamState state;
            int layerNumber;
            string layerId;

            (state, layerNumber, layerId) = CheckByLogFileName(recordFileDir);

            Stream xdrConverter_speicf = File.Open(SpecifFileDir, FileMode.Open);
            var data_speicf = new XdrConverter_Specific(xdrConverter_speicf);

            Stream xdrConverter_record = File.Open(recordFileDir, FileMode.Open);
            var data_record = new XdrConverter_Record(xdrConverter_record);

            // Add spot data
            if (data_record.ErrorCheck == false)
            {
                List<float> xPositions = new List<float>();
                List<float> yPositions = new List<float>();

                List<Int64> epochTime = new List<Int64>();

                bool spotContinue = false;

                //List<LogStruct_NCC> spots = new List<LogStruct_NCC>();

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

                        logSpots.Add(new LogStruct_NCC(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * logParameter.coeff_y), ((tempxPosition - data_speicf.icxOffset) * logParameter.coeff_x), state, tempstartEpochTime, tempendEpochTime));
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

                    logSpots.Add(new LogStruct_NCC(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * logParameter.coeff_y), ((tempxPosition - data_speicf.icxOffset) * logParameter.coeff_x), state, tempstartEpochTime, tempendEpochTime));
                    spotContinue = false;
                }
            }

            return (logSpots, layerNumber, state);
        }

        private string getLayerIdFromLogFileName(string LogDir)
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

        private (NccSpot.NccBeamState, int, string) CheckByLogFileName(string Dir)
        {
            string fileName = Path.GetFileNameWithoutExtension(Dir);

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

            return (state, layerNumber, layerId);
        }

        private struct LogParameter
        {
            public double coeff_x;
            public double coeff_y;
            public DateTime TimeREF; // 나중에 쓰일것
        }    
    }



    public class NccPlan
    {
        private List<NccPlanSpot> spots = new List<NccPlanSpot>();
        public NccPlan(string planFile)
        {
            if (planFile != null & planFile != "")
            {
                PlanFile = planFile;

                using (FileStream fs = new FileStream(planFile, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        string lines = new string("");
                        string[] tempString = new string[0];

                        int TempLayerNumber = 0;

                        tempString = sr.ReadLine().Split(",");

                        double LayerEnergy = Convert.ToDouble(tempString[2]);
                        double LayerMU = Convert.ToDouble(tempString[3]);
                        int LayerSpotCount = Convert.ToInt32(tempString[4]) / 2;

                        while ((lines = sr.ReadLine()) != null)
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

                                tempPlanSpot.LayerEnergy = LayerEnergy;
                                tempPlanSpot.LayerMU = LayerMU;
                                tempPlanSpot.LayerSpotCount = LayerSpotCount;
                                tempPlanSpot.LayerNumber = TempLayerNumber;

                                tempPlanSpot.Xposition = Convert.ToDouble(tempString[0]);
                                tempPlanSpot.Yposition = Convert.ToDouble(tempString[1]);
                                tempPlanSpot.Zposition = Convert.ToDouble(tempString[2]);
                                tempPlanSpot.MonitoringUnit = Convert.ToDouble(tempString[3]);

                                spots.Add(tempPlanSpot);
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
        public string PlanFile { get; private set; }
        
        public NccPlan GetPlanByLayerNumber(int layerNumber)
        {
            return null;
        }

        public (List<NccPlanSpot>, double layerEnergy) GetPlanSpotsByLayerNumber(int layerNumber)
        {
            List<NccPlanSpot> planSpotSingleLayer = (from planSpots in spots
                                                     where planSpots.LayerNumber == layerNumber
                                                     select planSpots).ToList();
            double layerEnergy = planSpotSingleLayer.Last().LayerEnergy;

            return (planSpotSingleLayer, layerEnergy);
        }

        public double TotalPlanMonitoringUnit { get; }            
        
        public int TotalPlanLayer { get; }
    }



    public struct NccPlanSpot
    {
        public int LayerNumber;
        public double LayerEnergy;
        public double LayerMU;
        public int LayerSpotCount;

        public double Xposition;
        public double Yposition;
        public double Zposition;
        public double MonitoringUnit;
    }
}
