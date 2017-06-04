#ifndef LSM6DS0_HEADER_FILE
#define LSM6DS0_HEADER_FILE

// Written from the following pdf
//http://www.st.com/content/ccc/resource/technical/document/datasheet/6e/09/28/0b/01/06/42/24/DM00101533.pdf/files/DM00101533.pdf/jcr:content/translations/en.DM00101533.pdf


typedef struct _AxesRaw_TypeDef
{

	int32_t AXIS_X;
	int32_t AXIS_Y;
	int32_t AXIS_Z;

} AxesRaw_TypeDef;


typedef enum LSM6DS0_InitModes
{
	LSM6DS0_GYRO = 1,
	LSM6DS0_ACCLEREMOTER = 0x2,
	LSM6DS0_BOTH = 0x3

} LSM6DS0_InitModes;


/*******************************************************************************
* Prototypes of global functions
*******************************************************************************/


/**
 * @brief Initialize LSM6DS0 to our needs in this project
 *
 * @param mode from LSM6DS0_InitModes
 * @return uint32_t
 */
uint32_t LSM6DS0_Init(LSM6DS0_InitModes mode);


/**
 * @brief Read the who am i register and check if the device actually exists
 * @return uint32_t 1 exists or 0 does not exists
 */
uint32_t LSM6DS0_CheckExists();

/**
 * @brief Read data from LSM6DS0 Gyroscope and calculate angular rate in mdps.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxes(AxesRaw_TypeDef *pData);

/**
 * @brief Read the all ther axis Raws data from LSM6DS0 Gyroscope output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxesRaw(int16_t *pData);


/**
 * @brief Read the X Raw data from LSM6DS0 Gyroscope output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxesXRaw(int16_t *pData);

/**
 * @brief Read the Y Raw data from LSM6DS0 Gyroscope output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxesYRaw(int16_t *pData);

/**
 * @brief Read the Z Raw data from LSM6DS0 Gyroscope output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Gyro_GetAxesZRaw(int16_t *pData);



/**
 * @brief Read all raw data from LSM6DS0 Accelerometer output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Acc_GetAxesRaw(int16_t *pData);


/**
 * @brief Read X raw data from LSM6DS0 Accelerometer output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Acc_GetAxesXRaw(int16_t *pData);

/**
 * @brief Read Y raw data from LSM6DS0 Accelerometer output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Acc_GetAxesYRaw(int16_t *pData);


/**
 * @brief Read Z raw data from LSM6DS0 Accelerometer output register.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Acc_GetAxesZRaw(int16_t *pData);

/**
 * @brief Read consequtive 2 register address as one 16 bit data
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_ReadRegister16(int16_t *pData, uint8_t reg);



/**
 * @brief Read data from LSM6DS0 Accelerometer and calculate linear acceleration in mg.
 * @param float *pfData
 * @retval None.
 */
void LSM6DS0_Acc_GetAxes(AxesRaw_TypeDef *pData);




#endif
