using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1125T : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = (val * 0.22222) - 61.11;
            str = tmp.ToString("0.####") + "°c";
            return tmp;
        }

    }
}
