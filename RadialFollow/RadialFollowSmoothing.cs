using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace RadialFollow
{
    [PluginName("AbstractQbit's Radial Follow Smoothing")]
    public class RadialFollowSmoothing : IPositionedPipelineElement<IDeviceReport>
    {
        public PipelinePosition Position => PipelinePosition.PreTransform;

        [Property("Outer Radius"), DefaultPropertyValue(50.0f), ToolTip
        (
            "Outer radius defines the max distance the cursor can lag behind the actual reading.\n\n" +
            "Units of measurement are raw points on the digitizer grid.\n" +
            "The value should be >= 0 and inner radius.\n" +
            "If smoothing leak is used, defines the point at which smoothing will be reduced,\n" +
            "instead of hard clamping the max distance between the tablet position and a cursor.\n\n" +
            "Default value is 50"
        )]
        public double OuterRadius
        {
            get { return rOuter; }
            set { rOuter = System.Math.Clamp(value, 0.0f, 1000000.0f); }
        }
        private double rOuter = 0;

        [Property("Inner Radius"), DefaultPropertyValue(25.0f), ToolTip
        (
            "Inner radius defines the max distance the tablet reading can deviate from the cursor without moving it.\n" +
            "This effectively creates a deadzone in which no movement is produced.\n\n" +
            "Units of measurement are raw points on the digitizer grid.\n" +
            "The value should be >= 0 and <= outer radius.\n" +
            "Be aware that using a soft knee can implicitly reduce the actual inner radius.\n\n" +
            "Default value is 25"
        )]
        public double InnerRadius
        {
            get { return rInner; }
            set { rInner = System.Math.Clamp(value, 0.0f, 1000000.0f); }
        }
        private double rInner = 0;

        [Property("Smoothing Coefficient"), DefaultPropertyValue(0.85f), ToolTip
        (
            "Smoothing coefficient determines how fast or slow the cursor will descend from the outer radius to the inner.\n\n" +
            "Possible value range is 0.0001..1, higher values mean more smoothing (slower descent to the inner radius).\n\n" +
            "Default value is 0.85"
        )]
        public double SmoothingCoefficient
        {
            get { return smoothCoef; }
            set { smoothCoef = System.Math.Clamp(value, 0.0001f, 1.0f); }
        }
        private double smoothCoef;

        [Property("Soft Knee Scale"), DefaultPropertyValue(0.0f), ToolTip
        (
            "Soft knee scale determines how soft the transition between smoothing inside and outside the outer radius is.\n\n" +
            "Possible value range is 0..10, higher values mean softer transition.\n" +
            "The effect is somewhat logarithmic, i.e. most of the change happens closer to zero.\n" +
            "Be aware that using a soft knee can implicitly reduce the actual inner radius.\n\n" +
            "Default value is 0"
        )]
        public double SoftKneeScale
        {
            get { return knScale; }
            set { knScale = System.Math.Clamp(value, 0.0f, 10.0f); updateDerivedParams(); }
        }
        private double knScale;

        [Property("Smoothing Leak Coefficient"), DefaultPropertyValue(0.0f), ToolTip
        (
            "Smoothing leak coefficient allows for input smooting to continue past outer radius at a reduced rate.\n\n" +
            "Possible value range is 0..1, 0 means no smoothing past outer radius, 1 means 100% of the smoothing gets through.\n\n" +
            "Default value is 0"
        )]
        public double SmoothingLeakCoefficient
        {
            get { return leakCoef; }
            set { leakCoef = System.Math.Clamp(value, 0.0f, 1.0f); }
        }
        private double leakCoef;

        public RadialFollowSmoothing() : base()
        {
        }

        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                Vector2 direction = report.Position - cursor;
                double distance = direction.Length();
                direction = Vector2.Normalize(direction);

                double rDyn = rOuterScaledFn(distance, xOffset, scaleComp);

                float distToMove = (float)Math.Max(distance - rDyn, 0);
                cursor = cursor + Vector2.Multiply(direction, distToMove);
                report.Position = cursor;
                value = report;
            }
            Emit?.Invoke(value);
        }

        Vector2 cursor;
        double xOffset, scaleComp;
        void updateDerivedParams()
        {
            if (knScale > 0.0001f)
            {
                xOffset = getXOffest();
                scaleComp = getScaleComp();
            }
            else
            {
                xOffset = -1;
                scaleComp = 1;
            }
        }

        /// Math functions

        double kneeFunc(double x) => x switch
        {
            < -3 => x,
            < 3 => Math.Log(Math.Tanh(Math.Exp(x)), Math.E),
            _ => 0,
        };

        double kneeScaled(double x) => knScale switch
        {
            > 0.0001f => knScale * kneeFunc(x / knScale) + 1,
            _ => x > 0 ? 1 : 1 + x,
        };

        double inverseTanh(double x) => Math.Log((1 + x) / (1 - x), Math.E) / 2;

        double inverseKneeScaled(double x) => knScale * Math.Log(inverseTanh(Math.Exp((x - 1) / knScale)), Math.E);

        double derivKneeScaled(double x)
        {
            var e = Math.Exp(x / knScale);
            var tanh = Math.Tanh(e);
            return (e - e * (tanh * tanh)) / tanh;
        }

        double getXOffest() => inverseKneeScaled(0);

        double getScaleComp() => derivKneeScaled(getXOffest());

        double leakedFn(double x, double offset, double scaleComp) => kneeScaled(x + offset) * (1 - leakCoef) + x * leakCoef * scaleComp;

        double smoothedFn(double x, double offset, double scaleComp) => leakedFn(x * smoothCoef / scaleComp, offset, scaleComp);

        double rInnerShiftedFn(double x, double offset, double scaleComp) => smoothedFn(x + (rInner * (1 - smoothCoef) / (rOuter * smoothCoef)), offset, scaleComp);

        double rOuterScaledFn(double x, double offset, double scaleComp) => rOuter * rInnerShiftedFn(x / rOuter, offset, scaleComp);
    }
}
