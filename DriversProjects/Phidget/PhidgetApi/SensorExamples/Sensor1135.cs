using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1135 : BaseSensor
    {
        public double changeDisplay(int val, out string str)
        {
            double tmp =val /13.62 - 36.7107;
            str = tmp.ToString("0.###") + "V";
            return tmp;
        }

    }
}
