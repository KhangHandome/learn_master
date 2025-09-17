#include "hal_i2c.h"

static uint8_t HAL_IP_I2C_GetRxFlag(void);
static uint8_t HAL_IP_I2C_GetTxFlag(void);

/* ---------------- I2C Init ---------------- */
/**
  * @brief  Initialize I2C1 peripheral in slave mode.
  * @param  clock_speed : Desired I2C bus speed (Hz).
  * @param  freq_mhz    : Peripheral clock frequency (MHz).
  * @retval None
  * @details Configures PB6 (SCL) and PB7 (SDA) as AF open-drain,
  *          sets timing registers (CR2, CCR, TRISE),
  *          assigns own slave address, enables ACK and I2C1.
  */
void HAL_IP_I2C_Init(uint16_t clock_speed,uint16_t freq_mhz)
{
    /* Enable clock for I2C1 */
    RCC->APB1ENR |= RCC_APB1ENR_I2C1EN;

    /* Configure PB6 (SCL), PB7 (SDA) as AF open-drain */
    GPIOB->CRL &= ~(GPIO_CRL_MODE6 | GPIO_CRL_CNF6);
    GPIOB->CRL |= (GPIO_CRL_MODE6 | GPIO_CRL_CNF6);
    GPIOB->CRL &= ~(GPIO_CRL_MODE7 | GPIO_CRL_CNF7);
    GPIOB->CRL |= (GPIO_CRL_MODE7 | GPIO_CRL_CNF7);

    /* Reset I2C1 */
    I2C1->CR1 |= I2C_CR1_SWRST;
    I2C1->CR1 &= ~I2C_CR1_SWRST;

    /* Setup Clock for I2C */
    I2C1->CR2 = freq_mhz;              // Peripheral clock frequency
    I2C1->TRISE = freq_mhz + 1;        // TRISE
    I2C1->CCR = (freq_mhz * 1000000) / (2 * clock_speed);

    /* Configure own slave address */
    I2C1->OAR1 = (STM32_SLAVE_ADDRESS << 1);

    /* Enable I2C and ACK */
    I2C1->CR1 |= I2C_CR1_PE | I2C_CR1_ACK;
}

/* ---------------- Slave Receiver ---------------- */
/**
  * @brief  Receive data as I2C slave.
  * @param  pData : Pointer to buffer for storing received data.
  * @param  size  : Number of bytes to receive.
  * @retval HAL_OK   : Reception completed successfully.
  * @retval HAL_N_OK : Error during reception.
  * @details Waits for address match (EV1),
  *          reads incoming bytes (EV2),
  *          sends NACK for last byte,
  *          waits for STOP condition (EV4).
  */
Status_t HAL_IP_I2C_Receive(uint8_t *pData, uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    static uint8_t counter = 0;
    uint16_t SR2_Register_Status ;

    I2C1->CR1 |= I2C_CR1_ACK;   // Enable ACK

    /* EV1: Wait for address match */
    while (!(I2C1->SR1 & I2C_SR1_ADDR));
    (void)I2C1->SR1;
    /* Read TRA to check slave receive or transmit
     * TRA = 0 : Slave receive
     * TRA = 1 : Slave transmit
     * */
    SR2_Register_Status = I2C1->SR2;
    if ( (SR2_Register_Status & I2C_SR2_TRA_Msk) == 0 )
    {
        /* EV2: Receive data */
        for (counter = 0; counter < size; counter++)
        {
            while (!HAL_IP_I2C_GetRxFlag()); // Wait RXNE=1
            pData[counter] = I2C1->DR;

            if (counter == (size - 1))
            {
                I2C1->CR1 &= (~I2C_CR1_ACK_Msk); // Send NACK for last byte
            }
        }
        retVal = HAL_OK;

        /* EV4: Wait for STOP */
        while (!(I2C1->SR1 & I2C_SR1_STOPF));
        (void)I2C1->SR1;
        I2C1->CR1 |= 0;
    }
    return retVal;
}

/* ---------------- Slave Transmitter ---------------- */
/**
  * @brief  Transmit data as I2C slave.
  * @param  pData : Pointer to buffer containing data to send.
  * @param  size  : Number of bytes to transmit.
  * @retval HAL_OK   : Transmission completed successfully.
  * @retval HAL_N_OK : Error during transmission.
  * @details Waits for address match (EV1),
  *          sequentially sends data to master (EV3),
  *          stops when master issues NACK (AF flag).
  */
Status_t HAL_IP_I2C_Transmit( uint8_t *pData, uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    static uint8_t counter = 0;
    uint16_t SR2_Register_Status ;

    /* EV1: Wait for address match */
    while (!(I2C1->SR1 & I2C_SR1_ADDR));
    (void)I2C1->SR1;

    /* Read TRA to check slave receive or transmit
     * TRA = 0 : Slave receive
     * TRA = 1 : Slave transmit
     * */
    SR2_Register_Status = I2C1->SR2;
    if ( SR2_Register_Status & I2C_SR2_TRA_Msk)
    {
        /* EV3: Send data until master NACK */
        for (counter = 0; counter < size; counter++)
        {
        	while (!HAL_IP_I2C_GetTxFlag());
            I2C1->DR = pData[counter];
        }
        retVal = HAL_OK;

        /* EV3-2: Wait for AF = NACK */
        while (!(I2C1->SR1 & I2C_SR1_AF));
        I2C1->SR1 &= ~I2C_SR1_AF;   // Clear AF
    }
    return retVal;
}

/* ---------------- Master Transmitter ---------------- */
/**
  * @brief  Transmit data as I2C master.
  * @param  slaveAddr : 7-bit address of target slave device.
  * @param  pData     : Pointer to buffer containing data to send.
  * @param  size      : Number of bytes to transmit.
  * @retval HAL_OK   : Transmission completed successfully.
  * @retval HAL_N_OK : Error during transmission.
  * @details Generates START (EV5),
  *          sends slave address + write bit (EV6),
  *          transmits all bytes (EV8),
  *          waits for BTF then issues STOP (EV8-2).
  */
Status_t HAL_IP_I2C_Master_Transmit( uint8_t slaveAddr,  uint8_t *pData,  uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    static uint8_t counter = 0;

    /* EV5: Send START */
    I2C1->CR1 |= I2C_CR1_START;
    while (!(I2C1->SR1 & I2C_SR1_SB));

    /* EV6: Send slave address + write */
    I2C1->DR = (slaveAddr << 1);
    while (!(I2C1->SR1 & I2C_SR1_ADDR));
    (void)I2C1->SR1;
    (void)I2C1->SR2;

    /* EV8: Send data */
    for (counter = 0; counter < size; counter++)
    {
        while (!(I2C1->SR1 & I2C_SR1_TXE));
        I2C1->DR = pData[counter];
    }

    /* EV8_2: Wait BTF then STOP */
    while (!(I2C1->SR1 & I2C_SR1_BTF));
    I2C1->CR1 |= I2C_CR1_STOP;

    retVal = HAL_OK;
    return retVal;
}

/* ---------------- Master Receiver ---------------- */
/**
  * @brief  Receive data as I2C master.
  * @param  slaveAddr : 7-bit address of target slave device.
  * @param  pData     : Pointer to buffer for storing received data.
  * @param  size      : Number of bytes to receive.
  * @retval HAL_OK   : Reception completed successfully.
  * @retval HAL_N_OK : Error during reception.
  * @details Generates START (EV5),
  *          sends slave address + read bit (EV6),
  *          handles single-byte or multi-byte reception,
  *          sends NACK before last byte and issues STOP.
  */
Status_t HAL_IP_I2C_Master_Receive( uint8_t slaveAddr,  uint8_t *pData,  uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    uint16_t i;

    /* EV5: Send START */
    I2C1->CR1 |= I2C_CR1_START;
    while (!(I2C1->SR1 & I2C_SR1_SB));

    /* EV6: Send slave address + read */
    I2C1->DR = (slaveAddr << 1) | 0x01;
    while (!(I2C1->SR1 & I2C_SR1_ADDR));

    if (size == 1)
    {
        /* Single byte receive */
        I2C1->CR1 &= ~I2C_CR1_ACK;
        (void)I2C1->SR1;
        (void)I2C1->SR2;
        I2C1->CR1 |= I2C_CR1_STOP;
        while (!(I2C1->SR1 & I2C_SR1_RXNE));
        pData[0] = I2C1->DR;
    }
    else
    {
        (void)I2C1->SR1;
        (void)I2C1->SR2;
        for (i = 0; i < size; i++)
        {
            if (i == size - 2)
            {
                I2C1->CR1 &= ~I2C_CR1_ACK;
                I2C1->CR1 |= I2C_CR1_STOP;
            }
            while (!(I2C1->SR1 & I2C_SR1_RXNE));
            pData[i] = I2C1->DR;
        }
    }

    retVal = HAL_OK;
    return retVal;
}

/* ---------------- Helper ---------------- */
static uint8_t HAL_IP_I2C_GetTxFlag(void)
{
    return (I2C1->SR1 & I2C_SR1_TXE);
}

static uint8_t HAL_IP_I2C_GetRxFlag(void)
{
    return (I2C1->SR1 & I2C_SR1_RXNE);
}

