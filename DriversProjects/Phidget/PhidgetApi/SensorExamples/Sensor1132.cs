using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1132 : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = val / 45.0;
            str = tmp.ToString("0.####") + "mA";
            return tmp;
        }

    }
}
