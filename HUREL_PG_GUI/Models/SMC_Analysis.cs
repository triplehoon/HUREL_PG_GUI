using CenterSpace.NMath.Core;
using HUREL_PG_GUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HUREL_PG_GUI.Models
{
    public class PlanPGMergedDataStruct : PlanStruct_SMC
    {
        public int[] ChannelCount = new int[144];
        public double[] ChannelCount_71 = new double[71];
    }

    public class RangeInPMMAStruct
    {
        public double Energy;
        public double Range;
    }

    public class TriggerStruct
    {
        public int[] Trigger;
        public BeamStatus beamStatus;
    }

    public enum BeamStatus
    {
        Tuning,
        Layer
    }

    public class SMCAnalysisClass
    {
        private List<PGStruct> PG_144 = new List<PGStruct>();
        private List<double[]> PG_71 = new List<double[]>();

        private double GlobalPush = 0;
        private int[] GridRange = { -42, 42 };
        private int GridPitch = 3;
        double[] xgrid_mm = new double[71];

        public SMCAnalysisClass()
        {
            xgrid_mm = GenerateXgrid_mm();
        }

        public void TestFunction()
        {
            VM_LineScanning.isStart = true;
            VM_LineScanning._EventTransfer.RaiseEvent();
        }


        public async Task<List<PlanPGMergedDataStruct>> GenerateMergedData_PostProcessing(List<PlanStruct_SMC> Plan, List<PGStruct> PG_raw)
        {
            List<PlanPGMergedDataStruct> MergedData = new List<PlanPGMergedDataStruct>();

            List<TriggerStruct> data_trig = genTrigbyCount_SMC(PG_raw, 2.5); // Tuning, Normal 두개 다 포함
            //List<TriggerStruct> data_trig = await Task.Run(() => genTrigbyCount_SMC(PG_raw, 2.5)); // Tuning, Normal 두개 다 포함
            List<TriggerStruct> data_trig_Normal = data_trig.FindAll(x => x.beamStatus == BeamStatus.Layer);

            if (Plan.Last().a3_LayerNumber != data_trig_Normal.Count)
            {
                MessageBox.Show($"Layer in Plan({Plan.Last().a3_LayerNumber}) != Layer in FPGA data({data_trig_Normal.Count})");
                return MergedData;
            }

            PG_144 = GetSegmentPGdist(PG_raw, Plan, data_trig_Normal);

            if (PG_144.Count != Plan.Count)
            {
                MessageBox.Show($"Segments in Plan({Plan.Count}) != Segments in FPGA data({PG_144.Count})");
                return MergedData;
            }

            MergedData = MergePlanPG(Plan, PG_144);

            return MergedData;
        }

        public async Task<List<PlanPGMergedDataStruct>> GenerateMergedData_RealTime(List<PlanStruct_SMC> Plan_AllLayer, List<PGStruct> PG_raw)
        {
            List<PlanPGMergedDataStruct> MergedData = new List<PlanPGMergedDataStruct>();            

            List<TriggerStruct> data_trig = genTrigbyCount_SMC(PG_raw, 2.5); // Tuning, Normal 두개 다 포함
            //List<TriggerStruct> data_trig = await Task.Run(() => genTrigbyCount_SMC(PG_raw, 2.5)); // Tuning, Normal 두개 다 포함
            List<TriggerStruct> data_trig_Normal = data_trig.FindAll(x => x.beamStatus == BeamStatus.Layer);

            List<PlanStruct_SMC> Plan = (from segments in Plan_AllLayer
                                         where segments.a3_LayerNumber <= data_trig_Normal.Count
                                         select segments).ToList();

            if (Plan.Last().a3_LayerNumber != data_trig_Normal.Count)
            {
                MessageBox.Show($"Layer in Plan({Plan.Last().a3_LayerNumber}) != Layer in FPGA data({data_trig_Normal.Count})");
                return MergedData;
            }

            PG_144 = GetSegmentPGdist(PG_raw, Plan, data_trig_Normal);

            if (PG_144.Count != Plan.Count)
            {
                MessageBox.Show($"Segments in Plan({Plan.Count}) != Segments in FPGA data({PG_144.Count})");
                return MergedData;
            }

            MergedData = MergePlanPG(Plan, PG_144);

            return MergedData;
        }

        // 미완성
        public async Task<List<SpotMapStruct>> GenerateSpotMap(List<PlanPGMergedDataStruct> mergedData, int SelectedLayer)
        {
            List<double[]> GaussianWeightMap = new List<double[]>();
            double[] RangeDifference;
            List<SpotMapStruct> SpotMap = new List<SpotMapStruct>(); // Return  

            List<PlanPGMergedDataStruct> Segments = (from segments in mergedData
                                                     where segments.a3_LayerNumber == SelectedLayer
                                                     select segments).ToList();
            double PlannedRange = CalculateRange(VM_LineScanning._Configuration_SMC.RangeInPMMA, Segments.Last().a5_Energy);

            GaussianWeightMap = GetGaussianWeightMap_SpotMap(Segments);
            RangeDifference = GetRangeDifference_SpotMap(Segments, GaussianWeightMap, PlannedRange - 65);
            SpotMap = GetSelectedLayerSpotMap(Segments, RangeDifference, -10f, 10f);

            return SpotMap;
        }

        public async Task<List<BeamRangeMapStruct>> GenerateBeamRangeMap(List<PlanPGMergedDataStruct> mergedData, double sigma, int StartLayer, int LastLayer, double cutoff_MU)
        {
            List<BeamRangeMapStruct> BeamRangeMap = new List<BeamRangeMapStruct>(); // Return

            double[] xpos;
            double[] ypos;

            double[,] map_diff;
            double[,] map_mu;

            float Shift_max = 10f;
            float Shift_min = -10f;

            sigma = 5;

            (xpos, ypos) = BeamRangeMapParameterSetting(GridRange, GridPitch);
            (map_diff, map_mu) = ShiftMerge(xpos, ypos, mergedData, sigma, StartLayer, LastLayer);
            BeamRangeMap = DrawBeamRangeMap(map_diff, map_mu, Shift_max, Shift_min, xpos, ypos, cutoff_MU);

            return BeamRangeMap;
        }



        /// <summary>
        /// 측정한 Chopper Signal 이용하여 Layer에 대한 trig 생성
        /// </summary>
        /// <param name="PG_raw">Continuous PG data</param>
        /// <param name="Threshold">Chopper signal threshold</param>
        /// <returns>Layer split trigger time</returns>
        public List<TriggerStruct> genTrigbyCount_SMC(List<PGStruct> PG_raw, double Threshold)
        {
            List<TriggerStruct> data_trig = new List<TriggerStruct>();

            bool isLayer = false;
            int DataCount = PG_raw.Count();

            int[] data_trig_temp = new int[2];

            for (int index = 0; index < DataCount; index++)
            {
                if (isLayer == false)
                {
                    if (PG_raw[index].ADC < Threshold)
                    {
                        isLayer = true;
                        data_trig_temp[0] = PG_raw[index].TriggerInputEndTime;
                    }
                }
                else
                {
                    if (PG_raw[index].ADC > Threshold)
                    {
                        isLayer = false;
                        data_trig_temp[1] = PG_raw[index].TriggerInputEndTime;

                        BeamStatus tempBeamStatus = new BeamStatus();
                        if (data_trig_temp[1] - data_trig_temp[0] < 10000)
                        {
                            tempBeamStatus = BeamStatus.Tuning;
                        }
                        else
                        {
                            tempBeamStatus = BeamStatus.Layer;
                        }

                        data_trig.Add(new TriggerStruct() { Trigger = data_trig_temp, beamStatus = tempBeamStatus });
                        data_trig_temp = new int[2];
                    }
                }
            }

            #region Debug
            //for (int i = 0; i < data_trig.Count; i++)
            //{
            //    Trace.WriteLine($"{data_trig[i].Trigger[0]}, {data_trig[i].Trigger[1]}, {data_trig[i].beamStatus}");
            //}

            //string FileName = "Debug_SMC_Trig.csv";
            //string Path_Txt = VM_LineScanning_PostProcessing.Directory_DebugFile + "\\" + FileName;
            //using (StreamWriter sr = new StreamWriter(Path_Txt))
            //{
            //    for (int i = 0; i < DataCount; i++)
            //    {
            //        sr.WriteLine($"{PG_raw[i].TriggerInputEndTime}, {PG_raw[i].ADC}");
            //    }
            //}

            //string DebugFileName = "Debug_SMC_Trig.csv";
            //string DebugPath_excel = VM_LineScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            //using (StreamWriter file = new StreamWriter(DebugPath_excel))
            //{
            //    for (int i = 0; i < data_trig.Count(); i++)
            //    {
            //        for (int j = 0; j < 2; j++)
            //        {
            //            file.Write($"{data_trig[i][j]}, ");
            //        }
            //        file.WriteLine($"");
            //    }
            //}
            #endregion

            return data_trig;
        }

        public bool CheckNumberOfLayers_PlanChopperSignal(int Layer_Plan, int Layer_ChopperSignal)
        {
            if (Layer_Plan != Layer_ChopperSignal)
            {
                Trace.WriteLine(
                    $"Total number of layer does not match \n" +
                    $"Layer (Chopper Signal): {Layer_ChopperSignal} \n" +
                    $"Layer (Plan)          : {Layer_Plan}");
                return false;
            }
            else
            {
                Trace.WriteLine(
                    $"Total number of layer matched \n" +
                    $"Layer (Chopper Signal = Plan): {Layer_Plan}");
                return true;
            }
        }

        public List<PGStruct> GetSegmentPGdist(List<PGStruct> PG_raw, List<PlanStruct_SMC> Plan, List<TriggerStruct> Trigger_Layer)
        {
            // 1. Trigger_Layer.Count 로 for문을 돌린다.
            // 2. for 문의 index 를 Layer로 이용하여,
            //    2-1. PG_raw 데이터에서, 해당 Layer의 FPGA_tick Interval을 구한다.
            //    2-2. Plan 데이터에서, 해당 Layer의 segment와 segment별 TimeRatio를 가져온다.
            // 3. 계산된 tick을 이용하여 PG_raw 에서의 segment 별 index를 구한다.
            // 4. 3에서 구한 index를 이용하여 segment 별 PG 분포(PG_144)를 구한다.
            // 5. MergedData 를 구한다(PG_144 -> PG_71 포함)
            // 6. Return

            List<PGStruct> PG_144 = new List<PGStruct>(); // Return

            int NumberOfLayer = Trigger_Layer.Count;
            for (int i = 0; i < NumberOfLayer; i++)
            {
                List<PlanStruct_SMC> Segments = (from segment in Plan
                                                 where segment.a3_LayerNumber == i + 1
                                                 select segment).ToList();

                int LayerTimeTick = Trigger_Layer[i].Trigger[1] - Trigger_Layer[i].Trigger[0];

                List<double> Info_Time = new List<double>();
                List<double> Info_Time_Cum = new List<double>();

                int NumberOfSegments = Segments.Count;
                for (int j = 0; j < NumberOfSegments; j++)
                {
                    Info_Time.Add(LayerTimeTick * Segments[j].a9_TimeRatio);
                    Info_Time_Cum.Add(Info_Time.Sum());
                }

                List<int> SegmentsIndex = new List<int>();
                SegmentsIndex.Add(PG_raw.FindIndex(x => x.TriggerInputStartTime >= Trigger_Layer[i].Trigger[0]));
                int index = 0;
                for (int j = 0; j < PG_raw.Count; j++)
                {
                    if (PG_raw[j].TriggerInputStartTime >= Trigger_Layer[i].Trigger[0] + Info_Time_Cum[index])
                    {
                        SegmentsIndex.Add(j);
                        index++;

                        if (SegmentsIndex.Count > NumberOfSegments)
                        {
                            break;
                        }
                    }
                }

                for (int j = 0; j < NumberOfSegments; j++)
                {
                    PG_144.Add(SplitToSegments(PG_raw, SegmentsIndex[j], SegmentsIndex[j + 1]));
                }

            }

            #region 
            // 1. 선택한 Plan Field(Sphere/Cubic) 중에서 해당 Layer의 segment를 가져온다(이미 4개)
            //List<PlanStruct_SMC> Segments = (from component in Plan_SMC_SelectedField
            //                                 where component.a3_LayerNumber == Layer
            //                                 select component).ToList();

            // 2. 각 Segment 별 FPGA_tick 과 TimeRatio를 이용하여 실제 tick(누적)을 구한다
            //int NumberOfSegments = Segments.Count;
            //double FPGA_tick = Trigger_Layer[1] - Trigger_Layer[0];

            //List<double> Info_Time = new List<double>();
            //List<double> Info_Time_Cum = new List<double>();

            //for (int i = 0; i < NumberOfSegments; i++)
            //{
            //    Info_Time.Add(FPGA_tick * Segments[i].a9_TimeRatio);
            //    Info_Time_Cum.Add(Info_Time.Sum());
            //}

            // 3. 계산된 누적 tick을 이용하여 PG_raw에서의 segment 별 index를 구해줌
            //List<int> Index_Segment_in_PGraw = new List<int>();

            // --- Slow
            //Index_Segment_in_PGraw.Add(PG_raw.FindIndex(x => x.TriggerInputStartTime >= Trigger_Layer[0]));
            //for (int i = 0; i < NumberOfSegments; i++)
            //{
            //    Index_Segment_in_PGraw.Add(PG_raw.FindIndex(x => x.TriggerInputStartTime >= Trigger_Layer[0] + Info_Time_Cum[i]));
            //}

            // --- Fast
            //Index_Segment_in_PGraw.Add(PG_raw.FindIndex(x => x.TriggerInputStartTime >= Trigger_Layer[0]));
            //int idx = 0;
            //for (int i = 0; i < PG_raw.Count; i++)
            //{
            //    if (PG_raw[i].TriggerInputStartTime >= Trigger_Layer[0] + Info_Time_Cum[idx])
            //    {
            //        Index_Segment_in_PGraw.Add(i);
            //        idx++;

            //        if (Index_Segment_in_PGraw.Count > NumberOfSegments)
            //        {
            //            break;
            //        }
            //    }
            //}
            #endregion

            // 4. segment 별 index를 이용하여 PG_144을 구하고, 이를 이용하여 PG_71을 계산함
            //for (int i = 0; i < NumberOfSegments; i++)
            //{
            //    PG_144.Add(SplitToSegments(PG_raw, Index_Segment_in_PGraw[i], Index_Segment_in_PGraw[i + 1]));
            //    //Trace.WriteLine($"{Index_Segment_in_PGraw[i]}");
            //}            

            //for (int i = 0; i < NumberOfSegments; i++)
            //{
            //    PG_71.Add(getPGdist(PG_144[i].ChannelCount, true, false));
            //}

            //#region [Debug] Split time for each segment (디버깅 완료: MATLAB 코드가 잘못됨)
            ////string DebugFileName = "Debug_PG144_Segment_New.csv";
            ////string DebugPath_excel = VM_LineScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            ////using (StreamWriter file = new StreamWriter(DebugPath_excel))
            ////{
            ////    for (int i = 0; i < PG_144.Count(); i++)
            ////    {
            ////        for (int j = 0; j < 144; j++)
            ////        {
            ////            file.Write($"{PG_144[i].ChannelCount[j]}, ");
            ////        }
            ////        file.WriteLine($"");
            ////    }
            ////}

            ////string DebugFileName = "Debug_PG71_Segment_New.csv";
            ////string DebugPath_excel = VM_LineScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
            ////using (StreamWriter file = new StreamWriter(DebugPath_excel))
            ////{
            ////    for (int i = 0; i < PG_71.Count(); i++)
            ////    {
            ////        for (int j = 0; j < 71; j++)
            ////        {
            ////            file.Write($"{PG_71[i][j]}, ");
            ////        }
            ////        file.WriteLine($"");
            ////    }
            ////}
            //#endregion

            //for (int i = 0; i < NumberOfSegments; i++)
            //{
            //    PlanPGMergedDataStruct temp = new PlanPGMergedDataStruct();

            //    temp.a1_PatientNumber = Segments[i].a1_PatientNumber;
            //    temp.a2_FieldNumber = Segments[i].a2_FieldNumber;
            //    temp.a3_LayerNumber = Segments[i].a3_LayerNumber;
            //    temp.a4_GantryAngle = Segments[i].a4_GantryAngle;
            //    temp.a5_Energy = Segments[i].a5_Energy;
            //    temp.a6_Xpos = Segments[i].a6_Xpos;
            //    temp.a7_Ypos = Segments[i].a7_Ypos;
            //    temp.ChannelCount_71 = PG_71[i];

            //    MergedData.Add(temp);
            //}

            return PG_144;
        }





        #region Sub-Functions (Private)

        private List<PlanPGMergedDataStruct> MergePlanPG(List<PlanStruct_SMC> Plan, List<PGStruct> PG_144)
        {
            List<PlanPGMergedDataStruct> MergedData = new List<PlanPGMergedDataStruct>();

            int NumberOfSegments = PG_144.Count;
            for (int i = 0; i < NumberOfSegments; i++)
            {
                PlanPGMergedDataStruct temp = new PlanPGMergedDataStruct();

                temp.a1_PatientNumber = Plan[i].a1_PatientNumber;
                temp.a2_FieldNumber = Plan[i].a2_FieldNumber;
                temp.a3_LayerNumber = Plan[i].a3_LayerNumber;
                temp.a4_GantryAngle = Plan[i].a4_GantryAngle;
                temp.a5_Energy = Plan[i].a5_Energy;
                temp.a6_Xpos = Plan[i].a6_Xpos;
                temp.a7_Ypos = Plan[i].a7_Ypos;
                temp.a8_Time = Plan[i].a8_Time;
                temp.a9_TimeRatio = Plan[i].a9_TimeRatio;

                temp.ChannelCount_71 = getPGdist(PG_144[i].ChannelCount, true, false);

                MergedData.Add(temp);
            }

            return MergedData;
        }

        private PGStruct SplitToSegments(List<PGStruct> PG_raw, int StartIndex, int EndIndex)
        {
            PGStruct SingleSegment = new PGStruct();

            SingleSegment.TriggerInputStartTime = PG_raw[StartIndex].TriggerInputStartTime;
            SingleSegment.TriggerInputEndTime = PG_raw[EndIndex].TriggerInputStartTime;

            for (int i = 0; i < 144; i++)
            {
                int tempSum = 0;
                for (int j = StartIndex - 1; j < EndIndex; j++) // NCC랑 다름. 근데 사실 j = StartIndex 가 맞음
                {
                    tempSum += PG_raw[j].ChannelCount[i];
                }
                SingleSegment.ChannelCount[i] = tempSum;
            }

            return SingleSegment;
        }

        private double[] getPGdist(int[] Ch_144_pre, bool missingVal, bool corrFactorsCheck) // NCC와 거의 동일
        {
            double[] Ch_144 = ConvertIntArrayToDoubleArray(Ch_144_pre);
            double[] Ch_71 = new double[71];

            if (missingVal == true)
            {
                Ch_144 = FillMissingScin(Ch_144);
            }

            //if (corrFactorsCheck == true)
            //{
            //    List<double[]> corrFactors = ReadCorrFactorsFile(VM_DAQsetting.corrFactorsDirectory);
            //    Ch_144 = GetEfficiencyCorrected(Ch_144, corrFactors);
            //}

            Ch_71 = MakeFrom144To71Channel(Ch_144);

            return Ch_71;
        }

        private double[] ConvertIntArrayToDoubleArray(int[] IntArray)
        {
            return IntArray.Select(i => (double)i).ToArray();
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

        private double CalculateRange(List<RangeInPMMAStruct> RangeInPMMA, double LayerEnergy)
        {
            int RightIndex = RangeInPMMA.FindIndex(x => x.Energy >= LayerEnergy);

            int LeftIndex = new int();
            if (RightIndex >= 1)
            {
                LeftIndex = RightIndex - 1;
            }
            else
            {
                // 있는지 확인
            }

            double Range = LinearInterp(RangeInPMMA, LayerEnergy, LeftIndex, RightIndex);

            return Range;
        }

        private double LinearInterp(List<RangeInPMMAStruct> RangeInPMMA, double LayerEnergy, int Left, int Right)
        {
            double X_Right = RangeInPMMA[Right].Energy - LayerEnergy;
            double X_Left = LayerEnergy - RangeInPMMA[Left].Energy;

            double Y_Right = RangeInPMMA[Right].Range;
            double Y_Left = RangeInPMMA[Left].Range;

            double Range = ((X_Right * Y_Left) + (X_Left * Y_Right)) / (X_Right + X_Left);

            return Range;
        }

        private List<double[]> GetGaussianWeightMap_SpotMap(List<PlanPGMergedDataStruct> Segments)
        {
            List<double[]> GaussianWeightMap = new List<double[]>(); // Return

            int SegmentsCounts = Segments.Count;

            for (int i = 0; i < SegmentsCounts; i++)
            {
                double[] Distance = new double[SegmentsCounts];

                for (int j = 0; j < SegmentsCounts; j++)
                {
                    Distance[j] = Math.Sqrt(Math.Pow(Segments[i].a6_Xpos - Segments[j].a6_Xpos, 2) + Math.Pow(Segments[i].a7_Ypos - Segments[j].a7_Ypos, 2));
                }

                double AggSigma = 7.8;
                //double AggSigma = 5;
                double[] GaussianWeightMap_temp = new double[SegmentsCounts];

                for (int j = 0; j < SegmentsCounts; j++)
                {
                    GaussianWeightMap_temp[j] = Math.Exp(-0.5 * Math.Pow(Distance[j] / AggSigma, 2));
                }

                GaussianWeightMap.Add(GaussianWeightMap_temp);
            }

            return GaussianWeightMap;
        }

        private double[] GetRangeDifference_SpotMap(List<PlanPGMergedDataStruct> Segments, List<double[]> Map, double PlannedRange)
        {
            int SegmentsCount = Segments.Count;

            double[] RangeDifference = new double[SegmentsCount]; // Return
            double[] range = new double[SegmentsCount];

            for (int i = 0; i < SegmentsCount; i++)
            {
                double[] AggregatedCounts_71 = new double[71];

                for (int j = 0; j < 71; j++)
                {
                    double Counts = 0;

                    for (int k = 0; k < SegmentsCount; k++)
                    {
                        if (Map[i][k] > 0.001)
                        {
                            Counts += Map[i][k] * Segments[k].ChannelCount_71[j];
                        }
                    }
                    AggregatedCounts_71[j] = Counts;
                }

                range[i] = GetRange_ver4p0(xgrid_mm, AggregatedCounts_71, PlannedRange);
                RangeDifference[i] = range[i] - (PlannedRange) - GlobalPush;
            }

            return RangeDifference;
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

        private (double[,], double[,]) ShiftMerge(double[] xpos, double[] ypos, List<PlanPGMergedDataStruct> mergedData, double sigma, int StartLayer, int LastLayer)
        {

            bool[,] flag_grid = new bool[xpos.Length, ypos.Length];

            double[,] map_diff = new double[xpos.Length, ypos.Length];
            double[,] map_mu = new double[xpos.Length, ypos.Length];
            double[,,] map_PGdist = new double[xpos.Length, ypos.Length, 71];

            double pitchX = xpos[1] - xpos[0];
            double pitchY = ypos[1] - ypos[0];

            double[] PGdist_shifted = new double[71];

            List<PlanPGMergedDataStruct> TotalSegments = mergedData.ToList();
            List<PlanPGMergedDataStruct> SelectedLayerSegments = (from segments in mergedData
                                                                  where segments.a3_LayerNumber >= StartLayer
                                                                  where segments.a3_LayerNumber <= LastLayer
                                                                  select segments).ToList();

            flag_grid = CreateGridFlag_BeamRangeMap(xpos, ypos, TotalSegments, pitchX, pitchY);
            (map_diff, map_mu) = CreateDiffMUmap_BeamRangeMap(xpos, ypos, SelectedLayerSegments, flag_grid, sigma);

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

        private bool[,] CreateGridFlag_BeamRangeMap(double[] xpos, double[] ypos, List<PlanPGMergedDataStruct> TotalSegments, double Xpitch, double Ypitch)
        {
            bool[,] flag_grid = new bool[xpos.Length, ypos.Length];
            int NumberOfSegments = TotalSegments.Count();

            for (int x = 0; x < xpos.Length; x++)
            {
                for (int y = 0; y < ypos.Length; y++)
                {
                    for (int i = 0; i < NumberOfSegments; i++)
                    {
                        double segment_Xpos = TotalSegments[i].a6_Xpos;
                        double segment_Ypos = TotalSegments[i].a7_Ypos;

                        //bool flag_X = (xpos[x] - (Xpitch / 2) < segment_Xpos) && (xpos[x] + (Xpitch / 2) > segment_Xpos);
                        //bool flag_Y = (ypos[y] - (Ypitch / 2) < segment_Ypos) && (ypos[y] + (Ypitch / 2) > segment_Ypos);

                        bool flag_X = (xpos[x] - 5 < segment_Xpos) && (xpos[x] + 5 > segment_Xpos);
                        bool flag_Y = (ypos[y] - 5 < segment_Ypos) && (ypos[y] + 5 > segment_Ypos);

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

        private (double[,], double[,]) CreateDiffMUmap_BeamRangeMap(double[] xpos, double[] ypos, List<PlanPGMergedDataStruct> SelectedSegments, bool[,] flag_grid, double sigma)
        {
            double[,] map_dur = new double[xpos.Length, ypos.Length];
            double[,] map_diff = new double[xpos.Length, ypos.Length];

            for (int i = 0; i < xpos.Length; i++) // X 방향으로 for문
            {
                for (int j = 0; j < ypos.Length; j++) // Y 방향으로 for문
                {
                    if (flag_grid[i, j] == true) // flag가 true일 경우, 
                    {
                        double[] PGdist_Shifted_Sum = new double[71];

                        for (int k = 0; k < SelectedSegments.Count; k++)
                        {
                            double segment_Xpos = SelectedSegments[k].a6_Xpos;
                            double segment_Ypos = SelectedSegments[k].a7_Ypos;

                            double distance = Math.Sqrt(Math.Pow(xpos[i] - segment_Xpos, 2) + Math.Pow(ypos[j] - segment_Ypos, 2));

                            if (distance <= 3 * sigma)
                            {
                                double[] PGdist_Shifted = new double[71]; // 위치가 바뀜
                                PGdist_Shifted = ShiftPGdistribution(SelectedSegments[k], VM_LineScanning._Configuration_SMC.RangeInPMMA);

                                double weight = Math.Exp(-0.5 * Math.Pow(distance / sigma, 2));
                                PGdist_Shifted = PGdist_Shifted.Select(x => x * weight).ToArray();

                                for (int p = 0; p < 71; p++)
                                {
                                    PGdist_Shifted_Sum[p] = PGdist_Shifted_Sum[p] + PGdist_Shifted[p];
                                }
                                map_dur[i, j] = map_dur[i, j] + weight * SelectedSegments[k].a8_Time; // 디버깅 완료, PGdist_Shifted_Sum도 얼추 맞는 것 같음 (2022-01-09 21:53)
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

                        map_diff[i, j] = GetRange_ver4p0(xgrid_mm, PGdist_Shifted_Sum, 0) - GlobalPush;
                    }
                    else
                    {
                        map_diff[i, j] = -10000;
                    }
                }
            }

            return (map_diff, map_dur);
        }

        private double[] ShiftPGdistribution(PlanPGMergedDataStruct Segment, List<RangeInPMMAStruct> RangeInPMMAList)
        {
            double[] PGdist_Shifted = new double[71];

            double SegmentEnergy = Segment.a5_Energy; // spot의 에너지를 알고 있고,
            double PlannedRange = CalculateRange(RangeInPMMAList, SegmentEnergy); // correction할 List를 알고 있으면,

            double isocenterDepth = 65;
            double[] xgrid_temp = xgrid_mm.Select(x => x - (PlannedRange - isocenterDepth)).ToArray(); // X축을 각 spot의 plan에서의 z 깊이만큼 빼줄수 있음
                        
            if (PlannedRange - isocenterDepth >= 0) // 빼주는 값이 0보다 큰 경우(빔이 0보다 깊에 들어감, 축이 왼쪽으로 이동하는 경우)
            {
                int index = xgrid_temp.ToList().FindIndex(a => a >= xgrid_mm[0] && a <= xgrid_mm[1]); // 옮겨진 X 축에서 -105 ~ -102 사이에 오는 index
                double left = xgrid_temp[index] - xgrid_mm[0];  // 내분점 left 길이 (index 기준)
                double right = xgrid_mm[1] - xgrid_temp[index]; // 내분점 right 길이

                for (int i = 0; i < 71 - index - 1; i++)
                {
                    PGdist_Shifted[i] = (right * Segment.ChannelCount_71[index + i] + left * Segment.ChannelCount_71[index + i - 1]) / 3; // 내분, [index] 기준에서 [index-1] 기준으로 옮겨옴에 주의
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
                    PGdist_Shifted[70 - index + i] = (right * Segment.ChannelCount_71[i + 1] + left * Segment.ChannelCount_71[i]) / 3;
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

        private double[] GenerateXgrid_mm()
        {
            double[] xgrid = new double[72];
            double[] xgrid_mm = new double[71];

            for (int i = 0; i < 72; i++)
            {
                xgrid[i] = -106.5 + 3 * i;
            }

            for (int i = 0; i < 71; i++)
            {
                xgrid_mm[i] = -105 + 3 * i;
            }

            return xgrid_mm;
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
                // Trace.WriteLine("Error Code JJR0004");
            }

            var locs = (from Pks in FoundPeaks
                        select Pks.X).ToArray();
            var pks = (from Pks in FoundPeaks
                       select Pks.Y).ToArray();

            return (pks, locs);
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

        private List<SpotMapStruct> GetSelectedLayerSpotMap(List<PlanPGMergedDataStruct> Segments, double[] RangeDifference, float min, float max)
        {
            List<SpotMapStruct> SpotMap = new List<SpotMapStruct>(); // Return

            int SpotsCounts = Segments.Count;
            for (int i = 0; i < SpotsCounts; i++)
            {
                SpotMapStruct temp = new SpotMapStruct(-Segments[i].a7_Ypos, Segments[i].a6_Xpos, Segments[i].a9_TimeRatio, SetColor(RangeDifference[i], min, max));
                SpotMap.Add(temp);
            }

            return SpotMap;
        }

        #endregion
    }
}
