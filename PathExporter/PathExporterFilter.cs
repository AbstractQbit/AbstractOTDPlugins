using System;
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
        FilterStage IFilter.FilterStage => FilterStage.PostTranspose;

        Vector2 IFilter.Filter(Vector2 point)
        {
            OutFile.WriteLine($"{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            return point;
        }

        private StreamWriter OutFile = new StreamWriter("output.csv", false);

    }
}
