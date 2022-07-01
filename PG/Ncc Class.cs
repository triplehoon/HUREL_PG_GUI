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

        /// <summary>
        /// Load plan file
        /// if plan is loaded return true
        /// And Make layers with plan file
        /// </summary>
        /// <param name="planFileDir"></param>
        /// <returns></returns>
        public bool LoadPlanFile(string planFileDir)
        {
            
            return false;
        }
    }

    public class NccPlan
    {
        private List<PlanSpot> spots = new List<PlanSpot>();
        public NccPlan(string planFile)
        {
            PlanFile = planFile;
           
            


            TotalPlanMonitoringUnit = 0;
            foreach (PlanSpot spot in spots)
            {
                TotalPlanMonitoringUnit += spot.MonitoringUnit;
            }

        }

        public string PlanFile { get; private set; }
        
        public NccPlan GetPlanByLayerNumber(int layerNumber)
        {
            return null;
        }

        public double TotalPlanMonitoringUnit { get; }

        
        private struct PlanSpot
        {
            public double MonitoringUnit;
        }
    }
}
