/**
 * @file fxpq3115_fifo.c
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief fxpq3115 with fifo API
 *
 */

#include "uart.h"
#include "fxpq3115.h"
#include "fxpq3115_drv.h"


//-----------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------

int FXPQ3115_InitFifo(FXPQ3115_OverSampleRationSelection sampleRate, uint8_t rowMode)
{
   int32_t status;

    /*! Initialize FXPQ3115 sensor driver. */
    status = FXPQ3115_Initialize();
    if (status == 0)
    {
        UART_Print("\r\n Sensor Initialization Failed\r\n");
        return -1;
    }
    UART_Print("\r\n Successfully Initialized Sensor\r\n");

    /*! Configure the FXPQ3115 sensor. */
    status = FXPQ3115_Configure(FXPQ3115_FifoMode, sampleRate, rowMode);

    return status;

}


