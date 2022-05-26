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

        [Property("Amplitude"), DefaultPropertyValue(10.0f), Unit("px")]
        public float Amplitude
        {
            get { return amp; }
            set { amp = Math.Clamp(value, 0.0f, 1000.0f); }
        }
        private float amp;

        [Property("Elastic amplitude"), DefaultPropertyValue(true)]
        public bool ElasticAmp { get; set; }


        [Property("Frequency"), DefaultPropertyValue(10.0f), Unit("Hz")]
        public float Freq
        {
            get { return freq; }
            set { freq = Math.Clamp(value, 0.0f, 1000.0f); }
        }
        private float freq;

        [Property("Elastic frequency"), DefaultPropertyValue(false)]
        public bool ElasticFreq { get; set; }


        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                if (!vec2IsFinite(prevRadial)) prevRadial = report.Position;
                if (!vec2IsFinite(prevPos)) prevPos = report.Position;

                var deltaRadial = report.Position - prevRadial;
                var delta = report.Position - prevPos;

                phase = (phase + (float)stopwatch.Restart().TotalSeconds * freq * (ElasticFreq ? delta.Length() / amp: 1)) % 1f;

                prevRadial += Vector2.Normalize(deltaRadial) * Math.Max(deltaRadial.Length() - amp, 0);

                deltaRadial = report.Position - prevRadial;
                prevPos = report.Position;

                report.Position += MathF.Sin(phase * MathF.PI * 2)
                                 * Vector2.Transform(ElasticAmp ? delta : deltaRadial, Matrix3x2.CreateRotation(MathF.PI / 2));
                value = report;
            }
            Emit?.Invoke(value);
        }

        private Vector2 prevRadial, prevPos;
        private float phase = 0;  // 0..1
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch();
        private bool vec2IsFinite(Vector2 vec) => float.IsFinite(vec.X) & float.IsFinite(vec.Y);
    }
}
