#include "stm32f103xb.h"

/* Define and typedef */
#define STM32_SLAVE_ADDRESS 0x20u
#define CLOCK_SPEED 10000u
#define FREQ        8u
typedef enum
{
    HAL_OK = 1,
    HAL_N_OK = 0
} Status_t ;

/* Private function prototypes */
static void HAL_IP_I2C_Init(uint16_t clock_speed,uint16_t freq_mhz);
static Status_t HAL_IP_I2C_Receive(uint8_t *pData, uint8_t size);
static Status_t HAL_IP_I2C_Transmit(uint8_t *pData, uint8_t size);
static Status_t HAL_IP_I2C_Master_Transmit(uint8_t slaveAddr, uint8_t *pData, uint8_t size);
static Status_t HAL_IP_I2C_Master_Receive(uint8_t slaveAddr, uint8_t *pData, uint8_t size);

static uint8_t HAL_IP_I2C_GetRxFlag(void);
static uint8_t HAL_IP_I2C_GetTxFlag(void);
static void LED_Init(void);
static void LED_Toggle(void);

int main(void)
{
    Status_t status = HAL_OK;
    uint8_t data_rx[20];

    /* Enable GPIO ports */
    RCC->APB2ENR |= RCC_APB2ENR_IOPAEN;
    RCC->APB2ENR |= RCC_APB2ENR_IOPBEN;
    RCC->APB2ENR |= RCC_APB2ENR_IOPCEN;

    LED_Init();
    HAL_IP_I2C_Init(CLOCK_SPEED,FREQ);

    while (1)
    {
        /* Slave Receive Example */
        status = HAL_IP_I2C_Receive(data_rx, 14);
        if (status == HAL_OK)
        {
            LED_Toggle();
        }
    }
}

/* ---------------- I2C Init ---------------- */
static void HAL_IP_I2C_Init(uint16_t clock_speed,uint16_t freq_mhz)
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
static Status_t HAL_IP_I2C_Receive(uint8_t *pData, uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    uint8_t counter = 0;

    I2C1->CR1 |= I2C_CR1_ACK;   // Enable ACK

    /* EV1: Wait for address match */
    while (!(I2C1->SR1 & I2C_SR1_ADDR));
    (void)I2C1->SR1;
    (void)I2C1->SR2;

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

    return retVal;
}

/* ---------------- Slave Transmitter ---------------- */
static Status_t HAL_IP_I2C_Transmit(uint8_t *pData, uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    uint8_t counter = 0;

    /* EV1: Wait for address match */
    while (!(I2C1->SR1 & I2C_SR1_ADDR));
    (void)I2C1->SR1;
    (void)I2C1->SR2;

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

    return retVal;
}

/* ---------------- Master Transmitter ---------------- */
static Status_t HAL_IP_I2C_Master_Transmit(uint8_t slaveAddr, uint8_t *pData, uint8_t size)
{
    Status_t retVal = HAL_N_OK;
    uint8_t counter = 0;

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
static Status_t HAL_IP_I2C_Master_Receive(uint8_t slaveAddr, uint8_t *pData, uint8_t size)
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

/* ---------------- LED ---------------- */
static void LED_Init(void)
{
    GPIOC->CRH &= ~(GPIO_CRH_MODE13 | GPIO_CRH_CNF13);
    GPIOC->CRH |= GPIO_CRH_MODE13_0;  // Output mode 10MHz
}

static void LED_Toggle(void)
{
    GPIOC->ODR ^= GPIO_ODR_ODR13;
}
