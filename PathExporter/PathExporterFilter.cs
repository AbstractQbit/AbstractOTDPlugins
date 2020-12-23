using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace AbstractOTDPlugins.PathExporter
{
    [PluginName("AbstractOTDPlugins.PathExporter")]
    public class PathExporterFilter : IFilter
    {
        FilterStage IFilter.FilterStage => FilterStage.PreInterpolate;

        Vector2 IFilter.Filter(Vector2 point)
        {
            OutFile.WriteLine($"{point.X.ToString(invc)},{point.Y.ToString(invc)}");
            return point;
        }

        private static CultureInfo invc = CultureInfo.InvariantCulture;
        private StreamWriter OutFile = new StreamWriter("output.csv", false);

    }
}
