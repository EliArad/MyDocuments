using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_Camera
{
    public class IRCameraSim : IRCamera
    {

        public IRCameraSim(Callback p): base (p)
        {

        }

        public override float[] TempMesure(int channel, out bool errors , int [] errorVector)
        {
            errors = false;
            if (errorVector != null)
            {
                for (int i = 0; i < errorVector.Length; i++)
                {
                    errorVector[i] = 0;
                }
            }
            TempArea[0] = 49;
            TempArea[1] = 49;
            TempArea[2] = 49;
            TempArea[3] = 49;
            TempArea[4] = 49;
            return TempArea;
        }
    }
}
