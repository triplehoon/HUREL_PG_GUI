using MATSMC;
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
    public class PlanFileClass_SMC
    {
        #region Main Function (Connected To ViewModel)

        public List<PlanStruct_SMC> LoadPlanFile(string Selected_DICOM_Path)
        {
            List<PlanStruct_SMC> PlanFile = new List<PlanStruct_SMC>();
            List<ConvertedTextFileStruct> PlanFile_raw = new List<ConvertedTextFileStruct>();

            string ConvertedTextFile = Generate_TextFile_FromDICOM(Selected_DICOM_Path);
            PlanFile_raw = Read_ConvertedTextFile(ConvertedTextFile);
            PlanFile = Generate_PlanFile(PlanFile_raw);

            return PlanFile;
        }

        #endregion

        #region Sub-Functions

        /// <summary>
        /// Dialog에서 선택한 RP 파일과 동일한 converted file의 이름이 있는지 확인함. 파일이 존재하지 경우 MATLAB .dll 이용하여 converted file 생성.
        /// </summary>
        /// <param name="Selected_DICOM_Path">Dialog에서 선택된 path</param>
        /// <returns>확장자 .txt 로 변경된 converted RP 파일명</returns>
        private string Generate_TextFile_FromDICOM(string Selected_DICOM_Path)
        {
            string MATSMC_dll_Path = Application.StartupPath; // [ bin - Debug - net5.0-windows ], MATSMC
            var ConvertedFileName = Path.GetFileName(Path.ChangeExtension(Selected_DICOM_Path, "txt")); // selected dialog path
            var ConvertedFileFullName = Path.Combine(MATSMC_dll_Path, ConvertedFileName);

            if (!File.Exists(ConvertedFileFullName))
            {
                ReadDicom Class = new ReadDicom();
                Class.ReadDicomFile(Selected_DICOM_Path); // Saved at: [ bin - Debug - net5.0-windows ] MATSMC.dll 이 있는 위치에 저장됨.
                // 메모리 해제 필요함
            }

            return ConvertedFileFullName;
        }

        /// <summary>
        /// Converted file(DICOM RP 파일에서 segment별 위치/MU 값 추출한 파일)을 읽어서 정보를 추출함
        /// </summary>
        /// <param name="path">확장자 .txt 로 변경된 converted RP 파일명</param>
        /// <returns>Converted file의 raw data format</returns>
        private List<ConvertedTextFileStruct> Read_ConvertedTextFile(string path)
        {
            List<ConvertedTextFileStruct> PlanFile_raw = new List<ConvertedTextFileStruct>();

            StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                string Line = sr.ReadLine();
                string[] StringArray = Line.Split(',');

                ConvertedTextFileStruct Plan_raw_temp = new ConvertedTextFileStruct();

                Plan_raw_temp.PatientNumber = Convert.ToDouble(StringArray[0]);
                Plan_raw_temp.FieldNumber = Convert.ToInt32(StringArray[1]);
                Plan_raw_temp.LayerNumber = Convert.ToInt32(StringArray[2]);
                Plan_raw_temp.GantryAngle = Convert.ToInt32(StringArray[3]);
                Plan_raw_temp.Energy = Convert.ToDouble(StringArray[4]);
                Plan_raw_temp.Xpos = Convert.ToDouble(StringArray[5]);
                Plan_raw_temp.Ypos = Convert.ToDouble(StringArray[6]);
                Plan_raw_temp.Time = Convert.ToDouble(StringArray[7]);

                PlanFile_raw.Add(Plan_raw_temp);
            }
            sr.Close();

            return PlanFile_raw;
        }

        /// <summary>
        /// Converted DICOM RP 파일을 raw format에서 사용 데이터 format으로 변경
        /// </summary>
        /// <param name="tempPlan">raw format plan data</param>
        /// <returns>SMC Plan</returns>
        private List<PlanStruct_SMC> Generate_PlanFile(List<ConvertedTextFileStruct> Plan_Raw)
        {
            List<PlanStruct_SMC> Plan = new List<PlanStruct_SMC>();

            var NumberOfField = Plan_Raw.Last().FieldNumber;

            for (int i = 1; i <= NumberOfField; i++) // a2 1(Sphere), 2(Cubic)
            {
                var TotalSegment_EachField = Plan_Raw.FindAll(x => x.FieldNumber == i);
                var NumberOfLayer_EachField = TotalSegment_EachField.Last().LayerNumber;

                for (int j = 1; j <= NumberOfLayer_EachField; j++) // Sphere, Cubic 선량분포 이루는 Layer 갯수별로
                {
                    var Segment_Layer_Field = TotalSegment_EachField.FindAll(x => x.LayerNumber == j);
                    var NumberOfSegment = Segment_Layer_Field.Count;
                    double Sum = Segment_Layer_Field.Sum(s => s.Time);                    

                    for (int k = 0; k < NumberOfSegment - 1; k++)
                    {
                        PlanStruct_SMC SingleSegment = new PlanStruct_SMC();

                        SingleSegment.a1_PatientNumber = Segment_Layer_Field[k].PatientNumber;
                        SingleSegment.a2_FieldNumber = Segment_Layer_Field[k].FieldNumber;
                        SingleSegment.a3_LayerNumber = Segment_Layer_Field[k].LayerNumber;
                        SingleSegment.a4_GantryAngle = Segment_Layer_Field[k].GantryAngle;
                        SingleSegment.a5_Energy = Segment_Layer_Field[k].Energy;
                        SingleSegment.a6_Xpos = (Segment_Layer_Field[k].Xpos + Segment_Layer_Field[k + 1].Xpos) / 2;
                        SingleSegment.a7_Ypos = (Segment_Layer_Field[k].Ypos + Segment_Layer_Field[k + 1].Ypos) / 2;
                        SingleSegment.a8_Time = Segment_Layer_Field[k].Time;
                        SingleSegment.a9_TimeRatio = Segment_Layer_Field[k + 1].Time / Sum;

                        SingleSegment.Xpos_Start = Segment_Layer_Field[k].Xpos;
                        SingleSegment.Xpos_End = Segment_Layer_Field[k + 1].Xpos;

                        SingleSegment.Ypos_Start = Segment_Layer_Field[k].Ypos;
                        SingleSegment.Ypos_End = Segment_Layer_Field[k + 1].Ypos;

                        Plan.Add(SingleSegment);
                    }
                }                
            }
            return Plan;
        }

        #endregion

        private class ConvertedTextFileStruct
        {
            public double PatientNumber;
            public int FieldNumber;
            public int LayerNumber;
            public int GantryAngle;
            public double Energy;
            public double Xpos;
            public double Ypos;
            public double Time;
        }
    }

    public class PlanStruct_SMC
    {
        public double a1_PatientNumber;
        public int a2_FieldNumber;
        public int a3_LayerNumber;
        public int a4_GantryAngle;
        public double a5_Energy;
        public double a6_Xpos;
        public double a7_Ypos;
        public double a8_Time;
        public double a9_TimeRatio;

        public double Xpos_Start;
        public double Xpos_End;
        public double Ypos_Start;
        public double Ypos_End;
    }
}
