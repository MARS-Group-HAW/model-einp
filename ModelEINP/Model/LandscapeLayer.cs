using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace ModelEINP.Model;

public class LandscapeLayer : AbstractLayer {
    
    #region Properties and Fields
    
    public static GeoHashEnvironment<AbstractAnimal> Environment { get; set; }

    private List<Bison> Bisons { get; set; }
    private List<Moose> Moose { get; set; }
    private List<Elk> Elks { get; set; }
    [PropertyDescription(Name = "Perimeter")]
    public Perimeter Fence { get; set; }

    private RegisterAgent _registerAgent;
    private UnregisterAgent _unregisterAgent;

    #endregion
    
    /// <summary>
    /// The LandscapeLayer registers the animals in the runtime system. In this way, the tick methods
    /// of the agents can be executed later. Then the expansion of the simulation area is calculated using
    /// the raster layers described in config.json. An environment is created with this bounding box.
    /// </summary>
    /// <param name="layerInitData"></param>
    /// <param name="registerAgentHandle"></param>
    /// <param name="unregisterAgentHandle"></param>
    /// <returns>true if the agents where registered</returns>
    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle) {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);
        _registerAgent = registerAgentHandle;
        _unregisterAgent = unregisterAgentHandle;

        // Calculate and expand extent
        var baseExtent = new Envelope(Fence.Extent.ToEnvelope());
        Console.WriteLine(new BoundingBox(baseExtent));

        // Create GeoHashEnvironment with the calculated extent
        Environment = GeoHashEnvironment<AbstractAnimal>.BuildByBBox(new BoundingBox(baseExtent), 1000);

        var agentManager = layerInitData.Container.Resolve<IAgentManager>();
        Bisons = agentManager.Spawn<Bison, LandscapeLayer>().ToList();
        Moose = agentManager.Spawn<Moose, LandscapeLayer>().ToList();
        Elks = agentManager.Spawn<Elk, LandscapeLayer>().ToList();
        
        return Bisons.Count + Moose.Count + Elks.Count > 0;
    }

    public void SpawnBison(LandscapeLayer landscapeLayer, Perimeter perimeter, 
        VegetationLayer vegetationLayer, VectorWaterLayer waterLayer, RasterWaterLayer rasterWaterLayer, AnimalType animalType, 
        bool isLeading, int herdId, double latitude, double longitude, Position position) {
        var newBison = new Bison(landscapeLayer, perimeter, vegetationLayer, waterLayer, rasterWaterLayer,
            Guid.NewGuid(), animalType, isLeading, herdId, latitude, longitude, position);
        Bisons.Add(newBison);
        _registerAgent(landscapeLayer, newBison);
    }
    
    public void SpawnElk(LandscapeLayer landscapeLayer, Perimeter perimeter, 
        VegetationLayer vegetationLayer, VectorWaterLayer waterLayer, RasterWaterLayer rasterWaterLayer, AnimalType animalType, 
        bool isLeading, int herdId, double latitude, double longitude, Position position) {
        var newElk = new Elk(landscapeLayer, perimeter, vegetationLayer, waterLayer, rasterWaterLayer,
            Guid.NewGuid(), animalType, isLeading, herdId, latitude, longitude, position);
        Elks.Add(newElk);
        _registerAgent(landscapeLayer, newElk);
    }
    
    public void SpawnMoose(LandscapeLayer landscapeLayer, Perimeter perimeter, 
        VegetationLayer vegetationLayer, VectorWaterLayer waterLayer, RasterWaterLayer rasterWaterLayer, AnimalType animalType, 
        bool isLeading, int herdId, double latitude, double longitude, Position position) {
        var newMoose = new Moose(landscapeLayer, perimeter, vegetationLayer, waterLayer, rasterWaterLayer,
            Guid.NewGuid(), animalType, isLeading, herdId, latitude, longitude, position);
        Moose.Add(newMoose);
        _registerAgent(landscapeLayer, newMoose);
    }

    public void removeAnimal(LandscapeLayer landscapeLayer, AbstractAnimal animal) {
        _unregisterAgent(landscapeLayer, animal);
        if (animal._animalType == AnimalType.BisonBull || animal._animalType == AnimalType.BisonCalf ||
            animal._animalType == AnimalType.BisonCow || animal._animalType == AnimalType.BisonCow) {
            Bisons.Remove((Bison)animal);
        } 
        else if (animal._animalType == AnimalType.ElkCalf || animal._animalType == AnimalType.ElkCow ||
              animal._animalType == AnimalType.ElkBull || animal._animalType == AnimalType.ElkNewborn) {
            Elks.Remove((Elk)animal);
        }
        else if (animal._animalType == AnimalType.MooseCalf || animal._animalType == AnimalType.MooseCow ||
                 animal._animalType == AnimalType.MooseBull || animal._animalType == AnimalType.MooseNewborn) {
            Moose.Remove((Moose)animal);
        }
    }
}
