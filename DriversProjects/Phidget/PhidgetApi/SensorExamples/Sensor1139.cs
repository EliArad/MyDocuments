using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1139 : BaseSensor
    {
        
        public double changeDisplay(int val, out string str)
        {
            double tmp = (val / 9) - 4.444;
            str = tmp.ToString("0.####") + "kPa";
            tmp = (val / 62.07) - 0.6444;
            str += tmp.ToString("0.####") + "psi";
            return tmp;
        }

    }
}
