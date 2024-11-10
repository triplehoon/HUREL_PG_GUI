using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterSpace.NMath.Core;

namespace HUREL.PG.NccHelper
{
    public enum NccBeamState
    {
        Tuning = 0,
        Normal = 1,
        Resume = 2,
        Unknown = 3
    }
    public class NccLayer : Layer
    {
        public NccLayer(string recordFileDir, string SpecifFileDir, double coeff_x, double coeff_y)
        {
            logSpots = new List<NccLogSpot>();
            if (!LoadLogFile(recordFileDir, SpecifFileDir, coeff_x, coeff_y))
            {
                IsLayerValid = false;
            };
        }

        #region Properties
        private List<NccLogSpot> logSpots;
        public List<NccLogSpot> LogSpots
        {
            get { return logSpots; }
        }

        public override string? LayerId
        {
            get
            {
                // different by beam state
                if (NccBeamState == NccBeamState.Normal)
                {
                    return LayerNumber.ToString();
                }
                else if (NccBeamState == NccBeamState.Tuning)
                {
                    return LayerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + BeamStateNumber.ToString();
                }
                else if (NccBeamState == NccBeamState.Resume)
                {
                    return LayerNumber.ToString() + "_part_" + PartNumber.ToString() + "_resume_" + BeamStateNumber.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public int BeamStateNumber { get; private set; } // not appropriate when part exist

        public NccBeamState NccBeamState { get; private set; }
        public bool IsLayerValid { get; private set; }
        public int PartNumber { get; private set; }

        // ex) tuning 1, 2, resume 1, 2
        public int OrderNumber { get; private set; }
        #endregion

        // ToString
        public override string ToString()
        {
            // LayerNumber, PartNumber, BeamStateNumber, LayerId, NccBeamState
            return LayerId + ", SpotCount: " + this.LogSpots.Count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="LogDir"></param>
        /// <returns>isValid, layerNumber, partNumber, beamStateNumber, layerId, beamState</returns>
        private static (bool, int, int, int, string, NccBeamState) GetInfoFromLogFileName(string LogDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(LogDir);

            if (Path.GetExtension(LogDir) != ".xdr")
            {
                return (false, 0, 0, 0, "", NccBeamState.Unknown);
            }
            // Return
            int layerNumber = Convert.ToInt32(fileName.Split('_')[4]);
            int partNumber = 1;
            string layerId;
            int beamStateNumber = 1;

            NccBeamState nccBeamState = NccBeamState.Unknown;

            if (fileName.Contains("_part_"))
            {
                // 0: 20220601(date) && 1: 182105(time) && 2: 461(usec).map && 3: record && 4: LayerNumber(xxxx) && 5: "part" && 6: PartNumber(xx) && 7: "tuning" or "resume" && 8: TuningNumber or ResumeNumber

                partNumber = Convert.ToInt32(fileName.Split('_')[6]);

                if (fileName.Contains("tuning"))
                {
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString() + "_tuning_" + beamStateNumber.ToString();
                    nccBeamState = NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[8]);

                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString() + "_tuning_" + beamStateNumber.ToString();
                    nccBeamState = NccBeamState.Resume;
                }
                else
                {
                    layerId = layerNumber.ToString() + "_part_" + partNumber.ToString();
                    nccBeamState = NccBeamState.Normal;
                }

            }
            else
            {
                if (fileName.Contains("tuning"))
                {
                    layerId = layerNumber.ToString() + "_tuning_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[6]);
                    nccBeamState = NccBeamState.Tuning;
                }
                else if (fileName.Contains("resume"))
                {
                    layerId = layerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(fileName.Split('_')[6]));
                    beamStateNumber = Convert.ToInt32(fileName.Split('_')[6]);
                    nccBeamState = NccBeamState.Resume;
                }
                else
                {
                    layerId = Convert.ToString(Convert.ToInt32(fileName.Split('_')[4]));
                    nccBeamState = NccBeamState.Normal;
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
            NccBeamState state;

            (isValidId, layerNumber, partNumber, beamStateNumber, layerId, state) = GetInfoFromLogFileName(recordFileDir);

            if (!isValidId)
            {
                return false;
            }

            LayerNumber = layerNumber + 1;
            PartNumber = partNumber;
            BeamStateNumber = beamStateNumber;
            NccBeamState = state;
            XdrConverter_Specific data_speicf;
            XdrConverter_Record data_record;
            try
            {
                Stream xdrConverter_speicf = File.Open(SpecifFileDir, FileMode.Open);
                data_speicf = new XdrConverter_Specific(xdrConverter_speicf);

                Stream xdrConverter_record = File.Open(recordFileDir, FileMode.Open);
                data_record = new XdrConverter_Record(xdrConverter_record);
            }
            catch (IOException ioException)
            {
                Debug.WriteLine(ioException);
                return false;
            }

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
    public class Ncc
    {

        public static NccLogParameter GetNccLogParameter(string configFilePath)
        {
            NccLogParameter logParameter = new NccLogParameter();
            // Log Config File Example
            // # SAD - M-id 21900
            // GTR3-PBS;SAD;DOUBLE;2;A;;;SAD parameter (X, Y);1915.8, 2300.2
            // GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X, Y);1148.16, 1202.49
            try
            {
                if (configFilePath.Contains(".idt_config.csv"))
                {
                    double sad_x, sad_y;
                    double distICtoIso_x, distICtoIso_y;

                    StreamReader sr = new StreamReader(configFilePath);
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

                                                return logParameter;
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
                return logParameter;
            }
            return logParameter;
        }

        static public int SortLayer(NccLayer layer1, NccLayer layer2)
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

        #region private functions

        #endregion

    }
    public struct NccLogParameter
    {
        public double coeff_x;
        public double coeff_y;
        public DateTime TimeREF; // 나중에 쓰일것
    }

    public record NccLogSpot(int LayerNumber = -1, string LayerID = "", double XPosition = 0, double YPosition = 0,
                             NccBeamState State = NccBeamState.Unknown, DateTime StartTime = new DateTime(), DateTime EndTime = new DateTime())
    {
        public override string ToString()
        {
            // [Spot, LayerNumber, State], startTime, endTime, XPosition, YPosition

            return StartTime.ToString("hh:mm:ss:fff") + ", " + EndTime.ToString("hh:mm:ss:fff") + ", " + XPosition.ToString("+00.00;-00.00;+00.00") + ", " + YPosition.ToString("+00.00;-00.00;+00.00") + " [Layer: " + LayerID + "]";
        }
    }

}