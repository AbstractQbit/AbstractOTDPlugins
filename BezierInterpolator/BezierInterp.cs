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
            watch = new Stopwatch();
        }
        public override SyntheticTabletReport Interpolate()
        {
            float alpha = (float)(watch.Elapsed.TotalMilliseconds * tabletRate / Hertz);
            var lerp1 = Vector3.Lerp(targetOld, controlPoint, alpha);
            var lerp2 = Vector3.Lerp(controlPoint, target, alpha);
            var res = Vector3.Lerp(lerp1, lerp2, alpha);
            SyntheticReport.Position = new Vector2(res.X, res.Y);
            SyntheticReport.Pressure = (uint)(res.Z);
            return SyntheticReport;
        }

        public override void UpdateState(SyntheticTabletReport report)
        {
            watch.Restart();
            SyntheticReport = new SyntheticTabletReport(report);

            controlPoint = controlPointNext;
            controlPointNext = new Vector3(SyntheticReport.Position, SyntheticReport.Pressure);

            targetOld = target;
            target = Vector3.Lerp(controlPoint, controlPointNext, 0.5f);
        }

        [Property("Native report rate")]
        public float tabletRate { get; set; }

        private SyntheticTabletReport SyntheticReport;
        private Stopwatch watch;
        private Vector3 controlPointNext, controlPoint, target, targetOld;
    }
}
