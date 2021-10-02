using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace RadialFollow
{
    [PluginName("AbstractQbit's Radial Follow Smoothing")]
    public class RadialFollowSmoothing : IPositionedPipelineElement<IDeviceReport>
    {
        public PipelinePosition Position => PipelinePosition.Pixels;

        [Property("Outer Radius"), DefaultPropertyValue(10.0f), Unit("px"), ToolTip
        (
            "Outer radius defines the max distance the cursor can lag behind the actual reading.\n\n" +
            "Unit of measurement is pixels.\n" +
            "The value should be >= 0 and inner radius.\n" +
            "If smoothing leak is used, defines the point at which smoothing will be reduced,\n" +
            "instead of hard clamping the max distance between the tablet position and a cursor.\n\n" +
            "Default value is 10.0 px"
        )]
        public double OuterRadius
        {
            get => radialCore.OuterRadius;
            set { radialCore.OuterRadius = value; }
        }

        [Property("Inner Radius"), DefaultPropertyValue(0.0f), Unit("px"), ToolTip
        (
            "Inner radius defines the max distance the tablet reading can deviate from the cursor without moving it.\n" +
            "This effectively creates a deadzone in which no movement is produced.\n\n" +
            "Unit of measurement is pixels.\n" +
            "The value should be >= 0 and <= outer radius.\n\n" +
            "Default value is 0.0 px"
        )]
        public double InnerRadius
        {
            get => radialCore.InnerRadius;
            set { radialCore.InnerRadius = value; }
        }

        [Property("Initial Smoothing Coefficient"), DefaultPropertyValue(0.95f), ToolTip
        (
            "Smoothing coefficient determines how fast or slow the cursor will descend from the outer radius to the inner.\n\n" +
            "Possible value range is 0.0001..1, higher values mean more smoothing (slower descent to the inner radius).\n\n" +
            "Default value is 0.95"
        )]
        public double SmoothingCoefficient
        {
            get => radialCore.SmoothingCoefficient;
            set { radialCore.SmoothingCoefficient = value; }
        }

        [Property("Soft Knee Scale"), DefaultPropertyValue(3.0f), ToolTip
        (
            "Soft knee scale determines how soft the transition between smoothing inside and outside the outer radius is.\n\n" +
            "Possible value range is 0..100, higher values mean softer transition.\n" +
            "The effect is somewhat logarithmic, i.e. most of the change happens closer to zero.\n\n" +
            "Default value is 3"
        )]
        public double SoftKneeScale
        {
            get => radialCore.SoftKneeScale;
            set { radialCore.SoftKneeScale = value; }
        }

        [Property("Smoothing Leak Coefficient"), DefaultPropertyValue(0.0f), ToolTip
        (
            "Smoothing leak coefficient allows for input smooting to continue past outer radius at a reduced rate.\n\n" +
            "Possible value range is 0..1, 0 means no smoothing past outer radius, 1 means 100% of the smoothing gets through.\n\n" +
            "Default value is 0"
        )]
        public double SmoothingLeakCoefficient
        {
            get => radialCore.SmoothingLeakCoefficient;
            set { radialCore.SmoothingLeakCoefficient = value; }
        }

        public RadialFollowSmoothing() : base()
        {
        }

        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                report.Position = radialCore.Filter(report.Position);
                value = report;
            }
            Emit?.Invoke(value);
        }

        RadialFollowCore radialCore = new RadialFollowCore();
    }
}
