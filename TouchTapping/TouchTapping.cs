using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace TouchTapping
{
    [PluginName("Touch Tapping")]
    public class TouchTappingPlugin : IPositionedPipelineElement<IDeviceReport>
    {
        public PipelinePosition Position => PipelinePosition.PreTransform;

        public TouchTappingPlugin() : base() { }

        public event Action<IDeviceReport> Emit;

        bool[] pressed = { false, false };
        public void Consume(IDeviceReport value)
        {
            if (value is ITouchReport report)
            {
                foreach (int kn in Enumerable.Range(0, 2))
                {
                    if (pressed[kn] == false & !(report.Touches[kn] is null))
                    {
                        pressed[kn] = true;
                        HandlePress(kn, report.Touches[kn].Position);
                    }
                    if (pressed[kn] == true & (report.Touches[kn] is null))
                    {
                        pressed[kn] = false;
                        HandleRelease(kn);
                    }
                }
            }
            Emit?.Invoke(value);
        }

        void HandlePress(int kn, Vector2 pos)
        {
            var bind = Binds.Where(a => !ActiveBinds.Contains(a))
                            .MinBy(a => (a.Lastpos - pos).Length());
            VirtualKeyboard.Press(bind.Key);
            bind.Lastpos = pos;
            ActiveBinds[kn] = bind;
        }

        void HandleRelease(int kn)
        {
            VirtualKeyboard.Release(ActiveBinds[kn].Key);
            ActiveBinds[kn] = null;
        }

        Keybind[] Binds = new Keybind[]
        {
            new Keybind{Key="Z"},
            new Keybind{Key="C"},
        };

        Keybind[] ActiveBinds = new Keybind[2];

        [Resolved]
        public IDriver Driver;

        [Resolved]
        public IVirtualKeyboard VirtualKeyboard;
    }

    public class Keybind
    {
        public string Key;
        public Vector2 Lastpos = Vector2.Zero;
    }

}
