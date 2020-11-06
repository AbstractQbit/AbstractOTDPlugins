using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
//using System.IO;

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

        Vector2 IFilter.Filter(Vector2 point)
        {
            pos = point;
            delta = pos - lastPos;

            dir = Vector2.Normalize(delta);
            accel = delta - lastDelta;
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

            //OutFile.WriteLine(stateToStr());

            if (float.IsNaN(pos.X) | float.IsNaN(pos.Y))
                return point;

            return pos;
        }

        private int nSamples = 2;

        private Vector2 pos, delta, dir, accel;
        private Vector2 lastPos, lastDelta, lastDir;

        //private String stateToStr()
        //{
        //    return $"pos = {pos}\n" +
        //        $"delta = {delta}\n" +
        //        $"dir = {dir}\n" +
        //        $"accel = {accel}\n" +
        //        $"lastPos = {lastPos}\n" +
        //        $"lastDelta = {lastDelta}\n" +
        //        $"lastDir = {lastDir}\n" +
        //        $"______\n";
        //}
        //private StreamWriter OutFile = new StreamWriter("output/out.txt", false);

        FilterStage IFilter.FilterStage => FilterStage.PostTranspose;
    }
}
