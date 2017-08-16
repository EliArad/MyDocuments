#include <stdio.h>
#include <stdlib.h>
#include <linux/i2c.h>
#include <linux/i2c-dev.h>
#include <sys/ioctl.h>
#include <fcntl.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>

#define MY_I2C_SLAVE_ID  0x7F

void main()
{
   int fd;
   
   char filename[26];
  
   sprintf(filename,"/dev/i2c-1");
    
   if ((fd = open(filename,O_RDWR)) < 0)
   {
       fprintf(stderr, "i2c_open open error: %s\n", strerror(errno));
       return;
   }
                  
   if (ioctl(fd , I2C_SLAVE , MY_I2C_SLAVE_ID) < 0)
   {
      fprintf(stderr, "i2c_open ioctl error: %s\n", strerror(errno));
      return;
   }
   printf("open\n");

   #define SIZEBUF  80
   char buf_write[SIZEBUF];
   char buf_read[SIZEBUF];
   for (int i = 0 ; i < SIZEBUF ; i++)
   {
      buf_write[i] = 1 + i;
   }
   int countok = 0;
   while (1)
   {
      if (write(fd, buf_write, sizeof(buf_write)) != sizeof(buf_write))   
      {
          fprintf(stderr, "i2c_write error: %s\n", strerror(errno));
          return;
      }   
      if (read(fd, buf_read, sizeof(buf_read)) != sizeof(buf_read))   
      {
          fprintf(stderr, "i2c_read error: %s\n", strerror(errno));
          return;
      }   
      if (memcmp(buf_write, buf_read , sizeof(buf_write)) != 0)
      {
         printf("error in compare buffers\n");
         for (int i = 0; i < sizeof(buf_write) ; i++)
         {
            printf("write %x  read %x\n" , buf_write[i] , buf_read[i]);
         }                 
      } else {
        printf("Ok[%d]\r" , ++countok);
        fflush(stdout);
      }
   }

   printf("finished\n");
   close(fd);

}