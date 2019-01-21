using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TemperatureBuilder  {
    
    public static float[,] BuildTemperature(float[,] Elevation, float seaLevel, float elevationWeight = 0.5f, Benchmark bench = null)
    {
        if (!(bench == null))
        {
            bench.StartBenchmark("Temperature");
        }
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float Center = (yDim - 1) / 2f;
        float[,] Temp = new float[xDim, yDim];
        float distanceFromCenterRatio;
        for (int y = 0; y < yDim; y++)
        {
            distanceFromCenterRatio = Mathf.Abs(Center - y)/Center;
            for (int x = 0; x < xDim; x++)
            {
                Temp[x, y] = GetTemp(Elevation[x, y], elevationWeight, seaLevel, distanceFromCenterRatio);
            }
        }
        MapUtil.TransformMapMinMax(ref Temp, MapUtil.dNormalize);
        if (!(bench == null))
        {
            bench.EndBenchmark("Temperature");
        }
        return Temp;
    }
    private static float GetTemp(float elevation, float elevationWeight, float seaLevel, float distanceFromCenterRatio)
    {
        float d = (1 - distanceFromCenterRatio);
        float e = elevation > seaLevel ? MapUtil.dNormalize(elevation, seaLevel, 1f, 1f, 0f) : 1f;
        if (true)
        {
            d = (float)Math.Pow(d,1.5);
            e = (float)Math.Pow(e,1.5);
        }
        float t = elevationWeight * e + (1 - elevationWeight) * d;
        return t;
    }
}
