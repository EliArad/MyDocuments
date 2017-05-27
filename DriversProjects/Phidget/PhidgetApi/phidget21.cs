using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets;         //needed for the interfacekit class and the phidget exception class
using Phidgets.Events;  //needed for the event handling classes
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Linq;

namespace Phidget21Api
{
    public enum PhidgetCallbackCode
    {
        NONE = 0,
        ATTACH = 1,
        DETACH = 2,
        ERROR = 4,
        INPUTCHANGE = 8,
        OUTPUTCHANGE = 0x10,
        SENSORCHANGE = 0x20,
        PHIDGET_READY = 0x40,
    }
    public struct PhidgetAnalogSensorDataRow
    {
        public string sensorName;
        public double value;
    }
    public class Phidget21
    { 
        public struct  Phidet21Info
        {
             public string attachedTxt;
             public string nameTxt;
             public string serialTxt;
             public string versionTxt;
             public string digiInNumTxt;
             public string digiOutNumTxt;
             public string sensorInNumTxt;
        }

        protected static object _locked = new object();
        protected int m_outputChangeEvents = 0;
        protected double[] m_specialSensors = new double[8];
        protected bool[] m_output = new bool[8];
        protected bool[] m_inputs = new bool[8];
        protected bool m_attached = false;
        protected bool m_open = false;
        protected BaseSensor[] n_specialSensors = new BaseSensor[3];
        protected Socket m_clientSocket;
        protected string m_phidgetError = string.Empty;
        protected Phidet21Info m_phidet21Info = new Phidet21Info();
        protected string[] sensorInArray = new string[8];
        protected EventWaitHandle m_attachedEvent = null;
        protected EventWaitHandle m_errorEvent = null;
        protected EventWaitHandle m_outEvent = null;
        protected EventWaitHandle m_inEvent = null;
        protected EventWaitHandle m_sensorEvent = null;
        protected EventWaitHandle m_detachEvent = null;
        protected EventWaitHandle m_readyEvent = null;
        protected bool m_readySent = false;
        protected Dictionary<PhidgetCallbackCode, int> m_eventCode = new Dictionary<PhidgetCallbackCode, int>();
        protected bool m_useDebugValue = false;
        protected PhidgetAnalogSensorDataRow[] m_config;

        public virtual void setEvents(EventWaitHandle ev_attached,
                                      EventWaitHandle ev_error,
                                      EventWaitHandle ev_out,
                                      EventWaitHandle ev_in,
                                      EventWaitHandle ev_sensor,
                                      EventWaitHandle ev_detach,
                                      EventWaitHandle ev_ready)
        {
            m_attachedEvent = ev_attached;
            m_errorEvent = ev_error;
            m_outEvent = ev_out;
            m_inEvent = ev_in;
            m_sensorEvent = ev_sensor;
            m_detachEvent = ev_detach;
            m_readyEvent = ev_ready;
        }

        public delegate void PhidgetCallback(PhidgetCallbackCode c);
        PhidgetCallback m_pCallback;

        private InterfaceKit ifKit = null;
        public Phidget21()
        {

            m_eventCode.Add(PhidgetCallbackCode.ATTACH, 0);
            m_eventCode.Add(PhidgetCallbackCode.DETACH, 0);
            m_eventCode.Add(PhidgetCallbackCode.ERROR, 0);
            m_eventCode.Add(PhidgetCallbackCode.INPUTCHANGE, 0);
            m_eventCode.Add(PhidgetCallbackCode.OUTPUTCHANGE, 0);
            m_eventCode.Add(PhidgetCallbackCode.SENSORCHANGE, 0);
            m_eventCode.Add(PhidgetCallbackCode.PHIDGET_READY, 0);

            n_specialSensors[0] = new Sensor1125H();
            n_specialSensors[1] = new Sensor1125T();
            n_specialSensors[2] = new SensorVoltage();            
        }
        public void SetPhidgetCallback(PhidgetCallback p)
        {
            m_pCallback = p;
        }
        public void SetSpecialSensorsCount(int count)
        {
            m_specialSensors = new double[count];
        }
        public virtual void Open()
        {
            try
            {
                if (m_open == true)
                    return;

                //InterfaceKit.enableLogging(Phidget.LogLevel.PHIDGET_LOG_DEBUG, "c:\\PhidgetLog.txt");

                if (ifKit == null)
                    ifKit = new InterfaceKit();

                ifKit.Attach += new AttachEventHandler(ifKit_Attach);
                ifKit.Detach += new DetachEventHandler(ifKit_Detach);
                ifKit.Error += new ErrorEventHandler(ifKit_Error);

                ifKit.InputChange += new InputChangeEventHandler(ifKit_InputChange);
                ifKit.OutputChange += new OutputChangeEventHandler(ifKit_OutputChange);
                ifKit.SensorChange += new SensorChangeEventHandler(ifKit_SensorChange);
                m_open = true;

            }
            catch (PhidgetException ex)
            {
                throw ((new SystemException("Error initialize Phidget21")));
            }

            openCmdLine(ifKit);
        }

        public void SetSensitivity(int [] value)
        {

            try
            {
                for (int i = 0; i < ifKit.sensors.Count; i++)
                {
                    ifKit.sensors[i].Sensitivity = value[0];
                }
            }
            catch (PhidgetException ex)
            {

            }
        }
        public void SetSensitivity(int sensor , int value)
        {

            try
            {
                ifKit.sensors[sensor].Sensitivity = value;
            }
            catch (PhidgetException ex)
            {

            }
        }

        public void SetConfig(PhidgetAnalogSensorDataRow [] p)
        {
            m_config = p;
        }
        public void UseDebugValues(bool b)
        {
            m_useDebugValue = b;
        }
        protected virtual void openCmdLine(Phidget p)
        {
            openCmdLine(p, null);
        }
        protected virtual void openCmdLine(Phidget p, String pass)
        {
            int serial = -1;         
            try
            {      
                p.open(serial);      
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
        //Sensor input change event handler
        //Set the textbox content based on the input index that is communicating
        //with the interface kit
        void ifKit_SensorChange(object sender, SensorChangeEventArgs e)
        { 
            //sensorInArray[e.Index].Text = e.Value.ToString();
            string str;
            switch (e.Index)
            {
                case 0:
                case 1:
                    {
                        double value = n_specialSensors[e.Index].changeDisplay(e.Value, out str);

                        if (m_useDebugValue == true)
                        {
                            m_specialSensors[0] = m_config[4].value;
                            m_specialSensors[1] = m_config[5].value;
                        }
                        else
                        {
                            m_specialSensors[e.Index] = value;
                        }

                        if (m_sensorEvent != null)
                        {
                            m_eventCode[PhidgetCallbackCode.SENSORCHANGE]++;
                            m_sensorEvent.Set();
                        }
                    }
                    break;
                case 2:
                {
                    double value = n_specialSensors[e.Index].changeDisplay(e.Value, out str);

                    if (m_useDebugValue == true)
                    {
                        m_specialSensors[2] = m_config[6].value;
                    }
                    else
                    {
                        m_specialSensors[e.Index] = value;
                    }

                    if (m_sensorEvent != null)
                    {
                        m_eventCode[PhidgetCallbackCode.SENSORCHANGE]++;
                        m_sensorEvent.Set();
                    }
                }
                break;
                default:
                {
                    Console.WriteLine("Unknown: " + e.Index);
                }
                break;
            }

           // if (advSensorForm != null)
               // advSensorForm.SetValue(e.Index, e.Value);
        }

        public virtual bool[] getOutputs()
        {
            if (m_attached == false)
                return null;
            bool [] b = new bool[8];

            for (int i = 0; i < b.Length;i++)
            {
                b[i] = ifKit.outputs[i];
            }
            return b;
        }
        public virtual bool getOutput(int port)
        {
            if (m_attached == false)
                return false;
            return ifKit.outputs[port];
        }
        public virtual bool[] getInputs()
        {
            return m_inputs;
        }

        public virtual double[] getSensors()
        {
            if (m_useDebugValue == true)
            {
                m_specialSensors[0] = m_config[4].value;
                m_specialSensors[1] = m_config[5].value;
                m_specialSensors[2] = m_config[6].value;
            }             
            return m_specialSensors;
        }

        //Digital output change event handler
        //Here we check or uncheck the corresponding output checkbox
        //based on the index of the output that generated the event
        void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
            m_outputChangeEvents++;
            Console.WriteLine("m_outputChangeEvents {0}", m_outputChangeEvents);
            if (Ready && m_readySent == false)
            {
                if (m_readyEvent != null)
                    m_readyEvent.Set();
                m_readySent = true;
                m_pCallback(PhidgetCallbackCode.PHIDGET_READY);
            }
            //digiOutDispArray[e.Index].Checked = e.Value;
            m_output[e.Index] = e.Value;

            if (m_outEvent != null)
            {
                m_eventCode[PhidgetCallbackCode.OUTPUTCHANGE]++;
                m_outEvent.Set();
            }
        }
        //Digital input change event handler
        //Here we check or uncheck the corresponding input checkbox based 
        //on the index of the digital input that generated the event
        void ifKit_InputChange(object sender, InputChangeEventArgs e)
        {
            //digiInArray[e.Index].Checked = e.Value;
            m_inputs[e.Index] =  e.Value;
            if (m_inEvent != null)
            {
                m_eventCode[PhidgetCallbackCode.INPUTCHANGE]++;
                m_inEvent.Set();
            }
        }

        //IfKit attach event handler
        //Here we'll display the interface kit details as well as determine how many output and input
        //fields to display as well as determine the range of values for input sensitivity slider
        void ifKit_Attach(object sender, AttachEventArgs e)
        {
            InterfaceKit ifKit = (InterfaceKit)sender;
            m_phidet21Info.attachedTxt = ifKit.Attached.ToString();
            m_phidet21Info.nameTxt = ifKit.Name;
            m_phidet21Info.serialTxt = ifKit.SerialNumber.ToString();
            m_phidet21Info.versionTxt = ifKit.Version.ToString();
            m_phidet21Info.digiInNumTxt = ifKit.inputs.Count.ToString();
            m_phidet21Info.digiOutNumTxt = ifKit.outputs.Count.ToString();
            m_phidet21Info.sensorInNumTxt = ifKit.sensors.Count.ToString();
            m_attached = true;
           
            if (m_attachedEvent != null)
            {
                m_eventCode[PhidgetCallbackCode.ATTACH]++;
                m_attachedEvent.Set();            
            }
            if (m_pCallback != null)
            {
                m_pCallback(PhidgetCallbackCode.ATTACH);
            }


            try
            {
                SetSensitivity(3, 300);
            }
            catch (Exception err)
            {

            }
        }
        public virtual void ClearEvent(PhidgetCallbackCode c)
        {
            m_eventCode[c] = 0;
        }
        public virtual void ClearAllEvents()
        {
            m_eventCode[PhidgetCallbackCode.ATTACH] = 0;
            m_eventCode[PhidgetCallbackCode.DETACH] = 0;
            m_eventCode[PhidgetCallbackCode.ERROR] = 0;
            m_eventCode[PhidgetCallbackCode.INPUTCHANGE] = 0;
            m_eventCode[PhidgetCallbackCode.OUTPUTCHANGE] = 0;
            m_eventCode[PhidgetCallbackCode.SENSORCHANGE] = 0;
        }
        public virtual void ClearPointEvent(PhidgetCallbackCode c)
        {
            if (m_eventCode[c] > 0)
            {
                m_eventCode[c]--;
            }
            else
            {
                throw (new SystemException("Error in clear event"));
            }
        }

        //Ifkit detach event handler
        //Here we display the attached status, which will be false as the device is disconnected. 
        //We will also clear the display fields and hide the inputs and outputs.
        void ifKit_Detach(object sender, DetachEventArgs e)
        {
            InterfaceKit ifKit = (InterfaceKit)sender;
            m_phidet21Info.attachedTxt = ifKit.Attached.ToString();
            m_phidet21Info.nameTxt = "";
            m_phidet21Info.serialTxt = "";
            m_phidet21Info.versionTxt = "";
            m_phidet21Info.digiInNumTxt = "";
            m_phidet21Info.digiOutNumTxt = "";
            m_phidet21Info.sensorInNumTxt = "";
            m_attached = false;
            if (m_detachEvent != null)
            {
                m_eventCode[PhidgetCallbackCode.DETACH]++;
                m_detachEvent.Set();
            }             
        }

        //Error event handler
        void ifKit_Error(object sender, ErrorEventArgs e)
        {
            Phidget phid = (Phidget)sender;

            switch (e.Type)
            {
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_BADPASSWORD:

                break;
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_PACKETLOST:
                    //Ignore this error - it's not useful in this context.
                break;
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_OVERRUN:
                    //Ignore this error - it's not useful in this context.
                break;
            }
            if (m_errorEvent != null)
            {
                m_phidgetError = e.Type.ToString();
                m_eventCode[PhidgetCallbackCode.ERROR]++;
                m_errorEvent.Set();
            }
             
        }
        public virtual string Error
        {
            get
            {
                return m_phidgetError;
            }
        }

        public virtual void Close()
        {
            if (m_attached == false)
                return; 
            if (m_open == false)
                return;
            ifKit.Attach -= new AttachEventHandler(ifKit_Attach);
            ifKit.Detach -= new DetachEventHandler(ifKit_Detach);
            ifKit.Error -= new ErrorEventHandler(ifKit_Error);

            ifKit.InputChange -= new InputChangeEventHandler(ifKit_InputChange);
            ifKit.OutputChange -= new OutputChangeEventHandler(ifKit_OutputChange);
            ifKit.SensorChange -= new SensorChangeEventHandler(ifKit_SensorChange);

            if (ifKit != null)
                ifKit.close();
            m_attached = false;
            m_outputChangeEvents = 0;
            m_readySent = false;
            m_open = false;

        }

        public virtual void SetClientSocket(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
        }
        public virtual bool Ready
        {
            get
            {
                try
                {
                    if (m_outputChangeEvents >= int.Parse(m_phidet21Info.digiOutNumTxt) && m_attached == true)
                        return true;
                    else
                        return false;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual bool Attached
        {
            get
            {
                return m_attached;
            }
        }

        public virtual void TogglePhidgetPort(int outputIndex)
        {
            bool x = !m_output[outputIndex];
            SetOutput(outputIndex, x);
        }
        public virtual void SetOutput(int outputIndex, bool enable)
        {
            while (Monitor.TryEnter(_locked) == false)
            {
                Console.WriteLine("Trying to lock.");
                Thread.Sleep(100);
            }
            if (Ready == false)
            {
                Monitor.Exit(_locked);
                throw (new SystemException("Cannot set output before phidget is Ready"));
            }

            ifKit.outputs[outputIndex] = enable;

            int count = 500;
            while (m_output[outputIndex] != enable)
            {
                if (count == 0)
                {
                    Monitor.Exit(_locked);
                    throw (new SystemException("Phidget setput timeout error"));
                }
                count--;
                Thread.Sleep(10);
            }

            Monitor.Exit(_locked);
        }
        public bool getOutputChange(int index)
        {
            return m_output[index];
        }

        public virtual Dictionary<PhidgetCallbackCode, int> getEvents()
        {
            return m_eventCode;
        }
        public virtual Phidet21Info getPhidet21Info()
        {
            return m_phidet21Info;
        }      
    }
}
