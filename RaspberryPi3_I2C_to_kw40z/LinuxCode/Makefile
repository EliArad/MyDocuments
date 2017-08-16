CC = arm-linux-gnueabihf-gcc

SRC=$(wildcard *.c)

i2c_test: $(SRC)
	$(CC) -o $@ $^ $(CFLAGS)  -I$(INCLUDE)  -L$(LIBS)
   
   
clean:
	rm *.o i2c_test
      