#include <linux/module.h>    // included for all kernel modules
#include <linux/kernel.h>    // included for KERN_INFO
#include <linux/init.h>      // included for __init and __exit macros
#include <linux/gpio.h>
#include <linux/kthread.h>  // for threads
#include <linux/time.h>   // for using jiffies 
#include <linux/timer.h>
#include <linux/delay.h>

MODULE_LICENSE("GPL");
MODULE_AUTHOR("Eli Arad");
MODULE_DESCRIPTION("Toggle led in raspberry pi 3 using module");
MODULE_VERSION("1.0");

//static char *gpionum = "gpionum";
int gpionum = 27;
module_param(gpionum, int, S_IRUGO); ///< Param desc. charp = char ptr, S_IRUGO can be read/not changed
MODULE_PARM_DESC(gpionum, "The gpio number to toggle");  ///< parameter description
static int gpio_led;

void CreateGpioThread(void);
static int m_moduleAlive = 0;
static struct task_struct *thread1 = NULL;


int threadfunc(void *data)
{
  printk ("Thread started..\n");
  while (m_moduleAlive == 1)
  {
     gpio_set_value(gpionum, 0);
     schedule();
     msleep(500);
     gpio_set_value(gpionum, 1);
     msleep(500);
     
  }
          
  return 0;
}

void CreateGpioThread()
{
   char gpio_thread[]="gpio_thread";
   thread1 = kthread_create(threadfunc,NULL,gpio_thread);
   if ( thread1 )
   {   
       wake_up_process(thread1);
   }
}

static int __init gpiok_init(void)
{

    printk(KERN_INFO "Hello world!\n");
    printk(KERN_INFO "gpio num %d\n" , gpionum);
    if (gpionum != 0)
    {
        gpio_led = gpio_request(gpionum , "MyLed");
        if (gpio_led != 0)
        {
             printk(KERN_INFO "Gpio allocation failed %d\n" , gpio_led);
        }
        else
        {
            printk(KERN_INFO "Gpio allocation ok for %d\n", gpionum);
            gpio_direction_output(gpionum, 1);
            gpio_export(gpionum, 0);
            
        }
        
        m_moduleAlive = 1;
        CreateGpioThread();
    }
    return 0;    // Non-zero return means that the module couldn't be loaded.

}

static void __exit gpiok_cleanup(void)
{
    m_moduleAlive = 0;
    if (thread1 != NULL)    
        kthread_stop(thread1);
    printk(KERN_INFO "Cleaning up module.\n");
}

module_init(gpiok_init);
module_exit(gpiok_cleanup);
