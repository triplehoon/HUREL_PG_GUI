namespace HUREL.PG
{
    public abstract class Spot
    {
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

        public void setLayerProperty(int layerNumber, double planEnergy)
        {
            LayerNumber = layerNumber;
            PlanEnergy = planEnergy;
        }
    }
    public abstract class Session
    {

    }




}
