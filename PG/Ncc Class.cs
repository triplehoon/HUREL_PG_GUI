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
        public NccBeamState BeamState { get; private set; }
        public DateTime BeamStartTime { get; private set; }
        public DateTime BeamEndTime { get; private set; } 
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
        public NccSpot(NccPlanSpot plan, NccLogSpot log) //
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
        public void ChangNccBeamState(NccBeamState state)
        {
            BeamState = state;
        }
    }

    public class NccLayer : Layer
    {
        private List<NccSpot> spots;
        public int BeamStateNumber { get; private set; }
        public bool IsLayerValid { get; private set; }

        public NccLayer(string recordFileDir, string SpecifFileDir, double para1, double para2)
        {
            spots = new List<NccSpot>();

            spots.Add(new NccSpot());
            IsLayerValid = false;
        }
        public NccSpot.NccBeamState NccBeamState { get; private set; }
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
    }

    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession:Session
    {
        private static double ToleranceDistBetweenPlanAndLogSpot = 3;

        private List<NccLayer> layers = new List<NccLayer>(); //////////////
        public List<NccLayer> Layers
        { 
            get 
            { 
                return layers; 
            } 
        }

        private NccPlan plan = new NccPlan("");

        private DateTime firstLayerFirstSpotLogTime;

        public bool IsPlanLoad { get; private set; }

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

            foreach(var chklayer in layers)
            {
                var layerInfo = getLayerIdFromLogFileName(recordFileDir);
                if (chklayer.LayerId == layerInfo)
                {
                    Debug.WriteLine("Layer file is already loaded");
                    return true;
                }
            }
            #endregion

            (List<NccLogSpot> logSpots, int logLayerNumber, NccSpot.NccBeamState state) = getLogSpotData(recordFileDir, SpecifFileDir);
            (List<NccPlanSpot> planSpots, double planLayerEnergy) = plan.GetPlanSpotsByLayerNumber(logLayerNumber);
            List<NccSpot> nccSpot = mergePlanLog(logSpots, planSpots);

            NccLayer layer = new NccLayer(nccSpot, logLayerNumber, planLayerEnergy);
            layers = insertLayer(layers, layer);

            return true;
        }

        #region Load Layer files
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
        private List<NccLayer> insertLayer(List<NccLayer> layers, NccLayer nccLayer)
        {
            NccSpot.NccBeamState state = nccLayer.GetSingleLogInfo();

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
        private (List<NccLogSpot>, int, NccSpot.NccBeamState) getLogSpotData(string recordFileDir, string SpecifFileDir)
        {
            // Return
            
            
            List<NccLogSpot> logSpots = new List<NccLogSpot>();

            NccSpot.NccBeamState state;
            int layerNumber;
            string layerId;

            (state, layerNumber, layerId) = checkByLogFileName(recordFileDir);

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

                        logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * logParameter.coeff_y), ((tempxPosition - data_speicf.icxOffset) * logParameter.coeff_x), state, tempstartEpochTime, tempendEpochTime));
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

                    logSpots.Add(new NccLogSpot(templayerNumber, layerId, ((tempyPosition - data_speicf.icyOffset) * logParameter.coeff_y), ((tempxPosition - data_speicf.icxOffset) * logParameter.coeff_x), state, tempstartEpochTime, tempendEpochTime));
                    spotContinue = false;
                }
            }

            return (logSpots, layerNumber, state);
        }
        private (NccSpot.NccBeamState, int, string) checkByLogFileName(string Dir)
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
            layerId = getLayerIdFromLogFileName(Dir);
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
        public string? PlanFile { get; private set; }      
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

    public record NccLogSpot(int LayerNumber = -1, string LayerID = "", double XPosition = 0, double YPosition = 0,
                                        NccSpot.NccBeamState State = NccSpot.NccBeamState.Unknown, DateTime StartTime = new DateTime(), DateTime EndTime = new DateTime());
    
    public record NccPlanSpot(int LayerNumber = -1, double LayerEnergy = 0, double LayerMU = 0, int LayerSpotCount = 0,
                              double Xposition = 0, double Yposition =0, double Zposition = 0, double MonitoringUnit = 0);
    

}
