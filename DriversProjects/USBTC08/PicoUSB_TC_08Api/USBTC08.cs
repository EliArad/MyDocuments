using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PicoUSB_TC_08Api
{
    public class USBTC08 : IDisposable
    {
        short m_handle = -1;
        public const int USBTC08_MAX_CHANNELS = 8;
        public const char TC_TYPE_K = 'K';
        public const int PICO_OK = 1;
        float[] tempbuffer = new float[9]; 

        public void Open()
        {
            if (m_handle != -1)
                return;
            m_handle = Imports.TC08OpenUnit();
            if (m_handle == 0)
            {
                throw (new SystemException("Unable to open pico tc 08 device"));
            }
        }

        public bool SetChannels()
        {

            short channel;
            short ok = 0;

            for (channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++)
            {
                ok = Imports.TC08SetChannel(m_handle, channel, TC_TYPE_K);
                if (ok != 1)
                    return false;
            }

            return ok == 1? true : false;
        }
        public bool SetChannels(short channel)
        {
            return Imports.TC08SetChannel(m_handle, (short)(channel - 1), TC_TYPE_K) == 1 ? true : false;
        }

        public unsafe float[] TC08GetSingle(out bool ok)
        {
            short status;            
            short overflow;
            ok = false;
            status = Imports.TC08GetSingle(m_handle, 
                                           tempbuffer, 
                                           &overflow, 
                                           Imports.TempUnit.USBTC08_UNITS_CENTIGRADE);
            if (status == PICO_OK)
            {
                ok = true;
            }
            return tempbuffer;
        }
        public short StopTC08()
        {
            return Imports.TC08Stop(m_handle);
        }

        public string GetDeviceInfo()
        {

            System.Text.StringBuilder line = new System.Text.StringBuilder(256);

            if (m_handle >= 0)
            {
                Imports.TC08GetFormattedInfo(m_handle, line, 256);
                return line.ToString();
            }
            else
            {
                return "Device not opened";
            }
        }

        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~USBTC08()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Imports.TC08CloseUnit(m_handle);
                m_handle = -1;
            }
        }
    }
}
