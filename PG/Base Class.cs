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
        public int LayerNumber { get; protected set; }
        abstract public string? LayerId { get;}
        public double LayerEnergy { get; protected set; }

        public virtual string PrintLayerInfo()
        {
            string str = string.Empty;
            return str;
        }
    }




}