using CenterSpace.NMath.Core;
using HUREL_PG_GUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HUREL_PG_GUI.Models
{
    #region Data Struct

    public class PlanLogMergedDataStruct
    {
        // Log_NCC
        public int Log_LayerNumber;
        public string Log_LayerID;
        public double Log_XPos;
        public double Log_YPos;
        public NCCBeamState Log_State;
        public DateTime Log_StartTime;
        public DateTime Log_EndTime;

        // Plan_NCC
        public double Plan_LayerEnergy;
        //public double Plan_LayerMU;
        //public int Plan_LayerSpot;
        public double Plan_XPos;
        public double Plan_YPos;
        public double Plan_ZPos;
        public double Plan_MU;
        //int public Plan_LayerNumber;

        public int Tick;
    }

    public class PlanLogPGMergedDataStruct : PlanLogMergedDataStruct
    {
        public int[] ChannelCount = new int[144];
        public double[] ChannelCount_71 = new double[71];

        public int TriggerStart;
        public int TriggerEnd;
    }

    public record SpotMapStruct(double X, double Y, double MU, SolidColorBrush Color);

    public record BeamRangeMapStruct(double X, double Y, double MU, SolidColorBrush Color);

    public record VM_BeamRangeMapStruct(double X, double Y, double MU, SolidColorBrush Color);

    public class GapPeakAndRangeStruct
    {
        public double Energy;
        public double GapValue;
    }

    enum Mode
    {
        RealTime,
        PostProcessing
    }

    #endregion

    public class NCCAnalysisClass
    {
        #region Parameter

        // ViewModel에서 가지고 오기
        private double Sigma = 7;
        private double Globalpush = -0.5;
        private int[] GridRange = { -60, 60 };
        private int GridPitch = 5;
        double[] xgrid_mm = new double[71];

        #endregion

        public NCCAnalysisClass() // Constructor
        {
            xgrid_mm = GenerateXgrid_mm();
        }

        #region Main Function
      
        public async Task<List<PlanLogPGMergedDataStruct>> GenerateMergedData_PostProcessing(List<PlanStruct_NCC> Plan, List<LogStruct_NCC> Log, List<PGStruct> PG_raw)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<PGStruct> PG_144 = new List<PGStruct>();
            List<PlanLogMergedDataStruct> PlanLog = new List<PlanLogMergedDataStruct>();
            List<PlanLogPGMergedDataStruct> MergedData = new List<PlanLogPGMergedDataStruct>();


            Task<List<PGStruct>> PGTask = SplitDataIntoSpots(PG_raw, 5, 40, 30); // original
            //Task<List<PGStruct>> PGTask = SplitDataIntoSpots(PG_raw, 4, 20, 10);
            Task<List<PlanLogMergedDataStruct>> PlanLogTask = MergePlanLog(Plan, Log, Mode.PostProcessing, 999, 3);

            PG_144 = await PGTask;
            PlanLog = await PlanLogTask;

            if (PG_144.Count != PlanLog.Count)
            {
                Trace.WriteLine($"# of Log({PlanLog.Count}) != # of PG({PG_144.Count}");
            }
            else
            {
                Trace.WriteLine($"# of Log({PlanLog.Count}) = # of PG({PG_144.Count}");
            }

            // 0. Parameter setting
            int RefTime_PG = 0;        // 임시
            int RefTime_PlanLog = 0;   // 임시
            int NumOfCompareSpot = 10; // 임시

            // 1-1. GetReferenceTime_PlanLog
            bool GetRefTime_PlanLog = false;
            for (int i = 0; i < PlanLog.Count - NumOfCompareSpot; i++)
            {
                if (PlanLog[i + NumOfCompareSpot].Tick - PlanLog[i].Tick < 0.5E6)
                {
                    GetRefTime_PlanLog = true;
                    RefTime_PlanLog = PlanLog[i].Tick;
                    break;
                }
            }
            if (GetRefTime_PlanLog == false)
            {
                RefTime_PlanLog = 0;
                MessageBox.Show("Can't get referencetime of PlanLog");
            }
            for (int i = 0; i < PlanLog.Count; i++)
            {
                PlanLog[i].Tick = PlanLog[i].Tick - RefTime_PlanLog;
            }

            // 1-2. GetReferenceTime_PG
            bool GetRefTime_PG = false;
            for (int i = 0; i < PG_144.Count - NumOfCompareSpot; i++)
            {
                if (PG_144[i + NumOfCompareSpot].TriggerInputStartTime - PG_144[i].TriggerInputStartTime < 0.5E6)
                {
                    GetRefTime_PG = true;
                    RefTime_PG = PG_144[i].TriggerInputStartTime;                    
                    break;
                }
            }
            if (GetRefTime_PG == false)
            {
                RefTime_PG = 0;
                MessageBox.Show("Can't get referencetime of PG");
            }
            for (int i = 0; i < PG_144.Count; i++)
            {
                PG_144[i].Tick = PG_144[i].TriggerInputStartTime - RefTime_PG;
            }

            // 2-1. MergePlanLogPG_v2
            bool MergePGandPlanLogwithTimeInfo = true; // false: 구버전, true: splitDataIntoSpots_v2.m 알고리즘 적용
            MergedData = MergePlanLogPG(PlanLog, PG_144, MergePGandPlanLogwithTimeInfo); // 144 ch -> 71 ch

            sw.Stop();
            Trace.WriteLine($"Ellapsed Time for Data Merging: {sw.ElapsedMilliseconds} ms");

            #region [Debug] (1) Trig Time, (2) 144Ch Counts, (3) 71Ch Counts (디버깅완료)

            #region [Debug] Split time for each spot (디버깅 완료)
            //string DebugFileName = "Debug_PG144_Split(50, 30)_Time.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < PG_144.Count(); i++)
            //    {
            //        file.Write($"{PG_144[i].TriggerInputStartTime}, ");
            //        file.WriteLine($"{PG_144[i].TriggerInputEndTime}");
            //    }
            //}
            #endregion

            #region [Debug] Channel counts(144ch) for each spot (디버깅 완료)
            //string DebugFileName = "Debug_PG144_Split(50, 30).csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < PG_144.Count(); i++)
            //    {
            //        for (int j = 0; j < 144; j++)
            //        {
            //            file.Write($"{PG_144[i].ChannelCount[j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            #region [Debug] Channel counts(71ch) for each spot (디버깅 완료)
            //string DebugFileName = "Debug_PG71_Split(50, 30).csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < MergedData.Count(); i++)
            //    {
            //        for (int j = 0; j < 71; j++)
            //        {
            //            file.Write($"{MergedData[i].ChannelCount_71[j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            #endregion

            return MergedData;
        }

        public async Task<List<PlanLogPGMergedDataStruct>> GenerateMergedData_RealTime(List<PlanStruct_NCC> Plan, List<LogStruct_NCC> Log, List<PGStruct> PG_raw)
        {
            List<PGStruct> PG_144 = new List<PGStruct>();
            List<PlanLogMergedDataStruct> PlanLog = new List<PlanLogMergedDataStruct>();
            List<PlanLogPGMergedDataStruct> MergedData = new List<PlanLogPGMergedDataStruct>();

            Task<List<PGStruct>> PGTask = SplitDataIntoSpots(PG_raw, 5, 40, 30);
            Task<List<PlanLogMergedDataStruct>> PlanLogTask = MergePlanLog(Plan, Log, Mode.RealTime, 999, 6);

            PG_144 = await PGTask;
            PlanLog = await PlanLogTask;

            var TimeGap_PGandLog = PG_144[0].TriggerInputStartTime - PlanLog[0].Tick;
            for (int i = 0; i< PlanLog.Count; i++)
            {
                PlanLog[i].Tick = PlanLog[i].Tick + TimeGap_PGandLog;
            }

            #region Debug: Plan, Log, PlanLog Counts
            //Trace.WriteLine($"");
            //Trace.WriteLine($"");
            //Trace.WriteLine($"Plan   : {Plan.Count}");
            //Trace.WriteLine($"Log    : {Log.Count}");
            //Trace.WriteLine($"Merged : {PlanLog.Count}");
            #endregion

            bool MergePGandPlanLogwithTimeInfo = true;
            MergedData = MergePlanLogPG(PlanLog, PG_144, MergePGandPlanLogwithTimeInfo); // 144 ch -> 71 ch, 시간별로 matching 해야 함

            return MergedData;
        }

        public async Task<List<SpotMapStruct>> GenerateSpotMap(List<PlanLogPGMergedDataStruct> mergedData, int SelectedLayer)
        {
            List<double[]> GaussianWeightMap = new List<double[]>();
            double[] RangeDifference;
            List<SpotMapStruct> SpotMap = new List<SpotMapStruct>(); // Return  

            List<PlanLogPGMergedDataStruct> Spots = (from spot in mergedData
                                                     where spot.Log_LayerNumber == SelectedLayer
                                                     where spot.Log_State != NCCBeamState.Tuning
                                                     select spot).ToList();
            double PeakToRangeGap = CalcGap(VM_SpotScanning._Configuration_NCC.GapPeakAndRange, Spots.Last().Plan_LayerEnergy);

            GaussianWeightMap = GetGaussianWeightMap_SpotMap(Spots);
            RangeDifference = GetRangeDifference_SpotMap(Spots, GaussianWeightMap, PeakToRangeGap);
            //SpotMap = GetSelectedLayerSpotMap(Spots, RangeDifference, -6.5f, 3f);
            SpotMap = GetSelectedLayerSpotMap(Spots, RangeDifference, -10f, 10f);

            //Trace.Write($"{Spots.Count} spots ");

            return SpotMap;
        }

        public async Task<List<BeamRangeMapStruct>> GenerateBeamRangeMap(List<PlanLogPGMergedDataStruct> mergedData, double sigma, int StartLayer, int LastLayer, double cutoff_MU)
        {
            List<BeamRangeMapStruct> BeamRangeMap = new List<BeamRangeMapStruct>();

            double[] xpos;
            double[] ypos;

            double[,] map_diff;
            double[,] map_mu;

            //float Shift_max = 3f;
            //float Shift_min = -6.5f;

            float Shift_max = 5f;
            float Shift_min = -5f;

            sigma = 7;

            //xgrid_mm = GenerateXgrid_mm();
            (xpos, ypos) = BeamRangeMapParameterSetting(GridRange, GridPitch);

            (map_diff, map_mu) = ShiftMerge(xpos, ypos, mergedData, sigma, StartLayer, LastLayer);
            BeamRangeMap = DrawBeamRangeMap(map_diff, map_mu, Shift_max, Shift_min, xpos, ypos, cutoff_MU);

            #region Debug
            //string DebugFileName = "Debug_map_diff.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < 25; i++)
            //    {
            //        for (int j = 0; j < 25; j++)
            //        {
            //            file.Write($"{map_diff[i, j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            return BeamRangeMap;
        }

        #endregion

        #region Sub-Functions (Private)

        private (double[,], double[,]) ShiftMerge(double[] xpos, double[] ypos, List<PlanLogPGMergedDataStruct> mergedData, double sigma, int StartLayer, int LastLayer)
        {

            bool[,] flag_grid = new bool[xpos.Length, ypos.Length];

            double[,] map_diff = new double[xpos.Length, ypos.Length];
            double[,] map_mu = new double[xpos.Length, ypos.Length];
            double[,,] map_PGdist = new double[xpos.Length, ypos.Length, 71];

            double pitchX = xpos[1] - xpos[0];
            double pitchY = ypos[1] - ypos[0];

            double[] PGdist_shifted = new double[71];

            List<PlanLogPGMergedDataStruct> TotalSpots = (from spot in mergedData
                                                          where spot.Log_State != NCCBeamState.Tuning
                                                          select spot).ToList();
            List<PlanLogPGMergedDataStruct> SelectedLayerSpots = (from spot in mergedData
                                                                  where spot.Log_LayerNumber >= StartLayer
                                                                  where spot.Log_LayerNumber <= LastLayer
                                                                  where spot.Log_State != NCCBeamState.Tuning
                                                                  select spot).ToList();

            //Trace.Write($"{SelectedLayerSpots.Count} spots ");

            flag_grid = CreateGridFlag_BeamRangeMap(xpos, ypos, TotalSpots, pitchX, pitchY);
            (map_diff, map_mu) = CreateDiffMUmap_BeamRangeMap(xpos, ypos, SelectedLayerSpots, flag_grid, sigma);

            #region [Debug]

            #region [Debug] flag_grid map (디버깅 완료)
            //string DebugFileName = "Debug_map_flag.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < 25; i++)
            //    {
            //        for (int j = 0; j < 25; j++)
            //        {
            //            if (flag_grid[i, j] == true)
            //            {
            //                file.Write($"1, ");
            //            }
            //            else
            //            {
            //                file.Write($"0, ");
            //            }
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            #region [Debug] map_mu map (디버깅 완료)
            // MATLAB 코드에는 tuning 빔에도 상당한 MU 할당되어 있음. GUI에서는 spot 빔에만 MU를 할당함.
            //string DebugFileName = "Debug_map_mu.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < 25; i++)
            //    {
            //        for (int j = 0; j < 25; j++)
            //        {
            //            file.Write($"{map_mu[i, j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            #region map_diff_map
            //string DebugFileName = "Debug_map_diff.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < 25; i++)
            //    {
            //        for (int j = 0; j < 25; j++)
            //        {
            //            file.Write($"{map_diff[i, j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            #endregion

            return (map_diff, map_mu);
        }

        private (double[,], double[,]) CreateDiffMUmap_BeamRangeMap(double[] xpos, double[] ypos, List<PlanLogPGMergedDataStruct> SelectedSpots, bool[,] flag_grid, double sigma)
        {
            int testIndex = 1;
            //List<GapPeakAndRangeStruct> gapPeakToRangeList = new List<GapPeakAndRangeStruct>();
            //gapPeakToRangeList = GapPeakAndRangeRead(VM_SpotScanning_PostProcessing.Directory_gapPeakAndRange);

            double[,] map_mu = new double[xpos.Length, ypos.Length];
            double[,] map_diff = new double[xpos.Length, ypos.Length];

            for (int i = 0; i < xpos.Length; i++) // X 방향으로 for문
            {
                for (int j = 0; j < ypos.Length; j++) // Y 방향으로 for문
                {
                    if (flag_grid[i, j] == true) // flag가 true일 경우, 
                    {
                        double[] PGdist_Shifted_Sum = new double[71];

                        for (int k = 0; k < SelectedSpots.Count; k++)
                        {
                            double spot_Xpos = SelectedSpots[k].Log_XPos;
                            double spot_Ypos = SelectedSpots[k].Log_YPos;

                            double distance = Math.Sqrt(Math.Pow(xpos[i] - spot_Xpos, 2) + Math.Pow(ypos[j] - spot_Ypos, 2));

                            if (distance <= 3 * sigma)
                            {
                                double[] PGdist_Shifted = new double[71]; // 위치가 바뀜
                                PGdist_Shifted = ShiftPGdistribution(SelectedSpots[k], VM_SpotScanning._Configuration_NCC.GapPeakAndRange);

                                double weight = Math.Exp(-0.5 * Math.Pow(distance / sigma, 2));
                                PGdist_Shifted = PGdist_Shifted.Select(x => x * weight).ToArray();

                                //for (int iii = 0; iii < PGdist_Shifted.Length; iii++)
                                //{
                                //    Trace.WriteLine($"{PGdist_Shifted[iii]}");
                                //}
                                //Trace.WriteLine("");

                                for (int p = 0; p < 71; p++)
                                {
                                    PGdist_Shifted_Sum[p] = PGdist_Shifted_Sum[p] + PGdist_Shifted[p];
                                }
                                map_mu[i, j] = map_mu[i, j] + weight * SelectedSpots[k].Plan_MU; // 디버깅 완료, PGdist_Shifted_Sum도 얼추 맞는 것 같음 (2022-01-09 21:53)
                            }
                        }

                        #region [Debug]
                        //string DebugFileName = "Debug_PGdist_Shifted_Sum.csv";
                        //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
                        //using (StreamWriter file = new StreamWriter(DebugPath_excel))
                        //{
                        //    for (int k = 0; k < PGdist_Shifted_Sum.Length; k++)
                        //    {
                        //        file.WriteLine($"{PGdist_Shifted_Sum[k]}, ");
                        //    }
                        //}
                        #endregion

                        map_diff[i, j] = GetRange_ver4p0(xgrid_mm, PGdist_Shifted_Sum, 0) - Globalpush;
                    }
                    else
                    {
                        map_diff[i, j] = -10000;
                    }
                }
            }

            return (map_diff, map_mu);
        }

        private double GetRange_ver4p0(double[] xgrid_mm, double[] dist_ShiftSum, double RefRangeValue)
        {
            double range = new double(); // Return

            #region [Debug] dist_ShiftSum
            //string DebugFileName = "Debug_dist_ShiftSum.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < dist_ShiftSum.Length; i++)
            //    {
            //        file.Write($"{dist_ShiftSum[i]}, ");                    
            //    }
            //    file.WriteLine($"");
            //}
            #endregion

            // (1) Setting
            double sigma_gaussFilt = 5; // gaussian filter window (mm)
            double cutoffLevel = 0.5;
            double offset = 0;
            double pitch = 3; // pitch (mm)
            double minPeakDistance = 10;

            // (2) Differentiation: centered finite difference
            double scope = 30;
            //double range_true = 0;

            double[] xgrid_diff = new double[69];
            double[] dist_diff = new double[69];
            double[] dist_diff_unfilt = new double[69];

            for (int i = 0; i < 69; i++)
            {
                xgrid_diff[i] = -102 + 3 * i; // -102 : 3 : 102
            }

            for (int i = 0; i < 69; i++)
            {
                dist_diff_unfilt[i] = -(dist_ShiftSum[i + 2] - dist_ShiftSum[i]) / (2 * pitch);
            }

            dist_diff = imgaussfilt(dist_diff_unfilt, sigma_gaussFilt / pitch);

            #region [Debug] dist_diff
            //string DebugFileName = "Debug_dist_diff.csv";
            //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < dist_diff.Length; i++)
            //    {
            //        file.Write($"{dist_diff[i]}, ");
            //    }
            //    file.WriteLine($"");
            //}

            #endregion

            // (3) Differentiation distribution 상에서 peak 결정
            (double[] pks, double[] locs) = findpeaks_NEW(dist_diff, xgrid_diff, minPeakDistance);
            (double val_pk, int loc_pk) = ChoosePeaks_withScope(pks, locs, RefRangeValue, scope);

            if ((val_pk, loc_pk) == (-10000, -10000))
            {
                return range = -10000;
            }

            // (4) Peak의 범위 지정

            double[] dist_diff_reverse = new double[69];

            for (int i = 0; i < 69; i++)
            {
                dist_diff_reverse[i] = -dist_diff[i];
            }

            DoubleVector data_reverse = new DoubleVector(dist_diff_reverse);
            PeakFinderRuleBased peakfind_reverse = new PeakFinderRuleBased(data_reverse);

            peakfind_reverse.LocatePeaks();
            double[] distanceFromPeak = new double[peakfind_reverse.NumberPeaks];
            for (int ii = 0; ii < peakfind_reverse.NumberPeaks; ii++)
            {
                distanceFromPeak[ii] = loc_pk - peakfind_reverse[ii].X;
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

            double sig_MR = new double();
            double sig_M = new double();

            for (int ii = LeftIndex; ii < RightIndex + 1; ii++)
            {
                if (dist_diff[ii] - baseline > 0)
                {
                    sig_MR += (dist_diff[ii] - baseline) * xgrid_diff[ii];
                    sig_M += dist_diff[ii] - baseline;
                }
            }

            range = (sig_MR / sig_M) + offset;

            return range;
        }

        private (double, int) ChoosePeaks_withScope(double[] pks, double[] locs, double range_true, double scope)
        {
            List<int> IndexList = new List<int>();
            List<double> PksList = new List<double>();
            List<double> LocsList = new List<double>();
            int ValidIndex = 0;

            foreach (var loc in locs)
            {
                if (3 * (loc - 34) > range_true - scope && 3 * (loc - 34) < range_true + scope)
                {
                    IndexList.Add(ValidIndex);
                    PksList.Add(pks[ValidIndex]);
                    LocsList.Add(locs[ValidIndex]);
                }
                ValidIndex++;
            }

            double val_pk;
            int loc_pk;

            if (PksList.Count() != 0)
            {
                val_pk = PksList.Max();
                loc_pk = (int)LocsList[PksList.IndexOf(val_pk)];
            }
            else
            {
                return (-10000, -10000);
            }

            if (ValidIndex > 0)
            {
                return (val_pk, loc_pk);
            }
            else
            {
                return (-10000, -10000);
            }
        }

        private (double[], double[]) findpeaks_NEW(double[] dist_diff, double[] xgrid_diff, double MinPeakDistance)
        {
            DoubleVector ChangedDistribution = new DoubleVector(dist_diff);
            PeakFinderRuleBased peakFind = new PeakFinderRuleBased(ChangedDistribution);

            peakFind.LocatePeaks();
            peakFind.ApplySortOrder(PeakFinderRuleBased.PeakSortOrder.Descending);

            List<Extrema> FoundPeaks = peakFind.GetAllPeaks();

            // MinPeakDistance 적용
            int index = 0;
            if (FoundPeaks.Count > 0)
            {
                while (true)
                {
                    if (index < FoundPeaks.Count())
                    {
                        FoundPeaks.RemoveAll(x => Math.Abs(x.X - FoundPeaks[index].X) < MinPeakDistance && x.X != FoundPeaks[index].X);

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
                Trace.WriteLine("Error Code JJR0004");
            }

            var locs = (from Pks in FoundPeaks
                        select Pks.X).ToArray();
            var pks = (from Pks in FoundPeaks
                       select Pks.Y).ToArray();

            return (pks, locs);
        }

        private (double, double, int, int) findpeaks(double[] dist_diff, double[] xgrid_diff, double ReferenceValue, double scope)
        {
            #region Searching (1) peakValue (다름)

            //DoubleVector xAxis = new DoubleVector(69, -102, 3);
            DoubleVector data = new DoubleVector(dist_diff);

            PeakFinderRuleBased peakfind = new PeakFinderRuleBased(data);

            peakfind.LocatePeaks();
            peakfind.ApplySortOrder(PeakFinderRuleBased.PeakSortOrder.Descending);

            List<Extrema> allPeaks = peakfind.GetAllPeaks();

            double[] validPeakIndex = (from validPeak in allPeaks
                                       where validPeak.X >= 34 + (ReferenceValue - scope) / 3 && validPeak.X <= 34 + (ReferenceValue + scope) / 3
                                       select validPeak.X).ToArray();

            int peakIndex = Convert.ToInt32(validPeakIndex[0]); //
            double peakValue = dist_diff[peakIndex];

            #endregion

            #region Searching (2) bottomLevel, (3) LeftIndex, (4) RightIndex

            double[] dist_diff_reverse = new double[69];

            for (int i = 0; i < 69; i++)
            {
                dist_diff_reverse[i] = -dist_diff[i];
            }

            DoubleVector data_reverse = new DoubleVector(dist_diff_reverse);
            PeakFinderRuleBased peakfind_reverse = new PeakFinderRuleBased(data_reverse);

            peakfind_reverse.LocatePeaks();
            double[] distanceFromPeak = new double[peakfind_reverse.NumberPeaks];
            for (int ii = 0; ii < peakfind_reverse.NumberPeaks; ii++)
            {
                distanceFromPeak[ii] = peakIndex - peakfind_reverse[ii].X;
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
                LeftIndex = peakIndex - Convert.ToInt32(tempLeft.Last());
            }
            else
            {
                LeftIndex = 0;
            }

            if (tempRight.Length != 0)
            {
                RightIndex = peakIndex - Convert.ToInt32(tempRight[0]);
            }
            else
            {
                RightIndex = 68;
            }

            double minValue_left = dist_diff[LeftIndex];
            double minValue_right = dist_diff[RightIndex];

            double bottomLevel = Math.Max(minValue_left, minValue_right);

            #endregion

            return (peakValue, bottomLevel, LeftIndex, RightIndex);
        }

        private double[] imgaussfilt(double[] dist_diff_unfilt, double sigmaValue)
        {
            double[] dist_diff = new double[69]; // Return: PG distribution after Gaussian Kernel
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
                //dist_diff_reverse[i] = -dist_diff_temp;

                dist_diff_temp = 0;
            }

            #endregion

            return dist_diff;
        }

        private double[] ShiftPGdistribution(PlanLogPGMergedDataStruct Spot, List<GapPeakAndRangeStruct> GapList)
        {
            double[] PGdist_Shifted = new double[71];

            double SpotEnergy = Spot.Plan_LayerEnergy; // spot의 에너지를 알고 있고,
            double GapPeakToRange = CalcGap(GapList, SpotEnergy); // correction할 List를 알고 있으면,

            double[] xgrid_temp = xgrid_mm.Select(x => x - (Spot.Plan_ZPos + GapPeakToRange)).ToArray(); // X축을 각 spot의 plan에서의 z 깊이만큼 빼줄수 있음

            if (Spot.Plan_ZPos + GapPeakToRange >= 0) // 빼주는 값이 0보다 큰 경우(빔이 0보다 깊에 들어감, 축이 왼쪽으로 이동하는 경우)
            {
                int index = xgrid_temp.ToList().FindIndex(a => a >= xgrid_mm[0] && a <= xgrid_mm[1]); // 옮겨진 X 축에서 -105 ~ -102 사이에 오는 index
                double left = xgrid_temp[index] - xgrid_mm[0];  // 내분점 left 길이 (index 기준)
                double right = xgrid_mm[1] - xgrid_temp[index]; // 내분점 right 길이

                for (int i = 0; i < 71 - index - 1; i++)
                {
                    PGdist_Shifted[i] = (right * Spot.ChannelCount_71[index + i] + left * Spot.ChannelCount_71[index + i - 1]) / 3; // 내분, [index] 기준에서 [index-1] 기준으로 옮겨옴에 주의
                }
                for (int i = 71 - index - 1; i < 71; i++)
                {
                    PGdist_Shifted[i] = PGdist_Shifted[71 - index - 1 - 1];
                }
            }
            else // 빼주는 값이 0보다 작은 경우(빔이 0보다 얕게 들어감, 축이 오른쪽으로 이동하는 경우)
            {
                int index = xgrid_temp.ToList().FindIndex(a => a >= xgrid_mm[69] && a <= xgrid_mm[70]);
                double left = xgrid_temp[index] - xgrid_mm[69];
                double right = xgrid_mm[70] - xgrid_temp[index];

                for (int i = 0; i < index + 1; i++)
                {
                    PGdist_Shifted[70 - index + i] = (right * Spot.ChannelCount_71[i + 1] + left * Spot.ChannelCount_71[i]) / 3;
                }

                if (index < 69)
                {
                    for (int i = 0; i < 70 - index; i++)
                    {
                        PGdist_Shifted[i] = PGdist_Shifted[70 - index];
                    }
                }
            }

            return PGdist_Shifted;
        }

        private double CalcGap(List<GapPeakAndRangeStruct> GapList, double SpotEnergy)
        {
            int index = GapList.FindIndex(spot => spot.Energy >= SpotEnergy);

            double Gap = GapList[index - 1].GapValue + ((GapList[index].GapValue - GapList[index - 1].GapValue) / (GapList[index].Energy - GapList[index - 1].Energy) * (SpotEnergy - GapList[index - 1].Energy));

            //double[] Xrange = {GapList[index - 1].Energy , GapList[index].Energy };
            //double[] Yrange = {GapList[index - 1].GapValue , GapList[index].GapValue };
            //double Gap = LinearInterp(Xrange, Yrange, SpotEnergy);

            return Gap;
        }

        private double LinearInterp(double[] Xrange, double[] Yrange, double X)
        {
            return ((X - Xrange[0]) * Yrange[1] + (Xrange[1] - X) * Yrange[0]) / (Xrange[1] - Xrange[0]);
        }

        private bool[,] CreateGridFlag_BeamRangeMap(double[] xpos, double[] ypos, List<PlanLogPGMergedDataStruct> TotalSpots, double Xpitch, double Ypitch)
        {
            bool[,] flag_grid = new bool[xpos.Length, ypos.Length];
            int NumberOfSpots = TotalSpots.Count();

            for (int x = 0; x < xpos.Length; x++)
            {
                for (int y = 0; y < ypos.Length; y++)
                {
                    for (int i = 0; i < NumberOfSpots; i++)
                    {
                        double spot_Xpos = TotalSpots[i].Log_XPos;
                        double spot_Ypos = TotalSpots[i].Log_YPos;

                        bool flag_X = (xpos[x] - (Xpitch / 2) < spot_Xpos) && (xpos[x] + (Xpitch / 2) > spot_Xpos);
                        bool flag_Y = (ypos[y] - (Ypitch / 2) < spot_Ypos) && (ypos[y] + (Ypitch / 2) > spot_Ypos);

                        if (flag_X && flag_Y)
                        {
                            flag_grid[x, y] = true;
                            break;
                        }
                    }
                }
            }



            return flag_grid;
        }

        private List<BeamRangeMapStruct> DrawBeamRangeMap(double[,] map_diff, double[,] map_MU, float Shift_max, float Shift_min, double[] xpos, double[] ypos, double cutoff_MU)
        {
            List<BeamRangeMapStruct> BeamRangeMap = new List<BeamRangeMapStruct>(); // Return

            for (int i = 0; i < xpos.Length; i++)
            {
                for (int j = 0; j < ypos.Length; j++)
                {
                    if (map_MU[i, j] > cutoff_MU)
                    {
                        BeamRangeMap.Add(new BeamRangeMapStruct(-ypos[j], xpos[i], map_MU[i, j], SetColor(map_diff[i, j], Shift_min, Shift_max)));
                    }
                    else
                    {
                        BeamRangeMap.Add(new BeamRangeMapStruct(-ypos[j], xpos[i], 0, SetColor(map_diff[i, j], Shift_min, Shift_max)));
                    }
                }
            }
            BeamRangeMap.Add(new BeamRangeMapStruct(-10000, -10000, 20, SetColor(0, Shift_min, Shift_max)));

            return BeamRangeMap;
        }

        private SolidColorBrush SetColor(double Diff, float min, float max) // 파일로 받아서 작성하도록 수정 필요
        {
            float alpha = 1f;
            float R, G, B;

            float interval = max - min;

            if (Diff == -10000)
            {
                R = 1f;
                G = 1f;
                B = 1;
            }
            else
            {
                if (Diff <= min)
                {
                    R = 0f;
                    G = 0f;
                    B = 0.0514f;
                }
                else if (Diff < min + (1 * interval / 20))
                {
                    R = 0f;
                    G = 0.051f;
                    B = 0.667f;
                } //
                else if (Diff < min + (2 * interval / 20))
                {
                    R = 0.004f;
                    G = 0.098f;
                    B = 0.804f;
                } //
                else if (Diff < min + (3 * interval / 20))
                {
                    R = 0.004f;
                    G = 0.145f;
                    B = 0.941f;
                } //
                else if (Diff < min + (4 * interval / 20))
                {
                    R = 0.063f;
                    G = 0.259f;
                    B = 0.984f;
                } //
                else if (Diff < min + (5 * interval / 20))
                {
                    R = 0.141f;
                    G = 0.396f;
                    B = 0.965f;
                } //
                else if (Diff < min + (6 * interval / 20))
                {
                    R = 0.216f;
                    G = 0.522f;
                    B = 0.949f;
                } //
                else if (Diff < min + (7 * interval / 20))
                {
                    R = 0.400f;
                    G = 0.627f;
                    B = 0.945f;
                } //
                else if (Diff < min + (8 * interval / 20))
                {
                    R = 0.584f;
                    G = 0.733f;
                    B = 0.945f;
                } //
                else if (Diff < min + (9 * interval / 20))
                {
                    R = 0.757f;
                    G = 0.835f;
                    B = 0.941f;
                } //
                else if (Diff < min + (10 * interval / 20))
                {
                    R = 0.941f;
                    G = 0.941f;
                    B = 0.941f;
                }  //
                else if (Diff < min + (11 * interval / 20))
                {
                    R = 0.933f;
                    G = 0.855f;
                    B = 0.757f;
                }  //
                else if (Diff < min + (12 * interval / 20))
                {
                    R = 0.925f;
                    G = 0.765f;
                    B = 0.576f;
                }  //
                else if (Diff < min + (13 * interval / 20))
                {
                    R = 0.918f;
                    G = 0.682f;
                    B = 0.408f;
                }  //
                else if (Diff < min + (14 * interval / 20))
                {
                    R = 0.910f;
                    G = 0.596f;
                    B = 0.224f;
                }  //
                else if (Diff < min + (15 * interval / 20))
                {
                    R = 0.890f;
                    G = 0.482f;
                    B = 0.141f;
                }  //
                else if (Diff < min + (16 * interval / 20))
                {
                    R = 0.871f;
                    G = 0.369f;
                    B = 0.059f;
                }  //
                else if (Diff < min + (17 * interval / 20))
                {
                    R = 0.820f;
                    G = 0.267f;
                    B = 0.008f;
                }  //
                else if (Diff < min + (18 * interval / 20))
                {
                    R = 0.718f;
                    G = 0.180f;
                    B = 0.004f;
                }  //
                else if (Diff < min + (19 * interval / 20))
                {
                    R = 0.608f;
                    G = 0.090f;
                    B = 0.004f;
                }  //
                else
                {
                    R = 0.510f;
                    G = 0.008f;
                    B = 0f;
                }
            }

            var color = new SolidColorBrush(Color.FromScRgb(alpha, R, G, B));
            //var color = Color.FromScRgb(alpha, R, G, B);
            return color;
        }

        private double[] GenerateXgrid_mm()
        {
            double[] xgrid = new double[72];
            double[] xgrid_mm = new double[71];

            double CameraCenterDepth = 100;

            for (int i = 0; i < 72; i++)
            {
                xgrid[i] = -106.5 + 3 * i;
            }

            for (int i = 0; i < 71; i++)
            {
                //xgrid_mm[i] = -105 + 3 * i;
                xgrid_mm[i] = -105 + 3 * i + CameraCenterDepth;
            }

            return xgrid_mm;
        }

        private (double[], double[]) BeamRangeMapParameterSetting(int[] GridRange, int GridPitch)
        {
            int NumberOfGrid = ((GridRange[1] - GridRange[0]) / GridPitch) + 1;

            double[] xpos = new double[NumberOfGrid];
            double[] ypos = new double[NumberOfGrid];

            for (int i = 0; i < NumberOfGrid; i++)
            {
                xpos[i] = GridRange[0] + GridPitch * i;
                ypos[i] = GridRange[0] + GridPitch * i;
            }

            return (xpos, ypos);
        }

        private List<PGStruct> SplitDataIntoSpots_Sync(List<PGStruct> PG_raw, int width, int ULD, int LLD)
        {
            List<PGStruct> PG_114 = new List<PGStruct>(); // Return

            bool isSpot = false;
            int DataCount = PG_raw.Count();

            int Index_SpotStart = 0;
            int Index_SpotEnd = 0;

            for (int index = 4; index < DataCount; index++) // 5번째부터 마지막까지
            {
                int sum_cnt = SumCount_Width(PG_raw, index, width); // width 고려 안되어있음

                if (isSpot == false)
                {
                    if (sum_cnt > ULD)
                    {
                        isSpot = true;
                        Index_SpotStart = index;
                        index += width;
                    }
                }
                else // isSpot == true
                {
                    if (sum_cnt < LLD)
                    {
                        isSpot = false;
                        Index_SpotEnd = index;
                        index += width;

                        PG_114.Add(SplitToSpots(PG_raw, Index_SpotStart, Index_SpotEnd));
                    }
                }
            }
            return PG_114;
        }

        private async Task<List<PGStruct>> SplitDataIntoSpots(List<PGStruct> PG_raw, int width, int ULD, int LLD)
        {
            List<PGStruct> PG_114 = new List<PGStruct>(); // Return

            await Task.Run(() =>
            {
                bool isSpot = false;
                int DataCount = PG_raw.Count();

                int Index_SpotStart = 0;
                int Index_SpotEnd = 0;

                for (int index = 4; index < DataCount; index++)
                {
                    int sum_cnt = SumCount_Width(PG_raw, index, width);

                    if (isSpot == false)
                    {
                        if (sum_cnt > ULD)
                        {
                            isSpot = true;
                            Index_SpotStart = index;
                            index += width;
                        }
                    }
                    else // isSpot == true
                    {
                        if (sum_cnt < LLD)
                        {
                            isSpot = false;
                            Index_SpotEnd = index;
                            index += width;

                            PG_114.Add(SplitToSpots(PG_raw, Index_SpotStart, Index_SpotEnd));
                        }
                    }
                }
            });
            return PG_114;
        }

        private PGStruct SplitToSpots(List<PGStruct> PGdata_Raw, int StartIndex, int EndIndex)
        {
            PGStruct SingleSpot = new PGStruct();

            SingleSpot.TriggerInputStartTime = PGdata_Raw[StartIndex].TriggerInputStartTime;
            SingleSpot.TriggerInputEndTime = PGdata_Raw[EndIndex].TriggerInputStartTime;      // TriggerInputEndTime -> TriggerInputStartTime 수정(2022-01-09 19:46)

            //SingleSpot.RealTime_Start = MeasurementStartTime.AddMilliseconds(SingleSpot.TriggerInputStartTime / 1000);
            //SingleSpot.RealTime_End = MeasurementStartTime.AddMilliseconds(SingleSpot.TriggerInputStartTime / 1000);

            for (int i = 0; i < 144; i++)
            {
                int tempSum = 0;
                for (int j = StartIndex; j < EndIndex; j++) // EndIndex + 1 -> EndIndex 수정(2022-01-09 19:59)
                {
                    tempSum += PGdata_Raw[j].ChannelCount[i];
                }
                SingleSpot.ChannelCount[i] = tempSum;
            }

            return SingleSpot;
        }

        private async Task<List<PlanLogMergedDataStruct>> MergePlanLog(List<PlanStruct_NCC> Plan, List<LogStruct_NCC> Log, Mode opt, int LayerNumber, int tol)
        {
            List<PlanLogMergedDataStruct> PlanLog = new List<PlanLogMergedDataStruct>();

            await Task.Run(() =>
            {
                if (opt == Mode.RealTime)
                {
                    LayerNumber = Log.Last().LayerNumber;
                }
                else // PostProcessing
                {
                    LayerNumber = Plan.Last().LayerNumber;
                }


                for (int Layer = 0; Layer < LayerNumber + 1; Layer++)
                {
                    List<PlanStruct_NCC> Plan_SingleLayer = Plan.FindAll((PlanStruct_NCC P) => P.LayerNumber == Layer).OrderBy(P => P.YPosition).ThenBy(P => P.XPosition).ToList();
                    List<LogStruct_NCC> Log_SingleLayer = Log.FindAll((LogStruct_NCC L) => L.LayerNumber == Layer);

                    int Index_Plan = 0;
                    for (int Index_Log = 0; Index_Log < Log_SingleLayer.Count; Index_Log++)
                    {
                        PlanLogMergedDataStruct tempPlanLog = new PlanLogMergedDataStruct();

                        // *********************************************** Logic 설명 *********************************************** //
                        // Tuning인 경우, Log file의 정보만을 이용하여 Plan Log 병합                                                  //
                        // Tuning아닌 경우, 2D difference를 검사함                                                                    //
                        // (1) 2D difference가 Tolerance 만족하는 경우, Plan Log 병합                                                 //
                        // (2) 만족하지 않는 경우, Index_Plan을 -2, -1, +1, +2 로 설정하여 Tolerance를 만족하는 경우가 있는지 확인,   //
                        //     만족하는 경우가 있다면 Plan의 Index를 조절하여 Plan Log 병합                                           //
                        //     만족하는 경우가 없다면 MessageBox 내보내고 return                                                      //
                        // ********************************************************************************************************** //

                        if (Log_SingleLayer[Index_Log].LayerID.Contains("tuning"))
                        {
                            tempPlanLog = PlanLogMerge_Tuning(Plan_SingleLayer[Index_Plan], Log_SingleLayer[Index_Log]);
                        }
                        else
                        {
                            if (CalDifference_2D(Plan_SingleLayer[Index_Plan], Log_SingleLayer[Index_Log]) <= tol)
                            {
                                tempPlanLog = PlanLogMerge(Plan_SingleLayer[Index_Plan], Log_SingleLayer[Index_Log]);
                                Index_Plan++;
                            }
                            else
                            {
                                if (CalDifference_2D(Plan_SingleLayer[Index_Plan - 1], Log_SingleLayer[Index_Log]) <= tol)
                                {
                                    Index_Plan--;
                                }
                                else if (CalDifference_2D(Plan_SingleLayer[Index_Plan + 1], Log_SingleLayer[Index_Log]) <= tol)
                                {
                                    Index_Plan++;
                                }
                                else if (CalDifference_2D(Plan_SingleLayer[Index_Plan - 2], Log_SingleLayer[Index_Log]) <= tol)
                                {
                                    Index_Plan -= 2;
                                }
                                else if (CalDifference_2D(Plan_SingleLayer[Index_Plan + 2], Log_SingleLayer[Index_Log]) <= tol)
                                {
                                    Index_Plan += 2;
                                }
                                else
                                {
                                    MessageBox.Show($"Plan_NCC file and Log_NCC file can't matched.", $"Error Code JJR0000", MessageBoxButton.OK, MessageBoxImage.Error);
                                    PlanLog = null;
                                    return;
                                }

                                tempPlanLog = PlanLogMerge(Plan_SingleLayer[Index_Plan], Log_SingleLayer[Index_Log]);
                                Index_Plan++;
                            }
                        }
                        PlanLog.Add(tempPlanLog);
                    }
                }
            });
            return PlanLog;
        }

        private List<PlanLogPGMergedDataStruct> MergePlanLogPG(List<PlanLogMergedDataStruct> PlanLog, List<PGStruct> PG, bool MergePGandPlanLogwithTimeInfo)
        {
            List<PlanLogPGMergedDataStruct> MergedData = new List<PlanLogPGMergedDataStruct>(); // Return

            int NumOfCompareSpot = 10; // 임시
            int TimeMargin = 100000;   // 임시, 100 ms

            int NumOfPGSpots = PG.Count;

            int Idx_PG = 0;
            int Idx_PlanLog = 0;

            while (Idx_PG < PG.Count && Idx_PlanLog < PlanLog.Count)
            {
                if (Math.Abs(PG[Idx_PG].Tick - PlanLog[Idx_PlanLog].Tick) < TimeMargin) // 시간 차가 100 ms 이하일 때
                {
                    MergedData.Add(NewMerge(PG[Idx_PG], PlanLog[Idx_PlanLog]));

                    Idx_PG++;
                    Idx_PlanLog++;
                }
                else
                {
                    bool isSpotMatched = false;
                    for (int Idx_PG_temp = Idx_PG; Idx_PG_temp < Math.Min(Idx_PG + NumOfCompareSpot, NumOfPGSpots); Idx_PG_temp++) // 현재의 PG index(=Idx_PG)로부터 뒤쪽으로 10개(=NumOfCompareSpot)까지의 PG trigger와 비교한다
                    {
                        if (Math.Abs(PG[Idx_PG_temp].Tick - PlanLog[Idx_PlanLog].Tick) < TimeMargin)
                        {
                            MergedData.Add(NewMerge(PG[Idx_PG_temp], PlanLog[Idx_PlanLog]));

                            Idx_PG = Idx_PG_temp + 1;
                            Idx_PlanLog++;

                            isSpotMatched = true;
                            break;
                        }
                    }

                    if (isSpotMatched == false)
                    {                        
                        Trace.WriteLine($"log spot number {Idx_PlanLog}({PlanLog[Idx_PlanLog].Log_State}) was not matched with PG trigger");

                        MergedData.Add(NewMerge_NotMatched(PlanLog[Idx_PlanLog]));
                        Idx_PlanLog++;

                        if (Idx_PG + NumOfCompareSpot > NumOfPGSpots)
                        {
                            break;
                        }
                    }
                }
            }

            return MergedData;
        }

        private PlanLogPGMergedDataStruct NewMerge(PGStruct PG, PlanLogMergedDataStruct PlanLog)
        {
            PlanLogPGMergedDataStruct Spot = new PlanLogPGMergedDataStruct();

            Spot.Log_LayerNumber = PlanLog.Log_LayerNumber;
            Spot.Log_LayerID = PlanLog.Log_LayerID;
            Spot.Log_XPos = PlanLog.Log_XPos;
            Spot.Log_YPos = PlanLog.Log_YPos;
            Spot.Log_State = PlanLog.Log_State;
            Spot.Log_StartTime = PlanLog.Log_StartTime;
            Spot.Log_EndTime = PlanLog.Log_EndTime;

            Spot.Plan_LayerEnergy = PlanLog.Plan_LayerEnergy;
            Spot.Plan_XPos = PlanLog.Plan_XPos;
            Spot.Plan_YPos = PlanLog.Plan_YPos;
            Spot.Plan_ZPos = PlanLog.Plan_ZPos;
            Spot.Plan_MU = PlanLog.Plan_MU;

            Spot.ChannelCount = PG.ChannelCount;
            Spot.TriggerStart = PG.TriggerInputStartTime;
            Spot.TriggerEnd = PG.TriggerInputEndTime;

            Spot.ChannelCount_71 = getPGdist(PG.ChannelCount, true, false);

            return Spot;
        }

        private PlanLogPGMergedDataStruct NewMerge_NotMatched(PlanLogMergedDataStruct PlanLog)
        {
            PlanLogPGMergedDataStruct Spot = new PlanLogPGMergedDataStruct();

            Spot.Log_LayerNumber = PlanLog.Log_LayerNumber;
            Spot.Log_LayerID = PlanLog.Log_LayerID;
            Spot.Log_XPos = PlanLog.Log_XPos;
            Spot.Log_YPos = PlanLog.Log_YPos;
            Spot.Log_State = PlanLog.Log_State;
            Spot.Log_StartTime = PlanLog.Log_StartTime;
            Spot.Log_EndTime = PlanLog.Log_EndTime;

            Spot.Plan_LayerEnergy = PlanLog.Plan_LayerEnergy;
            Spot.Plan_XPos = PlanLog.Plan_XPos;
            Spot.Plan_YPos = PlanLog.Plan_YPos;
            Spot.Plan_ZPos = PlanLog.Plan_ZPos;
            Spot.Plan_MU = PlanLog.Plan_MU;

            for (int k = 0; k < 144; k++)
            {
                Spot.ChannelCount[k] = 0;
            }
            Spot.TriggerStart = -10000;
            Spot.TriggerEnd = -10000;

            for (int k = 0; k < 71; k++)
            {
                Spot.ChannelCount_71[k] = 1;
            }

            return Spot;
        }





        private double[] getPGdist(int[] Ch_144_pre, bool missingVal, bool corrFactorsCheck)
        {
            double[] Ch_144 = ConvertIntArrayToDoubleArray(Ch_144_pre);
            double[] Ch_71 = new double[71];

            if (missingVal == true)
            {
                Ch_144 = FillMissingScin(Ch_144);
            }

            if (corrFactorsCheck == true)
            {
                List<double[]> corrFactors = ReadCorrFactorsFile(VM_DAQsetting.corrFactorsDirectory);
                Ch_144 = GetEfficiencyCorrected(Ch_144, corrFactors);
            }

            Ch_71 = MakeFrom144To71Channel(Ch_144);

            return Ch_71;
        }

        private double[] MakeFrom144To71Channel(double[] Ch_144)
        {
            double[] Ch_72 = new double[72];
            double[] Ch_71 = new double[71];

            double[] cnt_row1 = new double[36];
            double[] cnt_row2 = new double[36];
            double[] cnt_row3 = new double[36];
            double[] cnt_row4 = new double[36];

            double[] cnt_top = new double[36];
            double[] cnt_bot = new double[36];

            // ChCounts 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            // corrFactors 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //   0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17 ll 18  19  20  21  22  23  24  25  26  27  28  29  30  31  32  33  34  35 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            for (int i = 0; i < 18; i++)
            {
                cnt_row1[i] = Ch_144[89 - i];
                cnt_row2[i] = Ch_144[107 - i];
                cnt_row3[i] = Ch_144[125 - i];
                cnt_row4[i] = Ch_144[143 - i];

                cnt_row1[i + 18] = Ch_144[17 - i];
                cnt_row2[i + 18] = Ch_144[35 - i];
                cnt_row3[i + 18] = Ch_144[53 - i];
                cnt_row4[i + 18] = Ch_144[71 - i];
            }

            for (int i = 0; i < 36; i++)
            {
                cnt_bot[i] = cnt_row3[i] + cnt_row4[i];
                cnt_top[i] = cnt_row1[i] + cnt_row2[i];
            }

            for (int i = 0; i < 36; i++)
            {
                Ch_72[2 * i] = cnt_bot[i];
                Ch_72[2 * i + 1] = cnt_top[i];
            }

            for (int i = 0; i < 71; i++)
            {
                Ch_71[i] = (Ch_72[i] + Ch_72[i + 1]) / 2;
            }

            return Ch_71;
        }

        private double[] GetEfficiencyCorrected(double[] Ch_144, List<double[]> corrFactors)
        {
            // ChCounts 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            // corrFactors 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //   0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17 ll 18  19  20  21  22  23  24  25  26  27  28  29  30  31  32  33  34  35 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            for (int i = 0; i < 18; i++)
            {
                Ch_144[89 - i] = Ch_144[89 - i] * corrFactors[0][i];
                Ch_144[107 - i] = Ch_144[107 - i] * corrFactors[1][i];
                Ch_144[125 - i] = Ch_144[125 - i] * corrFactors[2][i];
                Ch_144[143 - i] = Ch_144[143 - i] * corrFactors[3][i];

                Ch_144[17 - i] = Ch_144[17 - i] * corrFactors[0][i + 18];
                Ch_144[35 - i] = Ch_144[35 - i] * corrFactors[1][i + 18];
                Ch_144[53 - i] = Ch_144[53 - i] * corrFactors[2][i + 18];
                Ch_144[71 - i] = Ch_144[71 - i] * corrFactors[3][i + 18];
            }

            return Ch_144;
        }

        private List<double[]> ReadCorrFactorsFile(string path)
        {
            List<double[]> corrFactors = new List<double[]>(4);

            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        string lines = null;
                        string[] tempString = null;

                        while ((lines = sr.ReadLine()) != null)
                        {
                            tempString = lines.Split("\t");
                            double[] tempCorrFactors = new double[36];

                            for (int i = 0; i < 36; i++)
                            {
                                tempCorrFactors[i] = Convert.ToDouble(tempString[i]);
                            }
                            corrFactors.Add(tempCorrFactors);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"File is not exist at {path}. Analysis preceeds without efficiency correction.", $"No File", MessageBoxButton.OK, MessageBoxImage.Information);
                for (int j = 0; j < 4; j++)
                {
                    double[] temp = new double[36];
                    for (int i = 0; i < 36; i++)
                    {
                        temp[i] = 1;
                    }
                    corrFactors.Add(temp);
                }
            }

            return corrFactors;
        }

        private double[] FillMissingScin(double[] Ch_144)
        {
            // SDATA_BUFFER 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  98  97  96  95  94  93  92  91  90  89  88  87  86  85  84  83  82  81 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 116 115 114 113 112 111 110 109 108 107 106 105 104 103 102 101 100  99 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 134 133 132 131 130 129 128 127 126 125 124 123 122 121 120 119 118 117 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 152 151 150 149 148 147 146 145 144 143 142 141 140 139 138 137 136 135 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            // ChCounts 기준
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //
            //  89  88  87  86  85  84  83  82  81  80  79  78  77  76  75  74  73  72 ll 17  16  15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0 //
            // 107 106 105 104 103 102 101 100  99  98  97  96  95  94  93  92  91  90 ll 35  34  33  32  31  30  29  28  27  26  25  24  23  22  21  20  19  18 //
            // 125 124 123 122 121 120 119 118 117 116 115 114 113 112 111 110 109 108 ll 53  52  51  50  49  48  47  46  45  44  43  42  41  40  39  38  37  36 //
            // 143 142 141 140 139 138 137 136 135 134 133 132 131 130 129 128 127 126 ll 71  70  69  68  67  66  65  64  63  62  61  60  59  58  57  56  55  54 //
            // --------------------------------- Left ---------------------------------ll-------------------------------- Right -------------------------------- //

            //Ch_144[0] = Ch_144[1];
            //Ch_144[18] = Ch_144[19];
            //Ch_144[36] = Ch_144[37];

            // ***** Method 1 ***** //
            //Ch_144[27] = Ch_144[28];
            //Ch_144[26] = Ch_144[25];

            // ***** Method 2 ***** //
            double sum1 = 0;
            double sum2 = 0;
            double sum3 = 0;
            double sum4 = 0;

            double ratio14 = 0;
            double ratio24 = 0;
            double ratio21 = 0;
            double ratio34 = 0;

            for (int k = 0; k < 18; k++)
            {
                sum1 += Ch_144[k] + Ch_144[72 + k];
                sum4 += Ch_144[54 + k] + Ch_144[126 + k];
            }
            ratio14 = sum1 / sum4;
            Ch_144[0] = Ch_144[54] * ratio14; // 1번 채널


            sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;

            for (int k = 0; k < 18; k++)
            {
                sum2 += Ch_144[18 + k] + Ch_144[90 + k];
                sum4 += Ch_144[54 + k] + Ch_144[126 + k];
            }
            ratio24 = sum2 / sum4;
            Ch_144[18] = Ch_144[54] * ratio24; // 19번 채널


            sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;

            for (int k = 0; k < 18; k++)
            {
                sum1 += Ch_144[k] + Ch_144[72 + k];
                sum2 += Ch_144[18 + k] + Ch_144[90 + k];
            }
            ratio21 = sum2 / sum1;
            Ch_144[26] = Ch_144[8] * ratio21; // 27번 채널


            sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;

            for (int k = 0; k < 18; k++)
            {
                sum1 += Ch_144[k] + Ch_144[72 + k];
                sum2 += Ch_144[18 + k] + Ch_144[90 + k];
            }
            ratio21 = sum2 / sum1;
            Ch_144[27] = Ch_144[9] * ratio21; // 28번 채널


            sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;

            for (int k = 0; k < 18; k++)
            {
                sum3 += Ch_144[36 + k] + Ch_144[108 + k];
                sum4 += Ch_144[54 + k] + Ch_144[126 + k];
            }
            ratio34 = sum3 / sum4;
            Ch_144[36] = Ch_144[54] * ratio34; // 37번 채널


            return Ch_144;
        }

        private double[] ConvertIntArrayToDoubleArray(int[] IntArray)
        {
            return IntArray.Select(i => (double)i).ToArray();
        }

        private int SumCount_Width(List<PGStruct> PGdata_Raw, int index, int width)
        {
            int sum_cnt = 0;
            int temp = 0;
            for (int i = index - 4; i < index + 1; i++) // width 5일 경우 (center-2, center-1, center, center+1, center+2) 의 합산값을 리턴
            {
                sum_cnt += PGdata_Raw[i].SumCounts;
                temp++;
            }

            return sum_cnt;
        }

        private PlanLogMergedDataStruct PlanLogMerge_Tuning(PlanStruct_NCC Plan, LogStruct_NCC Log)
        {
            PlanLogMergedDataStruct PlanLog = new PlanLogMergedDataStruct();

            PlanLog.Log_LayerNumber = Log.LayerNumber;
            PlanLog.Log_LayerID = Log.LayerID;
            PlanLog.Log_XPos = Log.XPosition;
            PlanLog.Log_YPos = Log.YPosition;
            PlanLog.Log_State = Log.State;
            PlanLog.Log_StartTime = Log.StartTime;
            PlanLog.Log_EndTime = Log.EndTime;

            PlanLog.Plan_LayerEnergy = 0;
            PlanLog.Plan_XPos = 0;
            PlanLog.Plan_YPos = 0;
            PlanLog.Plan_ZPos = 0;
            PlanLog.Plan_MU = 0;

            PlanLog.Tick = Log.Tick;

            return PlanLog;
        }

        private PlanLogMergedDataStruct PlanLogMerge(PlanStruct_NCC Plan, LogStruct_NCC Log)
        {
            PlanLogMergedDataStruct PlanLog = new PlanLogMergedDataStruct();

            PlanLog.Log_LayerNumber = Log.LayerNumber;
            PlanLog.Log_LayerID = Log.LayerID;
            PlanLog.Log_XPos = Log.XPosition;
            PlanLog.Log_YPos = Log.YPosition;
            PlanLog.Log_State = Log.State;
            PlanLog.Log_StartTime = Log.StartTime;
            PlanLog.Log_EndTime = Log.EndTime;

            PlanLog.Plan_LayerEnergy = Plan.LayerEnergy;
            PlanLog.Plan_XPos = Plan.XPosition;
            PlanLog.Plan_YPos = Plan.YPosition;
            PlanLog.Plan_ZPos = Plan.ZPosition;
            PlanLog.Plan_MU = Plan.MU;

            PlanLog.Tick = Log.Tick; // 0528

            return PlanLog;
        }

        private double CalDifference_2D(PlanStruct_NCC Plan, LogStruct_NCC Log)
        {
            return Math.Sqrt(Math.Pow(Plan.XPosition - Log.XPosition, 2) + Math.Pow(Plan.YPosition - Log.YPosition, 2));
        }

        private List<double[]> GetGaussianWeightMap_SpotMap(List<PlanLogPGMergedDataStruct> Spots)
        {
            List<double[]> GaussianWeightMap = new List<double[]>(); // Return

            int SpotsCounts = Spots.Count;

            for (int i = 0; i < SpotsCounts; i++)
            {
                double[] Distance = new double[SpotsCounts];

                for (int j = 0; j < SpotsCounts; j++)
                {
                    Distance[j] = Math.Sqrt(Math.Pow(Spots[i].Log_XPos - Spots[j].Log_XPos, 2) + Math.Pow(Spots[i].Log_YPos - Spots[j].Log_YPos, 2));
                }

                //int AggSigma = 5;
                double AggSigma = 7.8;
                //double AggSigma = 15;
                double[] GaussianWeightMap_temp = new double[SpotsCounts];

                for (int j = 0; j < SpotsCounts; j++)
                {
                    GaussianWeightMap_temp[j] = Math.Exp(-0.5 * Math.Pow(Distance[j] / AggSigma, 2));
                }

                GaussianWeightMap.Add(GaussianWeightMap_temp);
            }

            return GaussianWeightMap;
        }

        private double[] GetRangeDifference_SpotMap(List<PlanLogPGMergedDataStruct> Spots, List<double[]> Map, double PeakToRangeGap)
        {
            int SpotsCounts = Spots.Count;

            double[] RangeDifference = new double[SpotsCounts]; // Return
            double[] range = new double[SpotsCounts];

            for (int i = 0; i < SpotsCounts; i++)
            {
                double[] AggregatedCounts_71 = new double[71];

                for (int j = 0; j < 71; j++)
                {
                    double Counts = 0;

                    for (int k = 0; k < SpotsCounts; k++)
                    {
                        if (Map[i][k] > 0.001)
                        {
                            Counts += Map[i][k] * Spots[k].ChannelCount_71[j];
                        }
                    }
                    AggregatedCounts_71[j] = Counts;
                }

                range[i] = GetRange_ver4p0(xgrid_mm, AggregatedCounts_71, Spots[i].Plan_ZPos);
                //RangeDifference[i] = range[i] - (Spots[i].Plan_ZPos + PeakToRangeGap);
                RangeDifference[i] = range[i] - (Spots[i].Plan_ZPos + PeakToRangeGap) - Globalpush;
            }

            return RangeDifference;
        }

        private List<SpotMapStruct> GetSelectedLayerSpotMap(List<PlanLogPGMergedDataStruct> Spots, double[] RangeDifference, float min, float max)
        {
            List<SpotMapStruct> SpotMap = new List<SpotMapStruct>(); // Return

            int SpotsCounts = Spots.Count;
            for (int i = 0; i < SpotsCounts; i++)
            {
                SpotMapStruct temp = new SpotMapStruct(-Spots[i].Log_YPos, Spots[i].Log_XPos, Spots[i].Plan_MU, SetColor(RangeDifference[i], min, max));
                SpotMap.Add(temp);
            }

            for (int i = 0; i < SpotsCounts; i++)
            {
                Console.WriteLine($"{SpotMap[i].X}, {SpotMap[i].Y}, {RangeDifference[i]}");
            }

            return SpotMap;
        }

        #endregion
    }
}