using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace RapidNibbler;


[PluginName("RapidNibbler")]
public class RapidNibbler : IPositionedPipelineElement<IDeviceReport>
{
    public RapidNibbler() : base() { }
    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Press Sensitivity"), DefaultPropertyValue(200), Unit("raw pressure units")]
    public long PressSens { get; set; }

    [Property("Release Sensitivity"), DefaultPropertyValue(300), Unit("raw pressure units")]
    public long ReleaseSens { get; set; }

    [Property("Press Counterfloat"), DefaultPropertyValue(100), Unit("raw pressure units per tick")]
    public long PressCFloat { get; set; }

    [Property("Release Counterfloat"), DefaultPropertyValue(20), Unit("raw pressure units per tick")]
    public long ReleaseCFloat { get; set; }


    [Property("Debug Toggles"), DefaultPropertyValue(false)]
    public bool DebugToggles { get; set; }

    [Property("Debug Spam"), DefaultPropertyValue(false)]
    public bool DebugSpam { get; set; }

    public event Action<IDeviceReport?>? Emit;

    bool currPressState = false;
    long currThreshold = 0;

    public void Consume(IDeviceReport? value)
    {
        if (value is ITabletReport report)
        {
            var deltaToThreshold = report.Pressure - currThreshold;

            // trigger threshold crossing
            if (currPressState == false)
            {
                if (deltaToThreshold > 0) // crossed trigger thresh
                {
                    if (DebugToggles)
                        Log.Debug("RapidNibbler", $"Pressed! {report.Pressure}, currThreshold {currThreshold} changed to {report.Pressure - ReleaseSens}");
                    currPressState = true;
                    currThreshold = report.Pressure - ReleaseSens;
                }
                else // tighten pressure trigger
                {
                    currThreshold = Math.Min(report.Pressure + PressSens, currThreshold + ReleaseCFloat);
                }
            }
            else
            {
                if (deltaToThreshold <= 0) // crossed release thresh
                {
                    if (DebugToggles)
                        Log.Debug("RapidNibbler", $"Released! {report.Pressure}, currThreshold {currThreshold} changed to {report.Pressure + PressSens}");
                    currPressState = false;
                    currThreshold = report.Pressure + PressSens;
                }
                else // tighten release trigger
                {
                    currThreshold = Math.Max(report.Pressure - ReleaseSens, currThreshold - PressCFloat);
                }
            }

            currThreshold = Math.Max(0, currThreshold);

            if (DebugSpam)
                Log.Debug("RapidNibbler", $"Pressure {report.Pressure}, currThreshold {currThreshold}, currPressState {currPressState}");

            if (currPressState == false)
                report.Pressure = 0;
        }
        Emit?.Invoke(value);
    }

    private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch();
}
