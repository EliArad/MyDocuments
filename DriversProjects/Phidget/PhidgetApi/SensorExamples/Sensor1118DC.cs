using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1118DC : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = (val / 8.0) - 62.5;
            str = tmp.ToString("0.####") + "amps";
            return tmp;
        }
    }
}
