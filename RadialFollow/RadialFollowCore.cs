using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Timing;

namespace RadialFollow
{
    public class RadialFollowCore
    {

        public double OuterRadius
        {
            get { return rOuter; }
            set { rOuter = System.Math.Clamp(value, 0.0f, 1000000.0f); }
        }
        private double rOuter = 0;

        public double InnerRadius
        {
            get { return rInner; }
            set { rInner = System.Math.Clamp(value, 0.0f, 1000000.0f); }
        }
        private double rInner = 0;

        public double SmoothingCoefficient
        {
            get { return smoothCoef; }
            set { smoothCoef = System.Math.Clamp(value, 0.0001f, 1.0f); }
        }
        private double smoothCoef;

        public double SoftKneeScale
        {
            get { return knScale; }
            set { knScale = System.Math.Clamp(value, 0.0f, 100.0f); updateDerivedParams(); }
        }
        private double knScale;

        public double SmoothingLeakCoefficient
        {
            get { return leakCoef; }
            set { leakCoef = System.Math.Clamp(value, 0.0f, 1.0f); }
        }
        private double leakCoef;
        public float SampleRadialCurve(float dist) => (float)deltaFn(dist, xOffset, scaleComp);
        public double ResetMs = 50;
        public double GridScale = 1;

        Vector2 cursor;
        HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);

        public Vector2 Filter(Vector2 target)
        {
            Vector2 direction = target - cursor;
            float distToMove = SampleRadialCurve(direction.Length());
            direction = Vector2.Normalize(direction);
            cursor = cursor + Vector2.Multiply(direction, distToMove);

            // Catch NaNs and pen redetection
            if (!(float.IsFinite(cursor.X) & float.IsFinite(cursor.Y) & stopwatch.Restart().TotalMilliseconds < 50))
                cursor = target;

            return cursor;
        }

        double xOffset, scaleComp;
        void updateDerivedParams()
        {
            if (knScale > 0.0001f)
            {
                xOffset = getXOffest();
                scaleComp = getScaleComp();
            }
            else // Calculating them with functions would ause / by 0
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

        double rOuterAdjusted => GridScale * Math.Max(rOuter, rInner + 0.0001f);
        double rInnerAdjusted => GridScale * rInner;

        double leakedFn(double x, double offset, double scaleComp)
        => kneeScaled(x + offset) * (1 - leakCoef) + x * leakCoef * scaleComp;

        double smoothedFn(double x, double offset, double scaleComp)
        => leakedFn(x * smoothCoef / scaleComp, offset, scaleComp);

        double scaleToOuter(double x, double offset, double scaleComp)
        => (rOuterAdjusted - rInnerAdjusted) * smoothedFn(x / (rOuterAdjusted - rInnerAdjusted), offset, scaleComp);

        double deltaFn(double x, double offset, double scaleComp)
        => x > rInnerAdjusted ? x - scaleToOuter(x - rInnerAdjusted, offset, scaleComp) - rInnerAdjusted : 0;
    }
}