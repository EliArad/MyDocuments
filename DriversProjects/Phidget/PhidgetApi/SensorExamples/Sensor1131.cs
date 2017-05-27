using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1131 : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = 15.311 * Math.Exp(0.005199 * val);
            str = tmp.ToString("0.###") + "g";
            return tmp;
        }

    }
}
