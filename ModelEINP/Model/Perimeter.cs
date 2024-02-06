using System.Linq;
using Mars.Components.Layers;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace ModelEINP.Model;

/// <summary>
///     A vector layer that ingests a .geojson vector file with the walkable area by the agents.
/// </summary>
public class Perimeter : VectorLayer {
    /// <summary>
    ///     Checks for the coordinate whether this point is inside the perimeter (the defined polygon).
    /// </summary>
    /// <param name="coordinate">The coordinate to check</param>
    /// <returns>
    ///     Returns true if the coordinate is inside the perimeter.
    /// </returns>
    public bool IsPointInside(Position coordinate) {
        return Features.First().VectorStructured.Geometry.Contains(new Point(coordinate.X, coordinate.Y));
    }
}