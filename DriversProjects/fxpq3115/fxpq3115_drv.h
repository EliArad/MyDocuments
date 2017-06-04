/**
 * @file fxpq3115_drv.h
 *
 * @author EliA
 *
 * @date April 2017
 *
 * @brief fxpq3115 driver  header API
 *
 */

#ifndef FXPQ3115_FI_H_
#define FXPQ3115_FI_H_

/* Standard C Includes */
#include <stdint.h>

/* ISSDK Includes */
#include "fxpq3115.h"

typedef enum
{
	FXPQ3115_NormalMode,
	FXPQ3115_FifoMode
} FXPQ3115_ConfigModes;

typedef enum
{
	FXPQ3115_CalculatedMode,
	FXPQ3115_RowMode

} FXPQ3115_ValuesModes;

/*******************************************************************************
 * Macros
 ******************************************************************************/
#define FXPQ3115_ALTITUDE_CONV_FACTOR (65536)  /* Will give meters above MSL */
#define FXPQ3115_TEMPERATURE_CONV_FACTOR (256) /* Will give °C */


/*******************************************************************************
 * Definitions
 ******************************************************************************/
/*! @brief This defines the sensor specific information. */


/*! @brief This structure defines the fxpq3115 data buffer in Pressure Mode.*/
typedef struct
{
    uint32_t timestamp;  /*!< Time stamp value in micro-seconds. */
    uint32_t pressure;   /*!< Sensor pressure output: unsigned 20-bits justified to MSBs. */
    int16_t temperature; /*!< Sensor temperature output; 2's complement 12-bits justified to MSBs.
                              MS 8-bits are integer degrees Celsius; LS 4-bits are fractional degrees Celsius. */
} fxpq3115_pressuredata_t;

/*! @brief This structure defines the fxpq3115 data buffer in Altitude Mode.*/
typedef struct
{
    uint32_t timestamp;  /*!< Time stamp value in micro-seconds. */
    int32_t altitude;    /*!< Sensor pressure/altitude output: MS 16-bits are integer meters; LS 4-bits are fractional
                              meters. */
    int16_t temperature; /*!< Sensor temperature output; 2's complement 12-bits justified to MSBs.
                              MS 8-bits are integer degrees Celsius; LS 4-bits are fractional degrees Celsius. */
} fxpq3115_altitudedata_t;


uint32_t FXPQ3115_Initialize();


uint32_t FXPQ3115_Configure(FXPQ3115_ConfigModes mode , FXPQ3115_OverSampleRationSelection sampleRate, uint32_t rowMode);


#if 0
int32_t FXPQ3115_I2C_ReadData(fxpq3115_i2c_sensorhandle_t *pSensorHandle,
                             const registerreadlist_t *pReadList,
                             uint8_t *pBuffer);

#endif


int FXPQ3115_InitFifo(FXPQ3115_OverSampleRationSelection sampleRate, uint8_t rowMode);
void FXPQ3115_TestFifoMode();

void	    FXPQ3115_SetImmdiateMeasurement(uint8_t set);
uint8_t   FXPQ3115_WaitForAnyDataReady();
int32_t   FXPQ3115_ReadTemperature(uint32_t *rowTemperature);
uint32_t  FXPQ3115_ReadPressureInPascal();
uint32_t  FXPQ3115_ReadImmidiatePressure(uint32_t *pressureInPascals);

int32_t FXPQ3115_DeInit();

#endif // FXPQ3115_FI_H_
