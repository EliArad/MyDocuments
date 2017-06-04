/**
 * @file LSM6DS0.c
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief Gyro and accelremometer API for the LSM6DS0
 *
 */
#include <stdio.h>
#include <stdlib.h>
#include "LSM6DS0.h"
#include "i2c.h"
#include "uart.h"


#define LSM6DS0_SLAVE_ADDRESS  0xD6



#define LSM6DS0_G_ODR_PD                                ((uint8_t)0x00) /*!< Output Data Rate: Power-down*/
#define LSM6DS0_G_ODR_14_9HZ                            ((uint8_t)0x20) /*!< Output Data Rate: 14.9 Hz, cutoff 5Hz */
#define LSM6DS0_G_ODR_59_5HZ                            ((uint8_t)0x40) /*!< Output Data Rate: 59.5 Hz, cutoff 19Hz */
#define LSM6DS0_G_ODR_119HZ                             ((uint8_t)0x60) /*!< Output Data Rate: 119 Hz, cutoff 38Hz*/
#define LSM6DS0_G_ODR_238HZ                             ((uint8_t)0x80) /*!< Output Data Rate: 238 Hz, cutoff 76Hz*/
#define LSM6DS0_G_ODR_476HZ                             ((uint8_t)0xA0) /*!< Output Data Rate: 476 Hz, cutoff 100Hz*/
#define LSM6DS0_G_ODR_952HZ                             ((uint8_t)0xC0) /*!< Output Data Rate: 952 Hz, cutoff 100Hz*/
#define LSM6DS0_G_ODR_MASK                              ((uint8_t)0xE0)
#define LSM6DS0_G_XEN_DISABLE                           ((uint8_t)0x00) /*!< Gyroscope X-axis output enable: disable */
#define LSM6DS0_G_XEN_ENABLE                            ((uint8_t)0x08) /*!< Gyroscope X-axis output enable: enable */
#define LSM6DS0_G_XEN_MASK                              ((uint8_t)0x08)
#define LSM6DS0_G_YEN_DISABLE                           ((uint8_t)0x00) /*!< Gyroscopes Y-axis output enable: disable */
#define LSM6DS0_G_YEN_ENABLE                            ((uint8_t)0x10) /*!< Gyroscopes Y-axis output enable: enable */
#define LSM6DS0_G_YEN_MASK                              ((uint8_t)0x10)
#define LSM6DS0_G_ZEN_DISABLE                           ((uint8_t)0x00) /*!< Gyroscope Z-axis output enable: disable */
#define LSM6DS0_G_ZEN_ENABLE                            ((uint8_t)0x20) /*!< Gyroscope Z-axis output enable: enable */
#define LSM6DS0_G_ZEN_MASK                              ((uint8_t)0x20)
#define LSM6DS0_XL_ODR_PD                               ((uint8_t)0x00) /*!< Output Data Rate: Power-down*/
#define LSM6DS0_XL_ODR_10HZ                             ((uint8_t)0x20) /*!< Output Data Rate: 10 Hz*/
#define LSM6DS0_XL_ODR_50HZ                             ((uint8_t)0x40) /*!< Output Data Rate: 50 Hz */
#define LSM6DS0_XL_ODR_119HZ                            ((uint8_t)0x60) /*!< Output Data Rate: 119 Hz */
#define LSM6DS0_XL_ODR_238HZ                            ((uint8_t)0x80) /*!< Output Data Rate: 238 Hz */
#define LSM6DS0_XL_ODR_476HZ                            ((uint8_t)0xA0) /*!< Output Data Rate: 476 Hz */
#define LSM6DS0_XL_ODR_952HZ                            ((uint8_t)0xC0) /*!< Output Data Rate: 952 Hz */
#define LSM6DS0_XL_ODR_MASK                             ((uint8_t)0xE0)
#define LSM6DS0_XL_FS_2G                                ((uint8_t)0x00) /*!< Full scale: +- 2g */
#define LSM6DS0_XL_FS_4G                                ((uint8_t)0x10) /*!< Full scale: +- 4g */
#define LSM6DS0_XL_FS_8G                                ((uint8_t)0x18) /*!< Full scale: +- 8g */
#define LSM6DS0_XL_FS_MASK                              ((uint8_t)0x18)
#define LSM6DS0_XL_XEN_DISABLE                          ((uint8_t)0x00) /*!< Accelerometer X-axis output enable: disable */
#define LSM6DS0_XL_XEN_ENABLE                           ((uint8_t)0x08) /*!< Accelerometer X-axis output enable: enable */
#define LSM6DS0_XL_XEN_MASK                             ((uint8_t)0x08)
#define LSM6DS0_XL_YEN_DISABLE                          ((uint8_t)0x00) /*!< Accelerometer Y-axis output enable: disable */
#define LSM6DS0_XL_YEN_ENABLE                           ((uint8_t)0x10) /*!< Accelerometer Y-axis output enable: enable */
#define LSM6DS0_XL_YEN_MASK                             ((uint8_t)0x10)
#define LSM6DS0_XL_ZEN_DISABLE                          ((uint8_t)0x00) /*!< Accelerometer Z-axis output enable: disable */
#define LSM6DS0_XL_ZEN_ENABLE                           ((uint8_t)0x20) /*!< Accelerometer Z-axis output enable: enable */
#define LSM6DS0_XL_ZEN_MASK                             ((uint8_t)0x20)


/** @defgroup LSM6DS0_XG Gyroscope Full scale selection CTRL_REG1_G
 * @{
 */
#define LSM6DS0_G_FS_245                               ((uint8_t)0x00) /*!< Full scale: 245 dps*/
#define LSM6DS0_G_FS_500                               ((uint8_t)0x08) /*!< Full scale: 500 dps */
#define LSM6DS0_G_FS_2000                              ((uint8_t)0x18) /*!< Full scale: 2000 dps */
#define LSM6DS0_G_FS_MASK                              ((uint8_t)0x18)




typedef enum _LSM6DS0_Registers
{
	LSM6DS0_ACT_THS = 0x4,
	LSM6DS0_ACT_DUR = 0x5 ,
	LSM6DS0_INT_GEN_CFG_XL = 0x06 ,
	LSM6DS0_INT_GEN_THS_X_XL = 0x07,
	LSM6DS0_INT_GEN_THS_Y_XL = 0x08,
	LSM6DS0_INT_GEN_THS_Z_XL = 0x09,
	LSM6DS0_INT_GEN_DUR_XL = 0x0A,
	LSM6DS0_REFERENCE_G = 0x0B,
	LSM6DS0_INT_CTRL = 0x0C,
	LSM6DS0_WHO_AM_I  = 0x0F,
	LSM6DS0_CTRL_REG1_G=  0x10,
	LSM6DS0_CTRL_REG2_G = 0x11,
	LSM6DS0_CTRL_REG3_G = 0x12,
	LSM6DS0_ORIENT_CFG_G = 0x13,
	LSM6DS0_INT_GEN_SRC_G = 0x14,
	LSM6DS0_OUT_TEMP_L = 0x15 ,
	LSM6DS0_OUT_TEMP_H = 0x16 ,
	LSM6DS0_STATUS_REG = 0x17 ,
	LSM6DS0_OUT_X_L_G = 0x18 ,
	LSM6DS0_OUT_X_H_G = 0x19 ,
	LSM6DS0_OUT_Y_L_G = 0x1A ,
	LSM6DS0_OUT_Y_H_G = 0x1B ,
	LSM6DS0_OUT_Z_L_G = 0x1C ,
	LSM6DS0_OUT_Z_H_G = 0x1D ,
	LSM6DS0_CTRL_REG4 = 0x1E ,
	LSM6DS0_CTRL_REG5_XL = 0x1F,
	LSM6DS0_CTRL_REG6_XL = 0x20,
	LSM6DS0_CTRL_REG7_XL = 0x21 ,
	LSM6DS0_CTRL_REG8 = 0x22 ,
	LSM6DS0_CTRL_REG9 =0x23,
	LSM6DS0_CTRL_REG10 = 0x24,
	LSM6DS0_INT_GEN_SRC_XL  =0x26,
	LSM6DS0_STATUS_REG2 = 0x27 ,
	LSM6DS0_OUT_X_L_XL = 0x28 ,
	LSM6DS0_OUT_X_H_XL = 0x29 ,
	LSM6DS0_OUT_Y_L_XL = 0x2A ,
	LSM6DS0_OUT_Y_H_XL = 0x2B ,
	LSM6DS0_OUT_Z_L_XL = 0x2C ,
	LSM6DS0_OUT_Z_H_XL = 0x2D ,
	LSM6DS0_FIFO_CTRL = 0x2E ,
	LSM6DS0_FIFO_SRC = 0x2F ,
	LSM6DS0_INT_GEN_CFG_G = 0x30,
	LSM6DS0_INT_GEN_THS_XH_G = 0x31,
	LSM6DS0_INT_GEN_THS_XL_G = 0x32,
	LSM6DS0_INT_GEN_THS_YH_G = 0x33,
	LSM6DS0_INT_GEN_THS_YL_G = 0x34,
	LSM6DS0_INT_GEN_THS_ZH_G = 0x35,
	LSM6DS0_INT_GEN_THS_ZL_G = 0x36,
	LSM6DS0_INT_GEN_DUR_G = 0x37

} LSM6DS0_Registers;


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Register ACT_THS - Adderss 0x4   R/W
typedef union
{
    uint8_t Value;

    struct
    {
        unsigned ACT_THS :6;
        unsigned SLEEP_ON_INACT_EN:1;
    }u;

} LSM6DS0_ACT_THS_REG;


// ACT_THS register description
typedef enum  GyroscopeOoperatingModeDuringInactivity
{
	GyroscopeInPowerDown = 0,
	GyroscopeInSleepMode = 1
} GyroscopeOoperatingModeDuringInactivity;


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Linear acceleration sensor interrupt generator configuration register.

typedef union
{
    uint8_t Value;

    struct
    {
		unsigned XLIE_XL:1;
		unsigned XHIE_XL:1;
		unsigned YLIE_XL:1;
		unsigned YHIE_XL:1;
		unsigned ZLIE_XL:1;
		unsigned ZHIE_XL:1;
		unsigned _6D:1;
		unsigned AOI_XL :1;
    }u;

} LSM6DS0_INT_GEN_CFG_XL_REG;


#define  I_AM_LSM6DS0_XG  0x68

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Angular rate sensor control register 1.

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned BW_G:2;
    	unsigned MustBeZero:1;
		unsigned FS_G:2;
		unsigned ODR_G:3;
    }u;

} LSM6DS0_CTRL_REG1_G_REG;

typedef enum GyroscopeFullScaleSelection
{
	GFSS_245_DPS = 0,
	GFSS_500_DPS = 1,
	GFSS_NOT_AVAILABLE = 2,
	GFSS_2000_DPS =3

} GyroscopeFullScaleSelection;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned  HPCF0_G:4;
    	unsigned MustBeZero:2;
		unsigned HP_EN:1;
		unsigned LP_mode:1;
    }u;

} LSM6DS0_CTRL_REG3_G_REG;


// bit HP_EN in CTRL_REG3_G_REG  : default is disabled
typedef enum HighPassFilterEnable
{
	HPFDisabled = 0,
	HPFEnabled = 1

} HighPassFilterEnable;



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//FIFO control register


typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned FTH:5;
		unsigned FMODE:3;
    }u;

} LSM6DS0_FIFO_CTRL_REG;

typedef enum  FIFOModeSelection
{
	FIFO_mode_BypassMode_FIFOTurnedOff = 0,
	FIFO_mode_Stop_collecting_data_when_FIFO_is_full = 1,
	FIFO_mode_Selection_Reserved = 2,
	FIFO_mode_Continuous_mode_until_trigger_is_deasserted_then_FIFO_mode = 3,
	FIFO_mode_Bypass_mode_until_trigger_is_deasserted_then_Continuous_mode = 4,
	FIFO_mode_Continuous_mode_If_the_FIFO_is_full_the_new_sample_overwrites_the_older_sample = 5

} FIFOModeSelection;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//FIFO status control register

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned FSS:5;
		unsigned OVRN:1;
		unsigned FTH:1;
    }u;

} LSM6DS0_FIFO_SRC_REG;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Linear acceleration sensor control register 6.

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned BW_XL0:2;
    	unsigned BW_SCAL_ODR:1;
		unsigned FS_XL:2;
		unsigned ODR_XL:3;
    }u;

} LSM6DS0_CTRL_REG6_XL_REG;

typedef enum  AccelerometerFullScaleSelection
{
	ACC_FS_2G = 0,
	ACC_FS_16G = 1,
	ACC_FS_4G = 2,
	ACC_FS_8G = 3,

} AccelerometerFullScaleSelection;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


static uint32_t  LSM6DS0_IsExists = 0;
static LSM6DS0_CTRL_REG1_G_REG ctrl_reg1_g;
static LSM6DS0_CTRL_REG6_XL_REG ctrl_reg6_g;

void i2cSlaveWrite(uint8_t slaveaddress, uint8_t regAddress, uint8_t value)
{

	uint8_t data[2] = {regAddress, value};
	I2cTransmit (slaveaddress, data, 2, STOP_I2C);

}
uint8_t i2cSlaveRead(uint8_t slaveaddress, uint8_t regAddress)
{
	/* Send device address, with no STOP condition */
	I2cTransmit(slaveaddress, &regAddress, 1, NO_STOP_I2C);
	 /* Read data, with STOP condition  */
	uint8_t data;
	slaveaddress |= 0x01; //for read
	I2cRecieve (slaveaddress, &data, 1);

	return data;
}

static void LSM6dS0_InitGyroscope()
{
	uint8_t tmp1 = 0;


	tmp1 = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG1_G);

	/* Output Data Rate selection */
	tmp1 &= ~(LSM6DS0_G_ODR_MASK);
	tmp1 |= LSM6DS0_G_ODR_119HZ;
	/* Full scale selection */
	tmp1 &= ~(LSM6DS0_G_FS_MASK);
	tmp1 |= LSM6DS0_G_FS_2000;
	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG1_G, tmp1);
	tmp1 = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG1_G);


	tmp1 = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG4);
	/* Enable X axis selection */
	tmp1 &= ~(LSM6DS0_G_XEN_MASK);
	tmp1 |= LSM6DS0_G_XEN_ENABLE;

	/* Enable Y axis selection */
	tmp1 &= ~(LSM6DS0_G_YEN_MASK);
	tmp1 |= LSM6DS0_G_YEN_ENABLE;

	/* Enable Z axis selection */
	tmp1 &= ~(LSM6DS0_G_ZEN_MASK);
	tmp1 |= LSM6DS0_G_ZEN_ENABLE;

	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG4, tmp1);
}

static void LSM6DS0_InitAccelerometer()
{

	uint8_t tmp1;
	tmp1 = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG6_XL);

	/* Output Data Rate selection */
	tmp1 &= ~(LSM6DS0_XL_ODR_MASK);
	tmp1 |= LSM6DS0_XL_ODR_119HZ;

	/* Full scale selection */
	tmp1 &= ~(LSM6DS0_XL_FS_MASK);
	tmp1 |= LSM6DS0_XL_FS_2G;

	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG6_XL, tmp1);

	tmp1 = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG5_XL);
	/* Enable X axis selection */
	tmp1 &= ~(LSM6DS0_XL_XEN_MASK);
	tmp1 |= LSM6DS0_XL_XEN_ENABLE;

	/* Enable Y axis selection */
	tmp1 &= ~(LSM6DS0_XL_YEN_MASK);
	tmp1 |= LSM6DS0_XL_YEN_ENABLE;

	/* Enable Z axis selection */
	tmp1 &= ~(LSM6DS0_XL_ZEN_MASK);
	tmp1 |= LSM6DS0_XL_ZEN_ENABLE;

	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG5_XL, tmp1);

}


uint32_t LSM6DS0_Init(LSM6DS0_InitModes mode)
{

	LSM6DS0_IsExists = LSM6DS0_CheckExists();
	if (LSM6DS0_IsExists == 0)
		return 0;

	if (mode & LSM6DS0_GYRO)
		LSM6dS0_InitGyroscope();

	if (mode & LSM6DS0_ACCLEREMOTER)
		LSM6DS0_InitAccelerometer();


	LSM6DS0_CTRL_REG3_G_REG reg3g;
	reg3g.Value = 0;
	reg3g.u.HP_EN = 1;
	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG3_G, reg3g.Value); // enable high-pass filter for gyro

	//FIFO_CTRL_REG  fifo_ctrl;
	//fifo_ctrl.Value = 0;
	//fifo_ctrl.u.FMODE = FIFO_mode_BypassMode_FIFOTurnedOff;
	i2cSlaveWrite(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_FIFO_CTRL, 0x00); // Bypass mode, turn of FIFO


	// read Gyro register config for later calculations
	ctrl_reg1_g.Value = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG1_G);


	// read accelreometer register config for later calculations
	ctrl_reg6_g.Value = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_CTRL_REG6_XL);

	return 1;
}

uint32_t LSM6DS0_CheckExists()
{

	 uint8_t val = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, LSM6DS0_WHO_AM_I);
	 if (val == I_AM_LSM6DS0_XG)
		 return 1;
	 else
		 return 0;
}


/**
 * @brief Read raw data from LSM6DS0 Gyroscope output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxesRaw(int16_t *pData)
{

	LSM6DS0_ReadRegister16(&pData[0], LSM6DS0_OUT_X_L_G);
	LSM6DS0_ReadRegister16(&pData[1], LSM6DS0_OUT_Y_L_G);
	LSM6DS0_ReadRegister16(&pData[2], LSM6DS0_OUT_Z_L_G);

}


void LSM6DS0_Gyro_GetAxesXRaw(int16_t *pData)
{

	LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_X_L_G);
}


void LSM6DS0_Gyro_GetAxesYRaw(int16_t *pData)
{

	LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_Y_L_G);

}


void LSM6DS0_Gyro_GetAxesZRaw(int16_t *pData)
{

	LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_Z_L_G);

}

void LSM6DS0_Gyro_GetAxes(AxesRaw_TypeDef *pData)
{

   int16_t pDataRaw[3];
   float sensitivity = 0;


   LSM6DS0_Gyro_GetAxesRaw(pDataRaw);
   PRINTF("ctrl_reg1_g.u.FS_G = %d\n" , ctrl_reg1_g.u.FS_G);
   switch(ctrl_reg1_g.u.FS_G)
   {
	  case GFSS_245_DPS:
		  sensitivity = 8.75;
	  break;
	  case GFSS_500_DPS:
		 sensitivity = 17.50;
	  break;
	  case GFSS_2000_DPS:
		 sensitivity = 70;
	  break;
   }


  pData->AXIS_X = (int32_t)(pDataRaw[0] * sensitivity);
  pData->AXIS_Y = (int32_t)(pDataRaw[1] * sensitivity);
  pData->AXIS_Z = (int32_t)(pDataRaw[2] * sensitivity);
}


void LSM6DS0_Acc_GetAxesRaw(int16_t *pData)
{

    LSM6DS0_ReadRegister16(&pData[0], LSM6DS0_OUT_X_L_XL);
    LSM6DS0_ReadRegister16(&pData[1], LSM6DS0_OUT_Y_L_XL);
    LSM6DS0_ReadRegister16(&pData[2], LSM6DS0_OUT_Z_L_XL);
}

void LSM6DS0_Acc_GetAxesXRaw(int16_t *pData)
{
    LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_X_L_XL);
}

void LSM6DS0_Acc_GetAxesYRaw(int16_t *pData)
{
    LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_Y_L_XL);
}

void LSM6DS0_Acc_GetAxesZRaw(int16_t *pData)
{
    LSM6DS0_ReadRegister16(pData, LSM6DS0_OUT_Z_L_XL);
}


void LSM6DS0_Acc_GetAxes(AxesRaw_TypeDef *pData)
{

  int16_t pDataRaw[3];
  float sensitivity = 0;


  LSM6DS0_Acc_GetAxesRaw(pDataRaw);


	switch(ctrl_reg6_g.u.FS_XL)
	{
		case ACC_FS_2G:
			sensitivity = 0.061;
	   break;
	  case ACC_FS_4G:
		  sensitivity = 0.122;
	  break;
	  case ACC_FS_8G:
	     sensitivity = 0.244;
	  break;
	}


  pData->AXIS_X = (int32_t)(pDataRaw[0] * sensitivity);
  pData->AXIS_Y = (int32_t)(pDataRaw[1] * sensitivity);
  pData->AXIS_Z = (int32_t)(pDataRaw[2] * sensitivity);

}

void LSM6DS0_ReadRegister16(int16_t *pData, uint8_t reg)
{
    uint8_t tempReg[2] = {0,0};

    tempReg[0] = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, reg);
    tempReg[1] = i2cSlaveRead(LSM6DS0_SLAVE_ADDRESS, reg + 1);
    *pData = ((((int16_t)tempReg[1]) << 8)+(int16_t)tempReg[0]);
}
