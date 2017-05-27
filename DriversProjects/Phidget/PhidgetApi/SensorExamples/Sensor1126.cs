using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
 

namespace Phidget21Api
{
    public partial class Sensor1126 : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = (val / 18) - 27.777;
            str = tmp.ToString("0.####") + "kPa , ";
            tmp = (val * 0.008055) - 4.0277;
            str+= tmp.ToString("0.####") + "psi";
            return tmp;
        }
    }
}
