using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace IR_Camera
{
    public class IRCamera
    {
        bool Colors = true;
        public static readonly Int32 S_OK = 0;
        public static readonly Int32 S_FALSE = 1;

        protected IPC2 ipc = new IPC2(1);
        protected Process cmd;
        protected string Connection;
        protected float[] TempArea = new float[5];
        protected string OverTempCond;
        string m_imageExeFile = string.Empty;
        protected bool ipcInitialized = false, frameInitialized = false, Connected = false;

        public delegate Int32 _NewFrame(IntPtr data, IPC2.FrameMetadata Metadata);

        public delegate Int32 _OnFrameInit(Int32 frameWidth, Int32 frameHeight, Int32 frameDepth);

        _OnFrameInit pOnFrameInit;

        _NewFrame pNewFrame;

        public delegate void Callback(int code, string msg);

        Callback pCallback;
        public IRCamera(Callback p)
        {
            pCallback = p;
        }
        public Int16 LoadLayout(UInt16 index, string layoutFileName)
        {
            try
            {
                Int16 l = IPC2.LoadLayout(index, layoutFileName);
                return l;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        public void SetNewFrameCallbacks(_NewFrame nf , _OnFrameInit  frameInit)
        {
            pNewFrame = nf;
            pOnFrameInit = frameInit;
        }

        public int ErrorTempArea(int MaxTemp, int MinTemp, uint Area)
        {
            try
            {
                if (IPC2.GetTempMeasureArea(0, Area) > MaxTemp || IPC2.GetTempMeasureArea(0, Area) < MinTemp)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        
        public void OpenIRimeger(bool hide , string imageExeFile = @"C:\Program Files (x86)\Optris GmbH\PI Connect\Imager.exe")
        {
            try
            {
                if (File.Exists(imageExeFile) == false)
                {
                    throw (new SystemException("Imager.exe does not found in path\nPlease make sure the Imager PI Connect software for 64 bit is installed\n"));
                }
                m_imageExeFile = imageExeFile;
                 
                ProcessStartInfo startInfo = new ProcessStartInfo();
                
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                startInfo.FileName = imageExeFile;
                if (hide == true)
                    startInfo.Arguments = "/Invisible";
                else
                    startInfo.Arguments = "";
                Process p;
                if ((p = Process.Start(startInfo)) == null)
                {
                    throw (new SystemException("Error start Imager.exe PI Connect software"));
                }
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Thread.Sleep(2000);

                try
                {
                    if (!ipcInitialized || !Connected)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            if (InitIPC() == false)
                            {
                                pCallback(0, "Waiting for InitIPC up to 40 seconds");
                                if (ipcInitialized == true)
                                    break;
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (ipcInitialized == false)
                    {
                        throw (new SystemException("IPC Camera did was not able to connected and initialized"));
                    }
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }

            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        public void CloseIRineger()
        {
            try
            {

                if (m_imageExeFile == string.Empty)
                    return;

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = m_imageExeFile;
                startInfo.Arguments = "/Close";
                if (Process.Start(startInfo) == null)
                {
                    throw (new SystemException("Error start Imager.exe PI Connect software"));
                }
                ipcInitialized = false;
                Connected = false;

            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        public virtual float[] TempMesure(int channel, out bool errors , int [] errorVector)
        {
            lock (this)
            {
                try
                {
                    errors = false;
                    uint b = (uint)(channel * 5);
                    ushort b1 = (ushort)(channel * 5);
                    int count = 4;
                    int ErrorArea0 = 0;
                    int ErrorArea1 = 0;
                    int ErrorArea2 = 0;
                    int ErrorArea3 = 0;
                    int ErrorArea4 = 0;

                    while (count > 0)
                    {
                        errors = false;
                        TempArea[0] = IPC2.GetTempMeasureArea(0, b + 0);
                        ErrorArea0 = ErrorTempArea(50, 20, 0);
                        TempArea[1] = IPC2.GetTempMeasureArea(0, b + 1);
                        ErrorArea1 = ErrorTempArea(50, 20, 1);
                        TempArea[2] = IPC2.GetTempMeasureArea(0, b + 2);
                        ErrorArea2 = ErrorTempArea(50, 20, 2);
                        TempArea[3] = IPC2.GetTempMeasureArea(0, b + 3);
                        ErrorArea3 = ErrorTempArea(50, 20, 3);
                        TempArea[4] = IPC2.GetTempMeasureArea(0, b + 4);
                        ErrorArea4 = ErrorTempArea(50, 20, b + 4);
                        if (TempArea[0] < 105)
                            break;
                        Thread.Sleep(10);
                        count--;                        
                    }

                   
                    int[] pError = { ErrorArea0, ErrorArea1, ErrorArea2, ErrorArea3, ErrorArea4 };
                    errorVector = pError;
                    for (int i = 0; i < pError.Length; i++)
                    {
                        if (pError[i] > 0)
                        {
                            errors = true;
                        }
                    }
                    return TempArea;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        
        public bool InitIPC()
        {
            try
            {
                Int64 hr;
                if ((ipc != null) && !ipcInitialized)
                {
                    hr = IPC2.Init(0, "");
                    if (hr != 0)
                    {
                        ipcInitialized = false;
                        return false;
                    }
                    else
                    {
                        ipc.OnInitCompleted = new IPC2.delOnInitCompleted(OnInitCompleted);


                        ipc.OnServerStopped = new IPC2.delOnServerStopped(OnServerStopped);
                        IPC2.SetCallback_OnServerStopped(0, ipc.OnServerStopped);

                        ipc.OnFrameInit = new IPC2.delOnFrameInit(OnFrameInit);
                        Int32 u = IPC2.SetCallback_OnFrameInit(0, ipc.OnFrameInit);

                        ipc.OnNewFrameEx = new IPC2.delOnNewFrameEx(OnNewFrameEx);
                        IPC2.SetCallback_OnNewFrameEx(0, ipc.OnNewFrameEx);
                         

                        IPC2.SetCallback_OnInitCompleted(0, ipc.OnInitCompleted);
                        hr = IPC2.Run(0);
                        ipcInitialized = (hr == 0);
                        return true;
                    }                  
                }
                return ipcInitialized;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        Int32 OnServerStopped(Int32 reason)
        {
            ReleaseIPC();

            return 0;
        }
        public void EnableCalbacks(bool onoff)
        {
            if (onoff)
            {

                ipc.OnFrameInit = new IPC2.delOnFrameInit(OnFrameInit);
                Int32 u = IPC2.SetCallback_OnFrameInit(0, ipc.OnFrameInit);

                ipc.OnNewFrameEx = new IPC2.delOnNewFrameEx(OnNewFrameEx);
                IPC2.SetCallback_OnNewFrameEx(0, ipc.OnNewFrameEx);
            }
            else
            {
                Int32 u = IPC2.SetCallback_OnFrameInit(0, null);
                IPC2.SetCallback_OnNewFrameEx(0, null);
            }
        }
        Int32 OnFrameInit(Int32 frameWidth, Int32 frameHeight, Int32 frameDepth)
        {
            frameInitialized = true;
            pOnFrameInit(frameWidth, frameHeight, frameDepth);
            return 0;
        }
        private void ReleaseIPC()
        {
            Connected = false;
            if ((ipc != null) && ipcInitialized)
            {
                IPC2.Release(0);
                ipcInitialized = false;
            }
        }
        // will work with Imager.exe release > 2.0 only:
        Int32 OnNewFrameEx(IntPtr data, IntPtr Metadata)
        {
            if (!frameInitialized)
                return S_FALSE;
            return NewFrame(data, (IPC2.FrameMetadata)Marshal.PtrToStructure(Metadata, typeof(IPC2.FrameMetadata)));
        }

        Int32 NewFrame(IntPtr data, IPC2.FrameMetadata Metadata)
        {
            pNewFrame(data, Metadata);

            return 0;
        }

        Int32 OnInitCompleted()
        {
            try
            {
                //Colors = ((TIPCMode)IPC2.GetIPCMode(0) == TIPCMode.Colors);
                Connection = "Calibrait";
                //Thread.Sleep(20000);
                Connection = "Connected";
                Connected = true;
                pCallback(1, "init complete");

                return 0;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
    }
}
