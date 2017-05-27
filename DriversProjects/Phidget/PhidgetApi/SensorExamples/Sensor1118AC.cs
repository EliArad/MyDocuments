using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;

namespace Phidget21Api
{
    public partial class Sensor1118AC : BaseSensor
    {
        public override double changeDisplay(int val, out string str)
        {
            double tmp = val * 0.06938;
            str = tmp.ToString("0.#####") + "amps";
            return tmp;
        }
    }
}
