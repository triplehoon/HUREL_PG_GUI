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
        public void SetLogTick(int tick)
        {
            this.LogTick = tick;
        }
        // int[] ChannelCount, int SumCounts, int TriggerStartTime = 0, int TriggerEndTime = 0, double ADC = 0, int Tick = 0);

        public MultiSlitPg PgData { get; private set; }
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
            PgData = new MultiSlitPg(new int[0], 0);
            IsPgDataSet = false;
        }

        public void SetPgData(MultiSlitPg pg)
        {
            PgData = pg;
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

        static private List<GapPeakAndRange> gapPeakAndRange = new List<GapPeakAndRange>();
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
            if (gapPeakAndRange.Count == 0)
            {
                gapPeakAndRange.Add(new GapPeakAndRange(95.0900000000000, 1.32448612330958));
                gapPeakAndRange.Add(new GapPeakAndRange(97.3600000000000, 1.35335439631464));
                gapPeakAndRange.Add(new GapPeakAndRange(99.6800000000000, 1.38220078648799));
                gapPeakAndRange.Add(new GapPeakAndRange(100.510000000000, 1.41102531434278));
                gapPeakAndRange.Add(new GapPeakAndRange(102.710000000000, 1.43982909546839));
                gapPeakAndRange.Add(new GapPeakAndRange(104.800000000000, 1.46861404567794));
                gapPeakAndRange.Add(new GapPeakAndRange(106.790000000000, 1.49738195567133));
                gapPeakAndRange.Add(new GapPeakAndRange(108.710000000000, 1.52613435952034));
                gapPeakAndRange.Add(new GapPeakAndRange(110.560000000000, 1.55487270323130));
                gapPeakAndRange.Add(new GapPeakAndRange(112.520000000000, 1.58359832493369));
                gapPeakAndRange.Add(new GapPeakAndRange(114.120000000000, 1.61231300678904));
                gapPeakAndRange.Add(new GapPeakAndRange(115.830000000000, 1.64101915278195));
                gapPeakAndRange.Add(new GapPeakAndRange(117.540000000000, 1.66971903095405));
                gapPeakAndRange.Add(new GapPeakAndRange(119.220000000000, 1.69841516311782));
                gapPeakAndRange.Add(new GapPeakAndRange(120.910000000000, 1.72711009059537));
                gapPeakAndRange.Add(new GapPeakAndRange(122.600000000000, 1.75580594072091));
                gapPeakAndRange.Add(new GapPeakAndRange(124.290000000000, 1.78450368328742));
                gapPeakAndRange.Add(new GapPeakAndRange(125.560000000000, 1.81320294888093));
                gapPeakAndRange.Add(new GapPeakAndRange(127.220000000000, 1.84190314559150));
                gapPeakAndRange.Add(new GapPeakAndRange(128.850000000000, 1.87060399590957));
                gapPeakAndRange.Add(new GapPeakAndRange(130.420000000000, 1.89930495151694));
                gapPeakAndRange.Add(new GapPeakAndRange(131.970000000000, 1.92800423618358));
                gapPeakAndRange.Add(new GapPeakAndRange(133.480000000000, 1.95669741545726));
                gapPeakAndRange.Add(new GapPeakAndRange(134.950000000000, 1.98537903034979));
                gapPeakAndRange.Add(new GapPeakAndRange(136.390000000000, 2.01404238935368));
                gapPeakAndRange.Add(new GapPeakAndRange(137.830000000000, 2.04267642539212));
                gapPeakAndRange.Add(new GapPeakAndRange(139.270000000000, 2.07126192296322));
                gapPeakAndRange.Add(new GapPeakAndRange(140.690000000000, 2.09976548254288));
                gapPeakAndRange.Add(new GapPeakAndRange(142.110000000000, 2.12813462497131));
                gapPeakAndRange.Add(new GapPeakAndRange(143.540000000000, 2.15629761631917));
                gapPeakAndRange.Add(new GapPeakAndRange(145, 2.18382209107898));
                gapPeakAndRange.Add(new GapPeakAndRange(146.450000000000, 2.21118031663590));
                gapPeakAndRange.Add(new GapPeakAndRange(147.940000000000, 2.23836425604335));
                gapPeakAndRange.Add(new GapPeakAndRange(149.080000000000, 2.26536969171966));
                gapPeakAndRange.Add(new GapPeakAndRange(150.620000000000, 2.29218518324595));
                gapPeakAndRange.Add(new GapPeakAndRange(152.090000000000, 2.31880613883544));
                gapPeakAndRange.Add(new GapPeakAndRange(153.480000000000, 2.34525459819227));
                gapPeakAndRange.Add(new GapPeakAndRange(154.800000000000, 2.37156183665308));
                gapPeakAndRange.Add(new GapPeakAndRange(156.090000000000, 2.39776507056352));
                gapPeakAndRange.Add(new GapPeakAndRange(157.360000000000, 2.42391196690806));
                gapPeakAndRange.Add(new GapPeakAndRange(158.570000000000, 2.45004497803707));
                gapPeakAndRange.Add(new GapPeakAndRange(159.780000000000, 2.47618515728928));
                gapPeakAndRange.Add(new GapPeakAndRange(160.960000000000, 2.50234016261308));
                gapPeakAndRange.Add(new GapPeakAndRange(162.100000000000, 2.52849661197662));
                gapPeakAndRange.Add(new GapPeakAndRange(163.250000000000, 2.55460929841975));
                gapPeakAndRange.Add(new GapPeakAndRange(164.390000000000, 2.58060962209574));
                gapPeakAndRange.Add(new GapPeakAndRange(165.550000000000, 2.60642825072595));
                gapPeakAndRange.Add(new GapPeakAndRange(166.730000000000, 2.63199471815552));
                gapPeakAndRange.Add(new GapPeakAndRange(167.920000000000, 2.65723989899789));
                gapPeakAndRange.Add(new GapPeakAndRange(169.130000000000, 2.68208544435658));
                gapPeakAndRange.Add(new GapPeakAndRange(170.120000000000, 2.70645466727595));
                gapPeakAndRange.Add(new GapPeakAndRange(171.590000000000, 2.73029311200999));
                gapPeakAndRange.Add(new GapPeakAndRange(172.730000000000, 2.75357425602008));
                gapPeakAndRange.Add(new GapPeakAndRange(173.830000000000, 2.77627734450233));
                gapPeakAndRange.Add(new GapPeakAndRange(174.900000000000, 2.79838345545905));
                gapPeakAndRange.Add(new GapPeakAndRange(175.920000000000, 2.81989143789170));
                gapPeakAndRange.Add(new GapPeakAndRange(176.940000000000, 2.84082986982214));
                gapPeakAndRange.Add(new GapPeakAndRange(177.930000000000, 2.86123431405783));
                gapPeakAndRange.Add(new GapPeakAndRange(178.910000000000, 2.88112219015482));
                gapPeakAndRange.Add(new GapPeakAndRange(179.890000000000, 2.90049174496589));
                gapPeakAndRange.Add(new GapPeakAndRange(180.820000000000, 2.91933303786351));
                gapPeakAndRange.Add(new GapPeakAndRange(181.750000000000, 2.93764083581400));
                gapPeakAndRange.Add(new GapPeakAndRange(182.660000000000, 2.95540289964614));
                gapPeakAndRange.Add(new GapPeakAndRange(183.580000000000, 2.97261342601561));
                gapPeakAndRange.Add(new GapPeakAndRange(184.490000000000, 2.98928860380919));
                gapPeakAndRange.Add(new GapPeakAndRange(185.400000000000, 3.00547066027675));
                gapPeakAndRange.Add(new GapPeakAndRange(186.300000000000, 3.02121799227409));
                gapPeakAndRange.Add(new GapPeakAndRange(187.180000000000, 3.03660276032500));
                gapPeakAndRange.Add(new GapPeakAndRange(188.070000000000, 3.05169830843544));
                gapPeakAndRange.Add(new GapPeakAndRange(188.880000000000, 3.06655788377068));
                gapPeakAndRange.Add(new GapPeakAndRange(190.040000000000, 3.08121522278241));
                gapPeakAndRange.Add(new GapPeakAndRange(190.930000000000, 3.09567740198911));
                gapPeakAndRange.Add(new GapPeakAndRange(191.790000000000, 3.10993462486885));
                gapPeakAndRange.Add(new GapPeakAndRange(192.610000000000, 3.12394982207505));
                gapPeakAndRange.Add(new GapPeakAndRange(193.420000000000, 3.13765903504544));
                gapPeakAndRange.Add(new GapPeakAndRange(194.220000000000, 3.15099472904540));
                gapPeakAndRange.Add(new GapPeakAndRange(195, 3.16390070804518));
                gapPeakAndRange.Add(new GapPeakAndRange(195.800000000000, 3.17633326948404));
                gapPeakAndRange.Add(new GapPeakAndRange(196.560000000000, 3.18826185820897));
                gapPeakAndRange.Add(new GapPeakAndRange(197.300000000000, 3.19966438440083));
                gapPeakAndRange.Add(new GapPeakAndRange(198.040000000000, 3.21052515576883));
                gapPeakAndRange.Add(new GapPeakAndRange(198.770000000000, 3.22083900000084));
                gapPeakAndRange.Add(new GapPeakAndRange(199.490000000000, 3.23060633906663));
                gapPeakAndRange.Add(new GapPeakAndRange(200.220000000000, 3.23982470561698));
                gapPeakAndRange.Add(new GapPeakAndRange(200.930000000000, 3.24848397883403));
                gapPeakAndRange.Add(new GapPeakAndRange(201.640000000000, 3.25657822795008));
                gapPeakAndRange.Add(new GapPeakAndRange(202.360000000000, 3.26410345108389));
                gapPeakAndRange.Add(new GapPeakAndRange(203.040000000000, 3.27107544241298));
                gapPeakAndRange.Add(new GapPeakAndRange(203.730000000000, 3.27754045450384));
                gapPeakAndRange.Add(new GapPeakAndRange(204.440000000000, 3.28357423381417));
                gapPeakAndRange.Add(new GapPeakAndRange(204.660000000000, 3.28925999668215));
                gapPeakAndRange.Add(new GapPeakAndRange(205.530000000000, 3.29467548593349));
                gapPeakAndRange.Add(new GapPeakAndRange(206.180000000000, 3.29989409290817));
                gapPeakAndRange.Add(new GapPeakAndRange(206.810000000000, 3.30498044159194));
                gapPeakAndRange.Add(new GapPeakAndRange(207.420000000000, 3.30996820529000));
                gapPeakAndRange.Add(new GapPeakAndRange(208.020000000000, 3.31484750454686));
                gapPeakAndRange.Add(new GapPeakAndRange(208.610000000000, 3.31959211270586));
                gapPeakAndRange.Add(new GapPeakAndRange(209.190000000000, 3.32416084739921));
                gapPeakAndRange.Add(new GapPeakAndRange(209.760000000000, 3.32851238005695));
                gapPeakAndRange.Add(new GapPeakAndRange(210.320000000000, 3.33261189730305));
                gapPeakAndRange.Add(new GapPeakAndRange(210.870000000000, 3.33643595677459));
                gapPeakAndRange.Add(new GapPeakAndRange(211.420000000000, 3.33996053888725));
                gapPeakAndRange.Add(new GapPeakAndRange(211.930000000000, 3.34316621733117));
                gapPeakAndRange.Add(new GapPeakAndRange(212.440000000000, 3.34604221626492));
                gapPeakAndRange.Add(new GapPeakAndRange(212.940000000000, 3.34857673560159));
                gapPeakAndRange.Add(new GapPeakAndRange(213.430000000000, 3.35074946212062));
                gapPeakAndRange.Add(new GapPeakAndRange(213.920000000000, 3.35253460442024));
                gapPeakAndRange.Add(new GapPeakAndRange(214.400000000000, 3.35389007427644));
                gapPeakAndRange.Add(new GapPeakAndRange(214.850000000000, 3.35478737326274));
                gapPeakAndRange.Add(new GapPeakAndRange(215.320000000000, 3.35521488984241));
                gapPeakAndRange.Add(new GapPeakAndRange(215.790000000000, 3.35518404645186));
                gapPeakAndRange.Add(new GapPeakAndRange(216.310000000000, 3.35471810903203));
                gapPeakAndRange.Add(new GapPeakAndRange(216.790000000000, 3.35282941068405));
                gapPeakAndRange.Add(new GapPeakAndRange(217.240000000000, 3.35086875450197));
                gapPeakAndRange.Add(new GapPeakAndRange(217.650000000000, 3.34890629947680));
                gapPeakAndRange.Add(new GapPeakAndRange(218.030000000000, 3.34696973150185));
                gapPeakAndRange.Add(new GapPeakAndRange(218.390000000000, 3.34506768946619));
                gapPeakAndRange.Add(new GapPeakAndRange(218.750000000000, 3.34320168770092));
                gapPeakAndRange.Add(new GapPeakAndRange(219.100000000000, 3.34137006018419));
                gapPeakAndRange.Add(new GapPeakAndRange(219.440000000000, 3.33957032689728));
                gapPeakAndRange.Add(new GapPeakAndRange(219.800000000000, 3.33779996932194));
                gapPeakAndRange.Add(new GapPeakAndRange(220.150000000000, 3.33605600115766));
                gapPeakAndRange.Add(new GapPeakAndRange(220.520000000000, 3.33433455480907));
                gapPeakAndRange.Add(new GapPeakAndRange(220.910000000000, 3.33263077871214));
                gapPeakAndRange.Add(new GapPeakAndRange(221.330000000000, 3.33093978688910));
                gapPeakAndRange.Add(new GapPeakAndRange(221.790000000000, 3.32925815385148));
                gapPeakAndRange.Add(new GapPeakAndRange(221.860000000000, 3.32758364191405));
                gapPeakAndRange.Add(new GapPeakAndRange(222.380000000000, 3.32591437478676));
                gapPeakAndRange.Add(new GapPeakAndRange(222.750000000000, 3.32424765461560));
                gapPeakAndRange.Add(new GapPeakAndRange(223.120000000000, 3.32258033548480));
                gapPeakAndRange.Add(new GapPeakAndRange(223.470000000000, 3.32090887101729));
                gapPeakAndRange.Add(new GapPeakAndRange(223.820000000000, 3.31922969539556));
                gapPeakAndRange.Add(new GapPeakAndRange(224.150000000000, 3.31753936651333));
                gapPeakAndRange.Add(new GapPeakAndRange(224.470000000000, 3.31583473696996));
                gapPeakAndRange.Add(new GapPeakAndRange(224.780000000000, 3.31411294155765));
                gapPeakAndRange.Add(new GapPeakAndRange(225.100000000000, 3.31237211241693));
                gapPeakAndRange.Add(new GapPeakAndRange(225.460000000000, 3.31061234828160));
                gapPeakAndRange.Add(new GapPeakAndRange(225.880000000000, 3.30883606868832));
                gapPeakAndRange.Add(new GapPeakAndRange(226.290000000000, 3.30704669359046));
                gapPeakAndRange.Add(new GapPeakAndRange(226.690000000000, 3.30524739489246));
                gapPeakAndRange.Add(new GapPeakAndRange(227.100000000000, 3.30343961594278));
            }
        }

        public NccLayer()
        {
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
        public List<SpotMap> GetSpotMap()
        {
            // Return
            List<SpotMap> spotMap = new List<SpotMap>();

            List<NccSpot> spots = this.Spots;
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
            double layerEnergy = LayerEnergy;

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
                if (spots[i].PgData.ChannelCount.Count() == 0)
                {
                    continue;
                }

                double[] aggregatedPGdistribution = new double[144];

                for (int j = 0; j < 144; j++)
                {
                    double chCounts = 0;

                    for (int k = 0; k < spotCounts; k++)
                    {
                        if (gaussianWeightMap[i][k] > 0.001)
                        {
                            if (spots[k].PgData.ChannelCount.Count() == 0)
                            {
                                continue;
                            }
                            chCounts += gaussianWeightMap[i][k] * spots[k].PgData.ChannelCount[j];
                        }
                    }

                    aggregatedPGdistribution[j] = chCounts;
                }

                double isoDepth = 110; // mm unit

                bool is144ChCount = false;
                spotRange[i] = MultiSlitPg.GetRange(aggregatedPGdistribution, spots[i].PlanSpot.Zposition);
                spotRangeDifference[i] = spotRange[i] - (spots[i].PlanSpot.Zposition + gap);

                //Console.WriteLine($"{spots[i].XPosition}, {spots[i].YPosition}, {spots[i].PlanSpot.Zposition}, {spotRange[i]}, {spotRangeDifference[i]}");
                //Console.WriteLine($"{-spots[i].YPosition}, {spots[i].XPosition}, {spots[i].PlanSpot.Zposition}, {spotRangeDifference[i]}");
            }

            #endregion

            #region get spot map () - output: [spotMap]

            spotMap = new List<SpotMap>();
            for (int spotIndex = 0; spotIndex < spotCounts; spotIndex++)
            {
                spotMap.Add(new SpotMap(spots[spotIndex].XPosition, spots[spotIndex].YPosition, spots[spotIndex].PlanSpot.MonitoringUnit, spotRangeDifference[spotIndex]));
            }

            #endregion
            return spotMap;
        }
        public static List<SpotMap> GetSpotMap(List<NccLayer> layer)
        {
            // Return
            List<SpotMap> spotMap = new List<SpotMap>();

            List<NccSpot> spots = new List<NccSpot>();
            double layerEnergy = 0;
            foreach(NccLayer layerItem in layer)
            {
                spots.AddRange(layerItem.spots);
                layerEnergy += layerItem.LayerEnergy;
            }
            if (spots.Count == 0)
            {
                return spotMap; 
            }

            layerEnergy /= layer.Count;

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
                if (spots[i].PgData.ChannelCount.Count() == 0)
                {
                    continue;
                }

                double[] aggregatedPGdistribution = new double[144];

                for (int j = 0; j < 144; j++)
                {
                    double chCounts = 0;

                    for (int k = 0; k < spotCounts; k++)
                    {
                        if (gaussianWeightMap[i][k] > 0.001)
                        {
                            if (spots[k].PgData.ChannelCount.Count() == 0)
                            {
                                continue;
                            }
                            chCounts += gaussianWeightMap[i][k] * spots[k].PgData.ChannelCount[j];
                        }
                    }

                    aggregatedPGdistribution[j] = chCounts;
                }

                double isoDepth = 110; // mm unit

                bool is144ChCount = false;
                spotRange[i] = MultiSlitPg.GetRange(aggregatedPGdistribution, spots[i].PlanSpot.Zposition);
                spotRangeDifference[i] = spotRange[i] - (spots[i].PlanSpot.Zposition + gap);

                //Console.WriteLine($"{spots[i].XPosition}, {spots[i].YPosition}, {spots[i].PlanSpot.Zposition}, {spotRange[i]}, {spotRangeDifference[i]}");
                //Console.WriteLine($"{-spots[i].YPosition}, {spots[i].XPosition}, {spots[i].PlanSpot.Zposition}, {spotRangeDifference[i]}");
            }

            #endregion

            #region get spot map () - output: [spotMap]

            spotMap = new List<SpotMap>();
            for (int spotIndex = 0; spotIndex < spotCounts; spotIndex++)
            {
                spotMap.Add(new SpotMap(spots[spotIndex].XPosition, spots[spotIndex].YPosition, spots[spotIndex].PlanSpot.MonitoringUnit, spotRangeDifference[spotIndex]));
            }

            #endregion
            return spotMap;
        }


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
        private List<MultiSlitPg> pgRawData = new List<MultiSlitPg>();
        public List<MultiSlitPg> PgRawData
        {
            get
            {
                return pgRawData;
            }
            private set
            {
                pgRawData = value;
            }
        }
        public void SetMultiSlitPg(List<MultiSlitPg> pgRaw)
        {
            PgRawData = pgRaw;
            PgSpots = MultiSlitPg.SplitDataIntoSpot(PgRawData, 5, 40, 5);
            MergeNCCSpotData();
        }

        private List<MultiSlitPg> pgSpots = new List<MultiSlitPg>();
        public List<MultiSlitPg> PgSpots
        {
            get
            {
                return pgSpots;
            }
            private set
            {
                pgSpots = value;

            }
        }

        public string PgFileName { get; private set; }
        public string PlanFileName { get { return Path.GetFileName(plan.PlanFilePath); } }
        private DateTime firstLayerFirstSpotLogTime;

        public bool IsPlanLoad { get; private set; }
        public bool IsPgLoad { get; private set; }
        public int GetTotalPlanLayerCount
        {
            get { return plan.TotalPlanLayer; }
        }
        public bool IsConfigLogFileLoad { get; private set; }
        public bool IsGetReferenceTime { get; private set; }

        public bool IsReadyToStartSession
        {
            get
            {
                return IsPlanLoad;
            }
        }
        public string PatientName { get; private set; }
        public string PatientId { get; private set; }
        public NccSession()
        {
            PgFileName = "";
            IsPlanLoad = false;
            IsPgLoad = false;
            IsConfigLogFileLoad = false;
            IsGetReferenceTime = false;
            PatientName = "";
            PatientId = "";
          
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
        public bool LoadRecordSpecifLogFile(string recordFilePath, string specifFilePath)
        {
            #region Check loaded log files whether valid or invalid

            if (!File.Exists(recordFilePath))
            {
                Debug.WriteLine($"Log(record) file doesn't exist");
                return false;
            }

            if (!File.Exists(specifFilePath))
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

            if (!recordFilePath.Contains("map_record_"))
            {
                Debug.WriteLine($"Log(record) is not invalid");
                return false;
            }

            if (!specifFilePath.Contains("map_specif_"))
            {
                Debug.WriteLine($"Log(specif) is not invalid");
                return false;
            }
            bool chkLayerFileValid;
            string recordLayerId;
            string specifLayerId;
            (chkLayerFileValid, _, _, _, recordLayerId, _) = NccLayer.GetInfoFromLogFileName(recordFilePath);
            if (!chkLayerFileValid)
            {
                Debug.Assert(!chkLayerFileValid, "record file is not valid");
                return false;
            }
            (chkLayerFileValid, _, _, _, specifLayerId, _) = NccLayer.GetInfoFromLogFileName(specifFilePath);
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
                (_, _, _, _, string layerID, _) = NccLayer.GetInfoFromLogFileName(recordFilePath);
                if (chklayer.LayerId == layerID)
                {
                    Debug.WriteLine("Layer file is already loaded");
                    return true;
                }
            }
            #endregion

            NccLayer loadedLayer = new NccLayer(recordFilePath, specifFilePath, logParameter.coeff_x, logParameter.coeff_y, plan);
            //NccSpot.NccBeamState state = nccLayer.GetSingleLogInfo();
            NccSpot.NccBeamState state = loadedLayer.NccBeamState;

            Layers.Add(loadedLayer);
            Layers.Sort(SortLayer);



            return true;
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
            PgRawData = MultiSlitPg.LoadbinaryFile(pgDir);

            if (PgRawData.Count != 0)
            {
                PgFileName = pgDir;
                PgSpots = MultiSlitPg.SplitDataIntoSpot(PgRawData, 5, 40, 4);
                IsPgLoad = true;
                MergeNCCSpotData();
                return true;
            }
            else
            {
                IsPgLoad = false;
                return false;
            }
        }


       

        #region private functions
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
        private void MergeNCCSpotData()
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

            if (getRefTimePlanLog == false)
            {
                refTimePlanLog = 0;
                Debug.WriteLine($"Can't get reference time of PlanLog");
                return;
            }

            for (int i = 0; i < PlanLog.Count; i++)
            {
                int GapBetweenFirstSpot = Convert.ToInt32((PlanLog[i].BeamStartTime.Ticks - refTimePlanLog) / 10);
                PlanLog[i].SetLogTick(GapBetweenFirstSpot);
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
            }

            List<MultiSlitPg> pgSpots_withTick = new List<MultiSlitPg>();
            foreach (MultiSlitPg spot in pgSpots)
            {
                int pgTick = spot.TriggerStartTime - refTimePG;
                pgSpots_withTick.Add(new MultiSlitPg(spot.ChannelCount, spot.SumCounts, spot.TriggerStartTime, spot.TriggerEndTime, spot.ADC, pgTick));
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
                    PlanLog[index_PlanLog].SetPgData(pgSpots_withTick[index_PG]);

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
                            PlanLog[index_PlanLog].SetPgData(pgSpots_withTick[index_PG]);

                            index_PG = index_PG_temp + 1;
                            index_PlanLog++;

                            isSpotMatched = true;
                            break;
                        }
                    }

                    if (isSpotMatched == false)
                    {
                        Debug.Assert(true, $"log spot number {index_PlanLog} was not matched with PG trigger");

                        PlanLog[index_PlanLog].SetPgData(pgSpots_withTick[index_PG]);
                        index_PlanLog++;

                        if (index_PG + numOfCompareSpots > numOfPGSpots)
                        {
                            break;
                        }
                    }
                }
            }
            #endregion
            int planLogIndex = 0;
            foreach (NccLayer layer in layers)
            {
                for (int i = 0; i < layer.Spots.Count; ++i)
                {
                    if (planLogIndex >= PlanLog.Count)
                    {
                        break;
                    }
                    layer.Spots[i] = PlanLog[planLogIndex];
                    ++planLogIndex;
                }
                if (planLogIndex >= PlanLog.Count)
                {
                    break;
                }
            }
            return;
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
        public string PlanFilePath { get; private set; }
        public double TotalPlanMonitoringUnit { get; }
        public int TotalPlanLayer { get; }

        private List<NccPlanSpot> spots = new List<NccPlanSpot>();
        public NccPlan(string planFile)
        {
            PlanFilePath = "";
            if (planFile != null & planFile != "")
            {
                PlanFilePath = planFile!;

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
                if (!Path.IsPathFullyQualified(PGDir))
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
            for (int i = 1; i < list.Count; ++i)
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

        static public double GetRange(double[] pgDistribution, double refRangePos)
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
                }
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
            #region 1. Generate Gaussian kernel

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




        public static List<MultiSlitPg> SplitDataIntoSpot(List<MultiSlitPg> PG_raw, int width, int ULD, int LLD)
        {
            // Return data
            List<MultiSlitPg> PG_144 = new List<MultiSlitPg>();

            bool isSpot = false;
            int dataCount = PG_raw.Count();

            int index_SpotStart = 0;
            int index_SpotEnd = 0;

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
                        index += 15;
                    }
                }
                else
                {
                    //if (sum_Count < LLD)
                    if (PG_raw[index].SumCounts < LLD)
                    {
                        isSpot = false;
                        index_SpotEnd = index;
                        index += 15;

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

                        PG_144.Add(new MultiSlitPg(channelCount, sumCounts, triggerStartTime, triggerEndTime, 0, 0));
                    }
                }
            }
            return PG_144;
        }


    }



    public record SpotMap(double X, double Y, double MU, double RangeDifference);
    public record BeamRangeMap(double X, double Y, double MU, double RangeDifference);
    public record GapPeakAndRange(double energy, double GapValue);

}