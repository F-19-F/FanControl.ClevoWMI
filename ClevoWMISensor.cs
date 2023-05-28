using FanControl.Plugins;

namespace FanControl.ClevoWMI
{
    internal class ClevoWMISensor : IPluginSensor
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public float? Value { get; set; }

        public void Update()
        {
        }

        public uint Index { get; set; }

    }
}
