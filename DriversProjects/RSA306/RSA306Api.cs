using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;


namespace RSA306Lib
{
    public class RSA306Api
    {



        [DllImport(@"RSA306Api.dll")]
        public static extern int Configure();


        [DllImport("RSA306Api.dll")]
        public static extern double PeakSearch();

        [DllImport("RSA306Api.dll")]
        public static extern double PeakValue();
         
       


        public int Connect()
        {
            return Configure();
        }

        public double Peak_Search()
        {
            return PeakSearch();
        }

        public double Peak_Value()
        {
            return PeakValue();
        }
    }
}
