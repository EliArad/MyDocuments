using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


namespace Phidget21Api
{
    public partial class SensorBox  
    {
        int lastSensorValue = 0;
        BaseSensor sensorDisplay;

        //This function has the task of creating the correct sensor control based on what was selected
        public void SelectSensor(string select)
        {
            if (sensorDisplay != null)
            {
                sensorDisplay = null;
            }

            switch (select)
            {
                case "Base Value": sensorDisplay = new BaseSensor(); break;
                case "1118 - 50Amp Current Sensor [AC]": sensorDisplay = new Sensor1118AC(); break;
                case "1118 - 50Amp Current Sensor [DC]": sensorDisplay = new Sensor1118DC(); break;
                case "1119 - 20Amp Current Sensor [AC]": sensorDisplay = new Sensor1119AC(); break;
                case "1119 - 20Amp Current Sensor [DC]": sensorDisplay = new Sensor1119DC(); break;
                case "1122 - 30Amp Current Sensor [AC]": sensorDisplay = new Sensor1122AC(); break;
                case "1122 - 30Amp Current Sensor [DC]": sensorDisplay = new Sensor1122DC(); break;
                case "1123 - Precision Voltage Sensor": sensorDisplay = new Sensor1123(); break;
                case "1124 - Precision Temperature Sensor": sensorDisplay = new Sensor1124(); break;
                case "1125 - Humidity/Temperature Sensor [H]": sensorDisplay = new Sensor1125H(); break;
                case "1125 - Humidity/Temperature Sensor [T]": sensorDisplay = new Sensor1125T(); break;
                case "1127 - Precision Light Sensor": sensorDisplay = new Sensor1127(); break;
                case "1126 - Differential Gas Pressure Sensor": sensorDisplay = new Sensor1126(); break;
                case "1128 - Sonar Sensor": sensorDisplay = new Sensor1128(); break;
                case "1130 - pH Adapter Board [pH]": sensorDisplay = new Sensor1130pH(); break;
                case "1131 - Thin Force Sensor": sensorDisplay = new Sensor1131(); break;
                case "1132 - 4-20 mA Adapter": sensorDisplay = new Sensor1132(); break;
                case "1133 - Sound Sensor": sensorDisplay = new Sensor1133(); break;
                case "1135 - Precision Voltage Sensor": sensorDisplay = new Sensor1135(); break;
                case "1137 - Differential Gas Pressure Sensor ±7 kPa": sensorDisplay = new Sensor1137(); break;
                case "1138 - Differential Gas Pressure Sensor 50 kPa": sensorDisplay = new Sensor1138(); break;
                case "1139 - Differential Gas Pressure Sensor 100 kPa": sensorDisplay = new Sensor1139(); break;
                case "1141 - Absolute Gas Pressure Sensor 15-115 kPa": sensorDisplay = new Sensor1141(); break;
            }

            if (sensorDisplay != null)
            {
                //ChangeValue(lastSensorValue);
            }
        }

        //This function has the task of executing changeDisplay based on what was selected
        public void ChangeValue(int val, out string str)
        {
            lastSensorValue = val;
            if (sensorDisplay == null)
            {
                str = "Error , sensor is not initialize";
                return;
            }

            sensorDisplay.changeDisplay(val, out str);

        }
    }
}
