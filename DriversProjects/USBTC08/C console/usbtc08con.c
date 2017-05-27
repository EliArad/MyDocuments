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
 * Descrition
 *   C Example Program using the USB TC-08 driver
 *
 **************************************************************************/

#include "usbtc08.h"
#include <windows.h>
#include <stdio.h>


int main(void)
{
	short handle = 0;     /* The handle to a TC-08 returned by usb_tc08_open_unit() */
	char selection = 0;   /* User selection from teh main menu */
	float temp[9];        /* Buffer to store temperature readings from the TC-08 */
	int channel, reading; /* Loop counters */
	int retVal = 0;       /* Return value from driver calls indication success / error */
	USBTC08_INFO unitInfo;/* Struct to hold unit information */
	
	
	/* Print header information */
	printf ("Pico Technology USB TC-08 Console Example Program\n");
	printf ("-------------------------------------------------\n\n");
	printf ("Looking for USB TC-08 devices on the system\n\n");
	printf ("Progress: ");
	
	
	/* Try to open one USB TC-08 unit, if available 
	 * The simplest way to open the unit like is this:
	 *   handle = usb_tc08_open_unit();
	 * but that will cause your program to wait while the driver downloads
	 * firmware to any connected TC-08 units. If you're making an 
	 * interactive application, it's better to use 
	 * usb_tc08_open_unit_async() which returns immediately and allows you to 
	 * display some sort of progress indication to the user as shown below: 
	 */
	retVal = usb_tc08_open_unit_async();
	
	/* Make sure no errors occurred opening the unit */
	if (!retVal) {
		printf ("\n\nError opening unit. Exiting.\n");
		return -1;
	}
	/* Display a text "progress bar" while waiting for the unit to open */
	while ((retVal = usb_tc08_open_unit_progress(&handle,NULL)) == USBTC08_PROGRESS_PENDING)
	{
		/* Update our "progress bar" */
		printf("|");
		fflush(stdout);
		Sleep(200);
	}
		
	/* Determine whether a unit has been opened */
	if (retVal != USBTC08_PROGRESS_COMPLETE || handle <= 0) {
		printf ("\n\nNo USB TC-08 units could be opened. Exiting.\n");
		return -1;
	} else {
		printf ("\n\nUSB TC-08 opened successfully.\n");
	}
	
	/* Get the unit information */
	unitInfo.size = sizeof(unitInfo);
	usb_tc08_get_unit_info(handle, &unitInfo);
	
	printf("\nUnit information:\n");
	printf("Serial: %s \nCal date: %s \n", unitInfo.szSerial, unitInfo.szCalDate);

	/* Set up all channels */
	retVal = usb_tc08_set_channel(handle, 0,'C');
	for (channel = 1; channel < 9; channel++)
		retVal &= usb_tc08_set_channel(handle, channel,'K');
	
	/* Make sure this was successful */
	if (retVal){
		printf("\nEnabled all channels, selected Type K thermocouple.\n");
	} else {
		printf ("\n\nError setting up channels. Exiting.\n");
		usb_tc08_close_unit(handle);
		return -1;
	}
	
	/* Main menu loop */
	do {
		printf("\nPlease enter one of the following commands\n");
		printf("------------------------------------------\n\n");
		printf("S - Single reading on all channels\n");
		printf("C - Continuous reading on all channels\n");
		printf("X - Close the USB TC08 and exit \n");
		
		while (0 == scanf_s(" %c", &selection))
			; /* Do nothing until a character is entered */ 
		
		switch (selection) {
		
		case 'S':
		case 's': /* Single reading mode */
			printf("Getting single reading...");
			fflush(stdout);
			/* Request the reading */
			usb_tc08_get_single(handle, temp, NULL, USBTC08_UNITS_CENTIGRADE);
			printf(" done!\nCJC      : %3.2f C\n", temp[0]);
			for (channel = 1; channel < 9; channel++)
				printf("Channel %d: %3.2f C\n", channel, temp[channel]);
			break;
			
		
		case 'C':
		case 'c': /* Continuous (Streaming) mode */
			printf("Entering streaming mode. Collecting 10 readings.\n");
			printf("  CJC    Ch1    Ch2    Ch3    Ch4    Ch5    Ch6    Ch7    Ch8\n");
			/* Set the unit running */
			usb_tc08_run(handle, usb_tc08_get_minimum_interval_ms(handle));
			for (reading = 0; reading < 10; reading++){

				for (channel = 0; channel < 9; channel++) {
				
					/* Wait for a reading to be available */
					while (0 == (retVal = usb_tc08_get_temp(handle, temp, NULL, 1, NULL, 
							channel, USBTC08_UNITS_CENTIGRADE, 1))){
						Sleep(100); // Wait for a reading or error
					}
					/* Must check for errors (e.g. device could be unplugged) */
					if (retVal < 0) {
						printf ("\n\nError while streaming.\n");
						usb_tc08_stop(handle);
						return -1;
					}
					printf("%6.2f ", temp[0]);
				}
				printf("\n");
			}
			usb_tc08_stop(handle);
			break;
		}
		
	} while (selection != 'X' && selection != 'x');
	
	/* Close the TC-08 */
	usb_tc08_close_unit(handle);
	
	return 0;
}
