using System;
using System.Linq;
using Mars.Common;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using NetTopologySuite.Utilities;
using Position = Mars.Interfaces.Environments.Position;
using Mars.Interfaces.Annotations;

namespace ModelEINP.Model;

public abstract class AbstractAnimal : IPositionable, IAgent<LandscapeLayer> {

    [ActiveConstructor]
    public AbstractAnimal() {
    }
    
    [ActiveConstructor]
    public AbstractAnimal(
        LandscapeLayer landscapeLayer, 
        Perimeter perimeter,
        VegetationLayer vegetationLayer,
        VectorWaterLayer waterLayer,
        RasterWaterLayer rasterWaterLayer,
        Guid id,
        AnimalType animalType,
        bool isLeading,
        int herdId,
        double latitude, 
        double longitude,
        Position position) { 
        //Position = Position.CreateGeoPosition(longitude, latitude);
        Position = position;
        _landscapeLayer = landscapeLayer;
        _perimeter = perimeter;
        _vegetationLayer = vegetationLayer;
        _vectorWaterLayer = waterLayer;
        _rasterWaterLayer = rasterWaterLayer;
        _animalType = animalType;
        ID = id;
        _isLeading = isLeading;
        _herdId = herdId;
    }
    
    public Guid ID { get; set; }
    public abstract Position Position { get; set; }
    public abstract Position Target { get; set; }
    public double Bearing = 222.0;
    public const double Distance = 5000.0;
    public LandscapeLayer _landscapeLayer { get; set; }
    public abstract double Latitude { get; set; }
    public abstract double Longitude { get; set; }
    public Perimeter _perimeter { get; set; }
    public abstract double Hydration { get; set; }
    public abstract double Satiety { get; set; }
    public VectorWaterLayer _vectorWaterLayer { get; set; }
    public RasterWaterLayer _rasterWaterLayer { get; set; }
    public VegetationLayer _vegetationLayer { get; set; }
    
    public int _hoursLived;
    public AnimalType _animalType;
    public readonly int[] _reproductionYears = {2, 15};
    public bool _pregnant;
    public int _pregnancyDuration;
    public int _chanceOfDeath;
    public int Age { get; set; }
    public AnimalLifePeriod _LifePeriod;
    public MattersOfDeath MatterOfDeath { get; private set; }
    public bool IsAlive { get; set; } = true;

    [PropertyDescription (Name="isLeading")]
    protected bool _isLeading { get; }
    [PropertyDescription (Name="_herdId")]
    protected int _herdId { get; }
    
    public static Random _random = new ();
    
    /// <summary>
    /// Should be dependent from tick length
    /// </summary>
    [PropertyDescription]
    public int RandomWalkMaxDistanceInM { get; set;  }
    /// <summary>
    /// Should be dependent from tick length
    /// </summary>
    [PropertyDescription]
    public int RandomWalkMinDistanceInM { get; set;  }
    
    public const double MaxHydration = 100.0;
    public const double MaxSatiety = 100.0; 
    public const int MaxAge = 25;

    
    public void Init(LandscapeLayer layer) {
        _landscapeLayer = layer;

        var spawnPosition = new Position(Longitude, Latitude);
        _landscapeLayer = layer;
        
        if (_perimeter.IsPointInside(spawnPosition) && !_rasterWaterLayer.IsPointInside(spawnPosition)) {
            Position = Position.CreateGeoPosition(Longitude, Latitude);
        } else {
            throw new Exception($"Start point is not valid. Lon: {Longitude}, Lat: {Latitude}");
        }
    }
    
    public abstract void Tick();
    
    protected void DoRandomWalk(int numOfAttempts) {
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
        
        while (numOfAttempts > 0) {
            var randomDistance = _random.Next(RandomWalkMinDistanceInM, RandomWalkMaxDistanceInM);
            var randomDirection = _random.Next(0, 360);
            
            Target = Position.GetRelativePosition(randomDirection, randomDistance);
            
            if (_perimeter.IsPointInside(Target) && !_rasterWaterLayer.IsPointInside(Target)) {
                Position = Target;
                break;
            }
            numOfAttempts--;
        }
        
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
    }
    
    protected void LookForWaterAndDrink() {
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
        const int radius = 2000;
        var nearWaterSpots = _vectorWaterLayer.Explore(Position.PositionArray, radius)
            .ToList();

        if (!nearWaterSpots.Any()) return;
        
        var nearestWaterSpot = _vectorWaterLayer
            .Nearest(new []{Position.X, Position.Y})
            .VectorStructured
            .Geometry
            .Coordinates
            .Where(coordinate => _perimeter.IsPointInside(new Position(coordinate.X, coordinate.Y)))
            .OrderBy(coordinate => Position.DistanceInMTo(coordinate.X, coordinate.Y))
            .ToList();

        foreach (var point in nearestWaterSpot) {
            Target =  new Position(point.X, point.Y);
            
            if (_perimeter.IsPointInside(Target) && !_rasterWaterLayer.IsPointInside(Target)) {
                Position = Target;
                Hydration += 20;
                break;
            }
        }
        
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
    }

    protected void LookForFoodAndEat() {
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
        if (_vegetationLayer.IsPointInside(Position)) {
            var nearVegetationSpots = _vegetationLayer.Explore(Position, 20)
                .OrderByDescending(node => node.Node.Value)
                .ToList();

            foreach (var spot in nearVegetationSpots) {
                
                var targetX = spot.Node.NodePosition.X;
                var targetY = spot.Node.NodePosition.Y;

                var targetLon = _vegetationLayer.LowerLeft.X +
                                targetX * _vegetationLayer.CellWidth;
                var targetLat = _vegetationLayer.LowerLeft.Y +
                                targetY * _vegetationLayer.CellHeight;

                Target = new Position(targetLon, targetLat);

                if (_perimeter.IsPointInside(Target) && !_rasterWaterLayer.IsPointInside(Target)) {
                    Position = Target;
                    Satiety += 12;
                    break;
                }
            }
        }
        Assert.IsTrue(_perimeter.IsPointInside(Position) && !_rasterWaterLayer.IsPointInside(Position));
    }
    
    //every animals has different ways to consume food or hydration
    protected abstract void UpdateState();

    protected void BurnSatiety(double rate)
    {
        if (Satiety > 0) {
            if (Satiety > rate) {
                Satiety -= rate;
            } else {
                Satiety = 0;
            }
        }
    }

    protected void Dehydrate(double rate)
    {
        if (Hydration > 0) {
            if (Hydration > rate) {
                Hydration -= rate;
            } else {
                Hydration = 0;
            }
        }
    }
    public abstract void YearlyRoutine();

    public abstract AnimalLifePeriod GetAnimalLifePeriodFromAge(int age);
    
    public void Die(MattersOfDeath mannerOfDeath)
    {
        MatterOfDeath = mannerOfDeath;
        IsAlive = false;
        _landscapeLayer.removeAnimal(_landscapeLayer, this);
    }
    
}
