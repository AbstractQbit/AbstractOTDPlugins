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

        public override PipelinePosition Position => PipelinePosition.PostTransform;

        [Property("Pre-interpolation smoothing factor"), DefaultPropertyValue(1.0f), ToolTip
        (
            "Sets the factor of pre-interpolation simple exponential smoothing (aka EMA weight).\n\n" +
            "Possible values are 0.01 .. 1\n" +
            "Factor of 1 means no smoothing is applied, smaller values add smoothing."
        )]
        public float SmoothingFactor
        {
            get { return emaWeight; }
            set { emaWeight = System.Math.Clamp(value, 0.0f, 1.0f); }
        }
        private float emaWeight;

        [Property("Tilt smoothing factor"), DefaultPropertyValue(1.0f), ToolTip
        (
            "For tablets that report tilt, sets the factor of simple exponential smoothing (aka EMA weight) for tilt values.\n\n" +
            "Possible values are 0.01 .. 1\n" +
            "Factor of 1 means no smoothing is applied, smaller values add smoothing."
        )]
        public float TiltSmoothingFactor
        {
            get { return tiltWeight; }
            set { tiltWeight = System.Math.Clamp(value, 0.0f, 1.0f); }
        }
        private float tiltWeight;

        protected override void UpdateState()
        {
            float alpha = (float)(reportStopwatch.Elapsed.TotalSeconds * Frequency / reportMsAvg);

            if (State is ITiltReport tiltReport)
            {
                tiltReport.Tilt = Vector2.Lerp(previousTiltTraget, tiltTraget, alpha);
                State = tiltReport;
            }

            if (State is ITabletReport report && PenIsInRange())
            {
                var lerp1 = Vector3.Lerp(previousTarget, controlPoint, alpha);
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
            if (State is ITiltReport tiltReport)
            {
                if (!vec2IsFinite(tiltTraget)) tiltTraget = tiltReport.Tilt;
                previousTiltTraget = tiltTraget;
                tiltTraget += tiltWeight * (tiltReport.Tilt - tiltTraget);
            }

            if (State is ITabletReport report)
            {
                var consumeDelta = (float)reportStopwatch.Restart().TotalMilliseconds;
                if (consumeDelta < 150)
                    reportMsAvg += ((consumeDelta - reportMsAvg) * 0.1f);

                emaTarget = vec2IsFinite(emaTarget) ? emaTarget : report.Position;
                emaTarget += emaWeight * (report.Position - emaTarget);

                controlPoint = controlPointNext;
                controlPointNext = new Vector3(emaTarget, report.Pressure);

                previousTarget = target;
                target = Vector3.Lerp(controlPoint, controlPointNext, 0.5f);
            }
            else OnEmit();
        }

        private Vector2 emaTarget, tiltTraget, previousTiltTraget;
        private Vector3 controlPointNext, controlPoint, target, previousTarget;
        private HPETDeltaStopwatch reportStopwatch = new HPETDeltaStopwatch();
        private float reportMsAvg = 5;

        private bool vec2IsFinite(Vector2 vec) => float.IsFinite(vec.X) & float.IsFinite(vec.Y);
    }
}
