using Mars.Components.Layers;
using System.IO;
using System;
using ServiceStack;

namespace ModelEINP.Model;

public class TemperatureLayer: AbstractLayer
{
    private string[] _temps;
    
    #region Constructor
    public TemperatureLayer()
    {
        String path = Directory.GetCurrentDirectory() + "/Resources/open-meteo-53.60N112.93W736m.csv";
        try
        {
            _temps = File.ReadAllLines(path);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading temperature file: "+ e.Message);
        }
    }
    #endregion

    public double GetTemperature(long tick)
    {
        // + 4 because first 4 lines are discarded
        string[] parsed = _temps[tick + 4].Split(',');
        return parsed[1].ToDouble();
    }
}