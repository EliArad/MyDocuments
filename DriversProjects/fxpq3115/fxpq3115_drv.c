/**
 * @file fxpq3115_drv.c
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief FXP13115 Pressure sensor driver
 *
 */

#include "fxpq3115_drv.h"
#include "uart.h"
#include "i2c.h"

#define FXPQ3115_SLAVE_ADDRESS  (0x60 << 1)



static uint32_t FXPQ3115_IsExist = 0;
static uint32_t FXPQ3115_ConfigureNormal();
static FXPQ3115_CTRL_REG1_REG  m_ctrl_reg1;
static uint8_t  m_rowMode;

#define FXPQ3115_FIFO_WMRK_SIZE (32)    /* Buffer 8 Samples. */
#define FXPQ3115_F_SETUP_F_MODE_STOP_MODE      ((uint8_t) 0x80)  /*  FIFO stops accepting new samples when overflowed.    */



static inline void i2cSlaveWrite(uint8_t slaveaddress, uint8_t regAddress, uint8_t value)
{
	uint8_t data[2] = {regAddress, value};
	I2cTransmit (slaveaddress, data, 2, STOP_I2C);
}

static uint8_t i2cSlaveRead(uint8_t slaveaddress, uint8_t regAddress)
{
	/* Send device address, with no STOP condition */
	I2cTransmit(slaveaddress, &regAddress, 1, NO_STOP_I2C);
	 /* Read data, with STOP condition  */
	uint8_t data;
	slaveaddress |= 0x01; //for read
	I2cRecieve (slaveaddress, &data, 1);

	return data;
}

static inline void i2cSlaveReadEx(uint8_t slaveaddress, uint8_t regAddress , uint8_t *data, uint8_t size)
{
	//slaveaddress <<=1;  //shift the Add by 1 to make the write bit = 0;
	/* Send device address, with no STOP condition */
	I2cTransmit (slaveaddress, &regAddress, 1, NO_STOP_I2C);
    /* Read data, with STOP condition  */
   I2cRecieve (slaveaddress | 1, data, size);

   //PRINTF("data[0] = %d\n", data[0]);
   //PRINTF("data[1] = %d\n", data[1]);
   //PRINTF("data[2] = %d\n", data[2]);

}

uint32_t FXPQ3115_Initialize()
{
    //int32_t status;
   // uint8_t reg;

    /*!  Read and store the device's WHO_AM_I.*/

    if (0xC4 != i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_WHO_AM_I))
    {
   	 FXPQ3115_IsExist = 0;
    }
    else
    {
   	 FXPQ3115_IsExist = 1;
    }

    return FXPQ3115_IsExist;
}

typedef enum  FXPQ3115_ActiveStandby
{

	FXPQ3115_STANDBY = 0,
	FXPQ3115_ACTIVE = 1

} FXPQ3115_ActiveStandby;

static uint32_t FXPQ3115_ConfigureFifo()
{

	/*! @brief Register settings for FIFO (buffered) mode @ default 1 sample per second. */

   /* Set FIFO Mode and set FIFO Watermark Level. */

	 FXPQ3115_F_SETUP_REG  fsetup_reg;
    fsetup_reg.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_F_SETUP);
    fsetup_reg.u.F_WMRK = FXPQ3115_FIFO_WMRK_SIZE;
    fsetup_reg.u.F_MODE = 1; //FIFO contains the most recent samples when overflowed (circular buffer)
    	 	 	 	 	 	 	 	  //Oldest sample is discarded to be replaced by new sample
    i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_F_SETUP, fsetup_reg.Value);

    /* Enable Data Ready and Event flags for Pressure, Temperature or either. */
	 FXPQ3115_PT_DATA_CFG_REG  pt_data_reg;
	 pt_data_reg.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_PT_DATA_CFG);
	 pt_data_reg.u.DREM = 1;
	 pt_data_reg.u.PDEFE = 1;
	 pt_data_reg.u.TDEFE = 1;
	 i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_PT_DATA_CFG, pt_data_reg.Value);

	 /* Set Over Sampling Ratio to 128. */
	 FXPQ3115_CTRL_REG1_REG  ctrl_reg1;
	 ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);
	 ctrl_reg1.u.OS = FXPQ3115_OS_128;
	 i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, ctrl_reg1.Value);

	 return 1;
}

static uint32_t FXPQ3115_ConfigureNormal()
{

	FXPQ3115_PT_DATA_CFG_REG  pt_data_cfg;
	pt_data_cfg.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_PT_DATA_CFG);
	 /* Enable Data Ready and Event flags for Pressure, Temperature or either. */
	pt_data_cfg.u.TDEFE = 1;//1;
	pt_data_cfg.u.PDEFE = 1;//1;
	pt_data_cfg.u.DREM = 1;//1;
	i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_PT_DATA_CFG, pt_data_cfg.Value);

	FXPQ3115_CTRL_REG1_REG  ctrl_reg1;
	ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);
   /* Set Over Sampling Ratio to 128. */
	ctrl_reg1.u.OS = FXPQ3115_OS_128;
	i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, ctrl_reg1.Value);
	ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);

	FXPQ3115_CTRL_REG2_REG  ctrl_reg2;
	ctrl_reg2.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG2);
   /* Set Auto acquisition time step. */
	ctrl_reg2.u.ST = FXPQ3115_SAMPLING_EXPONENT;
	i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG2, ctrl_reg2.Value);

	return 1;

}

uint32_t FXPQ3115_ReadImmidiatePressure(uint32_t *pressureInPascals)
{
	 static uint8_t set = 0;
	 if (set == 0)
	 {
		 FXPQ3115_SetImmdiateMeasurement(1);
		 set = 1;
	 }
    if (FXPQ3115_WaitForAnyDataReady() == 1)
	 {
   	 /*! Read new raw sensor data from the FXPQ3115. */
		 *pressureInPascals = FXPQ3115_ReadPressureInPascal();
		 FXPQ3115_SetImmdiateMeasurement(0);
		 set = 0;
		 return 1;
	 }
    return 0;
}

uint32_t FXPQ3115_ReadPressureInPascal()
{

#if 1
	uint32_t msb = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_P_MSB);
	uint32_t csb = (uint32_t)i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_P_CSB);
	uint32_t lsb = (uint32_t)i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_P_LSB);
	if (m_rowMode == 0)
	{
		return (uint32_t) (((msb << 16) | (csb << 8) | (lsb & 0xC0)) >> 6) + ((uint32_t) (((lsb & 0x30) >> 4)) << 2);
	}
	else
	{
		return (uint32_t) (msb << 16) | (csb << 8) | lsb;
	}

#else

	uint8_t data[3];
	i2cSlaveReadEx(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_P_MSB , data, 3);

	return (uint32_t) (((data[0] << 16) | (data[1] << 8) | (data[2] & 0xC0)) >> 6) + ((uint32_t) (((data[2] & 0x30) >> 4)) << 2);

#endif

}

int32_t FXPQ3115_ReadTemperature(uint32_t *rowTemperature)
{
	uint8_t msb = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_T_MSB);
	uint8_t lsb = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_OUT_T_LSB);

	*rowTemperature = (msb << 4) | (lsb & 0xF);

	*rowTemperature = (float) ((signed char) msb) + (float) (lsb >> 4) * 0.0625;

	//int32_t tempInDegrees = *rowTemperature / FXPQ3115_TEMPERATURE_CONV_FACTOR;

	return *rowTemperature;
}

void FXPQ3115_SetImmdiateMeasurement(uint8_t set)
{
	//OST bit will initiate a measurement immediately.

	m_ctrl_reg1.u.OST = set;
	i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, m_ctrl_reg1.Value);
}

uint8_t FXPQ3115_WaitForAnyDataReady()
{

	FXPQ3115_DR_STATUS_REG  drStatus;
	drStatus.Value = 0;
	drStatus.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_DR_STATUS);
	return drStatus.u.PTDR;

}

uint32_t FXPQ3115_Configure(FXPQ3115_ConfigModes mode,
								    FXPQ3115_OverSampleRationSelection sampleRate,
									 uint32_t rowMode)
{

    if (FXPQ3115_IsExist == 0)
   	 return 0;

    m_rowMode = rowMode;
    /*! Put the device into standby mode so that configuration can be applied.*/
    volatile FXPQ3115_CTRL_REG1_REG  ctrl_reg1;
    ctrl_reg1.Value = 0;
    ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);
    ctrl_reg1.u.RAW = rowMode;
    ctrl_reg1.u.SBYB = FXPQ3115_STANDBY;
    i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, ctrl_reg1.Value);

    if (mode == FXPQ3115_NormalMode)
   	 FXPQ3115_ConfigureNormal(sampleRate);
    else
    if (mode == FXPQ3115_FifoMode)
       FXPQ3115_ConfigureFifo(sampleRate);


    /*! Put the device into active mode and ready for reading data.*/
 	 ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);
    ctrl_reg1.u.SBYB = FXPQ3115_ACTIVE;
    i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, ctrl_reg1.Value);
    m_ctrl_reg1.Value = ctrl_reg1.Value;

    return 1;
}

int32_t FXPQ3115_DeInit()
{

   FXPQ3115_CTRL_REG1_REG  ctrl_reg1;
   ctrl_reg1.Value = i2cSlaveRead(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1);
	ctrl_reg1.u.RST = FXPQ3115_DeviceResetEnabled;
	i2cSlaveWrite(FXPQ3115_SLAVE_ADDRESS, FXPQ3115_CTRL_REG1, ctrl_reg1.Value);

   return 1;
}
