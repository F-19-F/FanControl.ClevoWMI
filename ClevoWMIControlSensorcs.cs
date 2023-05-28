using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Management;
namespace FanControl.ClevoWMI
{
    internal class ClevoWMIControlSensor : IPluginControlSensor
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public float? Value { get; set; }
        public uint Index { get; set; }

        void IPluginControlSensor.Reset()
        {
            //throw new NotImplementedException();
        }

        void IPluginControlSensor.Set(float val)
        {
            //throw new NotImplementedException();
        }

        void IPluginSensor.Update()
        {
            //throw new NotImplementedException();
        }
    }
}