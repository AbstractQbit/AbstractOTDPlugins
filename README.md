# AbstractOTDPlugins
A set of plugins for OpenTabletDriver.


## Radial Follow Smoothing
The Ultimate Smoothing Plugin, providing cursor stability while still allowing to hit fast jumps with ease

### Outer Radius
Outer radius defines the max distance the cursor can lag behind the actual reading.

Unit of measurement is mm.  
The value should be >= 0 and inner radius.  
If smoothing leak is used, defines the point at which smoothing will be reduced,  
instead of hard clamping the max distance between the tablet position and a cursor.

Default value is 0.5

### Inner Radius
Inner radius defines the max distance the tablet reading can deviate from the cursor without moving it.

This effectively creates a deadzone in which no movement is produced.  
Unit of measurement is mm.  
The value should be >= 0 and <= outer radius.  
Be aware that using a soft knee can implicitly reduce the actual inner radius.

Default value is 0.25

### Smoothing Coefficient
Smoothing coefficient determines how fast or slow the cursor will descend from the outer radius to the inner.

Possible value range is 0.0001..1, higher values mean more smoothing (slower descent to the inner radius).

Default value is 0.85

### Soft Knee Scale
Soft knee scale determines how soft the transition between smoothing inside and outside the outer radius is.

Possible value range is 0..10, higher values mean softer transition.  
The effect is somewhat logarithmic, i.e. most of the change happens closer to zero.  
Be aware that using a soft knee can implicitly reduce the actual inner radius.

Default value is 0

### Smoothing Leak Coefficient
Smoothing leak coefficient allows for input smooting to continue past outer radius at a reduced rate.

Possible value range is 0..1, 0 means no smoothing past outer radius, 1 means 100% of the smoothing gets through.

Default value is 0


## Bezier interpolator
Interpolator that spatially smoothes out drawn lines by interpreting pen path as a continuous spline. Raw data points work as control points and midpoints between them work as end points of each curve. 
