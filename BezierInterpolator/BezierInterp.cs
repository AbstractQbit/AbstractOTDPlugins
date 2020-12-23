using System;
using System.Diagnostics;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet.Interpolator;
using OpenTabletDriver.Plugin.Timers;

namespace BezierInterpolator
{
    [PluginName("BezierInterpolator")]
    public class BezierInterp : Interpolator
    {
        public BezierInterp(ITimer scheduler) : base(scheduler)
        {
        }

        public override SyntheticTabletReport Interpolate()
        {
            float alpha = (float)((watch.Elapsed - lastReport).TotalMilliseconds * tabletRate / Hertz);
            var lerp1 = Vector3.Lerp(targetOld, controlPoint, alpha);
            var lerp2 = Vector3.Lerp(controlPoint, target, alpha);
            var res = Vector3.Lerp(lerp1, lerp2, alpha);
            SyntheticReport.Position = new Vector2(res.X, res.Y);
            SyntheticReport.Pressure = SyntheticReport.Pressure == 0 ? 0 : (uint)(res.Z);
            return SyntheticReport;
        }

        public override void UpdateState(SyntheticTabletReport report)
        {
            lastReport = watch.Elapsed;
            SyntheticReport = new SyntheticTabletReport(report);

            emaTarget += emaWeight * (SyntheticReport.Position - emaTarget);

            controlPoint = controlPointNext;
            controlPointNext = new Vector3(emaTarget, SyntheticReport.Pressure);

            targetOld = target;
            target = Vector3.Lerp(controlPoint, controlPointNext, 0.5f);
        }

        [SliderProperty("Native report rate", 1, 500, 133), Unit("Hz")]
        public float tabletRate { get; set; } = 133;

        [SliderProperty("EMA Weight", 0.1f, 1.0f, 1.0f)]
        public float emaWeight { get; set; } = 1;

        private SyntheticTabletReport SyntheticReport;
        private Stopwatch watch = Stopwatch.StartNew();
        private TimeSpan lastReport;
        private Vector2 emaTarget;
        private Vector3 controlPointNext, controlPoint, target, targetOld;
    }
}
