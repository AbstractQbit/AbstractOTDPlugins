using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace AbstractOTDPlugins.VectorPredictor
{
    [PluginName("AbstractOTDPlugins.VectorPredictor")]
    public class VectorPredictorFilter : IFilter
    {
        [SliderPropertyAttribute("N samples to look ahead", 1, 4, 2)]
        public float lookAhead 
        {
            get { return nSamples; }
            set { nSamples = (int)value; }
        }

        FilterStage IFilter.FilterStage => FilterStage.PostTranspose;

        Vector2 IFilter.Filter(Vector2 point)
        {
            Vector2 pos = point;
            Vector2 delta = pos - lastPos;

            if (delta == Vector2.Zero) // improper fix for cursor teleporting
                return point;

            Vector2 dir = Vector2.Normalize(delta);
            float vel = delta.Length();

            Vector2 accel = delta - lastDelta;
            accel = Vector2.Transform(accel, new Matrix3x2(lastDir.Y, lastDir.X, -lastDir.X, lastDir.Y, 0, 0)); // Should have normal accel on x and tangent on y

            lastPos = pos;
            lastDelta = delta;
            lastDir = dir;

            for (var i = 0; i < nSamples; ++i)
            {
                delta += Vector2.Transform(accel, new Matrix3x2(dir.Y, -dir.X, dir.X, dir.Y, 0, 0));
                dir = Vector2.Normalize(delta);
                pos += delta;
            }

            return pos;
        }

        private int nSamples = 2;

        private Vector2 lastPos, lastDelta, lastDir;
    }
}
