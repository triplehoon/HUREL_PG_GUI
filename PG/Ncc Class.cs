using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL.PG.Ncc
{ 

    public class NccSpot : Spot
    {
        public double PlanPositionZ { get; private set; }
        public string LogLayerId { get; private set; }
        public double LogPositionX { get; private set; }
        public double LogPositionY { get; private set; }

        public NccSpot(string logLayerId)
        {
            LogLayerId = logLayerId;
        }

        public enum NccBeamState
        {
            Normal, Tuning, Resume
        }
        public NccBeamState State { get; private set; }
        public int Tick { get; private set; }
        public void ChangNccBeamState(NccBeamState state)
        {
            State = state;
        }
    }
    public class NccLayer : Layer
    {
        private List<NccSpot> spots = new List<NccSpot>();

        private NccPlan layerPlan;

        private NccLayer(NccPlan plan)
        {
            this.layerPlan = plan;
        }
        public bool LogInfo(XdrDataRecorderRpcLayerConverter convertInfo)
        {
            if (convertInfo.ErrorCheck)
            {
                return false;
            }
            
            return true;
        }

        
    }

    /// <summary>
    /// Dicom, Plan, Log, PG
    /// </summary>
    public class NccSession:Session
    {
        private List<NccLayer> layers = new List<NccLayer>();

        private NccPlan plan = new NccPlan(null);

        public bool IsPlanLoad { get; private set; }

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
    }

    public class NccPlan
    {
        private List<PlanSpot> spots = new List<PlanSpot>();
        public NccPlan(string planFile)
        {
            if (planFile != null)
            {
                PlanFile = planFile;


                using (FileStream fs = new FileStream(planFile, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        string lines = null;
                        string[] tempString = null;

                        int TempLayerNumber = 0;

                        tempString = sr.ReadLine().Split(",");

                        double LayerEnergy = Convert.ToDouble(tempString[2]);
                        double LayerMU = Convert.ToDouble(tempString[3]);
                        int LayerSpotCount = Convert.ToInt32(tempString[4]) / 2;

                        while ((lines = sr.ReadLine()) != null)
                        {
                            PlanSpot tempPlanSpot = new PlanSpot();

                            if (lines.Contains("Layer")) // 다음 레이어의 header를 만날 때
                            {
                                tempString = lines.Split(",");

                                TempLayerNumber += 1;

                                LayerEnergy = Convert.ToDouble(tempString[2]);
                                LayerMU = Convert.ToDouble(tempString[3]);
                                LayerSpotCount = Convert.ToInt32(tempString[4]) / 2;
                            }
                            else // 해당 레이어의 데이터를 계속 만날 때 
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
                foreach (PlanSpot spot in spots)
                {
                    TotalPlanMonitoringUnit += spot.MonitoringUnit;
                }
            }
        }

        public string PlanFile { get; private set; }
        
        public NccPlan GetPlanByLayerNumber(int layerNumber)
        {
            return null;
        }

        public double TotalPlanMonitoringUnit { get; };

        
        private struct PlanSpot
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
}
