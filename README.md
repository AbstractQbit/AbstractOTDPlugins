# AbstractOTDPlugins
A set of plugins for OpenTabletDriver.

## Bezier interpolator
Interpolator that spatially smoothes out drawn lines by interpreting pen path as a continuous spline. Raw data points work as control points and midpoints between them work as end points of each curve. 

## VectorPredictor
A filter that tries to predict cursor position based on current tangental and normal acceleration.

This is an old attempt at compensating for hardware smoothing, please use [Reconstructor](https://github.com/X9VoiD/VoiDPlugins/wiki/Reconstructor) instead.

## PathExporter
A passthrough filter that records pen movements to a csv file.
