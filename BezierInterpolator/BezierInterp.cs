using System;
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
            float alpha = (float)(counter++ * tabletRate / Hertz);
            var lerp1 = Vector2.Lerp(targetOld, controlPoint, alpha);
            var lerp2 = Vector2.Lerp(controlPoint, target, alpha);
            SyntheticReport.Position = Vector2.Lerp(lerp1, lerp2, alpha);
            return SyntheticReport;
        }

        public override void UpdateState(SyntheticTabletReport report)
        {
            SyntheticReport = new SyntheticTabletReport(report);
            controlPoint = controlPointNext;
            controlPointNext = SyntheticReport.Position;
            targetOld = target;
            target = Vector2.Lerp(controlPoint, controlPointNext, 0.5f);
            counter = 0;
        }

        [Property("Native report rate")]
        public float tabletRate { get; set; }

        private SyntheticTabletReport SyntheticReport;
        private int counter = 0;
        private Vector2 controlPointNext, controlPoint, target, targetOld;
    }
}
