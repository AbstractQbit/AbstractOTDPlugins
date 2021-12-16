# Radial Follow Smoothing
The Ultimate Smoothing Plugin, providing cursor stability while still allowing to hit fast jumps with ease.  
To view how different parameters affect the operating curve, open [RadialFollow/RadialFollowGraphs.ggb](RadialFollow/RadialFollowGraphs.ggb) in a [GeoGebra calculator](https://www.geogebra.org/calculator).

## Outer Radius
Outer radius defines the max distance the cursor can lag behind the actual reading.

Unit of measurement is pixels or millimetres (screen space and tablet space modes respectively).  
The value should be >= 0 and inner radius.  
If smoothing leak is used, defines the point at which smoothing will be reduced,  
instead of hard clamping the max distance between the tablet position and a cursor.

Default value is 10.0 px / 1.0 mm

## Inner Radius
Inner radius defines the max distance the tablet reading can deviate from the cursor without moving it.

This effectively creates a deadzone in which no movement is produced.  
Unit of measurement is pixels or millimetres (screen space and tablet space modes respectively).  
The value should be >= 0 and <= outer radius.

Default value is 0.0 px / 0.0 mm

## Smoothing Coefficient
Smoothing coefficient determines how fast or slow the cursor will descend from the outer radius to the inner.

Possible value range is 0.0001..1, higher values mean more smoothing (slower descent to the inner radius).

Default value is 0.95

## Soft Knee Scale
Soft knee scale determines how soft the transition between smoothing inside and outside the outer radius is.

Possible value range is 0..100, higher values mean softer transition.  
The effect is somewhat logarithmic, i.e. most of the change happens closer to zero.

Default value is 2.0

## Smoothing Leak Coefficient
Smoothing leak coefficient allows for input smooting to continue past outer radius at a reduced rate.

Possible value range is 0..1, 0 means no smoothing past outer radius, 1 means 100% of the smoothing gets through.

Default value is 0.0

