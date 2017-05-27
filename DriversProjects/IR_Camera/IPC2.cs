using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace IR_Camera
{
    enum TIPCMode { Colors, Temps, ADUs };

    public class IPC2
    {
        #region Entry point
        public IPC2() { }

        public IPC2(UInt16 count)
        {
            try
            {
                //SetLogFile(".\\IPC Sample.log", 1, false);
                SetImagerIPCCount(count);
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
            
        }
        #endregion

        #region Structures
        public enum FlagState : int { FlagOpen, FlagClose, FlagOpening, FlagClosing, Error };

        [StructLayout(LayoutKind.Explicit)]
        public struct FrameMetadata
        {
            [FieldOffset(0)] public UInt16 Size;	// size of this structure
            [FieldOffset(4)] public UInt32 Counter;	// frame counter
            [FieldOffset(8)] public UInt32 CounterHW;	// frame counter hardware
            [FieldOffset(16)] public Int64 Timestamp;	// time stamp in UNITS (10000000 per second)
            [FieldOffset(24)] public Int64 TimestampMedia;
            [FieldOffset(32)] public FlagState FlagState;
            [FieldOffset(36)] public float TempChip;
            [FieldOffset(40)] public float TempFlag;
            [FieldOffset(44)] public float TempBox;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            [FieldOffset(48)] public UInt16[] PIFin;
            //[FieldOffset(48)] public UInt32 PIFin;
            // DI = PIFin[0] & 0x8000
            // AI1 = PIFin[0] & 0x03FF
            // AI2 = PIFin[1] & 0x03FF
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VideoFormat
        {
            public Int32 WidthIR;
            public Int32 HeightIR;
            public Int32 FramerateIR;
            public Int32 WidthVisible;
            public Int32 HeightVisible;
            public Int32 FramerateVisible;
        }

        public enum RotationMode : ushort
        {
            Off,		// off
            CW90,		// clockwise 90 degrees
            ACW90,	    // anti-clockwise 90 degrees
            CW180,	    // clockwise 180 degrees
            CWH,		// clockwise horizontal diagonal
            CWV,		// clockwise vertical diagonal
            ACWH,		// anti-clockwise horizontal diagonal
            ACWV,		// anti-clockwise vertical diagonal
            User		// user defined
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IRArranging
        {
            public RotationMode Rotation;
            public float RotationAngle;
            public Int32 Zoom;
            public Int32 ZoomRectLeft;
            public Int32 ZoomRectTop;
            public Int32 ZoomRectRight;
            public Int32 ZoomRectBttom;
        }

        public enum IPCState : ushort
        {
            InitCompleted = 0x0001,
            ServerStopped = 0x0002,
            ConfigChanged = 0x0004,
            FileCmdReady = 0x0008,
            FrameInit = 0x0010,
            VisFrameInit = 0x0020
        }

        public struct Version
        {
            public UInt16 Major;
            public UInt16 Minor;
            public UInt16 Build;
            public UInt16 Revision;

            public Version(Int64 vers)
            {
                Major = (UInt16)((vers >> 48) & 0xFFFF);
                Minor = (UInt16)((vers >> 32) & 0xFFFF);
                Build = (UInt16)((vers >> 16) & 0xFFFF);
                Revision = (UInt16)(vers & 0xFFFF);
            }
        }

        #endregion

        #region Library calls
//#if _WIN64
        
        //private const string strDLL = "ImagerIPC2x64.dll";
//#else
        private const string strDLL = "ImagerIPC2.dll";
//#endif

        #region Delegates
        public delegate Int32 delOnServerStopped(Int32 reason);
        public delegate Int32 delOnFrameInit(Int32 frameWidth, Int32 frameHeight, Int32 frameDepth);
        public delegate Int32 delOnNewFrame(IntPtr data, Int32 frameCounter);
        public delegate Int32 delOnNewFrameEx(IntPtr data, IntPtr Metadata);
        public delegate Int32 delOnInitCompleted();
        public delegate Int32 delOnConfigChanged(Int32 reserved);
        public delegate Int32 delOnFileCommandReady([MarshalAsAttribute(UnmanagedType.LPWStr)] string Filename);

        public delOnServerStopped OnServerStopped;
        public delOnFrameInit OnFrameInit;
        public delOnNewFrame OnNewFrame;
        public delOnNewFrameEx OnNewFrameEx;
        public delOnInitCompleted OnInitCompleted;
        public delOnConfigChanged OnConfigChanged;
        public delOnFileCommandReady OnFileCommandReady;
        #endregion

        #region General functions:
        [DllImport(strDLL, EntryPoint = "SetImagerIPCCount")]
        public static extern Int32 SetImagerIPCCount(UInt16 count);
        [DllImport(strDLL, EntryPoint = "InitImagerIPC")]
        public static extern Int32 Init(UInt16 index);
        [DllImport(strDLL, EntryPoint = "InitNamedImagerIPC")]
        public static extern Int32 Init(UInt16 index, [MarshalAsAttribute(UnmanagedType.LPWStr)] string InstanceName);
        [DllImport(strDLL, EntryPoint = "RunImagerIPC")]
        public static extern Int32 Run(UInt16 index);
        [DllImport(strDLL, EntryPoint = "ReleaseImagerIPC")]
        public static extern Int32 Release(UInt16 index);
        [DllImport(strDLL, EntryPoint = "AcknowledgeFrame")]
        public static extern Int32 AcknowledgeFrame(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetLogFile")]
        public static extern Int32 SetLogFile([MarshalAsAttribute(UnmanagedType.LPWStr)] string Filename, Int32 LogLevel, bool Append);
		[ DllImport(strDLL, EntryPoint = "Log") ]
        public static extern Int32 Log(UInt16 index, out char logstring, Int32 LogLevel); 
		[ DllImport(strDLL, EntryPoint = "GetFrameConfig") ]
        public static extern Int32 GetFrameConfig(UInt16 index, out Int32 Width, out Int32 Height, out Int32 Depth);
		[ DllImport(strDLL, EntryPoint = "GetFrame") ]
        public static extern Int32 GetFrame(UInt16 index, UInt16 timeout, IntPtr Buf, UInt32 BufSize, out FrameMetadata Metadata);
		[ DllImport(strDLL, EntryPoint = "GetVisibleFrameConfig") ]
        public static extern Int32 GetVisibleFrameConfig(UInt16 index, out Int32 Width, out Int32 Height, out Int32 Depth);
		[ DllImport(strDLL, EntryPoint = "GetVisibleFrame") ]
        public static extern Int32 GetVisibleFrame(UInt16 index, UInt16 timeout, IntPtr Buf, UInt32 BufSize, out FrameMetadata Metadata);
        #endregion

        #region Set callback functions:
        [DllImport(strDLL, EntryPoint = "SetCallback_OnServerStopped")]
        public static extern Int32 SetCallback_OnServerStopped(UInt16 index, delOnServerStopped ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnFrameInit")]
        public static extern Int32 SetCallback_OnFrameInit(UInt16 index, delOnFrameInit ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnNewFrame")]
        public static extern Int32 SetCallback_OnNewFrame(UInt16 index, delOnNewFrame ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnNewFrameEx")]
        public static extern Int32 SetCallback_OnNewFrameEx(UInt16 index, delOnNewFrameEx ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnVisibleFrameInit")]
        public static extern Int32 SetCallback_OnVisibleFrameInit(UInt16 index, delOnFrameInit ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnNewVisibleFrame")]
        public static extern Int32 SetCallback_OnNewVisibleFrame(UInt16 index, delOnNewFrame ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnNewVisibleFrameEx")]
        public static extern Int32 SetCallback_OnNewVisibleFrameEx(UInt16 index, delOnNewFrameEx ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnInitCompleted")]
        public static extern Int32 SetCallback_OnInitCompleted(UInt16 index, delOnInitCompleted ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnConfigChanged")]
        public static extern Int32 SetCallback_OnConfigChanged(UInt16 index, delOnConfigChanged ptr);
        [DllImport(strDLL, EntryPoint = "SetCallback_OnFileCommandReady")]
        public static extern Int32 SetCallback_OnFileCommandReady(UInt16 index, delOnFileCommandReady ptr);
        #endregion

        #region Get and Set functions
        [DllImport(strDLL, EntryPoint = "GetVersionApplication")]
        public static extern Int64 GetVersionApplication(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVersionHID_DLL")]
        public static extern Int64 GetVersionHID_DLL(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVersionCD_DLL")]
        public static extern Int64 GetVersionCD_DLL(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVersionIPC_DLL")]
        public static extern Int64 GetVersionIPC_DLL(UInt16 index);

        [DllImport(strDLL, EntryPoint = "GetFixedEmissivity")]
        public static extern float GetFixedEmissivity(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetFixedEmissivity")]
        public static extern float SetFixedEmissivity(UInt16 index, float Value);
        [DllImport(strDLL, EntryPoint = "GetFixedTransmissivity")]
        public static extern float GetFixedTransmissivity(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetFixedTransmissivity")]
        public static extern float SetFixedTransmissivity(UInt16 index, float Value);
        [DllImport(strDLL, EntryPoint = "GetFixedTempAmbient")]
        public static extern float GetFixedTempAmbient(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetFixedTempAmbient")]
        public static extern float SetFixedTempAmbient(UInt16 index, float Value);
        [DllImport(strDLL, EntryPoint = "SetPifOut")]
        public static extern float SetPifOut(UInt16 index, UInt16 channel, float Value);

        [DllImport(strDLL, EntryPoint = "GetTempTec")]
        public static extern float GetTempTec(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetTempTec")]
        public static extern float SetTempTec(UInt16 index, float Value);
        [DllImport(strDLL, EntryPoint = "GetFlag")]
        public static extern bool GetFlag(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetFlag")]
        public static extern bool SetFlag(UInt16 index, bool Value);
        [DllImport(strDLL, EntryPoint = "GetOpticsIndex")]
        public static extern UInt16 GetOpticsIndex(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetOpticsIndex")]
        public static extern UInt16 SetOpticsIndex(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetTempRangeIndex")]
        public static extern UInt16 GetTempRangeIndex(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetTempRangeIndex")]
        public static extern UInt16 SetTempRangeIndex(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetVideoFormatIndex")]
        public static extern UInt16 GetVideoFormatIndex(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetVideoFormatIndex")]
        public static extern UInt16 SetVideoFormatIndex(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetMainWindowEmbedded")]
        public static extern bool GetMainWindowEmbedded(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetMainWindowEmbedded")]
        public static extern bool SetMainWindowEmbedded(UInt16 index, bool Value);
        [DllImport(strDLL, EntryPoint = "GetMainWindowLocX")]
        public static extern UInt16 GetMainWindowLocX(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetMainWindowLocX")]
        public static extern UInt16 SetMainWindowLocX(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetMainWindowLocY")]
        public static extern UInt16 GetMainWindowLocY(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetMainWindowLocY")]
        public static extern UInt16 SetMainWindowLocY(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetMainWindowWidth")]
        public static extern UInt16 GetMainWindowWidth(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetMainWindowWidth")]
        public static extern UInt16 SetMainWindowWidth(UInt16 index, UInt16 Value);
        [DllImport(strDLL, EntryPoint = "GetMainWindowHeight")]
        public static extern UInt16 GetMainWindowHeight(UInt16 index);
        [DllImport(strDLL, EntryPoint = "SetMainWindowHeight")]
        public static extern UInt16 SetMainWindowHeight(UInt16 index, UInt16 Value);

        [DllImport(strDLL, EntryPoint = "GetHardware_Model")]
        public static extern byte GetHardware_Model(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetHardware_Spec")]
        public static extern byte GetHardware_Spec(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetSerialNumber")]
        public static extern UInt32 GetSerialNumber(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetSerialNumberULIS")]
        public static extern UInt32 GetSerialNumberULIS(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetFirmware_MSP")]
        public static extern UInt16 GetFirmware_MSP(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetFirmware_Cypress")]
        public static extern UInt16 GetFirmware_Cypress(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetPID")]
        public static extern UInt16 GetPID(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVID")]
        public static extern UInt16 GetVID(UInt16 index);

        [DllImport(strDLL, EntryPoint = "GetTempChip")]
        public static extern float GetTempChip(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempFlag")]
        public static extern float GetTempFlag(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempProc")]
        public static extern float GetTempProc(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempBox")]
        public static extern float GetTempBox(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempHousing")]
        public static extern float GetTempHousing(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempTarget")]
        public static extern float GetTempTarget(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetHumidity")]
        public static extern float GetHousing(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempMinRange")]
        public static extern float GetTempMinRange(UInt16 index, UInt32 Index);
        [DllImport(strDLL, EntryPoint = "GetTempMaxRange")]
        public static extern float GetTempMaxRange(UInt16 index, UInt32 Index);
        [DllImport(strDLL, EntryPoint = "GetTempRangeDecimal")]
        public static extern UInt16 GetTempRangeDecimal(UInt16 index, bool EffectiveValue);
        [DllImport(strDLL, EntryPoint = "GetTempRangeCount")]
        public static extern UInt16 GetTempRangeCount(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetOpticsFOV")]
        public static extern UInt16 GetOpticsFOV(UInt16 index, UInt32 Index);
        [DllImport(strDLL, EntryPoint = "GetOpticsCount")]
        public static extern UInt16 GetOpticsCount(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetMeasureAreaCount")]
        public static extern UInt16 GetMeasureAreaCount(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVideoFormatCount")]
        public static extern UInt16 GetVideoFormatCount(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetTempMeasureArea")]
        public static extern float GetTempMeasureArea(UInt16 index, UInt32 Index);
        [DllImport(strDLL, EntryPoint = "GetInitCounter")]
        public static extern UInt16 GetInitCounter(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetIPCState")]
        public static extern IPCState GetIPCState(UInt16 index, bool reset);
        [DllImport(strDLL, EntryPoint = "GetIPCMode")]
        public static extern UInt16 GetIPCMode(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetFrameQueue")]
        public static extern UInt16 GetFrameQueue(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVisibleFrameQueue")]
        public static extern UInt16 GetVisibleFrameQueue(UInt16 index);
        [DllImport(strDLL, EntryPoint = "GetVideoFormat")]
        public static extern Int32 GetVideoFormat(UInt16 index, Int32 IndexFormat, out VideoFormat videoFormat);
        [DllImport(strDLL, EntryPoint = "GetIRArranging")]
        public static extern Int32 GetIRArranging(UInt16 index, out IRArranging irArranging);
        [DllImport(strDLL, EntryPoint = "SetIRArranging")]
        public static extern Int32 SetIRArranging(UInt16 index, ref IRArranging irArranging);
        [DllImport(strDLL, EntryPoint = "GetPathOfStoredFile", CharSet = CharSet.Unicode)]
        private static extern Int32 GetPathOfStoredFile(UInt16 index, StringBuilder path, Int32 maxlen);
        public static string GetPathOfStoredFile(UInt16 index) {
            StringBuilder sb = new StringBuilder(1024);
            return (GetPathOfStoredFile(index, sb, sb.Capacity + 1) == 0) ? sb.ToString() : null;
        }
        #endregion

        #region Control commands
        [DllImport(strDLL, EntryPoint = "CloseApplication")]
        public static extern void CloseApplication(UInt16 index);
        [DllImport(strDLL, EntryPoint = "ReinitDevice")]
        public static extern void ReinitDevice(UInt16 index);
        [DllImport(strDLL, EntryPoint = "RenewFlag")]
        public static extern bool RenewFlag(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FileSnapshot")]
        public static extern bool FileSnapshot(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FileRecord")]
        public static extern bool FileRecord(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FileStop")]
        public static extern bool FileStop(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FilePlay")]
        public static extern bool FilePlay(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FilePause")]
        public static extern bool FilePause(UInt16 index);
        [DllImport(strDLL, EntryPoint = "FileOpen")]
        public static extern UInt16 FileOpen(UInt16 index, [MarshalAsAttribute(UnmanagedType.LPWStr)] string Filename);
        [DllImport(strDLL, EntryPoint = "LoadLayout")]
        public static extern Int16 LoadLayout(UInt16 index, [MarshalAsAttribute(UnmanagedType.LPWStr)] string Filename);
        #endregion

        #endregion
    }
}
