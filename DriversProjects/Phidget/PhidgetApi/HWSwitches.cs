using Phidget21Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectonJig
{
    public class HWSwitches :  Phidget21
    {
        public void Idle()
        {
            SetOutput(1, false);
            SetOutput(2, false);
            SetOutput(3, false);
        }
        public void State1()
        {
            SetOutput(1, false);
            SetOutput(2, false);
            SetOutput(3, true);
        }
        public void State2()
        {
            SetOutput(1, false);
            SetOutput(2, true);
            SetOutput(3, false);
        }

        public void OFF()
        {
            SetOutput(1, true);
            SetOutput(2, false);
            SetOutput(3, false);
        }

        public void EnableFan(bool enable)
        {
            SetOutput(0, enable);
        }
    }
}
