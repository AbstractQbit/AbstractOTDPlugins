using System.Numerics;
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
            float alpha = (float)(reportStopwatch.Elapsed.TotalSeconds * Hertz / reportMsAvg);
            var lerp1 = Vector3.Lerp(targetOld, controlPoint, alpha);
            var lerp2 = Vector3.Lerp(controlPoint, target, alpha);
            var res = Vector3.Lerp(lerp1, lerp2, alpha);
            SyntheticReport.Position = new Vector2(res.X, res.Y);
            SyntheticReport.Pressure = SyntheticReport.Pressure == 0 ? 0 : (uint)(res.Z);
            return SyntheticReport;
        }

        public override void UpdateState(SyntheticTabletReport report)
        {
            SyntheticReport = new SyntheticTabletReport(report);

            emaTarget += emaWeight * (SyntheticReport.Position - emaTarget);

            controlPoint = controlPointNext;
            controlPointNext = new Vector3(emaTarget, SyntheticReport.Pressure);

            targetOld = target;
            target = Vector3.Lerp(controlPoint, controlPointNext, 0.5f);
        }

        [Property("Pre-interpolation smoothing factor"), DefaultPropertyValue(1.0f), ToolTip
        (
            "Sets the factor of pre-interpolation simple exponential smoothing (aka EMA weight).\n\n" +
            "Possible values are 0.01 .. 1\n" +
            "Factor of 1 means no smoothing is applied,\n" +
            "smaller values add smoothing."
        )]
        public float SmoothingFactor
        {
            get { return emaWeight; }
            set { emaWeight = System.Math.Clamp(value, 0.0f, 1.0f); }
        }
        private float emaWeight;

        private SyntheticTabletReport SyntheticReport;
        private Vector2 emaTarget;
        private Vector3 controlPointNext, controlPoint, target, targetOld;
    }
}
