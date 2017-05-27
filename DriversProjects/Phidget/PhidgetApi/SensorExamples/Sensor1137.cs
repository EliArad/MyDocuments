using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1137 : BaseSensor
    {

        public double changeDisplay(int val, out string str)
        {
            double tmp = (val / 57.143) - 8.75;
            str = tmp.ToString("0.####") + "kPa";
            tmp = (val / 394.09) - 1.269;
            str += tmp.ToString("0.####") + "psi";
            return tmp;
        }
         
    }
}
