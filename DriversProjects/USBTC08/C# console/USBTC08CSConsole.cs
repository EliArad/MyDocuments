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
 * 
 * Description:
 *   This is a console-mode program that demonstrates how to use the
 *   TC08 driver using .NET
 *   
 **************************************************************************/

using System;
using System.IO;
using System.Threading;

namespace TC08example
{
    class ConsoleExample
    {
        private readonly short _handle;
        public const int USBTC08_MAX_CHANNELS = 8;
        public const char TC_TYPE_K = 'K';
        public const int PICO_OK = 1;
       

    private static void WaitForKey()
    {
        while (!Console.KeyAvailable) Thread.Sleep(100);
        if (Console.KeyAvailable) Console.ReadKey(true); // clear the key
    }

    /****************************************************************************
     * Read the device information
     ****************************************************************************/
    void GetDeviceInfo()
    {
        System.Text.StringBuilder line = new System.Text.StringBuilder(256);

        if (_handle >= 0)
        {
            Imports.TC08GetFormattedInfo(_handle, line, 256);
            Console.WriteLine("{0}", line);
        }
    }

    /****************************************************************************
     * Read temperature information from the unit
     ****************************************************************************/
    unsafe void GetValues()
    {
        short status;
        short chan;
        float[] tempbuffer = new float[9]; 
        short overflow;
        int lines=0;


        Console.Write("\n");
        // label the columns
        for (chan = 1; chan <= USBTC08_MAX_CHANNELS; chan++)
        {
            Console.Write("Chan{0}:    ", chan);
        }
        Console.Write("\n");

        do
        { 
            status = Imports.TC08GetSingle(_handle, tempbuffer, &overflow, Imports.TempUnit.USBTC08_UNITS_CENTIGRADE);
            if (status == PICO_OK)
            {
                for (chan = 1; chan <= USBTC08_MAX_CHANNELS; chan++)
                {
                    Console.Write("{0:0.0000}   ", tempbuffer[chan]);
                }
                Console.Write("\n");
                Thread.Sleep(1000);
            }

            if (++lines > 9)
            {
                Console.WriteLine("Hit any key to stop....\n");
                Console.WriteLine("Cold Junction Temperature: {0:0.0000}   \n", tempbuffer[0]);

                lines = 0;
                for (chan = 1; chan <= USBTC08_MAX_CHANNELS; chan++)
                {
                    Console.Write("Chan{0}:    ", chan);
                }
                Console.Write("\n");
            }
        }while (true);
        char ch = (Console.ReadKey().KeyChar);       // use up keypress
        status = Imports.TC08Stop(_handle);
    }

    /****************************************************************************
    * Read temperature information from the unit using streaming
    ****************************************************************************/
    unsafe void GetStreamingValues()
    {
        short status;
        short chan;
        int interval_ms;
        int buffer_size = 1024;

        float[][] tempbuffer = new float[9][];

        PinnedArray<float>[] pinned = new PinnedArray<float>[buffer_size];

        for (short channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++)
        {
            tempbuffer[channel] = new float[buffer_size];
            pinned[channel] = new PinnedArray<float>(tempbuffer[channel]);
        }


        int[] times_ms_buffer = new int[buffer_size];
        short[] overflow = new short[9];
        int lines = 0;
        int numberOfSamples = 0;

        // Find the time interval
        interval_ms = Imports.TC08GetMinIntervalMS(_handle);


        Console.Write("\n");

        int actual_interval_ms = Imports.TC08Run(_handle, interval_ms);

        do
        {
            Thread.Sleep(1000);

            if (actual_interval_ms > 0)
            {
                // Obtain readings for each channel
                for (chan = 0; chan <= USBTC08_MAX_CHANNELS; chan++)
                {
                    numberOfSamples = Imports.TC08GetTemp(_handle, tempbuffer[chan], times_ms_buffer, buffer_size,
                        out overflow[chan], chan, Imports.TempUnit.USBTC08_UNITS_CENTIGRADE, 0);

                    if (numberOfSamples == 1)
                    {
                        Console.WriteLine("Channel {0}: {1} reading.\n", chan, numberOfSamples);
                    }
                    else
                    {
                        Console.WriteLine("Channel {0}: {1} readings.\n", chan, numberOfSamples);
                    }

                    lines++;
                }

                // Label the columns
                Console.Write("Chan0 is the Cold Junction Temperature\n\n");

                for (chan = 0; chan <= USBTC08_MAX_CHANNELS; chan++)
                {
                    Console.Write("Chan{0}:    ", chan);
                }

                Console.Write("\n");

                // Print readings
                for (int i = 0; i < numberOfSamples; i++)
                {

                    for (int channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++)
                    {
                        Console.Write("{0:0.0000}\t", pinned[channel].Target[i]);
                    }

                    Console.Write("\n");

                }

                Console.Write("\n");
                Thread.Sleep(5000);
            }

            if (++lines > 9)
            {
                Console.WriteLine("Hit any key to stop....\n");
                //Console.WriteLine("Cold Junction Temperature: {0:0.0000}   \n", tempbuffer[0]);

                lines = 0;
            }

        } while (!Console.KeyAvailable);

        char ch = (Console.ReadKey().KeyChar);       // use up keypress
        status = Imports.TC08Stop(_handle);

        // Un-pin the arrays
        foreach (PinnedArray<float> p in pinned)
        {
            if (p != null)
            {
                p.Dispose();
            }

        }
    }

    /****************************************************************************
    *  Set channels 
    ****************************************************************************/
    void SetChannels()
    {
	    short channel;
	    short ok;

	    for (channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++)
	    {
            ok = Imports.TC08SetChannel(_handle, channel, TC_TYPE_K);

	    }
    }

    /****************************************************************************
    *  Run
    ****************************************************************************/
    public void Run()
    {
       
         //// main loop - read key and call routine
        char ch = ' ';
        while (ch != 'X')
        {
            Console.WriteLine("");
            Console.WriteLine("I - Device Info");
            Console.WriteLine("G - Get Temperatures");
            Console.WriteLine("S - Get Temperatures - Streaming");
            Console.WriteLine("X - exit");
            Console.WriteLine("Operation:");

            ch = char.ToUpper(Console.ReadKey().KeyChar);

            Console.WriteLine("\n");
            switch (ch)
            {
                case 'I':
                    GetDeviceInfo();
                    break;

                case 'G':
                    SetChannels();
                    GetValues();
                    break;

                case 'S':
                    SetChannels();
                    GetStreamingValues();
                    break;

                case 'X':
                    /* Handled by outer loop */
                    break;

                default:
                    Console.WriteLine("Invalid operation");
                    break;
            }
        }
    }


    private ConsoleExample(short handle)
    {
        _handle = handle;
    }


    static void Main()
    {
      Console.WriteLine("TC08 driver example program");
      Console.WriteLine("Version 1.0\n");

    

      //open unit and show splash screen
      Console.WriteLine("\n\nOpening the device...");

      short handle = Imports.TC08OpenUnit();
      Console.WriteLine("Handle: {0}", handle);
      if (handle == 0)
      {
        Console.WriteLine("Unable to open device");
        Console.WriteLine("Error code : {0}", handle);
        WaitForKey();
      }
      else
      {
        Console.WriteLine("Device opened successfully\n");

        ConsoleExample consoleExample = new ConsoleExample(handle);
        consoleExample.Run();

        Imports.TC08CloseUnit(handle);
      }
    }
  }
}  
