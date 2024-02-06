using Mars.Components.Layers;
using Mars.Interfaces.Environments;

namespace ModelEINP.Model; 

/// <summary>
///     Represents water spots in the Elk Island National Park.
/// </summary>
public class RasterWaterLayer : RasterLayer {
    
    public bool IsPointInside(Position position) {
        return Extent.Contains(position.X, position.Y) && GetValue(position) == 0;
    }
    
}