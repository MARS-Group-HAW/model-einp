using System;
using Mars.Components.Layers;
using Position = Mars.Interfaces.Environments.Position;

namespace ModelEINP.Model;

/// <summary>
///     This raster layer provides information about biomass of animals etc.
/// </summary>
/// 
public class VegetationLayer : RasterLayer {
    public bool IsPointInside(Position coordinate) {
        return Extent.Contains(coordinate.X, coordinate.Y) && Math.Abs(GetValue(coordinate)) > 0.0;
    }
}