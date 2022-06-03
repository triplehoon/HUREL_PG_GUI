using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUREL_PG_GUI.Models
{
    public class PlanFileClass_NCC
    {
       public static List<PlanStruct_NCC> LoadPlanFile(string path, bool FlattenFlag) // FlattenFlag는 false를 기본으로 사용
        {
            List<PlanStruct_NCC> PlanFile = new List<PlanStruct_NCC>(); // Return 받을 데이터

            try
            {
                if (path.EndsWith("pld") || path.EndsWith("txt"))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
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
                                PlanStruct_NCC tempPlanFile = new PlanStruct_NCC();

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

                                    tempPlanFile.LayerEnergy = LayerEnergy;
                                    tempPlanFile.LayerMU = LayerMU;
                                    tempPlanFile.LayerSpotCount = LayerSpotCount;
                                    tempPlanFile.LayerNumber = TempLayerNumber;

                                    tempPlanFile.XPosition = Convert.ToDouble(tempString[0]);
                                    tempPlanFile.YPosition = Convert.ToDouble(tempString[1]);
                                    tempPlanFile.ZPosition = Convert.ToDouble(tempString[2]);
                                    tempPlanFile.MU = Convert.ToDouble(tempString[3]);

                                    PlanFile.Add(tempPlanFile);
                                }
                            }
                        }
                    }
                }
                else if (path == "")
                {
                    //MessageBox.Show($"Plan_NCC File Load Canceled", $"Plan_NCC File Load Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PlanFile = null;
                }
                else
                {
                    MessageBox.Show($"InValid Data Extension", $"Plan_NCC File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PlanFile = null;
                }

                #region Flatten Code Region (HIDE)

                if (FlattenFlag == true)
                {
                    int LayerCount = PlanFile.Last().LayerNumber + 1;
                    int SpotIndex = 0;

                    for (int i = 0; i < LayerCount; i++)
                    {
                        var kk = (from kkk in PlanFile
                                  where kkk.LayerNumber == i
                                  select kkk.ZPosition).ToList();

                        var ww = kk.Average();
                        var www = kk.Count();

                        for (int j = 0; j < www; j++)
                        {
                            PlanFile[SpotIndex].ZPosition = ww;
                            SpotIndex++;
                        }
                    }
                }

                #endregion

                return PlanFile;
            }
            catch (Exception e)
            {
                //MessageBox.Show($"Error: {e}", $"Invalid '.pld' file loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }            
        }
    }
    public class PlanStruct_NCC
    {
        public double LayerEnergy;
        public double LayerMU;
        public int LayerSpotCount;

        public double XPosition;
        public double YPosition;
        public double ZPosition;

        public double MU;

        public int LayerNumber;
    }
}
