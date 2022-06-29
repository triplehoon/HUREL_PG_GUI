



namespace HUREL.PG
{    
    public abstract class Spot
    {
        public int LayerNumber { get; private set; }
        public double PlanPositionX { get; private set; }
        public double PlanPositionY { get; private set; }
        public double PlanMonitoringUnit { get; private set; }  
        public int PlanSpotIndexNumber { get; private set; }
        public virtual string PrintSpotInfo()
        {
            return string.Empty;
        }
        public Spot()
        {
            
        }
    }

    public abstract class Layer
    {
        public int LayerNumber { get; private set; }

        public double PlanEnergy { get; private set; }

        public virtual string PrintLayerInfo()
        {
            string str = string.Empty;
            return str;
        }
    }
    public abstract class Session
    {

    }

    


}
