



namespace HUREL.PG
{
    public class Spot
    {
        public int LayerNumer { get; }
        public double PositionX { get; }
        public double PositionY { get; }
        public double MonitoringUnit { get; }     

        public double Depth { get; }

        public string GetSpotInfo()
        {
            return string.Empty;
        }
        public Spot()
        {
            
        }        
    }

    public class NccSpot : Spot
    {       
        public static List<NccSpot> ReadLogSpotData(string FilePath)
        {
            return null;
        }
    }

    public class SmcSpot : Spot
    {

    }    

    public class Layer
    {
        List<Spot> SpotList = new List<Spot>();
        int LayerNumber;

        int GetSpotCount()
        {
            return SpotList.Count;  
        }


    }

    public class Session
    {
        
    }
}
