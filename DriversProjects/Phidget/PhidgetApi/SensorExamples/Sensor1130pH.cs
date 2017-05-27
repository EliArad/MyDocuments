using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1130pH : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = 0.0178 * val - 1.889;
            str = tmp.ToString("0.###");
            return tmp;
        }
    }
}
