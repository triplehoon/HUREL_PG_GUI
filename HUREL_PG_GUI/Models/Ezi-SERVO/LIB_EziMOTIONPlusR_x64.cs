using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FASTECH
{
    public partial class EziMOTIONPlusRLib
    {
        // Referred by ReturnCodes_Define.h
        // typedef enum _FMM_ERROR
        public const int FMM_OK = 0;

        public const int FMM_NOT_OPEN = 1;
        public const int FMM_INVALID_PORT_NUM = 2;
        public const int FMM_INVALID_SLAVE_NUM = 3;

        public const int FMC_DISCONNECTED = 5;
        public const int FMC_TIMEOUT_ERROR = 6;
        public const int FMC_CRCFAILED_ERROR = 7;
        public const int FMC_RECVPACKET_ERROR = 8;

        public const int FMM_POSTABLE_ERROR = 9;

        public const int FMP_FRAMETYPEERROR = 0x80;
        public const int FMP_DATAERROR = 0x81;
        public const int FMP_PACKETERROR = 0x82;

        public const int FMP_RUNFAIL = 0x85;
        public const int FMP_RESETFAIL = 0x86;
        public const int FMP_SERVOONFAIL1 = 0x87;
        public const int FMP_SERVOONFAIL2 = 0x88;
        public const int FMP_SERVOONFAIL3 = 0x89;

        public const int FMP_SERVOOFF_FAIL = 0x8A;
        public const int FMP_ROMACCESS = 0x8B;

        public const int FMP_PACKETCRCERROR = 0xAA;

        public const int FMM_UNKNOWN_ERROR = 0xFF;

        // Referred by COMM_Define.h
        // Constants
        public const int MAX_SLAVE_NUMS = 16;

        // Referred by FAS_EziMOTIONPlusR.h
        // Functions.

        ////------------------------------------------------------------------------------
        ////			Connection Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API BOOL WINAPI	FAS_Connect(BYTE nPortNo, DWORD dwBaud);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_Connect(byte nPortNo, uint dwBaud);

        //EZI_PLUSR_API BOOL WINAPI	FAS_OpenPort(BYTE nPortNo, DWORD dwBaud);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_OpenPort(byte nPortNo, uint dwBaud);

        //EZI_PLUSR_API BOOL WINAPI	FAS_AttachSlave(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AttachSlave(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API void WINAPI	FAS_Close(BYTE nPortNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern void FAS_Close(byte nPortNo);

        ////------------------------------------------------------------------------------
        ////			Log Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API void WINAPI	FAS_EnableLog(BOOL bEnable);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern void FAS_EnableLog(int bEnable);

        //EZI_PLUSR_API void WINAPI	FAS_SetLogLevel(enum LOG_LEVEL level);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern void FAS_SetLogLevel(int level);
        public static void FAS_SetLogLevel(LOG_LEVEL level)
        {
            switch (level)
            {
                case LOG_LEVEL.LOG_LEVEL_COMM:
                    FAS_SetLogLevel(0);
                    break;
                case LOG_LEVEL.LOG_LEVEL_PARAM:
                    FAS_SetLogLevel(1);
                    break;
                case LOG_LEVEL.LOG_LEVEL_MOTION:
                    FAS_SetLogLevel(2);
                    break;
                //case LOG_LEVEL.LOG_LEVEL_ALL:
                default:
                    FAS_SetLogLevel(3);
                    break;
            }
        }

        //EZI_PLUSR_API BOOL WINAPI	FAS_SetLogPath(LPCWSTR lpPath);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetLogPath([MarshalAs(UnmanagedType.LPWStr)] string lpPath);

        //EZI_PLUSR_API void WINAPI	FAS_PrintCustomLog(BYTE nPortNo, enum LOG_LEVEL level, LPCTSTR lpszMsg);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_PrintCustomLog(byte nPortNo, int level, [MarshalAs(UnmanagedType.LPWStr)] string lpszMsg);
        public static int FAS_PrintCustomLog(byte nPortNo, LOG_LEVEL level, string lpszMsg)
        {
            int nRtn = 0;

            switch (level)
            {
                case LOG_LEVEL.LOG_LEVEL_COMM:
                    nRtn = FAS_PrintCustomLog(nPortNo, 0, lpszMsg);
                    break;
                case LOG_LEVEL.LOG_LEVEL_PARAM:
                    nRtn = FAS_PrintCustomLog(nPortNo, 1, lpszMsg);
                    break;
                case LOG_LEVEL.LOG_LEVEL_MOTION:
                    nRtn = FAS_PrintCustomLog(nPortNo, 2, lpszMsg);
                    break;
                //case LOG_LEVEL.LOG_LEVEL_ALL:
                default:
                    nRtn = FAS_PrintCustomLog(nPortNo, 3, lpszMsg);
                    break;
            }

            return nRtn;
        }

        ////------------------------------------------------------------------------------
        ////			Info Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API BOOL WINAPI	FAS_IsSlaveExist(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_IsSlaveExist(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_GetSlaveInfo(BYTE nPortNo, BYTE iSlaveNo, BYTE* pType, LPSTR lpBuff, int nBuffSize);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_GetSlaveInfo(byte nPortNo, byte iSlaveNo, ref byte pType, StringBuilder lpBuff, int nBuffSize);
        public static int FAS_GetSlaveInfo(byte nPortNo, byte iSlaveNo, ref byte pType, ref string version)
        {
            int nRtn = FMM_OK;
            byte type = 0;
            StringBuilder sb = new StringBuilder(256);

            nRtn = FAS_GetSlaveInfo(nPortNo, iSlaveNo, ref type, sb, 256);
            if (nRtn == FMM_OK)
            {
                pType = type;

                version = sb.ToString();
            }

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_GetMotorInfo(BYTE nPortNo, BYTE iSlaveNo, BYTE* pType, LPSTR lpBuff, int nBuffSize);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_GetMotorInfo(byte nPortNo, byte iSlaveNo, ref byte pType, StringBuilder lpBuff, int nBuffSize);
        public static int FAS_GetMotorInfo(byte nPortNo, byte iSlaveNo, ref byte pType, ref string motor)
        {
            int nRtn = FMM_OK;
            byte type = 0;
            StringBuilder sb = new StringBuilder(256);

            nRtn = FAS_GetMotorInfo(nPortNo, iSlaveNo, ref type, sb, 256);
            if (nRtn == FMM_OK)
            {
                pType = type;

                motor = sb.ToString();
            }

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_GetSlaveInfoEx(BYTE nPortNo, BYTE iSlaveNo, DRIVE_INFO* lpDriveInfo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_GetSlaveInfoEx(byte nPortNo, byte iSlaveNo, byte[] lpDriveInfo);
        public static int FAS_GetSlaveInfoEx(byte nPortNo, byte iSlaveNo, ref DRIVE_INFO DriveInfo)
        {
            int nRtn = FMM_OK;
            byte[] buffer = new byte[DRIVE_INFO.BUFF_SIZE];

            nRtn = FAS_GetSlaveInfoEx(nPortNo, iSlaveNo, buffer);
            if (nRtn == FMM_OK)
            {
                DriveInfo.copy(buffer);
            }

            return nRtn;
        }

        ////------------------------------------------------------------------------------
        ////			Parameter Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_SaveAllParameters(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SaveAllParameters(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_SetParameter(BYTE nPortNo, BYTE iSlaveNo, BYTE iParamNo, long lParamValue);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetParameter(byte nPortNo, byte iSlaveNo, byte iParamNo, int lParamValue);

        //EZI_PLUSR_API int WINAPI		FAS_GetParameter(BYTE nPortNo, BYTE iSlaveNo, BYTE iParamNo, long* lParamValue);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetParameter(byte nPortNo, byte iSlaveNo, byte iParamNo, ref int lParamValue);

        //EZI_PLUSR_API int WINAPI		FAS_GetROMParameter(BYTE nPortNo, BYTE iSlaveNo, BYTE iParamNo, long* lRomParam);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetROMParameter(byte nPortNo, byte iSlaveNo, byte iParamNo, ref int lRomParam);

        ////------------------------------------------------------------------------------
        ////					IO Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_SetIOInput(BYTE nPortNo, BYTE iSlaveNo, DWORD dwIOSETMask, DWORD dwIOCLRMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetIOInput(byte nPortNo, byte iSlaveNo, uint dwIOSETMask, uint dwIOCLRMask);

        //EZI_PLUSR_API int WINAPI		FAS_GetIOInput(BYTE nPortNo, BYTE iSlaveNo, DWORD* dwIOInput);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIOInput(byte nPortNo, byte iSlaveNo, ref uint dwIOInput);

        //EZI_PLUSR_API int WINAPI		FAS_SetIOOutput(BYTE nPortNo, BYTE iSlaveNo, DWORD dwIOSETMask, DWORD dwIOCLRMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetIOOutput(byte nPortNo, byte iSlaveNo, uint dwIOSETMask, uint dwIOCLRMask);

        //EZI_PLUSR_API int WINAPI		FAS_GetIOOutput(BYTE nPortNo, BYTE iSlaveNo, DWORD* dwIOOutput);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIOOutput(byte nPortNo, byte iSlaveNo, ref uint dwIOOutput);

        //EZI_PLUSR_API int WINAPI		FAS_GetIOAssignMap(BYTE nPortNo, BYTE iSlaveNo, BYTE iIOPinNo, DWORD* dwIOLogicMask, BYTE* bLevel);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIOAssignMap(byte nPortNo, byte iSlaveNo, byte iIOPinNo, ref uint dwIOLogicMask, ref byte bLevel);

        //EZI_PLUSR_API int WINAPI		FAS_SetIOAssignMap(BYTE nPortNo, BYTE iSlaveNo, BYTE iIOPinNo, DWORD dwIOLogicMask, BYTE bLevel);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetIOAssignMap(byte nPortNo, byte iSlaveNo, byte iIOPinNo, uint dwIOLogicMask, byte bLevel);

        //EZI_PLUSR_API int WINAPI		FAS_IOAssignMapReadROM(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_IOAssignMapReadROM(byte nPortNo, byte iSlaveNo);

        ////------------------------------------------------------------------------------
        ////					Servo Driver Control Functions
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_ServoEnable(BYTE nPortNo, BYTE iSlaveNo, BOOL bOnOff);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ServoEnable(byte nPortNo, byte iSlaveNo, int bOnOff);

        //EZI_PLUSR_API int WINAPI		FAS_ServoAlarmReset(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ServoAlarmReset(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_StepAlarmReset(BYTE nPortNo, BYTE iSlaveNo, BOOL bReset);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_StepAlarmReset(byte nPortNo, byte iSlaveNo, int bReset);

        ////------------------------------------------------------------------------------
        ////					Read Status and Position
        ////------------------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_GetAxisStatus(BYTE nPortNo, BYTE iSlaveNo, DWORD* dwAxisStatus);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetAxisStatus(byte nPortNo, byte iSlaveNo, ref uint dwAxisStatus);

        //EZI_PLUSR_API int WINAPI		FAS_GetIOAxisStatus(BYTE nPortNo, BYTE iSlaveNo, DWORD* dwInStatus, DWORD* dwOutStatus, DWORD* dwAxisStatus);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIOAxisStatus(byte nPortNo, byte iSlaveNo, ref uint dwInStatus, ref uint dwOutStatus, ref uint dwAxisStatus);

        //EZI_PLUSR_API int WINAPI		FAS_GetMotionStatus(BYTE nPortNo, BYTE iSlaveNo, long* lCmdPos, long* lActPos, long* lPosErr, long* lActVel, WORD* wPosItemNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetMotionStatus(byte nPortNo, byte iSlaveNo, ref int lCmdPos, ref int lActPos, ref int lPosErr, ref int lActVel, ref ushort wPosItemNo);

        //EZI_PLUSR_API int WINAPI		FAS_GetAllStatus(BYTE nPortNo, BYTE iSlaveNo, DWORD* dwInStatus, DWORD* dwOutStatus, DWORD* dwAxisStatus, long* lCmdPos, long* lActPos, long* lPosErr, long* lActVel, WORD* wPosItemNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetAllStatus(byte nPortNo, byte iSlaveNo, ref uint dwInStatus, ref uint dwOutStatus, ref uint dwAxisStatus, ref int lCmdPos, ref int lActPos, ref int lPosErr, ref int lActVel, ref ushort wPosItemNo);

        //EZI_PLUSR_API int WINAPI	FAS_GetAllStatusEx(BYTE nPortNo, BYTE iSlaveNo, BYTE* pTypes, long* pDatas);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetAllStatusEx(byte nPortNo, byte iSlaveNo, byte[] pTypes, int[] pDatas);

        //EZI_PLUSR_API int WINAPI		FAS_SetCommandPos(BYTE nPortNo, BYTE iSlaveNo, long lCmdPos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetCommandPos(byte nPortNo, byte iSlaveNo, int lCmdPos);

        //EZI_PLUSR_API int WINAPI		FAS_SetActualPos(BYTE nPortNo, BYTE iSlaveNo, long lActPos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetActualPos(byte nPortNo, byte iSlaveNo, int lActPos);

        //EZI_PLUSR_API int WINAPI		FAS_ClearPosition(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ClearPosition(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_GetCommandPos(BYTE nPortNo, BYTE iSlaveNo, long* lCmdPos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetCommandPos(byte nPortNo, byte iSlaveNo, ref int lCmdPos);

        //EZI_PLUSR_API int WINAPI		FAS_GetActualPos(BYTE nPortNo, BYTE iSlaveNo, long* lActPos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetActualPos(byte nPortNo, byte iSlaveNo, ref int lActPos);

        //EZI_PLUSR_API int WINAPI		FAS_GetPosError(BYTE nPortNo, BYTE iSlaveNo, long* lPosErr);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetPosError(byte nPortNo, byte iSlaveNo, ref int lPosErr);

        //EZI_PLUSR_API int WINAPI		FAS_GetActualVel(BYTE nPortNo, BYTE iSlaveNo, long* lActVel);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetActualVel(byte nPortNo, byte iSlaveNo, ref int lActVel);

        //EZI_PLUSR_API int WINAPI		FAS_GetAlarmType(BYTE nPortNo, BYTE iSlaveNo, BYTE* nAlarmType);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetAlarmType(byte nPortNo, byte iSlaveNo, ref byte nAlarmType);

        ////------------------------------------------------------------------
        ////					Motion Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_MoveStop(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveStop(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_EmergencyStop(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_EmergencyStop(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_MovePause(BYTE nPortNo, BYTE iSlaveNo, BOOL bPause);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MovePause(byte nPortNo, byte iSlaveNo, int bPause);

        //EZI_PLUSR_API int WINAPI		FAS_MoveOriginSingleAxis(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveOriginSingleAxis(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_MoveSingleAxisAbsPos(BYTE nPortNo, BYTE iSlaveNo, long lAbsPos, DWORD lVelocity);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveSingleAxisAbsPos(byte nPortNo, byte iSlaveNo, int lAbsPos, uint lVelocity);

        //EZI_PLUSR_API int WINAPI		FAS_MoveSingleAxisIncPos(BYTE nPortNo, BYTE iSlaveNo, long lIncPos, DWORD lVelocity);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveSingleAxisIncPos(byte nPortNo, byte iSlaveNo, int lIncPos, uint lVelocity);

        //EZI_PLUSR_API int WINAPI		FAS_MoveToLimit(BYTE nPortNo, BYTE iSlaveNo, DWORD lVelocity, int iLimitDir);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveToLimit(byte nPortNo, byte iSlaveNo, uint lVelocity, int iLimitDir);

        //EZI_PLUSR_API int WINAPI		FAS_MoveVelocity(BYTE nPortNo, BYTE iSlaveNo, DWORD lVelocity, int iVelDir);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveVelocity(byte nPortNo, byte iSlaveNo, uint lVelocity, int iVelDir);

        //EZI_PLUSR_API int WINAPI		FAS_PositionAbsOverride(BYTE nPortNo, BYTE iSlaveNo, long lOverridePos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PositionAbsOverride(byte nPortNo, byte iSlaveNo, int lOverridePos);

        //EZI_PLUSR_API int WINAPI		FAS_PositionIncOverride(BYTE nPortNo, BYTE iSlaveNo, long lOverridePos);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PositionIncOverride(byte nPortNo, byte iSlaveNo, int lOverridePos);

        //EZI_PLUSR_API int WINAPI		FAS_VelocityOverride(BYTE nPortNo, BYTE iSlaveNo, DWORD lVelocity);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_VelocityOverride(byte nPortNo, byte iSlaveNo, uint lVelocity);

        //EZI_PLUSR_API int WINAPI		FAS_MoveLinearAbsPos(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lAbsPos, DWORD lFeedrate, WORD wAccelTime);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveLinearAbsPos(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lAbsPos, uint lFeedrate, ushort wAccelTime);

        //EZI_PLUSR_API int WINAPI		FAS_MoveLinearIncPos(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lIncPos, DWORD lFeedrate, WORD wAccelTime);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveLinearIncPos(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lIncPos, uint lFeedrate, ushort wAccelTime);

        //EZI_PLUSR_API int WINAPI		FAS_MoveLinearAbsPos2(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lAbsPos, DWORD lFeedrate, WORD wAccelTime);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveLinearAbsPos2(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lAbsPos, uint lFeedrate, ushort wAccelTime);

        //EZI_PLUSR_API int WINAPI		FAS_MoveLinearIncPos2(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lIncPos, DWORD lFeedrate, WORD wAccelTime);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveLinearIncPos2(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lIncPos, uint lFeedrate, ushort wAccelTime);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleAbsPos1(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirEndAbs, long* lplCirCenterAbs, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleAbsPos1(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirEndAbs, int[] lplCirCenterAbs, int iDirection, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleIncPos1(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirEndInc, long* lplCirCenterInc, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleIncPos1(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirEndInc, int[] lplCirCenterInc, int iDirection, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleAbsPos2(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirEndAbs, DWORD lRadius, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleAbsPos2(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirEndAbs, uint lRadius, int iDirection, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleIncPos2(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirEndInc, DWORD lRadius, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleIncPos2(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirEndInc, uint lRadius, int iDirection, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleAbsPos3(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirCenterAbs, DWORD nAngle, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleAbsPos3(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirCenterAbs, uint nAngle, int iDirecion, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI	FAS_MoveCircleIncPos3(BYTE nPortNo, BYTE nNoOfSlaves, BYTE* iSlavesNo, long* lplCirCenterInc, DWORD nAngle, int iDirection, DWORD lFeedrate, WORD wAccelTime, int bSCurve);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MoveCircleIncPos3(byte nPortNo, byte nNoOfSlaves, byte[] iSlavesNo, int[] lplCirCenterInc, uint nAngle, int iDirecion, uint lFeedrate, ushort wAccelTime, int bSCurve);

        //EZI_PLUSR_API int WINAPI		FAS_TriggerOutput_RunA(BYTE nPortNo, BYTE iSlaveNo, BOOL bStartTrigger, long lStartPos, DWORD dwPeriod, DWORD dwPulseTime);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_TriggerOutput_RunA(byte nPortNo, byte iSlaveNo, int bStartTrigger, int lStartPos, uint dwPeriod, uint dwPulseTime);

        //EZI_PLUSR_API int WINAPI		FAS_TriggerOutput_Status(BYTE nPortNo, BYTE iSlaveNo, BYTE* bTriggerStatus);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_TriggerOutput_Status(byte nPortNo, byte iSlaveNo, ref byte bTriggerStatus);

        //EZI_PLUSR_API int WINAPI	FAS_SetTriggerOutputEx(BYTE nPortNo, BYTE iSlaveNo, BYTE nOutputNo, BYTE bRun, WORD wOnTime, BYTE nTriggerCount, long* arrTriggerPosition);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetTriggerOutputEx(byte nPortNo, byte iSlaveNo, byte nOutputNo, byte bRun, ushort wOnTime, byte nTriggerCount, int[] arrTriggerPosition);

        //EZI_PLUSR_API int WINAPI	FAS_GetTriggerOutputEx(BYTE nPortNo, BYTE iSlaveNo, BYTE nOutputNo, BYTE* bRun, WORD* wOnTime, BYTE* nTriggerCount, long* arrTriggerPosition);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetTriggerOutputEx(byte nPortNo, byte iSlaveNo, byte nOutputNo, ref byte bRun, ref ushort wOnTime, ref byte nTriggerCount, int[] arrTriggerPosition);

        //EZI_PLUSR_API int WINAPI		FAS_MovePush(BYTE nPortNo, BYTE iSlaveNo, DWORD dwStartSpd, DWORD dwMoveSpd, long lPosition, WORD wAccel, WORD wDecel, WORD wPushRate, DWORD dwPushSpd, long lEndPosition, WORD wPushMode);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_MovePush(byte nPortNo, byte iSlaveNo, uint dwStartSpd, uint dwMoveSpd, int lPosition, ushort wAccel, ushort wDecel, ushort wPushRate, uint dwPushSpd, int lEndPosition, ushort wPushMode);

        //EZI_PLUSR_API int WINAPI		FAS_GetPushStatus(BYTE nPortNo, BYTE iSlaveNo, BYTE* nPushStatus);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetPushStatus(byte nPortNo, byte iSlaveNo, ref byte nPushStatus);

        ////------------------------------------------------------------------
        ////					Ex-Motion Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_MoveSingleAxisAbsPosEx(BYTE nPortNo, BYTE iSlaveNo, long lAbsPos, DWORD lVelocity, MOTION_OPTION_EX* lpExOption);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_MoveSingleAxisAbsPosEx(byte nPortNo, byte iSlaveNo, int lAbsPos, uint lVelocity, byte[] lpExOption);
        public static int FAS_MoveSingleAxisAbsPosEx(byte nPortNo, byte iSlaveNo, int lAbsPos, uint lVelocity, MOTION_OPTION_EX ExOption)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[MOTION_OPTION_EX.BUFF_SIZE];

            ExOption.copyto(buff);

            nRtn = FAS_MoveSingleAxisAbsPosEx(nPortNo, iSlaveNo, lAbsPos, lVelocity, buff);

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_MoveSingleAxisIncPosEx(BYTE nPortNo, BYTE iSlaveNo, long lIncPos, DWORD lVelocity, MOTION_OPTION_EX* lpExOption);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_MoveSingleAxisIncPosEx(byte nPortNo, byte iSlaveNo, int lIncPos, uint lVelocity, byte[] lpExOption);
        public static int FAS_MoveSingleAxisIncPosEx(byte nPortNo, byte iSlaveNo, int lAbsPos, uint lVelocity, MOTION_OPTION_EX ExOption)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[MOTION_OPTION_EX.BUFF_SIZE];

            ExOption.copyto(buff);

            nRtn = FAS_MoveSingleAxisIncPosEx(nPortNo, iSlaveNo, lAbsPos, lVelocity, buff);

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_MoveVelocityEx(BYTE nPortNo, BYTE iSlaveNo, DWORD lVelocity, int iVelDir, VELOCITY_OPTION_EX* lpExOption);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_MoveVelocityEx(byte nPortNo, byte iSlaveNo, uint lVelocity, int iVelDir, byte[] lpExOption);
        public static int FAS_MoveVelocityEx(byte nPortNo, byte iSlaveNo, uint lVelocity, int iVelDir, VELOCITY_OPTION_EX ExOption)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[VELOCITY_OPTION_EX.BUFF_SIZE];

            ExOption.copyto(buff);

            nRtn = FAS_MoveVelocityEx(nPortNo, iSlaveNo, lVelocity, iVelDir, buff);

            return nRtn;
        }

        ////------------------------------------------------------------------
        ////					All-Motion Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_AllMoveStop(BYTE nPortNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AllMoveStop(byte nPortNo);

        //EZI_PLUSR_API int WINAPI		FAS_AllEmergencyStop(BYTE nPortNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AllEmergencyStop(byte nPortNo);

        //EZI_PLUSR_API int WINAPI		FAS_AllMoveOriginSingleAxis(BYTE nPortNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AllMoveOriginSingleAxis(byte nPortNo);

        //EZI_PLUSR_API int WINAPI		FAS_AllMoveSingleAxisAbsPos(BYTE nPortNo, long lAbsPos, DWORD lVelocity);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AllMoveSingleAxisAbsPos(byte nPortNo, int lAbsPos, uint lVelocity);

        //EZI_PLUSR_API int WINAPI		FAS_AllMoveSingleAxisIncPos(BYTE nPortNo, long lIncPos, DWORD lVelocity);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_AllMoveSingleAxisIncPos(byte nPortNo, int lIncPos, uint lVelocity);

        ////------------------------------------------------------------------
        ////					Position Table Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_PosTableReadItem(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo, LPITEM_NODE lpItem);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_PosTableReadItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, byte[] lpItem);
        public static int FAS_PosTableReadItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, ref ITEM_NODE Item)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[ITEM_NODE.BUFF_SIZE];

            nRtn = FAS_PosTableReadItem(nPortNo, iSlaveNo, wItemNo, buff);
            if (nRtn == FMM_OK)
            {
                Item.copyfrom(buff);
            }

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_PosTableWriteItem(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo, LPITEM_NODE lpItem);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_PosTableWriteItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, byte[] lpItem);
        public static int FAS_PosTableWriteItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, ITEM_NODE Item)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[ITEM_NODE.BUFF_SIZE];

            Item.copyto(buff);

            nRtn = FAS_PosTableWriteItem(nPortNo, iSlaveNo, wItemNo, buff);

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_PosTableWriteROM(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableWriteROM(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_PosTableReadROM(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableReadROM(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_PosTableRunItem(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableRunItem(byte nPortNo, byte iSlaveNo, ushort wItemNo);

        //EZI_PLUSR_API int WINAPI		FAS_PosTableReadOneItem(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo, WORD wOffset, long* lPosItemVal);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableReadOneItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, ushort wOffset, ref int lPosItemVal);

        //EZI_PLUSR_API int WINAPI		FAS_PosTableWriteOneItem(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo, WORD wOffset, long lPosItemVal);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableWriteOneItem(byte nPortNo, byte iSlaveNo, ushort wItemNo, ushort wOffset, int lPosItemVal);

        //EZI_PLUSR_API int WINAPI		FAS_PosTableSingleRunItem(BYTE nPortNo, BYTE iSlaveNo, BOOL bNextMove, WORD wItemNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_PosTableSingleRunItem(byte nPortNo, byte iSlaveNo, int bNextMove, ushort wItemNo);

        ////------------------------------------------------------------------
        ////					Gap Control Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_GapControlEnable(BYTE nPortNo, BYTE iSlaveNo, WORD wItemNo, long lGapCompSpeed, long lGapAccTime, long lGapDecTime, long lGapStartSpeed);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GapControlEnable(byte nPortNo, byte iSlaveNo, ushort wItemNo, int lGapCompSpeed, int lGapAccTime, int lGapDecTime, int lGapStartSpeed);

        //EZI_PLUSR_API int WINAPI		FAS_GapControlDisable(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GapControlDisable(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI		FAS_IsGapControlEnable(BYTE nPortNo, BYTE iSlaveNo, BOOL* bIsEnable, WORD* wCurrentItemNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_IsGapControlEnable(byte nPortNo, byte iSlaveNo, ref int bIsEnable, ref ushort wCurrentItemNo);

        //EZI_PLUSR_API int WINAPI		FAS_GapControlGetADCValue(BYTE nPortNo, BYTE iSlaveNo, long* lADCValue);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GapControlGetADCValue(byte nPortNo, byte iSlaveNo, ref int lADCValue);

        //EZI_PLUSR_API int WINAPI		FAS_GapOneResultMonitor(BYTE nPortNo, BYTE iSlaveNo, BYTE* bUpdated, long* iIndex, long* lGapValue, long* lCmdPos, long* lActPos, long* lCompValue, long* lReserved);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GapOneResultMonitor(byte nPortNo, byte iSlaveNo, ref byte bUpdated, ref int iIndex, ref int lGapValue, ref int lCmdPos, ref int lActPos, ref int lCompValue, ref int lReserved);

        ////------------------------------------------------------------------
        ////					Alarm Type History Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI		FAS_GetAlarmLogs(BYTE nPortNo, BYTE iSlaveNo, ALARM_LOG* pAlarmLog);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_GetAlarmLogs(byte nPortNo, byte iSlaveNo, byte[] pAlarmLog);
        public static int FAS_GetAlarmLogs(byte nPortNo, byte iSlaveNo, ref ALARM_LOG AlarmLog)
        {
            int nRtn = FMM_OK;
            byte[] buff = new byte[ALARM_LOG.BUFF_SIZE];

            nRtn = FAS_GetAlarmLogs(nPortNo, iSlaveNo, buff);
            if (nRtn == FMM_OK)
            {
                AlarmLog.copy(buff);
            }

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI		FAS_ResetAlarmLogs(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ResetAlarmLogs(byte nPortNo, byte iSlaveNo);

        ////------------------------------------------------------------------
        ////					I/O Module Functions.
        ////------------------------------------------------------------------
        //EZI_PLUSR_API int WINAPI	FAS_GetInput(BYTE nPortNo, BYTE iSlaveNo, unsigned long* uInput, unsigned long* uLatch);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetInput(byte nPortNo, byte iSlaveNo, ref uint uInput, ref uint uLatch);

        //EZI_PLUSR_API int WINAPI	FAS_ClearLatch(BYTE nPortNo, BYTE iSlaveNo, unsigned long uLatchMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ClearLatch(byte nPortNo, byte iSlaveNo, uint uLatchMask);

        //EZI_PLUSR_API int WINAPI	FAS_GetLatchCount(BYTE nPortNo, BYTE iSlaveNo, unsigned char iInputNo, unsigned long* uCount);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetLatchCount(byte nPortNo, byte iSlaveNo, uint iInputNo, ref uint uCount);

        //EZI_PLUSR_API int WINAPI	FAS_GetLatchCountAll(BYTE nPortNo, BYTE iSlaveNo, unsigned long** ppuAllCount);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetLatchCountAll(byte nPortNo, byte iSlaveNo, uint[] ppuAllCount);

        //EZI_PLUSR_API int WINAPI	FAS_GetLatchCountAll32(BYTE nPortNo, BYTE iSlaveNo, unsigned long** ppuAllCount);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetLatchCountAll32(byte nPortNo, byte iSlaveNo, uint[] ppuAllCount);

        //EZI_PLUSR_API int WINAPI	FAS_ClearLatchCount(BYTE nPortNo, BYTE iSlaveNo, unsigned long uInputMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_ClearLatchCount(byte nPortNo, byte iSlaveNo, uint uInputMask);

        //EZI_PLUSR_API int WINAPI	FAS_GetOutput(BYTE nPortNo, BYTE iSlaveNo, unsigned long* uOutput, unsigned long* uStatus);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetOutput(byte nPortNo, byte iSlaveNo, ref uint uOutput, ref uint uStatus);

        //EZI_PLUSR_API int WINAPI	FAS_SetOutput(BYTE nPortNo, BYTE iSlaveNo, unsigned long uSetMask, unsigned long uClearMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetOutput(byte nPortNo, byte iSlaveNo, uint uSetMask, uint uClearMask);

        //EZI_PLUSR_API int WINAPI	FAS_SetTrigger(BYTE nPortNo, BYTE iSlaveNo, unsigned char uOutputNo, TRIGGER_INFO* pTrigger);
        [DllImport("EziMOTIONPlusRx64.dll")]
        protected static extern int FAS_SetTrigger(byte nPortNo, byte iSlaveNo, byte uOutputNo, byte[] pTrigger);
        public static int FAS_SetTrigger(byte nPortNo, byte iSlaveNo, byte uOutputNo, TRIGGER_INFO trigger)
        {
            int nRtn = FMM_OK;

            nRtn = FAS_SetTrigger(nPortNo, iSlaveNo, uOutputNo, trigger.ByteArray);
            if (nRtn == FMM_OK)
            {
            }

            return nRtn;
        }

        //EZI_PLUSR_API int WINAPI	FAS_SetRunStop(BYTE nPortNo, BYTE iSlaveNo, unsigned long uRunMask, unsigned long uStopMask);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetRunStop(byte nPortNo, byte iSlaveNo, uint uRunMask, uint uStopMask);

        //EZI_PLUSR_API int WINAPI	FAS_GetTriggerCount(BYTE nPortNo, BYTE iSlaveNo, unsigned char uOutputNo, unsigned long* uCount);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetTriggerCount(byte nPortNo, byte iSlaveNo, uint uOutputNo, ref uint uCount);

        //EZI_PLUSR_API int WINAPI	FAS_GetIOLevel(BYTE nPortNo, BYTE iSlaveNo, unsigned long* uIOLevel);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIOLevel(byte nPortNo, byte iSlaveNo, ref uint uIOLevel);

        //EZI_PLUSR_API int WINAPI	FAS_SetIOLevel(BYTE nPortNo, BYTE iSlaveNo, unsigned long uIOLevel);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetIOLevel(byte nPortNo, byte iSlaveNo, uint uIOLevel);

        //EZI_PLUSR_API int WINAPI	FAS_LoadIOLevel(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_LoadIOLevel(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI	FAS_SaveIOLevel(BYTE nPortNo, BYTE iSlaveNo);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SaveIOLevel(byte nPortNo, byte iSlaveNo);

        //EZI_PLUSR_API int WINAPI	FAS_GetInputFilter(BYTE nPortNo, BYTE iSlaveNo, unsigned short* filter);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetInputFilter(byte nPortNo, byte iSlaveNo, ref ushort filter);

        //EZI_PLUSR_API int WINAPI	FAS_SetInputFilter(BYTE nPortNo, BYTE iSlaveNo, unsigned short filter);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetInputFilter(byte nPortNo, byte iSlaveNo, ushort filter);

        //EZI_PLUSR_API int WINAPI	FAS_GetIODirection(BYTE nPortNo, BYTE iSlaveNo, unsigned long* direction);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_GetIODirection(byte nPortNo, byte iSlaveNo, ref uint direction);

        //EZI_PLUSR_API int WINAPI	FAS_SetIODirection(BYTE nPortNo, BYTE iSlaveNo, unsigned long direction);
        [DllImport("EziMOTIONPlusRx64.dll")]
        public static extern int FAS_SetIODirection(byte nPortNo, byte iSlaveNo, uint direction);
    }
}
