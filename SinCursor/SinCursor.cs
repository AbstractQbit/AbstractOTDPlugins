using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace SinCursor
{
    [PluginName("sin(cursor)")]
    public class SinCursor : IPositionedPipelineElement<IDeviceReport>
    {
        public SinCursor() : base() { }
        public PipelinePosition Position => PipelinePosition.PostTransform;

        [Property("Amount"), DefaultPropertyValue(10.0f)]
        public float Amount
        {
            get { return amount; }
            set { amount = Math.Clamp(value, 0.0f, 1000.0f); }
        }
        private float amount;


        [Property("Freq"), DefaultPropertyValue(10.0f)]
        public float Freq
        {
            get { return freq; }
            set { freq = Math.Clamp(value, 0.0f, 1000.0f); }
        }
        private float freq;

        [Property("Elastic amplitude"), DefaultPropertyValue(true)]
        public bool ElasticAmp { get; set; }

        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                if (!vec2IsFinite(thing)) thing = report.Position;

                var delta = report.Position - thing;
                thing += Vector2.Normalize(delta) * Math.Max(delta.Length() - amount, 0);
                if (!ElasticAmp) delta = report.Position - thing;

                report.Position += MathF.Sin((float)stopwatch.Elapsed.TotalSeconds * MathF.PI * 2 * freq)
                                 * Vector2.Transform(delta, Matrix3x2.CreateRotation(MathF.PI / 2));
                value = report;
            }
            Emit?.Invoke(value);
        }

        private Vector2 thing;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch();
        private bool vec2IsFinite(Vector2 vec) => float.IsFinite(vec.X) & float.IsFinite(vec.Y);
    }
}
