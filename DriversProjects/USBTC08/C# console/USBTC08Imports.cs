/**************************************************************************
 *
 *     ooooooooo.    o8o                       
 *     `888   `Y88.  `''                       
 *      888   .d88' oooo   .ooooo.   .ooooo.   
 *      888ooo88P'  `888  d88' `'Y8 d88' `88b  
 *      888          888  888       888   888  
 *      888          888  888   .o8 888   888  
 *     o888o        o888o `Y8bod8P' `Y8bod8P'  
 *
 *
 *    Copyright Pico Technology Ltd 1995-2015
 *
 * 
 *    For sales and general information visit
 *    https://www.picotech.com   https://www.picoauto.com
 *    
 *    For help and support visit
 *    https://www.picotech.com/tech-support
 * 
 *    If you have what it takes to join us visit
 *    http://pico.jobs/
 *
 * Description:
 *   This file contains all the .NET wrapper calls needed to support
 *   the console example. It also has the enums and structs required
 *   by the (wrapped) function calls.
 *   
 **************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;


public class Win32Interop
{
    [DllImport("crtdll.dll")]
    public static extern int _kbhit();
}


namespace TC08example
{
	unsafe class Imports
	{
		#region constants
		private const string _DRIVER_FILENAME = "usbtc08.dll";


		#endregion

		#region Driver enums


        public enum TempUnit : short 
        {   USBTC08_UNITS_CENTIGRADE, 
            USBTC08_UNITS_FAHRENHEIT,
            USBTC08_UNITS_KELVIN,
            USBTC08_UNITS_RANKINE
        }

		
		#endregion

		#region Driver Imports
		#region Callback delegates
		
		#endregion

		[DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_open_unit")]
		public static extern short TC08OpenUnit();

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_close_unit")]
        public static extern short TC08CloseUnit(short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_run")]
        public static extern short TC08Run(short handle,
                                           int interval
                                           );

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_stop")]
        public static extern short TC08Stop(short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_get_formatted_info")]
        public static extern short TC08GetFormattedInfo(short handle,
                                                        StringBuilder unit_info,
                                                        short string_length
                                                        );

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_set_channel")]
        public static extern short TC08SetChannel(short handle,
                                                  short channel,
                                                  char tc_type
                                                  );

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_get_single")]
        public static extern short TC08GetSingle(short handle,
                                                  float[] temp,
                                                  short *overflow_flags,
                                                  TempUnit units
                                                  );

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_get_temp")]
        public static extern int TC08GetTemp(short handle,
                                                  float[] temp_buffer,
                                                  int[] times_ms_buffer,
                                                  int buffer_length,
                                                  out short overflow_flag,
                                                  short channel,
                                                  TempUnit units,
                                                  short fill_missing
                                                  );

        [DllImport(_DRIVER_FILENAME, EntryPoint = "usb_tc08_get_minimum_interval_ms")]
        public static extern int TC08GetMinIntervalMS(short handle);

		#endregion
	}
}

