/*
 * Copyright (c) 2013 - 2014, Freescale Semiconductor, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * o Redistributions of source code must retain the above copyright notice, this list
 *   of conditions and the following disclaimer.
 *
 * o Redistributions in binary form must reproduce the above copyright notice, this
 *   list of conditions and the following disclaimer in the documentation and/or
 *   other materials provided with the distribution.
 *
 * o Neither the name of Freescale Semiconductor, Inc. nor the names of its
 *   contributors may be used to endorse or promote products derived from this
 *   software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
///////////////////////////////////////////////////////////////////////////////
// Includes
///////////////////////////////////////////////////////////////////////////////

 // Standard C Included Files
#include <stdio.h>
// SDK Included Files
#include "board.h"
#include "fsl_i2c_slave_driver.h"
#include "fsl_debug_console.h"

///////////////////////////////////////////////////////////////////////////////
// Definitions
///////////////////////////////////////////////////////////////////////////////
#define DATA_LENGTH        64
#define RECV_CMD           0    // receive command
#define RECV_DATA          1    // receive data
#define SEND_DATA          2    // send data

typedef struct
{
    uint8_t *pTxBuff;   // pointer points to transmit buffer
    uint32_t txSize;    // Size of transmit buffer
    uint8_t *pRxBuff;   // pointer points to receive buffer
    uint32_t rxSize;    // Size of receive buffer
} call_param_t;

volatile uint8_t i2cState = RECV_CMD;
volatile bool isDataValid = false;


extern int fifo_in_read_index;
extern int fifo_in_write_index ;
extern uint8_t FIFO_IN[FIFO_IN_SIZE];

extern int fifo_out_read_index;
extern int fifo_out_write_index ;
extern uint8_t FIFO_OUT[FIFO_OUT_SIZE];

/*!
 * @brief slave event callback.
 *
 * This callback function is used by the I2C slave IRQ handler to process an event
 *
 * @param instance I2C instance
 * @param i2cEvent The event type
 */
static void i2c_slave_callback(uint8_t instance,i2c_slave_event_t i2cEvent,void *callParam)
{
    //i2c_slave_state_t * slaveState = I2C_DRV_SlaveGetHandler(instance);
    //call_param_t *userData = (call_param_t*)callParam;

    return;

}

///////////////////////////////////////////////////////////////////////////////
// Code
///////////////////////////////////////////////////////////////////////////////
/*!
* @brief The i2c slave
* The function runs i2c slave with interrupt passive mode. Slave receive data from
* master and echo back to master
*/

int GetFifoInSize()
{
	if (fifo_in_read_index == fifo_in_write_index)
		return 0;

	if (fifo_in_write_index > fifo_in_read_index)
		return fifo_in_write_index - fifo_in_read_index;

	return  FIFO_IN_SIZE - fifo_in_read_index + fifo_in_write_index;
}

int GetFifoOutSize()
{
	if (fifo_out_read_index == fifo_out_write_index)
		return 0;

	if (fifo_in_write_index > fifo_out_read_index)
		return fifo_out_write_index - fifo_out_read_index;

	return  FIFO_OUT_SIZE - fifo_out_read_index + fifo_out_write_index;
}


int main(void)
{
    // Buffer store data to transfer
    uint8_t dataBuff[DATA_LENGTH] = {0};
    // State of slave
    i2c_slave_state_t slave;

    // Param for callback function
    call_param_t callParam =
    {
        .pTxBuff = dataBuff,
        .pRxBuff = dataBuff,
        .txSize = 164,
        .rxSize = 164,
    };
    // user configuration
    i2c_slave_user_config_t userConfig =
    {
        .address        = 0x7FU,
        .slaveCallback  = i2c_slave_callback,
        .callbackParam  = &callParam,
        .slaveListening = true,
#if FSL_FEATURE_I2C_HAS_START_STOP_DETECT
        .startStopDetect  = true,
#endif
#if FSL_FEATURE_I2C_HAS_STOP_DETECT
        .stopDetect       = true,
#endif
    };

    // Initialize hardware
    hardware_init();

    PRINTF("==================== I2C SLAVE CALLBACK ===================\r\n\r\n");
    PRINTF("Eli Slave is running ...");

    // Initialize slave
    I2C_DRV_SlaveInit(BOARD_I2C_INSTANCE, &userConfig, &slave);

    // Loop transfer
    int size;

    while(1)
    {
    	if (((size = GetFifoInSize()) > 0) &&
    		(GetFifoOutSize() < FIFO_OUT_SIZE))
    	{
    		for (int i = 0 ; i < size ; i++)
    		{
    			uint8_t x = FIFO_IN[fifo_in_read_index];
    			fifo_in_read_index = (fifo_in_read_index + 1) % FIFO_IN_SIZE;

    			FIFO_OUT[fifo_out_write_index] = x;
    			fifo_out_write_index = (fifo_out_write_index + 1) % FIFO_OUT_SIZE;

    		}
    	}
    }
}
/*******************************************************************************
 * EOF
 ******************************************************************************/
