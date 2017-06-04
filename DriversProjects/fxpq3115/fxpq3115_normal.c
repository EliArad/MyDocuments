/**
 * @file fxpq3115_normal.c
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief fxpq3115 normal mode API
 */

#include "uart.h"
#include "fxpq3115.h"
#include "fxpq3115_drv.h"
#include "fxpq3115_normal.h"




//-----------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------

int FXPQ3115_InitNormal(FXPQ3115_OverSampleRationSelection sampleRate, uint8_t rowMode)
{
   int32_t status;

    /*! Initialize FXPQ3115 sensor driver. */
    status = FXPQ3115_Initialize();
    if (status == 0)
    {
        UART_Print("\r\n Sensor Initialization Failed\r\n");
        return -1;
    }
    UART_Print("\r\n Successfully Initiliazed Sensor\r\n");

    /*! Configure the FXPQ3115 sensor. */
    status = FXPQ3115_Configure(FXPQ3115_NormalMode, sampleRate, rowMode);

    return status;

}

void FXPQ3115_TestMode()
{
	//int16_t tempInDegrees;
	uint32_t pressureInPascals;

	//uint32_t rowTemp;
	//int setOst = 1;

	for (;;) /* Forever loop */
	{
		  /*! Wait for data ready from the FXPQ3115. */
		  //if (setOst == 1)
		  {
		     FXPQ3115_SetImmdiateMeasurement(1);
		     //setOst = 0;
		  }
		  if (FXPQ3115_WaitForAnyDataReady() == 1)
		  {
				 /*! Read new raw sensor data from the FXPQ3115. */

				 pressureInPascals = FXPQ3115_ReadPressureInPascal();
				 /*! Process the sample and convert the raw sensor data. */

				 //tempInDegrees = FXPQ3115_ReadTemperature(&rowTemp);
				 PRINTF("%d Pa\n", pressureInPascals);
				 //PRINTF("\r\n Temperature = %d degC\r\n", tempInDegrees);
				 FXPQ3115_SetImmdiateMeasurement(0);
				 //setOst = 1;
	   }
	}
}
