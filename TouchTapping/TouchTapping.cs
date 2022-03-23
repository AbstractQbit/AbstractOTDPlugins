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
        [Property("Key 1"), DefaultPropertyValue("Z")]
        public string K1 { set; get; }
        [Property("Key 2"), DefaultPropertyValue("X")]
        public string K2 { set; get; }

        [BooleanProperty("Force full alt", "Keys always alternate (still only 2 simultaneous touches are allowed)"), DefaultPropertyValue(false)]
        public bool ForceAlternate { set; get; }

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

        int fullAltNextBind = 0;
        void HandlePress(int kn, Vector2 pos)
        {
            Keybind bind;
            if (ForceAlternate)
            {
                bind = Binds[fullAltNextBind];
                fullAltNextBind = fullAltNextBind == 0 ? 1 : 0;
            }
            else
            {
                bind = Binds.Where(a => !ActiveBinds.Contains(a))
                            .MinBy(a => (a.Lastpos - pos).Length());
            }
            VirtualKeyboard.Press(bind.Key);
            bind.Lastpos = pos;
            ActiveBinds[kn] = bind;
        }

        void HandleRelease(int kn)
        {
            VirtualKeyboard.Release(ActiveBinds[kn].Key);
            ActiveBinds[kn] = null;
        }

        [OnDependencyLoad]
        public void SetBinds()
        {
            if (!VirtualKeyboard.SupportedKeys.Contains(K1))
            {
                Log.WriteNotify("Touch Tapping", "K1 is set incorrectly");
                return;
            }
            if (!VirtualKeyboard.SupportedKeys.Contains(K2))
            {
                Log.WriteNotify("Touch Tapping", "K2 is set incorrectly");
                return;
            }
            Binds = new Keybind[]
            {
                new Keybind{Key=K1},
                new Keybind{Key=K2},
            };
        }

        Keybind[] Binds = new Keybind[]
        {
            new Keybind{Key="Z"},
            new Keybind{Key="X"},
        };

        Keybind[] ActiveBinds = new Keybind[2];

        [Resolved]
        public IVirtualKeyboard VirtualKeyboard;
    }

    public class Keybind
    {
        public string Key;
        public Vector2 Lastpos = Vector2.Zero;
    }

}
