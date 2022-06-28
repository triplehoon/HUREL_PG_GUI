using HUREL_PG_GUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HUREL_PG_GUI.Models
{
    public record LogStruct_NCC(int LayerNumber, string LayerID, double XPosition, double YPosition,
                                          NCCBeamState State, DateTime StartTime, DateTime EndTime, int Tick);
    public record LogStruct_NCC_temp(int LayerNumber, string LayerID, double XPosition, double YPosition,
                                      NCCBeamState State, DateTime StartTime, DateTime EndTime);
    public enum NCCBeamState
    {
        Normal, Tuning, Resume
    }
    
    public class NCCLogFileLoad
    {
        #region Parameters

        private class LogParameter
        {
            public double coeff_x;
            public double coeff_y;
            public DateTime TimeREF;
        }

        private double var_a1 = 2.0748;
        private double var_a2 = 2.501373;
        private double var_b1 = 0.80251;
        private double var_b2 = -1.18168;

        #endregion

        #region Main Function (Real-time // Post-processing)

        public List<LogStruct_NCC> LogDataLoad_RealTime(string FilePath) // 0528
        {
            List<LogStruct_NCC> LogData = new List<LogStruct_NCC>(); // Return
            LogData = ReadSynchronizedLogFile(FilePath);

            return LogData;
        }

        public List<LogStruct_NCC> LogDatasLoad_PostProcessing(string SelectedPath)
        {
            // Return Data
            List<LogStruct_NCC> LogFilesData;

            // Declare Variable
            string Log_Config;
            List<string> Logs_Record;
            List<string> Logs_Specif;

            LogParameter param;
            List<string> SortedLog;

            // Logic
            (Log_Config, Logs_Record, Logs_Specif) = SortByLogName(SelectedPath);
            param = GetParameter_Config(Log_Config);                              // SAD, DistanceICtoIsocenter (x, y)
            //param.TimeREF = GetReferenceDateTime_Record(Logs_Record, param);      // First Tuning Beam Time
            // VM_SpotScanning_PostProcessing.FirstTuningBeamTime = GetReferenceDateTime_Record(Logs_Record, param); // 0528
            SortedLog = SortLog_Record(Logs_Record);
            LogFilesData = ReadSortedLogFilesList(SortedLog, param);

            #region Old
            //List<string> LogFilesList = new List<string>();
            //List<string> LogFilesSortedDirectory = new List<string>();
            //List<string> LayerIDs = new List<string>();
            //List<int> LayerNumbers = new List<int>();

            //List<LogStruct_NCC> LogFilesData = new List<LogStruct_NCC>();

            //// 0. 반환할 데이터를 생성한다.
            //// 1. 경로를 지정한다.
            //// 2. 경로에 존재하는 (모든)로그파일의 이름을 가져온다.
            //// 3. 로그파일을 순서대로 정렬한다 ((Tuning1 -> Tuning2 -> Beam -> Resume))    ** part가 존재하는 파일은 어떻게 읽어나가야 하나?
            //// 4. 정렬된 각각의 로그파일을 하나씩 읽는다(Function)
            //// 5. 읽은것들을 계속 더해나간다.
            //// 6. return 한다.            

            //if (true)
            //{
            //    LogFilesList = Directory.GetFiles(SelectedPath).ToList();
            //    (LogFilesSortedDirectory, LayerIDs, LayerNumbers) = SortLogFilesList(LogFilesList, false);
            //    LogFilesData = ReadSortedLogFilesList(LogFilesSortedDirectory);
            //}
            //else
            //{
            //    //
            //}
            #endregion

            return LogFilesData;
        }

        public DateTime GetReferenceDateTime_Record(string LogName)
        {
            DateTime TimeREF = ReadSingleLog_Record(LogName)[0].StartTime;

            //string FirstTuningLog = Logs_Record.FindAll((string s) => s.Contains("tuning"))[0];
            //DateTime TimeREF = ReadSingleLog_Record(FirstTuningLog)[0].StartTime;

            return TimeREF;
        }
        #endregion

        #region Sub-Function
        private DateTime GetReferenceDateTime_Record(List<string> Logs_Record, LogParameter param_pre)
        {
            string FirstTuningLog = Logs_Record.FindAll((string s) => s.Contains("tuning"))[0];
            //var kk = ReadSingleLog_Record(FirstTuningLog);

            DateTime TimeREF = ReadSingleLog_Record(FirstTuningLog)[0].StartTime;

            return TimeREF;
        }
        private List<LogStruct_NCC_temp> ReadSingleLog_Record(string Log_Record)
        {
            List<LogStruct_NCC_temp> LogData = new List<LogStruct_NCC_temp>();

            NCCBeamState state;
            int LayerNumber;
            string LayerID;

            (state, LayerNumber, LayerID) = CategorizeLogFile(Log_Record);

            Stream xdrByteDate = File.Open(Log_Record, FileMode.Open);
            var xdrConverting = new XdrDataRecorderRpcLayerConverter(xdrByteDate);
            if (xdrConverting.ErrorCheck == false)
            {
                List<float> xPositions = new List<float>();
                List<float> yPositions = new List<float>();

                List<Int64> epochTime = new List<Int64>();

                bool spotContinue = false;

                foreach (var elementData in xdrConverting.elementData)
                {
                    if (spotContinue && (elementData.axisDataxPosition == -10000 && elementData.axisDatayPosition == -10000)) // 스팟이 끝날 때, 위치좌표의 평균 등을 수행
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

                        templayerNumber = LayerNumber;

                        xPositions.Clear();
                        yPositions.Clear();
                        epochTime.Clear();

                        LogData.Add(new LogStruct_NCC_temp(templayerNumber, LayerID, tempyPosition * var_a1 + var_b1, tempxPosition * var_a2 + var_b2, state, tempstartEpochTime, tempendEpochTime));
                        spotContinue = false;
                    }

                    if (!spotContinue && (elementData.axisDataxPosition != -10000 || elementData.axisDatayPosition != -10000)) // 스팟이 시작됨을 알려줌
                    {
                        spotContinue = true;
                    }

                    if (spotContinue) // 스팟이 지속될 때, 계속 데이터를 누적시킴
                    {
                        xPositions.Add(elementData.axisDataxPosition);
                        yPositions.Add(elementData.axisDatayPosition);
                        epochTime.Add(XdrDataRecorderRpcLayerConverter.XdrRead.ToLong(elementData.epochTimeData, elementData.nrOfMicrosecsData));
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

                    templayerNumber = LayerNumber;

                    xPositions.Clear();
                    yPositions.Clear();
                    epochTime.Clear();

                    LogData.Add(new LogStruct_NCC_temp(templayerNumber, LayerID, tempyPosition * var_a1 + var_b1, tempxPosition * var_a2 + var_b2, state, tempstartEpochTime, tempendEpochTime));
                    spotContinue = false;
                }
            }
            return LogData;
        }
        private LogParameter GetParameter_Config(string Log_Config)
        {
            // Return
            LogParameter param = new LogParameter();

            string line;

            double sad_x;
            double sad_y;
            double distICtoIso_x;
            double distICtoIso_y;

            double coeff_x;
            double coeff_y;

            int startIndex;
            string splitStr;
            List<string> tempParam = new List<string>();

            StreamReader sr = new StreamReader(Log_Config);
            while(!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line.Contains("# SAD - M-id 21900"))
                {
                    line = sr.ReadLine(); // "GTR3-PBS; SAD; DOUBLE; 2; A;;; SAD parameter (X,Y);1915.8,2300.2"
                    startIndex = line.IndexOf("SAD parameter (X,Y);");
                    splitStr = line.Substring(startIndex + 20);
                    tempParam = splitStr.Split(",").ToList();

                    sad_x = Convert.ToDouble(tempParam[0]);
                    sad_y = Convert.ToDouble(tempParam[1]);

                    line = sr.ReadLine(); // "GTR3-PBS;distanceFromIcToIsocenter;DOUBLE;2;A;;;Distance from Ic to Isocenter (X,Y);1148.16,1202.49"
                    startIndex = line.IndexOf("Distance from Ic to Isocenter (X,Y);");
                    splitStr = line.Substring(startIndex + 36);
                    tempParam = splitStr.Split(",").ToList();

                    distICtoIso_x = Convert.ToDouble(tempParam[0]);
                    distICtoIso_y = Convert.ToDouble(tempParam[1]);

                    param.coeff_x = sad_x / (sad_x - distICtoIso_x);
                    param.coeff_y = sad_y / (sad_y - distICtoIso_y);

                    break;
                } 
            }

            return param;
        }
        private (string, List<string>, List<string>) SortByLogName(string path)
        {
            List<string> LogList = Directory.GetFiles(path).ToList();

            string Log_Config = "";
            List<string> Logs_Record = new List<string>();
            List<string> Logs_Specif = new List<string>();

            Log_Config = LogList.Find((string s) => s.Contains("idt_config.csv"));
            Logs_Record = LogList.FindAll((string s) => s.Contains("map_record") && Path.GetExtension(s) == ".xdr");
            Logs_Specif = LogList.FindAll((string s) => s.Contains("map_specif") && Path.GetExtension(s) == ".xdr"); // 굳이 안필요 할 수 있음. Record 돌릴 때 text 대체하여 map_record -> map_specif

            return (Log_Config, Logs_Record, Logs_Specif);
        }
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        private List<string> SortLog_Record(List<string> Log_Record)
        {
            // Return
            List<string> LogSortedByLayer = new List<string>();
            List<string> LayerIDs = new List<string>();
            List<int> LayerNumbers = new List<int>();

            string LastLogName = Path.GetFileNameWithoutExtension(Log_Record.Last());
            int LastLayer = Convert.ToInt32(LastLogName.Split('_')[4]);

            for (int i = 0; i <= LastLayer; i++)
            {
                List<string> LogSingleLayer = Log_Record.FindAll((string s) => Convert.ToInt32(Path.GetFileNameWithoutExtension(s).Split("_")[4]) == i);

                if (LogSingleLayer.Any(s => s.Contains("part_")))
                {
                    LogSingleLayer = SortByPartNumber(LogSingleLayer); // 이 안쪽에서 이미 TNR sorting이 되어야 함 // Tuning -> Normal -> Resume
                    LogSortedByLayer.AddRange(LogSingleLayer);
                }
                else
                {
                    List<string> Log_temp = new List<string>();
                    Log_temp = SortLogFilesTNR(LogSingleLayer);
                    LogSortedByLayer.AddRange(Log_temp);
                }
            }

            #region File Order Verification(TEMP)
            //foreach (string s in LogSortedByLayer)
            //{
            //    Trace.WriteLine($"{Path.GetFileNameWithoutExtension(s)}");
            //}
            #endregion

            return LogSortedByLayer;
        }
        private List<string> SortByPartNumber(List<string> LogSortedByLayer)
        {
            #region File Order Verification(TEMP)
            //foreach (string s in LogSortedByLayer)
            //{
            //    Trace.WriteLine($"{Path.GetFileNameWithoutExtension(s)}");
            //}
            //Trace.WriteLine($"--------------------------------------");
            #endregion

            List<string> Log_Sorted = new List<string>();
            int LastPart = Convert.ToInt32(Path.GetFileNameWithoutExtension(LogSortedByLayer.Last()).Split('_')[6]); // 1, 2, 3, ...

            List<string> SortedLogs = new List<string>();
            List<string> LayerIDs = new List<string>();
            List<int> LayerNumbers = new List<int>();

            for (int i = 1; i <= LastPart; i++)
            {               
                List<string> LogSortedByPart = LogSortedByLayer.FindAll((string s) => Convert.ToInt32(Path.GetFileNameWithoutExtension(s).Split("_")[6]) == i);
                List<string> LogSortedByPartTNR = SortLogFilesTNR(LogSortedByPart);
                SortedLogs.AddRange(LogSortedByPartTNR); //////
            }

            #region File Order Verification(TEMP)
            //foreach (string s in SortedLogs)
            //{
            //    Trace.WriteLine($"{Path.GetFileNameWithoutExtension(s)}");
            //}
            //Trace.WriteLine($"--------------------------------------");
            //Trace.WriteLine($"--------------------------------------");
            //Trace.WriteLine($"--------------------------------------");
            #endregion

            return SortedLogs;
        }
        private List<string> SortLogFilesTNR(List<string> Log)
        {
            // Return
            List<string> SortedLogs = new List<string>();

            List<string> Tuning = new List<string>();
            List<string> Normal = new List<string>();
            List<string> Resume = new List<string>();
            List<string> Part = new List<string>();

            List<string> LayerIDs = new List<string>();
            List<int> LayerNumbers = new List<int>();

            List<string> SortedLogDirectory = new List<string>();

            // 01. Sort the log file in the directory (Tuning // Normal // Resume)
            Tuning = Log.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && s.Contains("tuning"));
            Normal = Log.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && !s.Contains("tuning") && !s.Contains("resume"));
            Resume = Log.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && s.Contains("resume"));

            // 02. Add to List
            SortedLogs.AddRange(Tuning);
            SortedLogs.AddRange(Normal);
            SortedLogs.AddRange(Resume);



            //// 02. Check the total layer count by using "Normal" file
            //foreach (string tempLayerNumber in Normal)
            //{
            //    string s = Path.GetFileNameWithoutExtension(tempLayerNumber);
            //    LayerNumbers.Add(Convert.ToInt32(s.Split('_')[4]));
            //}

            //// 03. Add to sorted list(full directory)
            //for (int Layer = 0; Layer < LayerNumbers.Count; Layer++)
            //{
            //    (SortedLogs, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogs, Layer, Tuning, LayerIDs);
            //    (SortedLogs, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogs, Layer, Normal, LayerIDs);
            //    (SortedLogs, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogs, Layer, Resume, LayerIDs);
            //}

            //LayerIDs = LayerIDs.Distinct().ToList();
            //LayerNumbers = LayerNumbers.Distinct().ToList();


            return SortedLogs;
        }
        private (List<string>, List<string>, List<int>) SortLogFilesList(List<string> LogFilesList, bool isPartFile)
        {
            List<string> Tuning = new List<string>();
            List<string> Normal = new List<string>();
            List<string> Resume = new List<string>();
            List<string> Part = new List<string>();

            List<string> LayerIDs = new List<string>();
            List<int> LayerNumbers = new List<int>();

            List<string> SortedLogDirectory = new List<string>();

            if (isPartFile == false)
            {
                // 01. Sort the log file in the directory (Tuning // Normal // Resume)
                Tuning = LogFilesList.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && s.Contains("tuning"));
                Normal = LogFilesList.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && !s.Contains("tuning") && !s.Contains("resume"));
                Resume = LogFilesList.FindAll((string s) => Path.GetExtension(s) == ".xdr" && s.Contains("record") && s.Contains("resume"));
                
                // 02. Check the total layer count by using "Normal" file
                foreach (string tempLayerNumber in Normal)
                {
                    string s = Path.GetFileNameWithoutExtension(tempLayerNumber);
                    LayerNumbers.Add(Convert.ToInt32(s.Split('_')[4]));
                }

                // 03. Add to sorted list(full directory)
                for (int Layer = 0; Layer < LayerNumbers.Count; Layer++)
                {
                    (SortedLogDirectory, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogDirectory, Layer, Tuning, LayerIDs);
                    (SortedLogDirectory, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogDirectory, Layer, Normal, LayerIDs);
                    (SortedLogDirectory, LayerIDs) = AddSortedLogFileFullDirectory(SortedLogDirectory, Layer, Resume, LayerIDs);

                    #region OLD (Not written as Function)
                    //// 01. Add Tuning File
                    //foreach (string Directory_Tuning in Tuning)
                    //{
                    //    string FileName_Tuning = Path.GetFileNameWithoutExtension(Directory_Tuning);
                    //    if (LayerNumberInt == Convert.ToInt32(FileName_Tuning.Split('_')[4]))
                    //    {
                    //        LogFilesSortedDirectory.Add(Directory_Tuning);
                    //    }
                    //}

                    //// 02. Add Normal File
                    //foreach (string Directory_Normal in Normal)
                    //{
                    //    string FileName_Normal = Path.GetFileNameWithoutExtension(Directory_Normal);
                    //    if (LayerNumberInt == Convert.ToInt32(FileName_Normal.Split('_')[4]))
                    //    {
                    //        LogFilesSortedDirectory.Add(Directory_Normal);
                    //    }
                    //}

                    //// 03. Add Resume File
                    //foreach (string Directory_Resume in Resume)
                    //{
                    //    string FileName_Resume = Path.GetFileNameWithoutExtension(Directory_Resume);
                    //    if (LayerNumberInt == Convert.ToInt32(FileName_Resume.Split('_')[4]))
                    //    {
                    //        LogFilesSortedDirectory.Add(Directory_Resume);
                    //    }
                    //}
                    #endregion
                }

                LayerIDs = LayerIDs.Distinct().ToList();
                LayerNumbers = LayerNumbers.Distinct().ToList();
            }
            else // part 파일을 고려해줄 경우
            {

            }

            #region Debug Tool
            ////////////////////////////////////////////////////////////////
            //Trace.WriteLine($"*** Tuning File List *** ");
            //int i = 1;
            //foreach (string s in Tuning)
            //{
            //    Trace.WriteLine($"{i} : {s}");
            //    i += 1;
            //}

            //Trace.WriteLine($"");
            //Trace.WriteLine($"*** Normal File List *** ");
            //i = 1;
            //foreach (string s in Normal)
            //{
            //    Trace.WriteLine($"{i} : {s}");
            //    i += 1;
            //}

            //Trace.WriteLine($"");
            //Trace.WriteLine($"*** Resume File List *** ");
            //i = 1;
            //foreach (string s in Resume)
            //{
            //    Trace.WriteLine($"{i} : {s}");
            //    i += 1;
            //}
            ////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////
            //Trace.WriteLine("");
            //Trace.WriteLine("*** NCC Log_NCC Files Full Directory ***");
            //foreach (string s in SortedLogDirectory)
            //{
            //    Trace.WriteLine($" {s}");
            //}
            //////////////////////////////////////////////////////////////
            #endregion

            return (SortedLogDirectory, LayerIDs, LayerNumbers);
        }
        private (List<string>, List<string>) AddSortedLogFileFullDirectory(List<string> pre_SortedLogDirectory, int LayerNumber, List<string> LogFiles, List<string> pre_LayerIDs)
        {
            List<string> SortedLogDirectory = pre_SortedLogDirectory;
            List<string> LayerIDs = pre_LayerIDs;

            foreach (string Directory in LogFiles)
            {
                string FileName = Path.GetFileNameWithoutExtension(Directory);
                if (LayerNumber == Convert.ToInt32(FileName.Split('_')[4]))
                {
                    SortedLogDirectory.Add(Directory);
                    LayerIDs.Add(GenerateLayerIDs_PostProcessing(Directory, LayerNumber));
                }
            }

            return (SortedLogDirectory, LayerIDs);
        }
        private string GenerateLayerIDs_PostProcessing(string Directory, int LayerNumber)
        {
            string FileName = Path.GetFileNameWithoutExtension(Directory).Split("\\").Last();
            string LayerID;

            if (FileName.Contains("tuning"))
            {
                LayerID = LayerNumber.ToString() + "_Tuning_" + Convert.ToString(Convert.ToInt32(FileName.Split('_')[6]));
            }
            else if (FileName.Contains("resume"))
            {
                LayerID = LayerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(FileName.Split('_')[6]));
            }
            else
            {
                LayerID = Convert.ToString(Convert.ToInt32(FileName.Split('_')[4]));
            }

            return LayerID;
        }        
        private (NCCBeamState, int, string) CategorizeLogFile(string Directory)
        {
            NCCBeamState state;
            string LayerID;

            string FileName = Path.GetFileNameWithoutExtension(Directory).Split("\\").Last();
            int LayerNumber = Convert.ToInt32(FileName.Split('_')[4]);

            if (FileName.Contains("_part_"))
            {
                // 0: 20220601(date) && 1: 182105(time) && 2: 461(usec).map && 3: record && 4: LayerNumber(xxxx) && 5: "part" && 6: PartNumber(xx) && 7: "tuning" or "resume" && 8: TuningNumber or ResumeNumber
                
                int PartNumber = Convert.ToInt32(FileName.Split('_')[6]);

                if (FileName.Contains("tuning"))
                {
                    int TuningNumber = Convert.ToInt32(FileName.Split('_')[8]);

                    LayerID = LayerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + TuningNumber.ToString();
                    state = NCCBeamState.Tuning;
                }
                else if (FileName.Contains("resume"))
                {
                    int ResumeNumber = Convert.ToInt32(FileName.Split('_')[8]);

                    LayerID = LayerNumber.ToString() + "_part_" + PartNumber.ToString() + "_tuning_" + ResumeNumber.ToString();
                    state = NCCBeamState.Resume;
                }
                else
                {
                    LayerID = LayerNumber.ToString() + "_part_" + PartNumber.ToString();
                    state = NCCBeamState.Normal;
                }
            }
            else
            {
                if (FileName.Contains("tuning"))
                {
                    LayerID = LayerNumber.ToString() + "_tuning_" + Convert.ToString(Convert.ToInt32(FileName.Split('_')[6]));
                    state = NCCBeamState.Tuning;
                }
                else if (FileName.Contains("resume"))
                {
                    LayerID = LayerNumber.ToString() + "_Resume_" + Convert.ToString(Convert.ToInt32(FileName.Split('_')[6]));
                    state = NCCBeamState.Resume;
                }
                else
                {
                    LayerID = Convert.ToString(Convert.ToInt32(FileName.Split('_')[4]));
                    state = NCCBeamState.Normal;
                }
            }

            // Trace.WriteLine($"[Trace: Load_NCC_Log.cs  NCCLogFileLoad.CategorizeLogFile] &&&&& FileName: {FileName},          Layer ID: {LayerID}");
            return (state, LayerNumber, LayerID);
        }
        private List<LogStruct_NCC> ReadSynchronizedLogFile(string FilePath) // 0528
        {
            List<LogStruct_NCC> LogData = new List<LogStruct_NCC>();

            NCCBeamState state;
            int LayerNumber;
            string LayerID;

            (state, LayerNumber, LayerID) = CategorizeLogFile(FilePath);

            bool isAccessible = false;            
            while (isAccessible == false)
            {
                try
                {
                    Stream xdrByteDate = File.Open(FilePath, FileMode.Open);

                    #region .xdr files read

                    var xdrConverting = new XdrDataRecorderRpcLayerConverter(xdrByteDate);
                    if (xdrConverting.ErrorCheck == false)
                    {
                        List<float> xPositions = new List<float>();
                        List<float> yPositions = new List<float>();

                        List<Int64> epochTime = new List<Int64>();

                        bool spotContinue = false;

                        foreach (var elementData in xdrConverting.elementData)
                        {
                            if (spotContinue && (elementData.axisDataxPosition == -10000 && elementData.axisDatayPosition == -10000)) // 스팟이 끝날 때, 위치좌표의 평균 등을 수행
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

                                templayerNumber = LayerNumber;

                                xPositions.Clear();
                                yPositions.Clear();
                                epochTime.Clear();

                                int Gap_FirstSpot = Convert.ToInt32((tempstartEpochTime.Ticks - VM_SpotScanning.FirstTuningBeamTime.Ticks) / 10);
                                LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, tempyPosition * var_a1 + var_b1, tempxPosition * var_a2 + var_b2, state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));
                                //LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, ((tempyPosition - xdrConverting_Specif.icyOffset) * param.coeff_y), ((tempxPosition - xdrConverting_Specif.icxOffset) * param.coeff_x), state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));
                                spotContinue = false;
                            }

                            if (!spotContinue && (elementData.axisDataxPosition != -10000 || elementData.axisDatayPosition != -10000)) // 스팟이 시작됨을 알려줌
                            {
                                spotContinue = true;
                            }

                            if (spotContinue) // 스팟이 지속될 때, 계속 데이터를 누적시킴
                            {
                                xPositions.Add(elementData.axisDataxPosition);
                                yPositions.Add(elementData.axisDatayPosition);
                                epochTime.Add(XdrDataRecorderRpcLayerConverter.XdrRead.ToLong(elementData.epochTimeData, elementData.nrOfMicrosecsData));
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

                            templayerNumber = LayerNumber;

                            xPositions.Clear();
                            yPositions.Clear();
                            epochTime.Clear();

                            int Gap_FirstSpot = Convert.ToInt32((tempstartEpochTime.Ticks - VM_SpotScanning.FirstTuningBeamTime.Ticks) / 10);
                            LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, tempyPosition * var_a1 + var_b1, tempxPosition * var_a2 + var_b2, state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));
                            //LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, tempyPosition * var_a1 + var_b1, tempxPosition * var_a2 + var_b2, state, tempstartEpochTime, tempendEpochTime));
                            spotContinue = false;
                        }
                    }

                    #endregion

                    isAccessible = true;
                }
                catch (System.IO.IOException e)
                {
                    Trace.WriteLine($"Load_NCC_Log.ReadSynchronizedLogFile: {e.Message}");
                    Thread.Sleep(5);
                }
            }

            return LogData;
        }
        private List<LogStruct_NCC> ReadSortedLogFilesList(List<string> Directory, LogParameter param)
        {
            List<LogStruct_NCC> LogData = new List<LogStruct_NCC>();

            bool isGetTimeTick = false;
            long RefTime_FirstTuning = 0;

            foreach (string SingleDirectory in Directory)
            {
                #region Log_Specif read

                string Log_Specif = SingleDirectory.Replace("_record_", "_specif_");
                Stream xdrByteData = File.Open(Log_Specif, FileMode.Open);
                var xdrConverting_Specif = new XdrConverter_Specific(xdrByteData); // smxOffset smyOffset icxOffset icyOffset

                #endregion

                #region Log_Record read

                Stream xdrByteDate = File.Open(SingleDirectory, FileMode.Open);
                var xdrConverting = new XdrDataRecorderRpcLayerConverter(xdrByteDate);

                #endregion

                NCCBeamState state;
                int LayerNumber;
                string LayerID;

                (state, LayerNumber, LayerID) = CategorizeLogFile(SingleDirectory);                

                #region Spot Data Extract
                if (xdrConverting.ErrorCheck == false)
                {
                    List<float> xPositions = new List<float>();
                    List<float> yPositions = new List<float>();

                    List<Int64> epochTime = new List<Int64>();

                    bool spotContinue = false;

                    foreach (var elementData in xdrConverting.elementData)
                    {
                        if (spotContinue && (elementData.axisDataxPosition == -10000 && elementData.axisDatayPosition == -10000)) // 스팟이 끝날 때, 위치좌표의 평균 등을 수행
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

                            templayerNumber = LayerNumber;

                            xPositions.Clear();
                            yPositions.Clear();
                            epochTime.Clear();
                            
                            if (isGetTimeTick == false)
                            {
                                RefTime_FirstTuning = tempstartEpochTime.Ticks;
                                isGetTimeTick = true;
                            }
                            int Gap_FirstSpot = Convert.ToInt32((tempstartEpochTime.Ticks - RefTime_FirstTuning) / 10);
                            LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, ((tempyPosition - xdrConverting_Specif.icyOffset) * param.coeff_y), ((tempxPosition - xdrConverting_Specif.icxOffset) * param.coeff_x), state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));
                            
                            spotContinue = false;
                        }

                        if (!spotContinue && (elementData.axisDataxPosition != -10000 || elementData.axisDatayPosition != -10000)) // 스팟이 시작됨을 알려줌
                        {
                            spotContinue = true;
                        }

                        if (spotContinue) // 스팟이 지속될 때, 계속 데이터를 누적시킴
                        {
                            xPositions.Add(elementData.axisDataxPosition);
                            yPositions.Add(elementData.axisDatayPosition);
                            epochTime.Add(XdrDataRecorderRpcLayerConverter.XdrRead.ToLong(elementData.epochTimeData, elementData.nrOfMicrosecsData));
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

                        templayerNumber = LayerNumber;

                        xPositions.Clear();
                        yPositions.Clear();
                        epochTime.Clear();

                        if (isGetTimeTick == false)
                        {
                            RefTime_FirstTuning = tempstartEpochTime.Ticks;
                            isGetTimeTick = true;
                        }
                        int Gap_FirstSpot = Convert.ToInt32((tempstartEpochTime.Ticks - RefTime_FirstTuning) / 10);
                        LogData.Add(new LogStruct_NCC(templayerNumber, LayerID, ((tempyPosition - xdrConverting_Specif.icyOffset) * param.coeff_y), ((tempxPosition - xdrConverting_Specif.icxOffset) * param.coeff_x), state, tempstartEpochTime, tempendEpochTime, Gap_FirstSpot));
                        
                        spotContinue = false;
                    }
                }
                #endregion
            }

            return LogData;
        }
        #endregion
    }
}