using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class Sensor1122DC : BaseSensor
    { 

        public double changeDisplay(int val, out string str)
        {
            double tmp = (val / 13.2) - 37.8787;
            str = tmp.ToString("0.#####") + "amps";
            return tmp;
        }

    }
}
