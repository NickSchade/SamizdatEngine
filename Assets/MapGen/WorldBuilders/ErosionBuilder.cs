using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ErosionBuilder {
    public static void HydraulicErosion(float[,] Elevation, float[,] WaterFlux, float erosionRate, Benchmark bench = null)
    {
        if (bench != null)
        {
            bench.StartBenchmark("Erode");
        }
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Elevation[x, y] -= erosionRate * WaterFlux[x, y];
            }
        }
        if (bench != null)
        {
            bench.EndBenchmark("Erode");
        }
    }
    public static void ThermalErosion(float[,] Elevation, float talusAngle = 0.5f, int iterations = 1, Benchmark bench = null)
    {
        if (bench != null)
        {
            bench.StartBenchmark("Thermal Erosion");
        }

        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);

        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    ThermalWorkhorse(Elevation, new MapLoc(x,y), talusAngle);
                }
            }
            MapUtil.TransformMapMinMax(ref Elevation, MapUtil.dNormalize, bench);
        }
        
        if (bench != null)
        {
            bench.EndBenchmark("Thermal Erosion");
        }
    }
    public static void ThermalWorkhorse(float[,] Elevation, MapLoc l, float talusAngle)
    {
        Dictionary<MapLoc, float> n = MapUtil.GetValidNeighbors(Elevation, l); // Thermal Erosion
        float di;
        float dtotal = 0f;
        float dmax = float.NegativeInfinity;
        foreach (KeyValuePair<MapLoc, float> kvp in n)
        {
            di = Elevation[l.x, l.y] - Elevation[kvp.Key.x, kvp.Key.y];
            if (di > talusAngle)
            {
                dtotal += di;
                di = di > dmax ? dmax : di;
            }
        }
        float startingElev = Elevation[l.x, l.y];
        foreach (KeyValuePair<MapLoc, float> kvp in n)
        {
            di = startingElev - Elevation[kvp.Key.x, kvp.Key.y];
            float delta = 0.5f * (dmax - talusAngle) * di / dtotal;
            Elevation[kvp.Key.x, kvp.Key.y] += delta;
            Elevation[l.x, l.y] -= delta;
        }
    }
	
}
