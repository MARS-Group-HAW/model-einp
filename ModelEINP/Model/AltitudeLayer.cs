using System;
using Mars.Components.Layers;
using Position = Mars.Interfaces.Environments.Position;

namespace ModelEINP.Model;

/// <summary>
///     This raster layer provides information about the height of an area.
/// </summary>
/// 
public class AltitudeLayer : RasterLayer {
    public bool IsPointInside(Position coordinate) {
        return Extent.Contains(coordinate.X, coordinate.Y) && GetValue(coordinate) > 0.0;
    }
    
    public double GetAltitudeAtPosition(Position coordinate) {
        if (!IsPointInside(coordinate)) {
            throw new Exception("Position not contained in heights data");
        }
        return GetValue(coordinate);
    }
}