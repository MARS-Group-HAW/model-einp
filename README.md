# Elk Island National Park Model

The EINP Model is a simple [MARS](https://www.mars-group.org/docs/tutorial/intro#what-is-mars) model that simulates the bison, elk and moose population of Elk Island National Park.

## Quickstart
To use the model clone the repository by running:
```bash
git clone https://github.com/Red-Sigma/einp-model.git
```

Then go into the newly downloaded project and run the simmulation:
```bash
cd einp-model/GeoRasterBlueprint/
dotnet run -sm config.json
```

This will generate three files (`Bison_trips.geojson`, `Elk_trips.geojson`,	`Moose_trips.geojson`) with the simmulated koordinates of the animals.

### viewing the output
The generated movement trajectories can be visualized by going to [kepler.gl](https://kepler.gl/demo) and uploading the generated files mentioned above.
