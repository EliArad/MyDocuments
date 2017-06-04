/**
 * @file fxpq3115.h
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief fxpq3115 header API
 *
 */

#ifndef FXPQ3115_H
#define FXPQ3115_H




#define FXPQ3115_STATUS  				0
#define FXPQ3115_OUT_P_MSB  			1
#define FXPQ3115_OUT_P_CSB 			2
#define FXPQ3115_OUT_P_LSB 			3
#define FXPQ3115_OUT_T_MSB 			4
#define FXPQ3115_OUT_T_LSB  			5
#define FXPQ3115_DR_STATUS 			6
#define FXPQ3115_OUT_P_DELTA_MSB  	7
#define FXPQ3115_OUT_P_DELTA_CSB  	8
#define FXPQ3115_OUT_P_DELTA_LSB		9
#define FXPQ3115_OUT_T_DELTA_MSB 	0xA
#define FXPQ3115_OUT_T_DELTA_LSB  	0xB
#define FXPQ3115_WHO_AM_I  			0xC
#define FXPQ3115_F_STATUS	 			0xD
#define FXPQ3115_F_DATA					0xE
#define FXPQ3115_F_SETUP				0xF
#define FXPQ3115_TIME_DLY  			0x10
#define FXPQ3115_FXPQ3115_SYSMOD		0x11h
#define FXPQ3115_INT_SOURCE			0x12
#define FXPQ3115_PT_DATA_CFG  		0x13
#define FXPQ3115_BAR_IN_MSB  			0x14
#define FXPQ3115_BAR_IN_LSB  			0x15
#define FXPQ3115_P_TGT_MSB				0x16
#define FXPQ3115_P_TGT_LSB  			0x17
#define FXPQ3115_T_TGT	  				0x18
#define FXPQ3115_P_WND_MSB				0x19
#define FXPQ3115_P_WND_LSB 			0x1A
#define FXPQ3115_T_WND  				0x1B
#define FXPQ3115_P_MIN_MSB  			0x1C
#define FXPQ3115_P_MIN_CSB  			0x1D
#define FXPQ3115_P_MIN_LSB  			0x1E
#define FXPQ3115_T_MIN_MSB  			0x1F
#define FXPQ3115_T_MIN_LSB  			0x20
#define FXPQ3115_P_MAX_MSB  			0x21
#define FXPQ3115_P_MAX_CSB  			0x22
#define FXPQ3115_P_MAX_LSB  			0x23
#define FXPQ3115_T_MAX_MSB  			0x24
#define FXPQ3115_T_MAX_LSB  			0x25
#define FXPQ3115_CTRL_REG1  			0x26
#define FXPQ3115_CTRL_REG2  			0x27
#define FXPQ3115_CTRL_REG3  			0x28
#define FXPQ3115_CTRL_REG4  			0x29
#define FXPQ3115_CTRL_REG5  			0x2A
#define FXPQ3115_OFF_P 		 			0x2B
#define FXPQ3115_OFF_T 		 			0x2C
#define FXPQ3115_OFF_H  				0x2D


typedef struct
{
    uint16_t writeTo; /* Address where the value is writes to.*/
    uint8_t value;    /* value. Note that value should be shifted based on the bit position.*/
    uint8_t mask;     /* mask of the field to be set with given value.*/
} registerwritelist_t;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

typedef union
{
    uint8_t Value;

    struct
    {
		unsigned SBYB:1;
		unsigned OST:1;
		unsigned RST:1;
		unsigned OS:3;
		unsigned RAW:1;
		unsigned ALT:1;
    }u;

} FXPQ3115_CTRL_REG1_REG;




typedef enum
{
	FXPQ3115_DeviceResetDisabled  = 0,
	FXPQ3115_DeviceResetEnabled = 1

} FXPQ3115_DeviceReset;


typedef enum
{
	FXPQ3115_OS_1 = 0,
	FXPQ3115_OS_2 = 1,
	FXPQ3115_OS_4 = 2,
	FXPQ3115_OS_8 = 3,
	FXPQ3115_OS_16 = 4,
	FXPQ3115_OS_32 = 5,
	FXPQ3115_OS_64 = 6,
	FXPQ3115_OS_128 = 7

}  FXPQ3115_OverSampleRationSelection;


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//sensor data register

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned TDEFE:1;
    	unsigned PDEFE:1;
		unsigned DREM:1;
		unsigned Reserved:5;
    }u;

} FXPQ3115_PT_DATA_CFG_REG;

typedef enum
{
	FXPQ3115_EventDetectionDisabled = 0,
	FXPQ3115_RaiseEventFlagOnNewTemperatureData = 1

} FXPQ3115_DataEventFlagEnableEnNewTemperaturData;

typedef enum
{
	FXPQ3115_EventDetectioDisabled = 0,
	FXPQ3115_RaiseEventFlagOnNewPressurAltitudeData  = 1

} FXPQ3115_DataEventFlagEnableOnNewPressure_altitude;


typedef enum
{

	FXPQ3115_DataReadyEventDetectionDisabled = 0,
	FXPQ3115_GenerateDataReadyEventFlagOnNewPressureAltitudeOrTemperature = 1,

} FXPQ3115_DataReadyEventMode;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//control register 2 (address 27h)



typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned ST:4;
    	unsigned ALARM_SEL:1;
		unsigned LOAD_OUTPUT:1;
		unsigned Reserved:2;
    }u;

} FXPQ3115_CTRL_REG2_REG;

/*! In FXPQ3115 the Auto Acquisition Time Step (ODR) can be set only in powers of 2 (i.e. 2^x, where x is the
 *  SAMPLING_EXPONENT).
 *  This gives a range of 1 second to 2^15 seconds (9 hours). */
#define FXPQ3115_SAMPLING_EXPONENT (1) /* 2 seconds */

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//DR_STATUS - status register

typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned Reserved:1;
    	unsigned TDR:1;
		unsigned PDR:1;
		unsigned PTDR:1;
		unsigned Reserved2:1;
		unsigned TOW:1;
		unsigned POW:1;
		unsigned PTOW:1;
    }u;

} FXPQ3115_DR_STATUS_REG;


//PTDR signals that a new acquisition for either
//pressure/altitude or temperature is available. PTDR is cleared anytime OUT_P_MSB or
//OUT_T_MSB register is read, when F_MODE is zero. PTDR is cleared by reading F_DATA register when F_MODE > 0.
typedef enum
{

	FXPQ3115_NoNewSetOfDataReady = 0,
	FXPQ3115_ANewSetOfDataIsReady = 1

} Pressure_altitudeOrTemperatureDataReady;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//F_SETUP- FIFO setup register (address 0Fh)


typedef union
{
    uint8_t Value;

    struct
    {
    	unsigned F_WMRK:6;
    	unsigned F_MODE:2;
    }u;

} FXPQ3115_F_SETUP_REG;







#endif
