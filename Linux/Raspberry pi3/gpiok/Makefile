obj-m := gpiok.o

KERNELDIR := /mnt/b/buildroot/rpi3_i2c0/buildroot-2017.05.1/output/build/linux-5e4ee836560d4c0371e109bf469e1ad808ae7a44

all:
	make ARCH=arm CROSS_COMPILE=arm-linux-gnueabihf-  -C $(KERNELDIR) M=$(PWD) modules
	
clean:
	rm -rf *.o *~ core .depend .*.cmd *.ko *.mod.c .tmp_versions