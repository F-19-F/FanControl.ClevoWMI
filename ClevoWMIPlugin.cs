using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Management;

namespace FanControl.ClevoWMI
{
    public class ClevoWMIPlugin : IPlugin2
    {
        private static ManagementObject clevoHW;
        List<ClevoWMISensor> sensors;
        List<ClevoWMIControlSensor> controllers;

        private readonly IPluginLogger logger;
        private readonly IPluginDialog dialog;

        public ClevoWMIPlugin(IPluginLogger logger, IPluginDialog dialog)
        {
            this.logger = logger;
            this.dialog = dialog;
        }

        public string Name => "Asus WMI";

        public void Close()
        {
            sensors = null;
            controllers = null;
            clevoHW.Dispose();
            clevoHW = null;
        }

        public void Initialize()
        {
            try
            {
                clevoHW = GetInstance(new ManagementScope(@"root\wmi"), "CLEVO_GET");
            }
            catch
            {
                logger.Log("ClevoWMIPlugin Initialization failed.");
            }
        }

        public void Load(IPluginSensorsContainer _container)
        {
            if (clevoHW is null)
            {
                return;
            }
            try
            {
                ManagementBaseObject wmiSensorNumResult = clevoHW.InvokeMethod("GetFANCount", null, null);
                uint funCount = (uint)wmiSensorNumResult["Data"] + 1;
                sensors = new List<ClevoWMISensor>();
                controllers = new List<ClevoWMIControlSensor>();
                for (uint i = 0; i < funCount; i++)
                {
                    var sensor = new ClevoWMISensor()
                    {
                        Index = i,
                        Id = $"ClevoWMI{i}",
                        Name = $"FAN{i+1}",
                    };
                    var controller = new ClevoWMIControlSensor()
                    {
                        Index = i,
                        Id = $"ClevoWMICtl{i}",
                        Name = $"FAN{i + 1}_CTRL",
                    };
                    sensors.Add(sensor);
                    controllers.Add(controller);
                    _container.FanSensors.Add(sensor);
                    _container.ControlSensors.Add(controller);
                }
                Update();
            }
            catch (ManagementException e)
            {
                logger.Log($"Loading sensors failed: {e.Message}");
            }
        }

        public void Update()
        {
            if (clevoHW is null || sensors is null)
            {
                return;
            }
            try
            {
                ManagementBaseObject rpms12 = clevoHW.InvokeMethod("GetFan12RPM", null, null);
                ManagementBaseObject rpms34 = clevoHW.InvokeMethod("GetFan34RPM", null, null);
                uint[] rpms = new uint[4];
                UInt32 raw12 = (UInt32)rpms12["Data"];
                UInt32 raw34 = (UInt32)rpms34["Data"];
                UInt16 raw1 = (UInt16)(raw12 >> 16);
                UInt16 raw2 = (UInt16)(raw12 & 0xFFFF);
                UInt16 raw3 = (UInt16)(raw12 >> 16);
                UInt16 raw4 = (UInt16)(raw12 & 0xFFFF);
                rpms[0] = (uint)Math.Round((2156220.0 / raw1));
                rpms[1] = (uint)Math.Round((2156220.0 / raw2));
                rpms[2] = (uint)Math.Round((2156220.0 / raw3));
                rpms[3] = (uint)Math.Round((2156220.0 / raw4));
                for (int i= 0;i<sensors.Count;i++)
                {
                    // var sourceParam = clevoHW.GetMethodParameters("sensor_update_buffer");
                    // sourceParam["Source"] = source.Key;
                    //clevoHW.InvokeMethod("sensor_update_buffer", sourceParam, null);
                    string method_name = $"Fan{i+1}Info";
                    ManagementBaseObject result =  clevoHW.InvokeMethod(method_name, null, null);
                    if (result == null)
                    {
                        continue;
                    }
                    var value = (UInt32)result["Data"];
                    //var temp = (value & 0x00FF0000) >> 16;
                    var duty = (uint)Math.Round((value & 0xFF) / 255.0 * 100.0);
                    //var duty = (uint)(((value & 0xFF)/255)*100);
                    sensors[i].Value = rpms[i];
                    controllers[i].Value = duty;
                }
            }
            catch (ManagementException e)
            {
                logger.Log($"Updating sensor values failed: {e.Message}");
            }
        }

        ManagementObject GetInstance(ManagementScope scope, string path)
        {
            ManagementClass cls = new ManagementClass(scope.Path.Path, path, null);
            foreach (ManagementObject inst in cls.GetInstances())
            {
                return inst;
            }
            return null;
        }
        public static ManagementObject getClevoHW()
        {
            return clevoHW;
        }
    }
}
