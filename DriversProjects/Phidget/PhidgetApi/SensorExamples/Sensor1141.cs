using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1141 : BaseSensor
    {


        public override double changeDisplay(int val, out string str)
        {
            
            double tmp = (val / 9.2) + 10.652;
            str = tmp.ToString("0.####") + "kPa";
            tmp = (val / 63.45) + 1.54;
            str += tmp.ToString("0.####") + "psi";
            return tmp;
        }
 

    }
}
