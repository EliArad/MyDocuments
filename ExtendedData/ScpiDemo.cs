using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Ivi.Visa.Interop;



namespace Intel_unit_Test
{
    public class ScpiDemo
    {
             ResourceManager ioMgr = new ResourceManager();
             FormattedIO488 instrument = new FormattedIO488();

            //Open the VISA session using a socket port:
            //  VSA on a PC:                        Port=5025 
            //  VSA on an X-Series Signal Analyzer: Port=5024
           
            
            
        
        public ScpiDemo(string openString)
        {
             IVisaSession session = null;
             try
            {                
                session = ioMgr.Open(openString, AccessMode.NO_LOCK, 3000, "");
                instrument.IO = (IMessage)session;
                instrument.IO.SendEndEnabled = false;
                instrument.IO.Timeout = 10000;                       //in milliseconds
                instrument.IO.TerminationCharacterEnabled = true;   //Defaults to false            

                //Determine whether the VSA process is running
                bool isCreated = false;
               instrument.WriteString("*IDN?");
               IdnString = instrument.ReadString();  
            }
            catch (COMException ex)
            {
                Console.WriteLine("Failed to connect to the VSA SCPI interface.");
                Console.WriteLine("   Ensure that the SCPI interface has been started from the");
                Console.WriteLine("      VSA=>Utilities=>SCPI Configuration menu.");
                Console.WriteLine("   Refer to the SCPI Getting Started documentation for instructions on enabling SCPI.");
                Console.WriteLine("Details:");
                Console.WriteLine( ex.Message);

                Console.WriteLine("Press enter to exit demo");
                Console.ReadLine();

                return;
            }
            
        }

        ~ScpiDemo()
        {
            //Close the connection   
            try
            {
                instrument.IO.Close();
                
            }

            catch { }

        }  
       
        public string IdnString { get; set; }
         

        public void SendScpi(string Command)
        {
            instrument.WriteString(Command);
        }

        

        public string QueryScpi (string Command)
        {
            string res = string.Empty;
            instrument.WriteString(Command);
            res  = instrument.ReadString();
            res = res.Replace("\n", "");
            return res;

        }

        public void OutQueryScpi(string Command, out string Res)
        {
            //string res = string.Empty;
            instrument.WriteString(Command);
            Res = instrument.ReadString();
           
        }

        public void Close()
        {
            instrument.FlushRead();
            instrument.IO.Clear();            
            instrument.IO.Close();

        }

    }
}
