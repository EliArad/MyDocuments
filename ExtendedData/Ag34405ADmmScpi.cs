using Intel_unit_Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agilent34405ADmm
{
    public class Ag34405ADmmScpi : Ag34405ADmm
    {
        enum LAST_CONFIG
        {
            AC_VOLTAGE,
            DC_VOLTAGE,
            AC_CURRENT,
            DC_CURRENT,
            RESISTORS,
            NONE
        }

        LAST_CONFIG m_config = LAST_CONFIG.NONE;
        ScpiDemo m_scpi;
        public Ag34405ADmmScpi(string visaName) : base(visaName)
        {

        }

        public override void Close()
        {
            lock (m_lock)
            {
                m_scpi.Close();
            }
        }
        
        public override void Initialize()
        {
            lock (m_lock)
            {
                try
                {
                    m_scpi = new ScpiDemo(m_visaName);
                    
                    //Set up the DMM for immediate trigger
                    m_scpi.SendScpi(":TRIGger:SOURce IMMediate");

                    m_initialize = true;

                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public override double ReadDCVoltage(bool trigger, int avg = 1)
        {
            lock (m_lock)
            {

                try
                {
                    
                    if (m_config != LAST_CONFIG.DC_VOLTAGE)
                    {
                        m_scpi.SendScpi(":CONFigure:VOLTage:DC AUTO,MAX");
                        m_config = LAST_CONFIG.DC_VOLTAGE;
                    }
                    m_scpi.SendScpi("INIT");


                    double data = 0;
                    if (trigger == true)
                    {
                        int ctr;
                        for (ctr = 0; ctr < avg; ctr++)
                        {
                            m_scpi.SendScpi(":INITiate:IMMediate");
                            Thread.Sleep(10);
                            data += Fetch();
                        }
                        return data / ctr;
                    }
                    else 
                    {
                        string res = m_scpi.QueryScpi(":READ?");
                        Console.WriteLine("Raw DC Volts measurement: {0} Volts", res);
                        return double.Parse(res);
                    }                    
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override void AutoRange()
        {
            lock (m_lock)
            {
                try
                {
                  
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override double Fetch()
        {
            lock (m_lock)
            {
                try
                {
                    m_scpi.SendScpi(":INITiate:IMMediate");

                    //Fetch data from memory

                    double data = double.Parse(m_scpi.QueryScpi(":FETCh?"));
                    return data;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override double ReadACCurrent(bool trigger, int avg = 1)
        {
            lock (m_lock)
            {
                try
                {
                    if (m_config != LAST_CONFIG.AC_CURRENT)
                    {
                        m_scpi.SendScpi(":CONFigure:CURRent:AC AUTO,MAX");
                        m_config = LAST_CONFIG.AC_CURRENT;
                    }


                    double data = 0;

                    if (trigger == true)
                    {
                        //Measure  10 times

                        int ctr;
                        for (ctr = 0; ctr < avg; ctr++)
                        {
                            data += Fetch();
                        }
                        return data / ctr;
                    }
                    else
                    {

                        //Configure the meter to 100mA range and fast mode (least resolution)
                        //driver34405.Current.ACCurrent.Configure(100E-3, Agilent34405ResolutionEnum.Agilent34405ResolutionLeast);

                        data = double.Parse(m_scpi.QueryScpi(":READ?"));
                        Console.WriteLine("AC Current measurement: {0} Amps", data);
                        return data;
                    }
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override double ReadDCCurrent(bool trigger, double range, int avg = 1)
        {
            lock (m_lock)
            {
                try
                {
                    double data = 0;

                    //Configure the meter to 100mA range and fast mode (least resolution)
                    //driver34405.Current.DCCurrent.Configure(100E-3, Agilent34405ResolutionEnum.Agilent34405ResolutionLeast);

                    if (m_config != LAST_CONFIG.DC_CURRENT)
                    {
                        m_scpi.SendScpi(":CONFigure:CURRent:DC AUTO, MAX");
                        m_config = LAST_CONFIG.DC_CURRENT;
                    }
                     

                    m_scpi.SendScpi("INIT");

                    if (trigger == true)
                    {
                        //Measure  10 times

                        int ctr;
                        for (ctr = 0; ctr < avg; ctr++)
                        {
                            m_scpi.SendScpi(":INITiate:IMMediate");
                            Thread.Sleep(100);
                            //Init trigger
                            data += Fetch();
                            Console.WriteLine("{0} AC Current", data);
                        }
                        return data / ctr;
                    }
                    else
                    {

                        data = double.Parse(m_scpi.QueryScpi(":READ?"));
                        Console.WriteLine("DC Current measurement: {0} Amps", data);
                        return data;
                    }
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override double ReadResistance()
        {
            lock (m_lock)
            {
                try
                {
                    double data;
                    //Configure the meter to 10k ohm range and fast mode (least resolution)
                    //driver34405.Resistance.Configure(10E+3, Agilent34405ResolutionEnum.Agilent34405ResolutionLeast);

                    if (m_config != LAST_CONFIG.RESISTORS)
                    {
                        m_scpi.SendScpi(":CONFigure:RESistance AUTO,MAX");
                        m_config = LAST_CONFIG.RESISTORS;
                    }

                    

                    data = double.Parse(m_scpi.QueryScpi(":READ?"));
                    Console.WriteLine("2-Wire Resistance measurement: {0} Ohms", data);

                    return data;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public override void SelfTest()
        {
            lock (m_lock)
            {
                try
                {
                    string res = m_scpi.QueryScpi("*TST?");
                    if (res == "+1")
                    {
                        throw (new SystemException("selftest failed"));
                    }
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
    }
}
