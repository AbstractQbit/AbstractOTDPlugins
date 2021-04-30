using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace BezierInterpolator
{
    [PluginName("BezierInterpolator")]
    public class BezierInterp : AsyncPositionedPipelineElement<IDeviceReport>
    {
        public BezierInterp() : base()
        {
        }

        protected override void UpdateState()
        {
            if (State is ITabletReport report & PenIsInRange())
            {
                float alpha = (float)(reportStopwatch.Elapsed.TotalSeconds * Frequency / reportMsAvg);
                var lerp1 = Vector3.Lerp(targetOld, controlPoint, alpha);
                var lerp2 = Vector3.Lerp(controlPoint, target, alpha);
                var res = Vector3.Lerp(lerp1, lerp2, alpha);
                report.Position = new Vector2(res.X, res.Y);
                report.Pressure = report.Pressure == 0 ? 0 : (uint)(res.Z);
                State = report;
                OnEmit();
            }
        }

        protected override void ConsumeState()
        {
            if (State is ITabletReport report)
            {
                var consumeDelta = (float)reportStopwatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150)
                    reportMsAvg += ((consumeDelta - reportMsAvg) * 0.1f);

                emaTarget += emaWeight * (report.Position - emaTarget);

                controlPoint = controlPointNext;
                controlPointNext = new Vector3(emaTarget, report.Pressure);

                targetOld = target;
                target = Vector3.Lerp(controlPoint, controlPointNext, 0.5f);
            }
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

        public override PipelinePosition Position => PipelinePosition.PostTransform;

        private float emaWeight;

        private Vector2 emaTarget;
        private Vector3 controlPointNext, controlPoint, target, targetOld;
        private HPETDeltaStopwatch reportStopwatch = new HPETDeltaStopwatch();
        private float reportMsAvg = 5;
    }
}
