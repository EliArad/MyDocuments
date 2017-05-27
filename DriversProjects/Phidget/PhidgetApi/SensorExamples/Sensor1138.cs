using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1138 : BaseSensor
    {


        public override double changeDisplay(int val, out string str)
        {
            double tmp = (val / 18) - 2.222;
            str = tmp.ToString("0.####") + "kPa";
            tmp = (val / 124.14) - 0.322;
            str += tmp.ToString("0.####") + "psi";
            return tmp;
        }

    }
}
