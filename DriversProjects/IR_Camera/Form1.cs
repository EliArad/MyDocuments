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

namespace IR_Camera
{
    public partial class Form1 : Form
    {
      
        bool Painted = false;
        bool m_running = true;
        public IRCamera IR;
        int[] errorsVect = new int[5];
        
        Thread m_thread = null;
        Int32 FrameWidth, FrameHeight, FrameDepth, FrameSize;
        Bitmap bmp;
        bool Colors = true;
        byte[] rgbValues;
        Int16[] Values;

        
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            try
            {
                IR_Camera.IRCamera.Callback p = new IRCamera.Callback(IRApiCallback);
                IR = new IRCamera(p);

                IR_Camera.IRCamera._NewFrame n  = new IRCamera._NewFrame(NewFrame);
                IR_Camera.IRCamera._OnFrameInit o = new IRCamera._OnFrameInit(OnFrameInit);
                IR.SetNewFrameCallbacks(n, o);
                IR.OpenIRimeger(false);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }            
        }

        Int32 OnFrameInit(Int32 frameWidth, Int32 frameHeight, Int32 frameDepth)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            FrameSize = FrameWidth * FrameHeight;
            FrameDepth = frameDepth;
          
            bmp = new Bitmap(FrameWidth, FrameHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            Int32 stride = bmpData.Stride;
            bmp.UnlockBits(bmpData);
            rgbValues = new Byte[stride * FrameHeight];
            Values = new Int16[FrameSize];
            pictureBox.Size = new System.Drawing.Size(FrameWidth, FrameHeight);
            UpdateSize();
            return 0;
        }

        void UpdateSize()
        {
            Size = new System.Drawing.Size(pictureBox.Right + 20, Math.Max(buttonFlagRenew.Bottom, pictureBox.Bottom) + 50);
        }
        Int32 NewFrame(IntPtr data, IPC2.FrameMetadata Metadata)
        {
            labelFrameCounter.Text = "Frame counter HW/SW: " + Metadata.CounterHW.ToString() + "/" + Metadata.Counter.ToString();
            labelPIF.Text =
                "PIF   DI:" + ((Metadata.PIFin[0] >> 15) == 0).ToString() +
                "     AI1:" + (Metadata.PIFin[0] & 0x3FF).ToString() +
                "     AI2:" + (Metadata.PIFin[1] & 0x3FF).ToString();

            switch (Metadata.FlagState)
            {
                case IPC2.FlagState.FlagOpen: labelFlag.Text = "open"; labelFlag.ForeColor = Color.Green; labelFlag.BackColor = labelFlag1.BackColor; break;
                case IPC2.FlagState.FlagClose: labelFlag.Text = "closed"; labelFlag.ForeColor = Color.White; labelFlag.BackColor = Color.Red; break;
                case IPC2.FlagState.FlagOpening: labelFlag.Text = "opening"; labelFlag.ForeColor = SystemColors.WindowText; labelFlag.BackColor = Color.Yellow; break;
                case IPC2.FlagState.FlagClosing: labelFlag.Text = "closing"; labelFlag.ForeColor = SystemColors.WindowText; labelFlag.BackColor = Color.Yellow; break;
                default: labelFlag.Text = ""; labelFlag.ForeColor = labelFlag1.ForeColor; labelFlag.BackColor = labelFlag1.BackColor; break;
            }

            for (Int32 x = 0; x < FrameSize; x++)
                Values[x] = Marshal.ReadInt16(data, x * 2);
            if (!Painted)
            {
                GetBitmap(bmp, Values);
                pictureBox.Invalidate();
                Painted = true;
            }

            return 0;
        }
        byte LoByte(Int16 val) { return BitConverter.GetBytes(val)[0]; }
        byte HiByte(Int16 val) { return BitConverter.GetBytes(val)[1]; }
        byte clip(Int32 val) { return (byte)((val <= 255) ? ((val > 0) ? val : 0) : 255); }

        void GetBitmap(Bitmap Bmp, Int16[] values)
        {
            Int32 stride_diff;
            // Lock the bitmap's bits.  
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Bmp.Width, Bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = Bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, Bmp.PixelFormat);
            stride_diff = bmpData.Stride - FrameWidth * 3;

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            if (Colors)
            {
                for (Int32 dst = 0, src = 0, y = 0; y < FrameHeight; y++, dst += stride_diff)
                    for (Int32 x = 0; x < FrameWidth; x++, src++, dst += 3)
                    {
                        Int32 C = (Int32)LoByte(values[src]) - 16;
                        Int32 D = (Int32)HiByte(values[src - (src % 2)]) - 128;
                        Int32 E = (Int32)HiByte(values[src - (src % 2) + 1]) - 128;
                        rgbValues[dst] = clip((298 * C + 516 * D + 128) >> 8);
                        rgbValues[dst + 1] = clip((298 * C - 100 * D - 208 * E + 128) >> 8);
                        rgbValues[dst + 2] = clip((298 * C + 409 * E + 128) >> 8);
                    }
            }
            else
            {
                Int16 mn, mx;
                GetBitmap_Limits(values, out mn, out mx);
                double Fact = 255.0 / (mx - mn);

                for (Int32 dst = 0, src = 0, y = 0; y < FrameHeight; y++, dst += stride_diff)
                    for (Int32 x = 0; x < FrameWidth; x++, src++, dst += 3)
                        rgbValues[dst] = rgbValues[dst + 1] = rgbValues[dst + 2] = (byte)Math.Min(Math.Max((Int32)(Fact * (values[src] - mn)), 0), 255);
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            // Unlock the bits.
            Bmp.UnlockBits(bmpData);
        }

        void GetBitmap_Limits(Int16[] Values, out Int16 min, out Int16 max)
        {
            Int32 y;
            double Sum, Mean, Variance;
            min = Int16.MinValue;
            max = Int16.MaxValue;
            if (Values == null) return;

            Sum = 0;
            for (y = 0; y < FrameSize; y++)
                Sum += Values[y];
            Mean = (double)Sum / FrameSize;
            Sum = 0;
            for (y = 0; y < FrameSize; y++)
                Sum += (Mean - Values[y]) * (Mean - Values[y]);
            Variance = Sum / FrameSize;
            Variance = Math.Sqrt(Variance);
            Variance *= 3;  // 3 Sigma
            min = (Int16)(Mean - Variance);
            max = (Int16)(Mean + Variance);
        }

        void IRApiCallback(int code, string msg)
        {
            if (code == 1)
            {
                m_thread = new Thread(ShowIR);
                m_thread.Start();
                IR.LoadLayout(0, "PA ATE");
            } 
            this.Text = "IR Camera example - " + msg;
        }

        void ShowIR()
        {

            while (m_running)
            {
                bool errors;
                float[] TempArea = IR.TempMesure(0, out errors, errorsVect);

                label6.Text = String.Format("Target1-Temp: {0:##0.0}°C", TempArea[0]);
                label7.Text = String.Format("Target2-Temp: {0:##0.0}°C", TempArea[1]);
                label8.Text = String.Format("Target3-Temp: {0:##0.0}°C", TempArea[2]);
                label9.Text = String.Format("Target4-Temp: {0:##0.0}°C", TempArea[3]);
                label10.Text = String.Format("Target5-Temp: {0:##0.0}°C", TempArea[4]);                           
                Thread.Sleep(200);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_running = false;
                if (m_thread != null)
                    m_thread.Join();
                //IR.CloseIRineger();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bmp, 0, 0);
            Painted = false;
        }
    }
}
 
