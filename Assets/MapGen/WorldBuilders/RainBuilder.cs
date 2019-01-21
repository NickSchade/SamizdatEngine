using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public struct Vec
{
    public float x, y;
    public Vec(float _x, float _y)
    {
        x = _x;
        y = _y;
    }
    public Vec GetRotatedVector(float rotationInDegrees)
    {
        float rad = DegreesToRadians(rotationInDegrees);
        float cs = (float)Math.Cos(rad);
        float sn = (float)Math.Sin(rad);
        float px = x * cs - y * sn;
        float py = x * sn + y * cs;
        return new Vec(px, py);
    }
    private float DegreesToRadians(float degrees)
    {
        return (float)(degrees * (Math.PI / 180));
    }
}

public enum RainMethod { Equal, Noise, Wind};


public class RainBuilder  {
    public float[,] Elevation, Temperature, Rain, WindMagnitude;
    public List<Vec>[,] WindVectorsList;
    public Vec[,] WindVectors;
    public int xDim, yDim;
    public float seaLevel;

    public Wind[,] wind;

	public RainBuilder(float[,] _Elevation, float[,] _Temperature, float _seaLevel)
    {
        Elevation = _Elevation;
        Temperature = _Temperature;
        xDim = Elevation.GetLength(0);
        yDim = Elevation.GetLength(1);
        seaLevel = _seaLevel;
        Rain = new float[xDim, yDim];
        WindVectors = new Vec[xDim, yDim];
        WindVectorsList = new List<Vec>[xDim, yDim];
        WindMagnitude = new float[xDim, yDim];
    }

    public void BuildRain(RainMethod pm, Benchmark bench = null)
    {
        Benchmark.Start(bench, "Rain"); // etc
        if (pm == RainMethod.Equal)
        {
            BuildRainByEqual();
        }
        else if (pm == RainMethod.Noise)
        {
            BuildRainByNoise();
        }
        else if (pm == RainMethod.Wind)
        {
            WindSpawn ws = WindSpawn.TradeLinear;
            WindRainType rt = WindRainType.HeatBasedContinuous;
            WindEvaporationType et = WindEvaporationType.WaterAndHeat;
            WindTurnType wt = WindTurnType.TerrainBased;
            BuildRainByWind(1000,ws,rt,et,wt,bench);
            MapUtil.TransformMapMinMax(ref Rain, MapUtil.dNormalize);
        }
        if (bench != null)
        {
            bench.EndBenchmark("Rain");
        }
    }
    private void BuildRainByEqual()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Rain[x, y] = 1f;
            }
        }
    }
    private void BuildRainByNoise()
    {
        MidpointDisplacement mpd = new MidpointDisplacement(MapUtil.nFromDims(xDim,yDim));
        Rain = mpd.Elevation;
        MapUtil.TransformMapMinMax(ref Rain, MapUtil.dNormalize);
    }
    private void BuildRainByWind(int maxIter, WindSpawn windSpawn, WindRainType rainType, WindEvaporationType evapType, WindTurnType windTurnType, Benchmark bench = null)
    {
        Benchmark.Start(bench, "Wind");
        List<Wind> winds = new List<Wind>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                WindVectorsList[x, y] = new List<Vec>();
                Wind w = new Wind(x, y, Elevation, windSpawn);
                WindVectorsList[x, y].Add(new Vec(w.velocity.x, w.velocity.y));
                winds.Add(w);
            }
        }
        int iter = 0;
        int numberOfWinds = winds.Count;
        while (iter < maxIter)
        {
            numberOfWinds = winds.Count;
            if (numberOfWinds == 0)
            {
                break;
            }
            else
            {
                BuildRainByWindWorkhorse(ref winds, windSpawn, rainType, evapType, windTurnType);
            }
            iter++;
        }
        MapUtil.TransformMap(ref Rain, MapUtil.dExponentiate, 0.125f);
        TabulateWindflow();
        if (bench != null)
        {
            bench.EndBenchmark("Wind");
        }
    }
    private void BuildRainByWindWorkhorse(ref List<Wind> winds, WindSpawn windSpawn, WindRainType rainType, WindEvaporationType evapType, WindTurnType windTurnType)
    {
        //  Combine Winds at the same location
        CombineWinds(ref winds, windSpawn);
        // Add to WindFlow
        foreach (Wind w in winds)
        {
            WindVectorsList[w.approxMapLoc.x, w.approxMapLoc.y].Add(new Vec(w.velocity.x, w.velocity.y));
        }
        //  Move wind in the direction vector
        foreach (Wind w in winds)
        {
            w.Blow(Elevation, Temperature);
        }
        //  If the wind moves off the map, remove it
        RemoveOffWindMaps(ref winds);
        //  Rain
        foreach (Wind w in winds)
        {
            w.Rain(rainType, ref Rain, Elevation, seaLevel);
        }
        // Evaporate
        foreach (Wind w in winds)
        {
            w.Evaporate(evapType, Elevation, Temperature, seaLevel);
        }
        // Turn Wind
        foreach (Wind w in winds)
        {
            w.TurnWind(windTurnType, Elevation, seaLevel);
        }
    }
    private void RemoveOffWindMaps(ref List<Wind> winds)
    {
        List<Wind> newWinds = new List<Wind>();
        for (int i = 0; i < winds.Count; i++)
        {
            if (winds[i].WindOnMap == true)
            {
                newWinds.Add(winds[i]);
            }
        }
        winds = newWinds;
    }
    private void CombineWinds(ref List<Wind> winds, WindSpawn ws, bool addNewWinds = false)
    {
        List<Wind>[,] stackedWindsList = new List<Wind>[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                stackedWindsList[x, y] = new List<Wind>();
            }
        }
        foreach (Wind w in winds)
        {
            stackedWindsList[w.approxMapLoc.x, w.approxMapLoc.y].Add(w);
        }
        List<Wind> newWinds = new List<Wind>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (addNewWinds)
                {
                    stackedWindsList[x, y].Add(new Wind(x, y, Elevation, ws));
                }
                if (stackedWindsList[x,y].Count > 0)
                {
                    newWinds.Add(CombineWindsAtMapLocation(stackedWindsList[x, y]));
                }
            }
        }
        winds = newWinds;
    }
    private Wind CombineWindsAtMapLocation(List<Wind> windsAtMapLocation)
    {
        List<float> xl = new List<float>();
        List<float> yl = new List<float>();
        List<float> xv = new List<float>();
        List<float> yv = new List<float>();
        List<float> water = new List<float>();
        foreach (Wind w in windsAtMapLocation)
        {
            xl.Add(w.actualMapLoc.x);
            yl.Add(w.actualMapLoc.y);
            xv.Add(w.velocity.x);
            yv.Add(w.velocity.y);
            water.Add(w.Water);
        }
        float _x = xl.Average();
        float _y = yl.Average();
        float _xv = xv.Average();
        float _yv = yv.Average();
        float _water = water.Average();
        return new Wind(_x, _y, _xv, _yv, _water, Elevation);
    }
    private void TabulateWindflow()
    {
        float[,] xa = new float[xDim, yDim];
        float[,] ya = new float[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                List<float> xl = new List<float>();
                List<float> yl = new List<float>();
                foreach (Vec v in WindVectorsList[x, y])
                {
                    xl.Add(v.x);
                    yl.Add(v.y);
                }
                float fx = xl.Sum();
                float fy = yl.Sum();
                xa[x, y] = fx;
                ya[x, y] = fy;
                WindMagnitude[x, y] = (float)Math.Sqrt(fx * fx + fy * fy);
            }
        }
        MapUtil.TransformMapMinMax(ref WindMagnitude, MapUtil.dNormalize);
        MapUtil.TransformMap(ref WindMagnitude, MapUtil.dExponentiate, 0.25f);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                WindVectors[x, y] = new Vec(xa[x,y], ya[x,y]);
            }
        }
    }
}

public enum WindSpawn {Easterly, TradeSine, TradeLinear };
public enum WindEvaporationType { WaterOnly, WaterAndHeat}
public enum WindRainType { UphillDiscrete, HeatBasedDiscrete, UphillContinuous, HeatBasedContinuous};
public enum WindTurnType { None, RandomWalk, TerrainBased};

public class Wind
{
    public Vec velocity;
    public Vec actualMapLoc;
    public MapLoc approxMapLoc;
    public float Water, deltaHeight, deltaTemp, seaLevel;
    public int xDim, yDim;
    public bool WindOnMap, WindOverWater;
    public Wind(float _x, float _y, float[,] Elevation, WindSpawn ws)
    {
        Water = 0.1f;
        deltaHeight = 0f;
        deltaTemp = 0f;
        actualMapLoc = new Vec(_x, _y);
        approxMapLoc = new MapLoc((int)_x, (int)_y);
        WindOnMap = true;
        xDim = Elevation.GetLength(0);
        yDim = Elevation.GetLength(1);
        if (ws == WindSpawn.Easterly)
        {
            velocity = new Vec(1, 0);
        }
        else if (ws == WindSpawn.TradeSine)
        {
            velocity = TradeWindSine();
        }
        else if (ws == WindSpawn.TradeLinear)
        {
            velocity = TradeWindLinear();
        }
    }
    public Wind(float _x, float _y, float _xv, float _yv, float _water, float[,] Elevation)
    {
        actualMapLoc = new Vec(_x, _y);
        approxMapLoc = new MapLoc((int)_x, (int)_y);
        velocity = new Vec(_xv, _yv);
        Water = _water;
        xDim = Elevation.GetLength(0);
        yDim = Elevation.GetLength(1);
    }
    public void Blow(float[,] Elevation, float[,] Temperature)
    {
        deltaHeight = Elevation[approxMapLoc.x, approxMapLoc.y];
        deltaTemp = Temperature[approxMapLoc.x, approxMapLoc.y];
        actualMapLoc = new Vec(actualMapLoc.x + velocity.x, actualMapLoc.y + velocity.y);
        approxMapLoc = new MapLoc((int)actualMapLoc.x, (int)actualMapLoc.y);
        WindOnMap = true;
        WindOnMap = (actualMapLoc.x < 0 || actualMapLoc.x >= xDim) ? false : WindOnMap;
        WindOnMap = (approxMapLoc.x < 0 || approxMapLoc.x >= xDim) ? false : WindOnMap;
        WindOnMap = (actualMapLoc.y < 0 || actualMapLoc.y >= yDim) ? false : WindOnMap;
        WindOnMap = (approxMapLoc.y < 0 || approxMapLoc.y >= yDim) ? false : WindOnMap;
        if (WindOnMap)
        {
            deltaHeight = Elevation[approxMapLoc.x, approxMapLoc.y] - deltaHeight;
            deltaTemp = Temperature[approxMapLoc.x, approxMapLoc.y] - deltaTemp;
        }
    }
    public void Rain(WindRainType rt, ref float[,] Rain, float [,] Elevation, float seaLevel)
    {
        if (rt == WindRainType.UphillDiscrete)
        {
            if (deltaHeight > 0f && Elevation[approxMapLoc.x,approxMapLoc.y] > seaLevel)
            {
                float rainAmount = 0.1f * Water;
                RainWorkhorse(ref Rain, rainAmount);
            }
        }
        else if (rt == WindRainType.HeatBasedDiscrete)
        {
            if (deltaTemp < -0f && Elevation[approxMapLoc.x, approxMapLoc.y] > seaLevel)
            {
                float rainAmount = 0.1f * Water;
                RainWorkhorse(ref Rain, rainAmount);
            }
        }
        else if (rt == WindRainType.UphillContinuous)
        {
            if (deltaHeight > 0f && Elevation[approxMapLoc.x, approxMapLoc.y] > seaLevel)
            {
                float rainAmount = deltaHeight * Water;
                RainWorkhorse(ref Rain, rainAmount);
            }
        }
        else if (rt == WindRainType.HeatBasedContinuous)
        {
            if (deltaTemp < -0f && Elevation[approxMapLoc.x, approxMapLoc.y] > seaLevel)
            {
                float rainAmount = -deltaTemp * Water;
                RainWorkhorse(ref Rain, rainAmount);
            }
            if (Water > 10)
            {
                float rainAmount = 0.1f * Water;
                RainWorkhorse(ref Rain, rainAmount);
            }
        }
    }
    private void RainWorkhorse(ref float[,] Rain, float rainAmount)
    {
        Water -= rainAmount;
        Rain[approxMapLoc.x, approxMapLoc.y] += Water;
    }
    public void Evaporate(WindEvaporationType et, float[,] Elevation, float[,] Temperature, float seaLevel)
    {
        if (et == WindEvaporationType.WaterOnly)
        {
            if (Elevation[approxMapLoc.x,approxMapLoc.y] < seaLevel)
            {
                Water += 1f;
            }
        }
        else if (et == WindEvaporationType.WaterAndHeat)
        {
            if (Elevation[approxMapLoc.x, approxMapLoc.y] < seaLevel)
            {
                Water += Temperature[approxMapLoc.x,approxMapLoc.y];
            }
        }
    }
    public void TurnWind(WindTurnType wt, float[,] Elevation, float seaLevel)
    {
        if (wt == WindTurnType.None)
        {
            // do nothing
        }
        else if (wt == WindTurnType.RandomWalk)
        {
            int r = UnityEngine.Random.Range(-1, 1);
            velocity = new Vec(velocity.x, r);
        }
        else if (wt == WindTurnType.TerrainBased)
        {
            if (Elevation[approxMapLoc.x, approxMapLoc.y] > seaLevel)
            {
                velocity.x -= deltaHeight;
                velocity.y -= deltaHeight;
                Vec lVec = velocity.GetRotatedVector(-90);
                Vec rVec = velocity.GetRotatedVector(90);
                MapLoc lMapLoc = new MapLoc((int)(Math.Max(1,actualMapLoc.x + lVec.x)), (int)Math.Max(1,actualMapLoc.y + lVec.y));
                MapLoc rMapLoc = new MapLoc((int)(Math.Max(1,actualMapLoc.x + rVec.x)), (int)Math.Max(1,actualMapLoc.y + rVec.y));
                try
                {
                    float fDiff = Elevation[lMapLoc.x, lMapLoc.y] - Elevation[rMapLoc.x, rMapLoc.y];
                    velocity.x += lMapLoc.x * fDiff;
                    velocity.y += lMapLoc.y * fDiff;
                }
                catch
                {
                    // ¯\_(ツ)_/¯
                }
            }
        }
    }
    private Vec TradeWindSine()
    {
        float yC = (yDim - 1) / 2f;
        float EquatorToPolePosition = Math.Abs(approxMapLoc.y - yC) / yC;
        float yAdjustedInput = MapUtil.dNormalize(EquatorToPolePosition, 0f, 1f, -0.499f, 0.999f);
        //float xDir = (float)Math.Tan(Math.PI * yAdjustedInput);
        //float yDir = (float)Math.Tan(Math.PI * 0.5f - Math.PI * yAdjustedInput);
        float xDir = (float)Math.Tan(yAdjustedInput);
        float yDir = (float)Math.Tan(0.5f - yAdjustedInput);

        //Debug.Log("Wind at " + approxMapLoc.y + "("+EquatorToPolePosition+") is "+xDir+","+yDir);
        return new Vec(xDir, yDir);
    }
    private Vec TradeWindLinear()
    {
        float yC = (yDim - 1) / 2f;
        float EquatorToPolePosition = Math.Abs(approxMapLoc.y - yC) / yC;
        EquatorToPolePosition = MapUtil.dNormalize(EquatorToPolePosition, 0f, 1f, -0.499f, 0.999f);
        float xDir, yDir;
        if (EquatorToPolePosition <= 0.0f)
        {
            float newLerp = MapUtil.dNormalize(EquatorToPolePosition, -0.5f, 0f);
            xDir = -1f + newLerp;
            yDir = -newLerp;
        }
        else if (EquatorToPolePosition <= 0.5f)
        {
            float newLerp = MapUtil.dNormalize(EquatorToPolePosition, 0f, 0.5f);
            xDir = newLerp;
            yDir = 1f - newLerp;
        }
        else
        {
            float newLerp = MapUtil.dNormalize(EquatorToPolePosition, 0.5f, 1.0f);
            xDir = -1f + newLerp;
            yDir = -newLerp;
        }
        //Debug.Log("Wind at " + approxMapLoc.y + "(" + EquatorToPolePosition + ") is " + xDir + "," + yDir);
        return new Vec(xDir, yDir);
    }
}










