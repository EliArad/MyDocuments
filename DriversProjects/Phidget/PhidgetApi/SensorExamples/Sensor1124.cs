using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1124 : BaseSensor
    {

        public double changeDisplay(int val, out string str)
        {
            double tmp = (val * 0.2222) - 61.111;
            str = tmp.ToString("0.####") + "°c";
            return tmp;
        }
    }
}
