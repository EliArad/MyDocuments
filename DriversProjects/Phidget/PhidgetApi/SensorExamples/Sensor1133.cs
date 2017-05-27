using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1133: BaseSensor
    {
        public int sensorValue;
        public Sensor1133()
        {
          
        }

        public double changeDisplay(int val, out string str)
        {
            double tmp = 16.801 * Math.Log(val) + 9.872;
            str = tmp.ToString("0.###") + " dB";
            return tmp;
        }

    }
}
