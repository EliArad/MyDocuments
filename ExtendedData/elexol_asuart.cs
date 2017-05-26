using EleXolIO24RApi;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EleXolIO24RApi
{
    public class elexol_asuart
    {

        SerialPort  _serialPort = new SerialPort();
        byte[] buf = new byte[250];
        byte[] m_direction = new byte[1];

        public elexol_asuart(string COMPORT)
        {
            _serialPort.PortName = COMPORT;
            _serialPort.BaudRate = 115200;
            _serialPort.Parity =  Parity.None;
            _serialPort.DataBits =  8;
            _serialPort.StopBits = StopBits.One;
            //_serialPort.Handshake =  Handshake.XOnXOff;
        }
        public elexol_asuart GetBase()
        {
            return this;
        }
        public virtual bool Open()
        {
            _serialPort.Open();
            return _serialPort.IsOpen;
        }
        public virtual void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            
        }
        public virtual void setAllDirection(DIRECTION_ALL direction)
        {
            lock (this)
            {
                string[] strPorts = { "!A", "!B", "!C" };
                foreach (string s in strPorts)
                {
                    _serialPort.Write(s);
                    if (direction == DIRECTION_ALL.OUTPUT)
                        m_direction[0] = 0x0;
                    else
                        m_direction[0] = 0xFF;
                    _serialPort.Write(m_direction, 0, 1);
                }
            }
        }

        public virtual void setDirection(PORT_NUMBER port, DIRECTION_ALL direction)
        {
            lock (this)
            {
                string portStr = string.Empty;
                switch (port)
                {
                    case PORT_NUMBER.PORTA:
                        portStr = "!A";
                    break;
                    case PORT_NUMBER.PORTB:
                        portStr = "!B";
                    break;
                    case PORT_NUMBER.PORTC:
                        portStr = "!C";
                    break;
                }
               
                if (direction == DIRECTION_ALL.OUTPUT)
                    m_direction[0] = 0x0;
                else
                    m_direction[0] = 0xFF;
                _serialPort.Write(portStr);
                _serialPort.Write(m_direction, 0, 1);
            }
        }

        public virtual void setDirection(PORT_NUMBER port, DIRECTION direction, byte inputPins)
        {
            lock (this)
            {
                string portStr = string.Empty;
                switch (port)
                {
                    case PORT_NUMBER.PORTA:
                        portStr = "!A";
                        break;
                    case PORT_NUMBER.PORTB:
                        portStr = "!B";
                        break;
                    case PORT_NUMBER.PORTC:
                        portStr = "!C";
                        break;
                }

                if (direction == DIRECTION.OUTPUT)
                    m_direction[0] &= inputPins;
                else
                    m_direction[0] |= inputPins;
                _serialPort.Write(portStr);
                _serialPort.Write(m_direction, 0, 1);
            }
        }
        public virtual void Write(PORT_NUMBER port, byte value)
        {

            string portStr = string.Empty;
            switch (port)
            {
                case PORT_NUMBER.PORTA:
                    portStr = "A";
                    break;
                case PORT_NUMBER.PORTB:
                    portStr = "B";
                    break;
                case PORT_NUMBER.PORTC:
                    portStr = "C";
                    break;
            }
            _serialPort.Write(portStr);          
            _serialPort.Write(buf, 0 , 1);
        }

        public virtual void Write(char port, byte value)
        {

            _serialPort.Write(port.ToString());
            _serialPort.Write(buf, 0, 1);
        }

        public virtual void Write(PORT_NUMBER port, byte writePins, byte value)
        {
            setDirection(port, DIRECTION.OUTPUT, writePins);
            string portStr = string.Empty;
            switch (port)
            {
                case PORT_NUMBER.PORTA:
                    portStr = "A";
                    break;
                case PORT_NUMBER.PORTB:
                    portStr = "B";
                    break;
                case PORT_NUMBER.PORTC:
                    portStr = "C";
                    break;
            }
            _serialPort.Write(portStr);
            _serialPort.Write(buf, 0, 1);
        }
        public virtual bool ReadPort(PORT_NUMBER port, out byte valueRead)
        {
            valueRead = 0;
            string portStr = string.Empty;
            switch (port)
            {
                case PORT_NUMBER.PORTA:
                    portStr = "a";
                    break;
                case PORT_NUMBER.PORTB:
                    portStr = "a";
                    break;
                case PORT_NUMBER.PORTC:
                    portStr = "a";
                    break;
            }
            _serialPort.Write(portStr);
            setDirection(port, DIRECTION_ALL.INPUT);

            int timeout = 0;
            while (_serialPort.BytesToRead == 0)
            {
                Thread.Sleep(10);
                if (timeout == 100)
                    return false;
                timeout++;
            }

            if (_serialPort.BytesToRead == 1)
            {
                _serialPort.Read(buf, 0, 1);
                valueRead = buf[0];
                Console.WriteLine(buf[0].ToString());
                return true;
            }
            else
            {
                return false;
            }

        }
        public virtual bool Read(PORT_NUMBER port, byte InputPins, out byte valueRead)
        {
            valueRead = 0;
            string portStr = string.Empty;
            switch (port)
            {
                case PORT_NUMBER.PORTA:
                    portStr = "a";
                    break;
                case PORT_NUMBER.PORTB:
                    portStr = "a";
                    break;
                case PORT_NUMBER.PORTC:
                    portStr = "a";
                    break;
            }

            _serialPort.Write(portStr);

            setDirection(port, DIRECTION.INPUT, InputPins);

            int timeout = 0;
            while (_serialPort.BytesToRead == 0)
            {
                Thread.Sleep(10);
                if (timeout == 100)
                    return false;
                timeout++;
            }

            if (_serialPort.BytesToRead == 1)
            {
                _serialPort.Read(buf, 0, 1);
                valueRead = buf[0];
                Console.WriteLine(buf[0].ToString());
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
