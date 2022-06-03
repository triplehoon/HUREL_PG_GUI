using System;
using System.Text;

namespace FASTECH
{
	public partial class EziMOTIONPlusRLib
	{
		////------------------------------------------------------------------
		////                 Device Type Defines.
		////------------------------------------------------------------------
		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_ST
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_ST				1
		//#define DEVNAME_EZI_SERVO_PLUS_R_ST				"Ezi-SERVO Plus-R-ST"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_ST = 1;
		public const string DEVNAME_EZI_SERVO_PLUS_R_ST = "Ezi-SERVO Plus-R-ST";

		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_ADC_ST
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_ADC_ST			3
		//#define DEVNAME_EZI_SERVO_PLUS_R_ADC_ST			"Ezi-SERVO Plus-R-ADC"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_ADC_ST = 3;
		public const string DEVNAME_EZI_SERVO_PLUS_R_ADC_ST = "Ezi-SERVO Plus-R-ADC";

		//#ifndef DEVTYPE_EZI_MOTIONLINK
		//#define	DEVTYPE_EZI_MOTIONLINK					10
		//#define DEVNAME_EZI_MOTIONLINK					"Ezi-MotionLink"
		//#endif
		public const byte DEVTYPE_EZI_MOTIONLINK = 10;
		public const string DEVNAME_EZI_MOTIONLINK = "Ezi-MotionLink";

		//#ifndef DEVTYPE_EZI_MOTIONLINK2
		//#define	DEVTYPE_EZI_MOTIONLINK2					11
		//#define DEVNAME_EZI_MOTIONLINK2					"Ezi-MotionLink2"
		//#endif
		public const byte DEVTYPE_EZI_MOTIONLINK2 = 11;
		public const string DEVNAME_EZI_MOTIONLINK2 = "Ezi-MotionLink2";

		//#ifndef DEVTYPE_EZI_STEP_PLUS_R_ST
		//#define	DEVTYPE_EZI_STEP_PLUS_R_ST				20
		//#define DEVNAME_EZI_STEP_PLUS_R_ST				"Ezi-STEP Plus-R-ST"
		//#endif
		public const byte DEVTYPE_EZI_STEP_PLUS_R_ST = 20;
		public const string DEVNAME_EZI_STEP_PLUS_R_ST = "Ezi-STEP Plus-R-ST";

		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_ALL_ST
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_ALL_ST			30
		//#define DEVNAME_EZI_SERVO_PLUS_R_ALL_ST			"Ezi-SERVO Plus-R-ALL"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_ALL_ST = 30;
		public const string DEVNAME_EZI_SERVO_PLUS_R_ALL_ST = "Ezi-SERVO Plus-R-ALL";

		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_ALL_ABS
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_ALL_ABS		35
		//#define DEVNAME_EZI_SERVO_PLUS_R_ALL_ABS		"Ezi-SERVO Plus-R-ALL Abs"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_ALL_ABS = 35;
		public const string DEVNAME_EZI_SERVO_PLUS_R_ALL_ABS = "Ezi-SERVO Plus-R-ALL Abs";

		//#ifndef DEVTYPE_EZI_STEP_PLUS_R_ALL_ST
		//#define	DEVTYPE_EZI_STEP_PLUS_R_ALL_ST			40
		//#define DEVNAME_EZI_STEP_PLUS_R_ALL_ST			"Ezi-STEP Plus-R-ALL"
		//#endif
		public const byte DEVTYPE_EZI_STEP_PLUS_R_ALL_ST = 40;
		public const string DEVNAME_EZI_STEP_PLUS_R_ALL_ST = "Ezi-STEP Plus-R-ALL";

		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_MINI
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_MINI			50
		//#define DEVNAME_EZI_SERVO_PLUS_R_MINI			"Ezi-SERVO Plus-R-Mini"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_MINI = 50;
		public const string DEVNAME_EZI_SERVO_PLUS_R_MINI = "Ezi-SERVO Plus-R-Mini";

		//#ifndef DEVTYPE_EZI_STEP_PLUS_R_MINI
		//#define	DEVTYPE_EZI_STEP_PLUS_R_MINI			60
		//#define DEVNAME_EZI_STEP_PLUS_R_MINI			"Ezi-STEP Plus-R-Mini"
		//#endif
		public const byte DEVTYPE_EZI_STEP_PLUS_R_MINI = 60;
		public const string DEVNAME_EZI_STEP_PLUS_R_MINI = "Ezi-STEP Plus-R-Mini";

		//#ifndef DEVTYPE_EZI_SERVO_PLUS_R_ALL60I
		//#define	DEVTYPE_EZI_SERVO_PLUS_R_ALL60I			31
		//#define DEVNAME_EZI_SERVO_PLUS_R_ALL60I			"Ezi-SERVO Plus-R-ALL 60i"
		//#endif
		public const byte DEVTYPE_EZI_SERVO_PLUS_R_ALL60I = 31;
		public const string DEVNAME_EZI_SERVO_PLUS_R_ALL60I = "Ezi-SERVO Plus-R-ALL 60i";

		//#ifndef DEVTYPE_EZI_SERVO2_PLUS_R_ST
		//#define	DEVTYPE_EZI_SERVO2_PLUS_R_ST			100
		//#define DEVNAME_EZI_SERVO2_PLUS_R_ST			"Ezi-SERVO II Plus-R-ST"
		//#endif
		public const byte DEVTYPE_EZI_SERVO2_PLUS_R_ST = 100;
		public const string DEVNAME_EZI_SERVO2_PLUS_R_ST = "Ezi-SERVO II Plus-R-ST";

		//#ifndef DEVTYPE_S_SERVO_PLUS_R_ST
		//#define	DEVTYPE_S_SERVO_PLUS_R_ST				102
		//#define DEVNAME_S_SERVO_PLUS_R_ST				"S-SERVO Plus-R-ST"
		//#endif
		public const byte DEVTYPE_S_SERVO_PLUS_R_ST = 102;
		public const string DEVNAME_S_SERVO_PLUS_R_ST = "S-SERVO Plus-R-ST";

		////------------------------------------------------------------------
		////                 Device Type Defines. (for V8)
		////------------------------------------------------------------------
		//// Driver Type A group : Ezi-SERVO Plus-R Inc. Family
		//#ifndef DEVTYPEV8_EZI_SERVO_PLUS_R_ST
		//#define	DEVTYPEV8_EZI_SERVO_PLUS_R_ST			1
		//#define DEVNAMEV8_EZI_SERVO_PLUS_R_ST			"Ezi-SERVO Plus-R Inc."
		//#endif

		//// Driver Type B group : only Motion Controller (without Drive) family
		//#ifndef DEVTYPEV8_EZI_MOTIONLINK
		//#define	DEVTYPEV8_EZI_MOTIONLINK				10
		//#define DEVNAMEV8_EZI_MOTIONLINK				"Ezi-MotionLink"
		//#endif

		//// Driver Type C group : Ezi-STEP Plus-R Family
		//#ifndef DEVTYPEV8_EZI_STEP_PLUS_R_ST
		//#define	DEVTYPEV8_EZI_STEP_PLUS_R_ST			20
		//#define DEVNAMEV8_EZI_STEP_PLUS_R_ST			"Ezi-STEP Plus-R"
		//#endif

		//// Driver Type D group : Ezi-SERVO Plus-R Abs. Family
		//#ifndef DEVTYPEV8_EZI_SERVO_PLUS_R_ABS_ST
		//#define	DEVTYPEV8_EZI_SERVO_PLUS_R_ABS_ST		30
		//#define DEVNAMEV8_EZI_SERVO_PLUS_R_ABS_ST		"Ezi-SERVO Plus-R Abs."
		//#endif

		//// Driver Type E group : Ezi-SERVO Plus-R BLDC Family
		//#ifndef DEVTYPEV8_EZI_SERVO_PLUS_R_BLDC_ST
		//#define	DEVTYPEV8_EZI_SERVO_PLUS_R_BLDC_ST		40
		//#define DEVNAMEV8_EZI_SERVO_PLUS_R_BLDC_ST		"Ezi-SERVO Plus-R BLDC F60"
		//#endif

		//// Driver Type F group : only Motion Controller (without Drive) family
		//#ifndef DEVTYPEV8_EZI_MOTIONLINK2
		//#define	DEVTYPEV8_EZI_MOTIONLINK2				110
		//#define DEVNAMEV8_EZI_MOTIONLINK2				"Ezi-MotionLink2"
		//#endif

		////------------------------------------------------------------------
		////                 Device Type Defines. (Ezi-IO)
		////------------------------------------------------------------------

		//#ifndef DEVTYPE_EZI_IO_PLUS_R_IN16
		//#define	DEVTYPE_EZI_IO_PLUS_R_IN16			150
		//#define DEVNAME_EZI_IO_PLUS_R_IN16			"Ezi-IO Plus-R-IN16"
		//#endif
		public const byte DEVTYPE_EZI_IO_PLUS_R_IN16 = 150;
		public const string DEVNAME_EZI_IO_PLUS_R_IN16 = "Ezi-IO Plus-R-IN16";

		//#ifndef DEVTYPE_EZI_IO_PLUS_R_OUT16
		//#define	DEVTYPE_EZI_IO_PLUS_R_OUT16			160
		//#define DEVNAME_EZI_IO_PLUS_R_OUT16			"Ezi-IO Plus-R-OUT16"
		//#endif
		public const byte DEVTYPE_EZI_IO_PLUS_R_OUT16 = 160;
		public const string DEVNAME_EZI_IO_PLUS_R_OUT16 = "Ezi-IO Plus-R-OUT16";

		//#ifndef DEVTYPE_EZI_IO_PLUS_R_I8O8
		//#define	DEVTYPE_EZI_IO_PLUS_R_I8O8			155
		//#define DEVNAME_EZI_IO_PLUS_R_I8O8			"Ezi-IO Plus-R-I8O8"
		//#endif
		public const byte DEVTYPE_EZI_IO_PLUS_R_I8O8 = 155;
		public const string DEVNAME_EZI_IO_PLUS_R_I8O8 = "Ezi-IO Plus-R-I8O8";

		////------------------------------------------------------------------
		////                 Device Type Defines. (Etc)
		////------------------------------------------------------------------
		//#define DEVTYPE_BOOT_ROM						0xFF
		//#define DEVTYPE_BOOT_ROM_2					0xFE

		////------------------------------------------------------------------
		////                 Motion Direction Defines.
		////------------------------------------------------------------------

		//static const int	DIR_INC = 1;
		//static const int	DIR_DEC = 0;
		public const int DIR_INC = 1;
		public const int DIR_DEC = 0;

		////------------------------------------------------------------------
		////                 Axis Status Flag Defines.
		////------------------------------------------------------------------

		//static const int	MAX_AXIS_STATUS		= 32;
		public const byte MAX_AXIS_STATUS = 32;

		////------------------------------------------------------------------
		////           GetAllStatusEx Function
		////------------------------------------------------------------------

		//#define ALLSTATUSEX_ITEM_COUNT		12
		public const byte ALLSTATUSEX_ITEM_COUNT = 12;

		//#define STATUSEX_TYPE_NONE			0
		//#define STATUSEX_TYPE_INPUT			1
		//#define STATUSEX_TYPE_OUTPUT		2
		//#define STATUSEX_TYPE_AXISSTATUS	3
		//#define STATUSEX_TYPE_CMDPOS		4
		//#define STATUSEX_TYPE_ACTPOS		5
		//#define STATUSEX_TYPE_ACTVEL		6
		//#define STATUSEX_TYPE_POSERR		7
		//#define STATUSEX_TYPE_PTNO			8
		//#define STATUSEX_TYPE_ALARMTYPE		9
		//#define STATUSEX_TYPE_TEMPERATURE	10
		//#define STATUSEX_TYPE_CURRENT		11
		//#define STATUSEX_TYPE_LOAD			12
		//#define STATUSEX_TYPE_PEAKLOAD		13
		//#define STATUSEX_TYPE_ENCVEL		14
		//#define STATUSEX_TYPE_INPUT_HIGH	15
		//#define STATUSEX_TYPE_PTNO_RUNNING	16
		//#define STATUSEX_TYPE_ADVALUE0		30	// Ezi-IO AD: Ch0 & Ch1 AD Value
		//#define STATUSEX_TYPE_ADVALUE2		31	// Ezi-IO AD: Ch2 & Ch3 AD Value
		//#define STATUSEX_TYPE_ADVALUE4		32	// Ezi-IO AD: Ch4 & Ch5 AD Value
		//#define STATUSEX_TYPE_ADVALUE6		33	// Ezi-IO AD: Ch6 & Ch7 AD Value

		public const byte STATUSEX_TYPE_NONE = 0;
		public const byte STATUSEX_TYPE_INPUT = 1;
		public const byte STATUSEX_TYPE_OUTPUT = 2;
		public const byte STATUSEX_TYPE_AXISSTATUS = 3;
		public const byte STATUSEX_TYPE_CMDPOS = 4;
		public const byte STATUSEX_TYPE_ACTPOS = 5;
		public const byte STATUSEX_TYPE_ACTVEL = 6;
		public const byte STATUSEX_TYPE_POSERR = 7;
		public const byte STATUSEX_TYPE_PTNO = 8;
		public const byte STATUSEX_TYPE_ALARMTYPE = 9;
		public const byte STATUSEX_TYPE_TEMPERATURE = 10;
		public const byte STATUSEX_TYPE_CURRENT = 11;
		public const byte STATUSEX_TYPE_LOAD = 12;
		public const byte STATUSEX_TYPE_PEAKLOAD = 13;
		public const byte STATUSEX_TYPE_ENCVEL = 14;
		public const byte STATUSEX_TYPE_INPUT_HIGH = 15;
		public const byte STATUSEX_TYPE_PTNO_RUNNING = 16;
		public const byte STATUSEX_TYPE_ADVALUE0 = 30;
		public const byte STATUSEX_TYPE_ADVALUE2 = 31;
		public const byte STATUSEX_TYPE_ADVALUE4 = 32;
		public const byte STATUSEX_TYPE_ADVALUE6 = 33;

		////------------------------------------------------------------------
		////                 Input/Output Assigning Defines.
		////------------------------------------------------------------------

		//static const BYTE	LEVEL_LOW_ACTIVE	= 0;
		//static const BYTE	LEVEL_HIGH_ACTIVE	= 1;
		public const byte LEVEL_LOW_ACTIVE = 0;
		public const byte LEVEL_HIGH_ACTIVE = 1;

		//static const BYTE	IN_LOGIC_NONE	= 0;
		//static const BYTE	OUT_LOGIC_NONE	= 0;
		public const byte IN_LOGIC_NONE = 0;
		public const byte OUT_LOGIC_NONE = 0;

		////------------------------------------------------------------------
		////                 POSITION TABLE Defines.
		////------------------------------------------------------------------

		//static const WORD	MAX_LOOP_COUNT	= 100;
		//static const WORD	MAX_WAIT_TIME	= 60000;
		public const ushort MAX_LOOP_COUNT = 100;
		public const ushort MAX_WAIT_TIME = 60000;

		//static const WORD	PUSH_RATIO_MIN	= 20;
		//static const WORD	PUSH_RATIO_MAX	= 90;
		public const ushort PUSH_RATIO_MIN = 20;
		public const ushort PUSH_RATIO_MAX = 90;

		//static const DWORD	PUSH_SPEED_MIN	= 1;
		//static const DWORD	PUSH_SPEED_MAX	= 100000;
		public const uint PUSH_SPEED_MIN = 1;
		public const uint PUSH_SPEED_MAX = 100000;

		//static const DWORD	PUSH_PULSECOUNT_MIN	= 1;
		//static const DWORD	PUSH_PULSECOUNT_MAX	= 10000;
		public const uint PUSH_PULSECOUNT_MIN = 1;
		public const uint PUSH_PULSECOUNT_MAX = 10000;

		//typedef enum
		//{
		//    CMD_ABS_LOWSPEED = 0,
		//    CMD_ABS_HIGHSPEED,
		//    CMD_ABS_HIGHSPEEDDECEL,
		//    CMD_ABS_NORMALMOTION,
		//    CMD_INC_LOWSPEED,
		//    CMD_INC_HIGHSPEED,
		//    CMD_INC_HIGHSPEEDDECEL,
		//    CMD_INC_NORMALMOTION,
		//    CMD_MOVE_ORIGIN,
		//    CMD_COUNTERCLEAR,
		//    CMD_PUSH_ABSMOTION,
		//    CMD_STOP,

		//    CMD_MAX_COUNT,

		//    CMD_NO_COMMAND = 0xFFFF,
		//} COMMAND_LIST;

		public const ushort CMD_ABS_LOWSPEED = 0;
		public const ushort CMD_ABS_HIGHSPEED = 1;
		public const ushort CMD_ABS_HIGHSPEEDDECEL = 2;
		public const ushort CMD_ABS_NORMALMOTION = 3;
		public const ushort CMD_INC_LOWSPEED = 4;
		public const ushort CMD_INC_HIGHSPEED = 5;
		public const ushort CMD_INC_HIGHSPEEDDECEL = 6;
		public const ushort CMD_INC_NORMALMOTION = 7;
		public const ushort CMD_MOVE_ORIGIN = 8;
		public const ushort CMD_COUNTERCLEAR = 9;
		public const ushort CMD_PUSH_ABSMOTION = 10;
		public const ushort CMD_STOP = 11;

		public const ushort CMD_MAX_COUNT = 12;

		public const ushort CMD_NO_COMMAND = 0xFFFF;

		//#ifndef	DEFINE_ITEM_NODE
		//#define DEFINE_ITEM_NODE

		//typedef union
		//{
		//    WORD	wBuffer[32];		// 64 bytes

		//    struct
		//    {
		//        LONG	lPosition;

		//        DWORD	dwStartSpd;
		//        DWORD	dwMoveSpd;

		//        WORD	wAccelTime;
		//        WORD	wDecelTime;

		//        WORD	wCommand;
		//        WORD	wWaitTime;
		//        WORD	wContinuous;
		//        WORD	wBranch;

		//        WORD	wCond_branch0;
		//        WORD	wCond_branch1;
		//        WORD	wCond_branch2;
		//        WORD	wLoopCount;
		//        WORD	wBranchAfterLoop;
		//        WORD	wPTSet;
		//        WORD	wLoopCountCLR;

		//        WORD	bCheckInpos;		// 0 : Check Inpos, 1 : Don't Check.

		//        LONG	lTriggerPos;
		//        WORD	wTriggerOnTime;

		//        WORD	wPushRatio;
		//        DWORD	dwPushSpeed;
		//        LONG	lPushPosition;
		//        WORD	wPushMode;
		//    };
		//} ITEM_NODE, *LPITEM_NODE;

		public const ushort OFFSET_POSITION = 0;
		public const ushort OFFSET_LOWSPEED = 4;
		public const ushort OFFSET_HIGHSPEED = 8;
		public const ushort OFFSET_ACCELTIME = 12;
		public const ushort OFFSET_DECELTIME = 14;
		public const ushort OFFSET_COMMAND = 16;
		public const ushort OFFSET_WAITTIME = 18;
		public const ushort OFFSET_CONTINUOUS = 20;
		public const ushort OFFSET_JUMPTABLENO = 22;
		public const ushort OFFSET_JUMPPT0 = 24;
		public const ushort OFFSET_JUMPPT1 = 26;
		public const ushort OFFSET_JUMPPT2 = 28;
		public const ushort OFFSET_LOOPCOUNT = 30;
		public const ushort OFFSET_LOOPJUMPTABLENO = 32;
		public const ushort OFFSET_PTSET = 34;
		public const ushort OFFSET_LOOPCOUNTCLEAR = 36;
		public const ushort OFFSET_CHECKINPOSITION = 38;
		public const ushort OFFSET_TRIGGERPOSITION = 40;
		public const ushort OFFSET_TRIGGERONTIME = 44;
		public const ushort OFFSET_PUSHRATIO = 46;
		public const ushort OFFSET_PUSHSPEED = 48;
		public const ushort OFFSET_PUSHPOSITION = 52;
		public const ushort OFFSET_PUSHMODE = 56;

		public const ushort OFFSET_BLANK = 58;

		public class ITEM_NODE
		{
			byte[] buffer;

			public const int BUFF_SIZE = 64;

			public ITEM_NODE()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copyfrom(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(data, 0, buffer, 0, size);
			}

			public void copyto(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(buffer, 0, data, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;

				wCommand = ushort.MaxValue;
				//lPosition = 0;
				//dwStartSpd = 0;
				//dwMoveSpd = 0;
				//wAccelTime = 0;
				//wDecelTime = 0;
				//wWaitTime = 0;
				//wContinuous = 0;
				wBranch = ushort.MaxValue;
				wCond_branch0 = ushort.MaxValue;
				wCond_branch1 = ushort.MaxValue;
				wCond_branch2 = ushort.MaxValue;
				//wLoopCount = 0;
				wBranchAfterLoop = ushort.MaxValue;
				//wPTSet = 0;
				wLoopCountCLR = ushort.MaxValue;
				//bCheckInpos = 0;
				//lTriggerPos = 0;
				//wTriggerOnTime = 0;
				//wPushRatio = 0;
				//dwPushSpeed = 0;
				//lPushPosition = 0;
				//wPushMode = 0;
			}

			public int lPosition
			{
				get { return BitConverter.ToInt32(buffer, OFFSET_POSITION); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_POSITION, 4); }
			}

			public uint dwStartSpd
			{
				get { return BitConverter.ToUInt32(buffer, OFFSET_LOWSPEED); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_LOWSPEED, 4); }
			}

			public uint dwMoveSpd
			{
				get { return BitConverter.ToUInt32(buffer, OFFSET_HIGHSPEED); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_HIGHSPEED, 4); }
			}

			public ushort wAccelTime
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_ACCELTIME); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_ACCELTIME, 2); }
			}

			public ushort wDecelTime
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_DECELTIME); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_DECELTIME, 2); }
			}

			public ushort wCommand
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_COMMAND); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_COMMAND, 2); }
			}

			public ushort wWaitTime
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_WAITTIME); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_WAITTIME, 2); }
			}

			public ushort wContinuous
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_CONTINUOUS); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_CONTINUOUS, 2); }
			}

			public ushort wBranch
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_JUMPTABLENO); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_JUMPTABLENO, 2); }
			}

			public ushort wCond_branch0
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_JUMPPT0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_JUMPPT0, 2); }
			}

			public ushort wCond_branch1
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_JUMPPT1); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_JUMPPT1, 2); }
			}

			public ushort wCond_branch2
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_JUMPPT2); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_JUMPPT2, 2); }
			}

			public ushort wLoopCount
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_LOOPCOUNT); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_LOOPCOUNT, 2); }
			}

			public ushort wBranchAfterLoop
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_LOOPJUMPTABLENO); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_LOOPJUMPTABLENO, 2); }
			}

			public ushort wPTSet
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_PTSET); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_PTSET, 2); }
			}

			public ushort wLoopCountCLR
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_LOOPCOUNTCLEAR); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_LOOPCOUNTCLEAR, 2); }
			}

			public ushort bCheckInpos		// 0 : Check Inpos, 1 : Don't Check.
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_CHECKINPOSITION); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_CHECKINPOSITION, 2); }
			}

			public int lTriggerPos
			{
				get { return BitConverter.ToInt32(buffer, OFFSET_TRIGGERPOSITION); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_TRIGGERPOSITION, 4); }
			}

			public ushort wTriggerOnTime
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_TRIGGERONTIME); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_TRIGGERONTIME, 2); }
			}

			public ushort wPushRatio
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_PUSHRATIO); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_PUSHRATIO, 2); }
			}

			public uint dwPushSpeed
			{
				get { return BitConverter.ToUInt32(buffer, OFFSET_PUSHSPEED); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_PUSHSPEED, 4); }
			}

			public int lPushPosition
			{
				get { return BitConverter.ToInt32(buffer, OFFSET_PUSHPOSITION); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_PUSHPOSITION, 4); }
			}

			public ushort wPushMode
			{
				get { return BitConverter.ToUInt16(buffer, OFFSET_PUSHMODE); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, OFFSET_PUSHMODE, 2); }
			}
		}

		//#endif

		////------------------------------------------------------------------
		////                 EX Commands Option Defines.
		////------------------------------------------------------------------
		//#pragma pack(1)

		//typedef union
		//{
		//    BYTE	byBuffer[32];

		//    struct
		//    {
		//        union
		//        {
		//            DWORD dwOptionFlag;
		//            struct
		//            {
		//                unsigned BIT_IGNOREEXSTOP	: 1;

		//                unsigned BIT_USE_CUSTOMACCEL	: 1;
		//                unsigned BIT_USE_CUSTOMDECEL	: 1;

		//                //unsigned BITS_RESERVED	: 13;
		//            };
		//        } flagOption;

		//        WORD	wCustomAccelTime;
		//        WORD	wCustomDecelTime;

		//        //BYTE	buffReserved[24];
		//    };
		//} MOTION_OPTION_EX;
		public class MOTION_OPTION_EX
		{
			byte[] buffer;

			public const int BUFF_SIZE = 32;

			public MOTION_OPTION_EX()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copyto(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(buffer, 0, data, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;
			}

			public int dwOptionFlag
			{
				get { return BitConverter.ToInt32(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 0, 4); }
			}

			public bool BIT_IGNOREEXSTOP
			{
				get { byte by; by = (byte)(buffer[0] & 0x01); return (by != 0); }
				set { byte by; by = (value) ? (byte)(buffer[0] | 0x01) : (byte)(buffer[0] & 0xFE); buffer[0] = by; }
			}

			public bool BIT_USE_CUSTOMACCEL
			{
				get { byte by; by = (byte)(buffer[0] & 0x02); return (by != 0); }
				set { byte by; by = (value) ? (byte)(buffer[0] | 0x02) : (byte)(buffer[0] & 0xFD); buffer[0] = by; }
			}

			public bool BIT_USE_CUSTOMDECEL
			{
				get { byte by; by = (byte)(buffer[0] & 0x04); return (by != 0); }
				set { byte by; by = (value) ? (byte)(buffer[0] | 0x04) : (byte)(buffer[0] & 0xFB); buffer[0] = by; }
			}

			public ushort wCustomAccelTime
			{
				get { return BitConverter.ToUInt16(buffer, 4); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 4, 2); }
			}

			public ushort wCustomDecelTime
			{
				get { return BitConverter.ToUInt16(buffer, 6); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 6, 2); }
			}
		}


		//typedef union
		//{
		//    BYTE	byBuffer[32];

		//    struct
		//    {
		//        union
		//        {
		//            DWORD dwOptionFlag;
		//            struct
		//            {
		//                unsigned BIT_IGNOREEXSTOP	: 1;
		//                unsigned BIT_USE_CUSTOMACCDEC	: 1;

		//                //unsigned BITS_RESERVED	: 14;
		//            };
		//        } flagOption;

		//        WORD	wCustomAccDecTime;

		//        //BYTE	buffReserved[26];
		//    };
		//} VELOCITY_OPTION_EX;
		public class VELOCITY_OPTION_EX
		{
			byte[] buffer;

			public const int BUFF_SIZE = 32;

			public VELOCITY_OPTION_EX()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copyto(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(buffer, 0, data, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;
			}

			public int dwOptionFlag
			{
				get { return BitConverter.ToInt32(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 0, 4); }
			}

			public bool BIT_IGNOREEXSTOP
			{
				get { byte by; by = (byte)(buffer[0] & 0x01); return (by != 0); }
				set { byte by; by = (value) ? (byte)(buffer[0] | 0x01) : (byte)(buffer[0] & 0xFE); buffer[0] = by; }
			}

			public bool BIT_USE_CUSTOMACCDEC
			{
				get { byte by; by = (byte)(buffer[0] & 0x02); return (by != 0); }
				set { byte by; by = (value) ? (byte)(buffer[0] | 0x02) : (byte)(buffer[0] & 0xFD); buffer[0] = by; }
			}

			public ushort wCustomAccDecTime
			{
				get { return BitConverter.ToUInt16(buffer, 4); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 4, 2); }
			}
		}

		//#pragma pack()

		////------------------------------------------------------------------
		////                 Alarm Type Defines.
		////------------------------------------------------------------------
		//typedef enum
		//{
		//    ALARM_NONE = 0,

		//    /*  1 */ ALARM_OVERCURRENT,
		//    /*  2 */ ALARM_OVERSPEED,
		//    /*  3 */ ALARM_STEPOUT,
		//    /*  4 */ ALARM_OVERLOAD,
		//    /*  5 */ ALARM_OVERTEMPERATURE,
		//    /*  6 */ ALARM_OVERBACKEMF,
		//    /*  7 */ ALARM_MOTORCONNECT,
		//    /*  8 */ ALARM_ENCODERCONNECT,
		//    /*  9 */ ALARM_LOWMOTORPOWER,
		//    /* 10 */ ALARM_INPOSITION,
		//    /* 11 */ ALARM_SYSTEMHALT,
		//    /* 12 */ ALARM_ROMDEVICE,
		//    /* 13 */ ALARM_RESERVED0,
		//    /* 14 */ ALARM_HIGHINPUTVOLTAGE,
		//    /* 15 */ ALARM_POSITIONOVERFLOW,
		//    /* 16 */ ALARM_POSITIONCHANGED,

		//    MAX_ALARM_NUM
		//} ALARM_TYPE;

		public const ushort ALARM_NONE = 0;
		public const ushort ALARM_OVERCURRENT = 1;
		public const ushort ALARM_OVERSPEED = 2;
		public const ushort ALARM_STEPOUT = 3;
		public const ushort ALARM_OVERLOAD = 4;
		public const ushort ALARM_OVERTEMPERATURE = 5;
		public const ushort ALARM_OVERBACKEMF = 6;
		public const ushort ALARM_MOTORCONNECT = 7;
		public const ushort ALARM_ENCODERCONNECT = 8;
		public const ushort ALARM_LOWMOTORPOWER = 9;
		public const ushort ALARM_INPOSITION = 10;
		public const ushort ALARM_SYSTEMHALT = 11;
		public const ushort ALARM_ROMDEVICE = 12;
		//public const ushort ALARM_RESERVED0 = 0;
		public const ushort ALARM_HIGHINPUTVOLTAGE = 14;
		public const ushort ALARM_POSITIONOVERFLOW = 15;
		public const ushort ALARM_POSITIONCHANGED = 16;

		//static LPCTSTR ALARM_DESCRIPTION[MAX_ALARM_NUM] =
		//{
		//    NULL,
		//    /*  1 */ _T("Over Current"),
		//    /*  2 */ _T("Over Speed"),
		//    /*  3 */ _T("Position Tracking"),
		//    /*  4 */ _T("Over Load"),
		//    /*  5 */ _T("Over Temperature"),
		//    /*  6 */ _T("Over Back EMF"),
		//    /*  7 */ _T("No Motor Connect"),
		//    /*  8 */ _T("No Encoder Connect"),
		//    /*  9 */ _T("Low Motor Power"),
		//    /* 10 */ _T("Inposition Error"),
		//    /* 11 */ _T("System Halt"),
		//    /* 12 */ _T("ROM Device Error"),
		//    /* 13 */ NULL,
		//    /* 14 */ _T("High Input Voltage"),
		//    /* 15 */ _T("Position Overflow"),
		//    /* 16 */ _T("Position Changed")
		//};

		public static string[] ALARM_DESCRIPTION =
		{
			"Over Current",
			"Over Speed",
			"Position Tracking",
			"Over Load",
			"Over Temperature",
			"Over Back EMF",
			"No Motor Connect",
			"No Encoder Connect",
			"Low Motor Power",
			"Inposition Error",
			"System Halt",
			"ROM Device Error",
			"",
			"High Input Voltage",
			"Position Overflow",
			"Position Changed",
		};

		//#define MAX_ALARM_LOG		(30)
		public const ushort MAX_ALARM_LOG = 30;

		//typedef struct
		//{
		//    BYTE nAlarmCount;
		//    BYTE nAlarmLog[MAX_ALARM_LOG];
		//} ALARM_LOG;
		public class ALARM_LOG
		{
			byte[] buffer;

			public const int BUFF_SIZE = 31;

			public ALARM_LOG()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copy(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(data, 0, buffer, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;
			}

			public byte nAlarmCount
			{
				get { return buffer[0]; }
				set { buffer[0] = nAlarmCount; }
			}

			public byte nAlarmLog(uint index)
			{
				return (index < MAX_ALARM_LOG) ? buffer[index + 1] : (byte)0;
			}
		}

		////------------------------------------------------------------------
		////                 Drive Information Defines.
		////------------------------------------------------------------------
		//#pragma pack(1)

		//typedef struct _DRIVE_INFO
		//{
		//    unsigned short nVersionNo[4];	// Drive Version Number (Major Ver/Minor Ver/Bug fix/Build No.) (?)
		//    char sVersion[30];				// Drive Version string

		//    unsigned short nDriveType;		// Drive Model
		//    unsigned short nMotorType;		// Motor Model
		//    char sMotorInfo[20];			// Motor Info.(?)

		//    unsigned short nInPinNo;		// Input Pin Number
		//    unsigned short nOutPinNo;		// Output Pin Number

		//    unsigned short nPTNum;			// Position Table Item Number

		//    unsigned short nFirmwareType;	// Firmware Type Information
		//} DRIVE_INFO;

		//#pragma pack()
		public class DRIVE_INFO
		{
			byte[] buffer;

			public const int BUFF_SIZE = 70;

			public DRIVE_INFO()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copy(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(data, 0, buffer, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;
			}

			public ushort nVersionNo0	// Drive Version Number (Major Ver/Minor Ver/Bug fix/Build No.) (?)
			{
				get { return BitConverter.ToUInt16(buffer, 0); }
			}

			public ushort nVersionNo1	// Drive Version Number (Major Ver/Minor Ver/Bug fix/Build No.) (?)
			{
				get { return BitConverter.ToUInt16(buffer, 2); }
			}

			public ushort nVersionNo2	// Drive Version Number (Major Ver/Minor Ver/Bug fix/Build No.) (?)
			{
				get { return BitConverter.ToUInt16(buffer, 4); }
			}

			public ushort nVersionNo3	// Drive Version Number (Major Ver/Minor Ver/Bug fix/Build No.) (?)
			{
				get { return BitConverter.ToUInt16(buffer, 6); }
			}

			public string sVersion		// Drive Version string
			{
				get
				{
					byte[] byVal = new byte[30];
					int index;

					for (index = 0; index < 30; index++)
					{
						if (buffer[8 + index] == 0x00)
							break;
					}

					Buffer.BlockCopy(buffer, 8, byVal, 0, Math.Min(30, index + 1));
					return Encoding.Default.GetString(byVal).Replace("\0", "");
				}
			}

			public ushort nDriveType	// Drive Model
			{
				get { return BitConverter.ToUInt16(buffer, 38); }
			}

			public ushort nMotorType	// Motor Model
			{
				get { return BitConverter.ToUInt16(buffer, 40); }
			}

			public string sMotorInfo	// Motor Info.(?)
			{
				get
				{
					byte[] byVal = new byte[20];
					int index;

					for (index = 0; index < 20; index++)
					{
						if (buffer[42 + index] == 0x00)
							break;
					}

					Buffer.BlockCopy(buffer, 42, byVal, 0, Math.Min(20, index + 1));
					return Encoding.Default.GetString(byVal).Replace("\0", "");
				}
			}

			public ushort nInPinNo		// Input Pin Number
			{
				get { return BitConverter.ToUInt16(buffer, 62); }
			}

			public ushort nOutPinNo		// Output Pin Number
			{
				get { return BitConverter.ToUInt16(buffer, 64); }
			}

			public ushort nPTNum		// Position Table Item Number
			{
				get { return BitConverter.ToUInt16(buffer, 66); }
			}

			public ushort nFirmwareType	// Firmware Type Information
			{
				get { return BitConverter.ToUInt16(buffer, 68); }
			}
		}

		////------------------------------------------------------------------
		////                 I/O Module Defines.
		////------------------------------------------------------------------

		//#pragma pack(2)

		//typedef union
		//{
		//    BYTE	byBuffer[12];

		//    struct
		//    {
		//        unsigned short	wPeriod;
		//        unsigned short	wReserved1;
		//        unsigned short	wOnTime;
		//        unsigned short	wReserved2;
		//        unsigned long	wCount;
		//    };
		//} TRIGGER_INFO;

		//#pragma pack()

		public class TRIGGER_INFO
		{
			byte[] buffer;
			public byte[] ByteArray { get { return buffer; } }

			public const int BUFF_SIZE = 12;

			public TRIGGER_INFO()
			{
				buffer = new byte[BUFF_SIZE];
			}

			public void copy(byte[] data)
			{
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(data, 0, buffer, 0, size);
			}

			public void Clear()
			{
				for (int i = 0; i < BUFF_SIZE; i++)
					buffer[i] = 0;
			}

			public ushort wPeriod
			{
				get { return BitConverter.ToUInt16(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 0, 2); }
			}

			public ushort wReserved1
			{
				get { return BitConverter.ToUInt16(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 2, 2); }
			}

			public ushort wOnTime
			{
				get { return BitConverter.ToUInt16(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 4, 2); }
			}

			public ushort wReserved2
			{
				get { return BitConverter.ToUInt16(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 6, 2); }
			}

			public uint wCount
			{
				get { return BitConverter.ToUInt32(buffer, 0); }
				set { byte[] byVal = BitConverter.GetBytes(value); Buffer.BlockCopy(byVal, 0, buffer, 8, 4); }
			}
		}

		////------------------------------------------------------------------
		////                 Ez-IO Plus-AD Defines
		////------------------------------------------------------------------

		//#define MAX_AD_CHANNEL		16

		//enum AD_RANGE
		//{
		//    ADRANGE_10_to_10 = 0,		//  -10V ~  10V [2.441mV]
		//    ADRANGE_5_to_5,			//   -5V ~   5V [1.22mV]
		//    ADRANGE_2_5_to_2_5,		// -2.5V ~ 2.5V [0.61mV]
		//    ADRANGE_0_to_10,			//    0V ~  10V [1.22mV]
		//    ADRANGE_0_to_20_C,		//   0mA ~ 20mA [2.44mV]

		//    ADRANGE_EX_10_to_10 = 0x10,	// (Read Only, External Switch)  -10V ~  10V [2.441mV]
		//    ADRANGE_EX_5_to_5,			// (Read Only, External Switch)   -5V ~   5V [1.22mV]
		//    ADRANGE_EX_2_5_to_2_5,		// (Read Only, External Switch) -2.5V ~ 2.5V [0.61mV]
		//    ADRANGE_EX_0_to_10,			// (Read Only, External Switch)    0V ~  10V [1.22mV]
		//};
		public enum AD_RANGE
		{
			ADRANGE_10_to_10 = 0,	//  -10V ~  10V [2.441mV]
			ADRANGE_5_to_5,			//   -5V ~   5V [1.22mV]
			ADRANGE_2_5_to_2_5,		// -2.5V ~ 2.5V [0.61mV]
			ADRANGE_0_to_10,		//    0V ~  10V [1.22mV]
			ADRANGE_0_to_20_C,		//   0mA ~ 20mA [2.44mV]

			ADRANGE_EX_10_to_10 = 0x10,	// (Read Only, External Switch)  -10V ~  10V [2.441mV]
			ADRANGE_EX_5_to_5,			// (Read Only, External Switch)   -5V ~   5V [1.22mV]
			ADRANGE_EX_2_5_to_2_5,		// (Read Only, External Switch) -2.5V ~ 2.5V [0.61mV]
			ADRANGE_EX_0_to_10,			// (Read Only, External Switch)    0V ~  10V [1.22mV]
		};

		//#pragma pack(1)

		//typedef union
		//{
		//    BYTE	byBuffer[112];

		//    struct DATA
		//    {
		//        char	range;
		//        short	rawdata;
		//        float	converted;
		//    } channel[16];
		//} AD_RESULT;
		public class AD_RESULT
		{
			char[] range;
			short[] rawdata;
			float[] converted;

			public const int BUFF_SIZE = 112;

			public AD_RESULT(byte[] data)
			{
				range = new char[16];
				rawdata = new short[16];
				converted = new float[16];

				copyfrom(data);
			}

			private void copyfrom(byte[] data)
			{
				byte[] buffer = new byte[BUFF_SIZE];
				int size = (data.GetLength(0) < BUFF_SIZE) ? data.GetLength(0) : BUFF_SIZE;
				Buffer.BlockCopy(data, 0, buffer, 0, size);

				for (int i = 0; i < 16; i++)
				{
					range[i] = (char)(buffer[i * 7]);
					rawdata[i] = (short)(BitConverter.ToInt16(buffer, i * 7 + 1));
					converted[i] = (short)(BitConverter.ToSingle(buffer, i * 7 + 3));
				}
			}

			public char[] Range { get { return range; } }
			public short[] RawData { get { return rawdata; } }
			public float[] Converted { get { return converted; } }
		}

		//enum AD_DATA_TYPE
		//{
		//    TYPE_AD_RANGE = 0,
		//    TYPE_AD_FILTER_LENGTH,
		//    TYPE_AD_FILTER_OFFSET,
		//};
		public enum AD_DATA_TYPE
		{
			TYPE_AD_RANGE = 0,
			TYPE_AD_FILTER_LENGTH,
			TYPE_AD_FILTER_OFFSET,
		};

		//typedef union
		//{
		//    BYTE	byBuffer[16];

		//    short	channel[8];
		//} AD_BUFFER;
		public class AD_BUFFER
		{
			short[] channel;

			public const int BUFF_SIZE = 8;

			public AD_BUFFER(byte[] buffer)
			{
				channel = new short[8];
				if (buffer.Length == 16)
				{
					for (int i = 0; i < 8; i++)
						channel[i] = BitConverter.ToInt16(buffer, i * 2);
				}
			}

			public short[] Channel { get { return channel; } }
		}

		//#pragma pack()

		////------------------------------------------------------------------
		////                 LOG Level Defines
		////------------------------------------------------------------------
		public enum LOG_LEVEL
		{
			LOG_LEVEL_COMM = 0,		// Communication Log only
			LOG_LEVEL_PARAM,		// Communication Log and parameter functions
			LOG_LEVEL_MOTION,		// Communication Log and parameter, motion, I/O functions
			LOG_LEVEL_ALL,			// Communication Log and all functions
		};
	}
}
