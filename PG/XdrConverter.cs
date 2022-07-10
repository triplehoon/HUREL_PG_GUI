using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL.PG.Ncc
{
    //public class XdrDataRecorderRpcLayerConverter
    public class XdrConverter_Record
    {

        #region 주석처리
        /*
         * JAVA xdrDecode
       1 this.beamlineId = xdr.xdrDecodeInt();
       2 this.layerId = xdr.xdrDecodeInt();
       3 int $size = xdr.xdrDecodeInt(); this.events = new PbsDataRecorderRpcLayerEvent[$size]; for (int $idx = 0; $idx < $size; $idx++) this.events[$idx] = new PbsDataRecorderRpcLayerEvent(xdr);
       4 this.acquisitionPeriod = xdr.xdrDecodeInt();
       5 this.sgcuLastElementId = xdr.xdrDecodeInt();
       6 this.rcuLastElementId = xdr.xdrDecodeInt();
       7 this.fcuLastElementId = xdr.xdrDecodeInt();
       8 this.necuLastElementId = xdr.xdrDecodeInt();
       9 int $size = xdr.xdrDecodeInt(); this.data = new PbsDataRecorderRpcLayerData[$size]; for (int $idx = 0; $idx < $size; $idx++) this.data[$idx] = new PbsDataRecorderRpcLayerData(xdr);
      10 this.nozzleEntranceLastElementData = new OptionalScanningControllerBeamDataAtNozzleEntrance(xdr);
      11 this.lastElementData = new ScanningControllerBeamData(xdr);
      12 this.failureCause = new ScanningControllerFailureCause(xdr);  


         */
        //5 bytes is the version of pbsDataRecorder

        #endregion

        //1, 2
        public int beamlineId;
        public int layerID;
        //3 
        public int sizeOfEvents;

        //4,5,6,7,8
        public int acquisitionPeriod;
        public int sgcuLastElementId;
        public int rcuLastElementId;
        public int fcuLastElementId;
        public int necuLastElementId;
        //9

        #region
        /*
         *     
       a this.time = new PbsDataRecorderTime(xdr);
       b this.elementId = xdr.xdrDecodeInt();
       c this.beamProfile = new PbsDataRecorderRpcBeamProfileData(xdr);
       d this.ic1Data = new PbsDataRecorderRpcAxisData(xdr);
       e this.axisData = new PbsDataRecorderRpcAxisData(xdr);
       f this.dose = new PbsDataRecorderRpcDoseData(xdr);
       g this.smps = new PbsDataRecorderRpcSmpsData(xdr);
       h this.quad = new PbsDataRecorderRpcQuadData(xdr);
         */
        #endregion

        public int sizeOfLayerData;
        //a

        #region
        /*
        //10 nozzleEntranceLastElementData
        public int dataAvailable;
        // in case dataAvailable is 0
            public int nozzlelayerId;
            public int nozzleelementId;
            public float nozzlexDose;
            public float nozzleyDose;
            public float nozzlexPosition;
            public float nozzleyPosition;
            public float nozzlexWidth;
            public float nozzleyWidth;
            public float[] nozzlexChannels = new float[32];
            public float[] nozzleyChannels = new float[32];
            public int nozzlexValid;
            public int nozzleyValid;
        
        //11 last ElementData
        public int lastlayerId ;
        public int lastelementId ;
        public float lastxDose  ;
        public float lastyDose  ;
        public float lastxPositio;
        public float lastyPositio;
        public float lastxWidth ;
        public float lastyWidth ;
        public float[] lastxChannels = new float[32];
        public float[] lastyChannels = new float[32];
        public int lastxValid;
        public int lastyValid;

        //12 FailureCause
        public int failureCausesize;
        public int[] failureCause;
        //gatewayerrorreport
        public int gatewayerror;
        public int gatewaycontroller;
        public int gatewayreturnCode;
        // 작성 중
        */
        #endregion

        char[] version = new char[4];
        public ElementEvent[] elementEvent = new ElementEvent[0];
        public ElementData[] elementData = new ElementData[0];
        public bool ErrorCheck = false;
        // When read appropriate Xdr file, it returns true.

        //public XdrDataRecorderRpcLayerConverter(Stream _xdrfile) // 계속해서 4개씩 읽어나감
        public XdrConverter_Record(Stream _xdrfile) // 계속해서 4개씩 읽어나감
        {
            if (_xdrfile.CanRead == false)
            {
                ErrorCheck = true;
                return;
            }
            try
            {
                BinaryReader rbin = new BinaryReader(_xdrfile);
                rbin.ReadByte();
                for (int i = 0; i < version.Length; i++)
                    version[i] = rbin.ReadChar(); //version == 1.43 always
                beamlineId = XdrRead.XdrInt(rbin);
                layerID = XdrRead.XdrInt(rbin);
                string version_string = new string(version);
                if (version_string.Equals("1.43") != false)
                {
                    //read event data
                    sizeOfEvents = XdrRead.XdrInt(rbin);
                    elementEvent = new ElementEvent[sizeOfEvents];
                    for (int i = 0; i < sizeOfEvents; i++)
                    {
                        elementEvent[i] = new ElementEvent();
                        elementEvent[i].typeEvent = XdrRead.XdrInt(rbin);
                        elementEvent[i].epochTimeEvent = XdrRead.XdrInt(rbin);
                        elementEvent[i].nrOfMicrosecsEvent = XdrRead.XdrInt(rbin);
                    }
                    acquisitionPeriod = XdrRead.XdrInt(rbin);
                    sgcuLastElementId = XdrRead.XdrInt(rbin);
                    rcuLastElementId = XdrRead.XdrInt(rbin);
                    fcuLastElementId = XdrRead.XdrInt(rbin);
                    necuLastElementId = XdrRead.XdrInt(rbin);

                    sizeOfLayerData = XdrRead.XdrInt(rbin);
                    elementData = new ElementData[sizeOfLayerData];
                    for (int i = 0; i < sizeOfLayerData; i++)
                    {
                        elementData[i] = new ElementData(); /* id of the selected beamline */
                        elementData[i].epochTimeData = XdrRead.XdrInt(rbin);
                        elementData[i].nrOfMicrosecsData = XdrRead.XdrInt(rbin);
                        elementData[i].elementID = XdrRead.XdrInt(rbin);
                        /* raw feedback from iseu (Beam current at cyclotron exit) */
                        elementData[i].beamCurrent = XdrRead.XdrFloat(rbin);

                        /* raw feedback of Hall probe magnetic fields */
                        elementData[i].FieldX = XdrRead.XdrFloat(rbin);
                        elementData[i].FieldY = XdrRead.XdrFloat(rbin);

                        /* strip current, measured over a single timeslice */
                        elementData[i].ic1xDoseRate = XdrRead.XdrFloat(rbin);
                        elementData[i].ic1yDoseRate = XdrRead.XdrFloat(rbin);
                        /* strip current, measured over a single timeslice */
                        elementData[i].ic1xDose = XdrRead.XdrFloat(rbin);
                        elementData[i].ic1yDose = XdrRead.XdrFloat(rbin);
                        /* width of the beam in X and Y direction */
                        elementData[i].ic1xWidth = XdrRead.XdrFloat(rbin);
                        elementData[i].ic1yWidth = XdrRead.XdrFloat(rbin);
                        /* position the beam in X and Y direction */
                        elementData[i].ic1xPosition = XdrRead.XdrFloat(rbin);
                        elementData[i].ic1yPosition = XdrRead.XdrFloat(rbin);

                        elementData[i].axisDataxDoseRate = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDatayDoseRate = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDataxDose = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDatayDose = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDataxWidth = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDatayWidth = XdrRead.XdrFloat(rbin);
                        //elementData[i].axisDataxPosition = XdrRead.XdrFloat(rbin);
                        //elementData[i].axisDatayPosition = XdrRead.XdrFloat(rbin);

                        elementData[i].axisDataxPosition = XdrRead.XdrFloat(rbin);
                        elementData[i].axisDatayPosition = XdrRead.XdrFloat(rbin);

                        /* IC2/IC3 current, measured over a single timeslice */
                        elementData[i].primaryDoseRate = XdrRead.XdrFloat(rbin); ;
                        elementData[i].redundantDoseRate = XdrRead.XdrFloat(rbin);
                        /* sum of IC2/IC3 dose over a single timeslice */
                        elementData[i].primaryDose = XdrRead.XdrFloat(rbin); ;
                        elementData[i].redundantDose = XdrRead.XdrFloat(rbin); ;
                        /* SMPS feedbacks in both directions X-Y (primary and redundant */
                        elementData[i].smpsprimaryCurrentX = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsprimaryVoltageX = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsredundantCurrentX = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsredundantVoltageX = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsprimaryCurrentY = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsprimaryVoltageY = XdrRead.XdrFloat(rbin);
                        elementData[i].smpsredundantCurrentY = XdrRead.XdrFloat(rbin); ;
                        elementData[i].smpsredundantVoltageY = XdrRead.XdrFloat(rbin);
                        /* Quad feedbacks primary and redundant */
                        elementData[i].quadprimaryCurrentX = XdrRead.XdrFloat(rbin);
                        elementData[i].quadprimaryVoltageX = XdrRead.XdrFloat(rbin);
                        elementData[i].quadredundantCurrentX = XdrRead.XdrFloat(rbin);
                        elementData[i].quadredundantVoltageX = XdrRead.XdrFloat(rbin); ;
                        elementData[i].quadprimaryCurrentY = XdrRead.XdrFloat(rbin);
                        elementData[i].quadprimaryVoltageY = XdrRead.XdrFloat(rbin); ;
                        elementData[i].quadredundantCurrentY = XdrRead.XdrFloat(rbin);
                        elementData[i].quadredundantVoltageY = XdrRead.XdrFloat(rbin);
                    }
                    rbin.Close();

                    ErrorCheck = false;
                }
                else
                    ErrorCheck = true;
            }
            catch
            {
                ErrorCheck = true;
            }
        }
        public struct ElementEvent
        {
            public int typeEvent;
            public int epochTimeEvent;
            public int nrOfMicrosecsEvent;
        }
        public struct ElementData
        {

            public int epochTimeData;
            public int nrOfMicrosecsData;
            //b
            public int elementID;
            //c
            public float beamCurrent;
            public float FieldX;
            public float FieldY;
            //d ic1 Data
            public float ic1xDoseRate;
            public float ic1yDoseRate;
            public float ic1xDose;
            public float ic1yDose;
            public float ic1xWidth;
            public float ic1yWidth;
            public float ic1xPosition;
            public float ic1yPosition;
            //e axisData ic2, 3
            public float axisDataxDoseRate;
            public float axisDatayDoseRate;
            public float axisDataxDose;
            public float axisDatayDose;
            public float axisDataxWidth;
            public float axisDatayWidth;
            public float axisDataxPosition;
            public float axisDatayPosition;
            //f 
            public float primaryDoseRate;
            public float redundantDoseRate;
            public float primaryDose;
            public float redundantDose;
            //g smps
            public float smpsprimaryCurrentX;
            public float smpsprimaryVoltageX;
            public float smpsredundantCurrentX;
            public float smpsredundantVoltageX;
            public float smpsprimaryCurrentY;
            public float smpsprimaryVoltageY;
            public float smpsredundantCurrentY;
            public float smpsredundantVoltageY;
            //h quad
            public float quadprimaryCurrentX;
            public float quadprimaryVoltageX;
            public float quadredundantCurrentX;
            public float quadredundantVoltageX;
            public float quadprimaryCurrentY;
            public float quadprimaryVoltageY;
            public float quadredundantCurrentY;
            public float quadredundantVoltageY;
        }
        public static class XdrRead
        {
            public static byte[] intBuffer = new byte[4];
            public static byte[] floatBuffer = new byte[4];

            public static int XdrInt(BinaryReader binaryReader)
            {
                for (int i = 0; i < 4; i++)
                {
                    intBuffer[3 - i] = binaryReader.ReadByte();
                }
                return BitConverter.ToInt32(intBuffer, 0);
            }
            public static float XdrFloat(BinaryReader binaryReader)
            {
                for (int i = 0; i < 4; i++)
                {
                    floatBuffer[3 - i] = binaryReader.ReadByte();

                }
                return BitConverter.ToSingle(floatBuffer, 0);
            }
            public static long ToLong(int left, int right)
            {
                return left * (long)1000 + right / (long)1000;
            }
        }
    }

    public class XdrConverter_Specific
    {
        char[] version = new char[4];
        public bool ErrorCheck = false;
        public int sizeOfElement;

        #region Data Struct

        public float id;
        public double range;
        public double totalCharge;
        public int diagnosticMode;
        public double kFactor;
        public ScanningControllerPbsLayerElement[] elements = new ScanningControllerPbsLayerElement[0];
        public float smxOffset;
        public float smyOffset;
        public float icxOffset;
        public float icyOffset;

        public struct ScanningControllerPbsLayerElement
        {
            public int type;
            public int spotId;
            public float xCurrentSetpoint;
            public float yCurrentSetpoint;
            public float targetCharge;
            public float beamCurrentSetpoint;
            public float xQuadCurrentSetpoint;
            public float yQuadCurrentSetpoint;
            public float maxDuration;
            public float minPrimaryCharge;
            public float maxPrimaryCharge;
            public float minSecondaryCharge;
            public float maxSecondaryCharge;
            public float minTernaryCharge;
            public float maxTernaryCharge;
            public float minNozzleEntranceCharge;
            public float maxNozzleEntranceCharge;
            public float xMinPrimaryCurrentFeedback;
            public float yMinPrimaryCurrentFeedback;
            public float xMaxPrimaryCurrentFeedback;
            public float yMaxPrimaryCurrentFeedback;
            public float xMinPrimaryVoltageFeedback;
            public float yMinPrimaryVoltageFeedback;
            public float xMaxPrimaryVoltageFeedback;
            public float yMaxPrimaryVoltageFeedback;
            public float xMinSecondaryCurrentFeedback;
            public float yMinSecondaryCurrentFeedback;
            public float xMaxSecondaryCurrentFeedback;
            public float yMaxSecondaryCurrentFeedback;
            public float xMinSecondaryVoltageFeedback;
            public float yMinSecondaryVoltageFeedback;
            public float xMaxSecondaryVoltageFeedback;
            public float yMaxSecondaryVoltageFeedback;
            public float xQuadMinPrimaryCurrentFeedback;
            public float xQuadMaxPrimaryCurrentFeedback;
            public float yQuadMinPrimaryCurrentFeedback;
            public float yQuadMaxPrimaryCurrentFeedback;
            public float xQuadMinPrimaryVoltageFeedback;
            public float xQuadMaxPrimaryVoltageFeedback;
            public float yQuadMinPrimaryVoltageFeedback;
            public float yQuadMaxPrimaryVoltageFeedback;
            public float xQuadMinSecondaryCurrentFeedback;
            public float xQuadMaxSecondaryCurrentFeedback;
            public float yQuadMinSecondaryCurrentFeedback;
            public float yQuadMaxSecondaryCurrentFeedback;
            public float xQuadMinSecondaryVoltageFeedback;
            public float xQuadMaxSecondaryVoltageFeedback;
            public float yQuadMinSecondaryVoltageFeedback;
            public float yQuadMaxSecondaryVoltageFeedback;
            public float xMinField;
            public float yMinField;
            public float xMaxField;
            public float yMaxField;
            public float minPrimaryDoseRate;
            public float maxPrimaryDoseRate;
            public float minSecondaryDoseRate;
            public float maxSecondaryDoseRate;
            public float minTernaryDoseRate;
            public float maxTernaryDoseRate;
            public float minNozzleEntranceDoseRate;
            public float maxNozzleEntranceDoseRate;
            public float minCycloBeam;
            public float maxCycloBeam;
            public float xMinBeamWidth;
            public float xMaxBeamWidth;
            public float yMinBeamWidth;
            public float yMaxBeamWidth;
            public float xPositionLow;
            public float yPositionLow;
            public float xPositionHigh;
            public float yPositionHigh;
            public float xMinNozzleEntrancePositionThreshold;
            public float xMaxNozzleEntrancePositionThreshold;
            public float yMinNozzleEntrancePositionThreshold;
            public float yMaxNozzleEntrancePositionThreshold;
            public float xMinNozzleEntranceWidthThreshold;
            public float xMaxNozzleEntranceWidthThreshold;
            public float yMinNozzleEntranceWidthThreshold;
            public float yMaxNozzleEntranceWidthThreshold;
        }

        #endregion

        // com -> iba -> tcs.beam -> bds.devices.rpc -> datarecorder -> v1_42 -> ScanningControllerPbsLayer.class
        public XdrConverter_Specific(Stream _xdrfile)
        {
            try
            {
                BinaryReader rbin = new BinaryReader(_xdrfile);
                rbin.ReadByte();
                for (int i = 0; i < version.Length; i++)
                {
                    version[i] = rbin.ReadChar();
                }

                #region JAVA code

                // this.id.xdrEncode(xdr);
                // xdr.xdrEncodeDouble(this.range);
                // xdr.xdrEncodeDouble(this.totalCharge);
                // xdr.xdrEncodeInt(this.diagnosticMode);
                // xdr.xdrEncodeDouble(this.kFactor);
                // int $size = this.elements.length;
                // xdr.xdrEncodeInt($size);
                // for (int $idx = 0; $idx < $size; )
                // {
                //     this.elements[$idx].xdrEncode(xdr); $idx++;
                // }
                // xdr.xdrEncodeFloat(this.smxOffset);
                // xdr.xdrEncodeFloat(this.smyOffset);
                // xdr.xdrEncodeFloat(this.icxOffset);
                // xdr.xdrEncodeFloat(this.icyOffset);

                #endregion

                id = XdrRead.XdrInt(rbin);
                range = XdrRead.XdrDouble(rbin);
                totalCharge = XdrRead.XdrDouble(rbin);
                diagnosticMode = XdrRead.XdrInt(rbin);
                kFactor = XdrRead.XdrDouble(rbin);

                sizeOfElement = XdrRead.XdrInt(rbin);
                elements = new ScanningControllerPbsLayerElement[sizeOfElement];

                for (int i = 0; i < sizeOfElement; i++)
                {
                    elements[i] = new ScanningControllerPbsLayerElement();

                    elements[i].type = XdrRead.XdrInt(rbin);
                    elements[i].spotId = XdrRead.XdrInt(rbin);
                    elements[i].xCurrentSetpoint = XdrRead.XdrFloat(rbin);
                    elements[i].yCurrentSetpoint = XdrRead.XdrFloat(rbin);
                    elements[i].targetCharge = XdrRead.XdrFloat(rbin);
                    elements[i].beamCurrentSetpoint = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadCurrentSetpoint = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadCurrentSetpoint = XdrRead.XdrFloat(rbin);
                    elements[i].maxDuration = XdrRead.XdrFloat(rbin);
                    elements[i].minPrimaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].maxPrimaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].minSecondaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].maxSecondaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].minTernaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].maxTernaryCharge = XdrRead.XdrFloat(rbin);
                    elements[i].minNozzleEntranceCharge = XdrRead.XdrFloat(rbin);
                    elements[i].maxNozzleEntranceCharge = XdrRead.XdrFloat(rbin);
                    elements[i].xMinPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMinPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMinPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMinPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMinSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMinSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMinSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMinSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMinPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMaxPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMinPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMaxPrimaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMinPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMaxPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMinPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMaxPrimaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMinSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMaxSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMinSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMaxSecondaryCurrentFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMinSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xQuadMaxSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMinSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].yQuadMaxSecondaryVoltageFeedback = XdrRead.XdrFloat(rbin);
                    elements[i].xMinField = XdrRead.XdrFloat(rbin);
                    elements[i].yMinField = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxField = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxField = XdrRead.XdrFloat(rbin);
                    elements[i].minPrimaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].maxPrimaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].minSecondaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].maxSecondaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].minTernaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].maxTernaryDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].minNozzleEntranceDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].maxNozzleEntranceDoseRate = XdrRead.XdrFloat(rbin);
                    elements[i].minCycloBeam = XdrRead.XdrFloat(rbin);
                    elements[i].maxCycloBeam = XdrRead.XdrFloat(rbin);
                    elements[i].xMinBeamWidth = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxBeamWidth = XdrRead.XdrFloat(rbin);
                    elements[i].yMinBeamWidth = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxBeamWidth = XdrRead.XdrFloat(rbin);
                    elements[i].xPositionLow = XdrRead.XdrFloat(rbin);
                    elements[i].yPositionLow = XdrRead.XdrFloat(rbin);
                    elements[i].xPositionHigh = XdrRead.XdrFloat(rbin);
                    elements[i].yPositionHigh = XdrRead.XdrFloat(rbin);
                    elements[i].xMinNozzleEntrancePositionThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxNozzleEntrancePositionThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].yMinNozzleEntrancePositionThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxNozzleEntrancePositionThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].xMinNozzleEntranceWidthThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].xMaxNozzleEntranceWidthThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].yMinNozzleEntranceWidthThreshold = XdrRead.XdrFloat(rbin);
                    elements[i].yMaxNozzleEntranceWidthThreshold = XdrRead.XdrFloat(rbin);
                }

                smxOffset = XdrRead.XdrFloat(rbin);
                smyOffset = XdrRead.XdrFloat(rbin);
                icxOffset = XdrRead.XdrFloat(rbin);
                icyOffset = XdrRead.XdrFloat(rbin);

                rbin.Close();

                ErrorCheck = false;
            }
            catch
            {
                ErrorCheck = true;
            }
        }

        public static class XdrRead
        {
            public static byte[] intBuffer = new byte[4];
            public static byte[] floatBuffer = new byte[4];
            public static byte[] doubleBuffer = new byte[8];

            public static int XdrInt(BinaryReader binaryReader)
            {
                for (int i = 0; i < 4; i++)
                {
                    intBuffer[3 - i] = binaryReader.ReadByte();
                }
                return BitConverter.ToInt32(intBuffer, 0);
            }
            public static float XdrFloat(BinaryReader binaryReader)
            {
                for (int i = 0; i < 4; i++)
                {
                    floatBuffer[3 - i] = binaryReader.ReadByte();
                }
                return BitConverter.ToSingle(floatBuffer, 0);
            }
            public static double XdrDouble(BinaryReader binaryReader)
            {
                for (int i = 0; i < 8; i++)
                {
                    doubleBuffer[7 - i] = binaryReader.ReadByte();
                }
                return BitConverter.ToDouble(doubleBuffer, 0);
            }
            public static long ToLong(int left, int right)
            {
                return left * (long)1000 + right / (long)1000;
            }
        }
    }
}
