using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;

/// <summary>
/// TO DO: MapGen - Continents: Create Map by running mapgen multiple times, creating a different continent each time
/// TO DO: Elevation - Fill Depressions: Planchon-Darboux algorithm
/// TO DO: Elevation - Add Spikey Blob Masks: https://azgaar.wordpress.com/2017/04/01/heightmap/
/// TO DO: MapPainter - Display View that fills in different colors for different areas
/// TO DO: Intermediate Colors - find a simple package w/ 20-30 colors instead of only 8.
/// TO DO: Animate Generation - waits are tough in unity, so this is hard until i learn that
/// TO DO: Simple City growth algorithm: each city w/ an empty neighbor builds a new city at that location which maximizes resources [f(waterFlux,Temp)] and minimizes costs [f(distance from new loc to all existing cities)]
/// </summary>


public enum WaterBodyPrefence { None, Islands, Continent, Lakes, Coast };

public class MapGen
{
    public Benchmark bench;
    public int xDim, yDim, iExpand;
    public float[,] Elevation, Rain, WaterFlux, Temperature, WindMagnitude, Regions, Flatness, Fertility, Harbor;
    public Vec[,] WindVector, Downhill;
    public float seaLevel, riverLevel, iceLevel;
    public float percentSea, percentRiver;
    public ElevationBuilder elevationBuilder;
    public RainBuilder RainBuilder;
    public FlowBuilder flowBuilder;

    WaterBodyPrefence prefWaterBody = WaterBodyPrefence.Continent;

    // Use this for initialization
    public MapGen(int _xDim, int _yDim, int _iExpand = 1, float _percentSea = 0.5f, float _percentRiver = 0.01f)
    {
        xDim = _xDim;
        yDim = _yDim;
        iExpand = _iExpand;
        seaLevel = 0.5f;
        riverLevel = 0.5f;
        iceLevel = 0.3f;
        percentRiver = _percentRiver;
        percentSea = _percentSea;

        Elevation = new float[xDim, yDim];
        WaterFlux = new float[xDim, yDim];
        Rain = new float[xDim, yDim];
        Temperature = new float[xDim, yDim];
        WindMagnitude = new float[xDim, yDim];
        Regions = new float[xDim, yDim];
        Flatness = new float[xDim, yDim];
        Fertility = new float[xDim, yDim];
        Harbor = new float[xDim, yDim];
    }

    // GENERATION
    public void GenerateMap()
    {
        bench = new Benchmark("Generate Map");
        Benchmark outerBench = new Benchmark("Enitre Gen");
        outerBench.StartBenchmark("Entire Gen");

        CreateElevation();

        ApplyPreferencesWaterBody();

        CreateElevationAdjustments(35,0.5f);

        CreateRain(RainMethod.Wind);

        CreateFluxErosion(3,30);

        //PaintRegions();

        bench.WriteBenchmarkToDebug();
        outerBench.EndBenchmark("Entire Gen");
        outerBench.WriteBenchmarkToDebug();
    }
    public void ApplyPreferencesWaterBody()
    {
        MapUtil.TransformApplyPreferencesWaterBody(ref Elevation, seaLevel, prefWaterBody, bench);
        SetTemperature();

    }
    public void CreateElevation()
    {
        Debug.Log("Creating Elevation of Dimensions [" + xDim + "," + yDim + "]");
        elevationBuilder = new ElevationBuilder(MapUtil.nFromDims(xDim,yDim));
        elevationBuilder.SetElevationWithMidpointDisplacement(iExpand, bench: bench);
        //elevationBuilder.TrimToDimensions(xDim, yDim);
        Elevation = elevationBuilder.Elevation;

        xDim = Elevation.GetLength(0);
        yDim = Elevation.GetLength(1);
        Debug.Log("Creating Elevation of Dimensions [" + xDim + "," + yDim + "]");
        WaterFlux = new float[xDim, yDim];
        Rain = new float[xDim, yDim];
        Temperature = new float[xDim, yDim];
        WindMagnitude = new float[xDim, yDim];
        Regions = new float[xDim, yDim];
        Flatness = new float[xDim, yDim];
        Fertility = new float[xDim, yDim];
        Harbor = new float[xDim, yDim];
        SetTemperature();
    }
    public void CreateElevationAdjustments(int iterDepression = 25, float waterPercent = 0.5f)
    {
        ResolveDepression(iterDepression, waterPercent);
        SetTemperature(); // Temp now in case we need it for rain
    }
    public void ResolveDepression(int iterDepression = 35, float waterPercent = 0.5f)
    {
        MapUtil.TransformResolveDepressions(ref Elevation, iterDepression, bench, false);
        MapUtil.TransformEqualizeMapByLevel(ref seaLevel, ref Elevation, waterPercent, bench);
    }
    public void SetTemperature()
    {
        Temperature = TemperatureBuilder.BuildTemperature(Elevation, seaLevel); 

    }
    public void CreateRain(RainMethod rainMethod)
    {
        RainBuilder = new RainBuilder(Elevation,Temperature,seaLevel);
        RainBuilder.BuildRain(rainMethod, bench);
        Rain = RainBuilder.Rain;
        WindMagnitude = RainBuilder.WindMagnitude;
        WindVector = RainBuilder.WindVectors;
    }
    public void CreateFluxErosion(int erosionIterations, int flowIterations)
    {
        flowBuilder = new FlowBuilder(Elevation, Rain);
        WaterFlux = flowBuilder.Flow;
        RiverErosionLoop(erosionIterations,flowIterations);
        MapUtil.TransformResolveDepressions(ref Elevation, 1, bench);
        Temperature = TemperatureBuilder.BuildTemperature(Elevation, seaLevel);
        Downhill = MapUtil.GetDownhillVectors(Elevation);
        SetFlatness();
        SetFertility();
        SetHarbor();
    }
    public void ErosionStep()
    {
        ErosionBuilder.HydraulicErosion(Elevation, WaterFlux, 0.3f);
    }
    public void FlowStep(int flowIterations)
    {
        flowBuilder.FlowStep(flowIterations, bench);
        MapUtil.TransformMap(ref WaterFlux, MapUtil.dExponentiate, 0.5f, bench);
        MapUtil.TransformMapMinMax(ref WaterFlux, MapUtil.dNormalize, bench);
        MapUtil.TransformMapMinMax(ref Elevation, MapUtil.dNormalize, bench);
    }
    public void RiverErosionLoop(int erosionIterations, int flowIterations)
    {
        for (int i = 0; i < erosionIterations; i++)
        {
            FlowStep(flowIterations);
            ErosionStep();
            MapUtil.TransformResolveDepressions(ref Elevation, 10, bench); 
        }
        MapUtil.TransformMapMinMax(ref Elevation, MapUtil.dNormalize, bench);
        MapUtil.TransformEqualizeMapByLevel(ref seaLevel, ref Elevation, percentSea, bench);
        MapUtil.TransformEqualizeMapByLevelAboveSea(ref riverLevel, ref WaterFlux, percentRiver, Elevation, seaLevel);
    }
    public void PaintRegions()
    {
        MapPainter mp = new MapPainter(Elevation, seaLevel);
        Regions = mp.BuildRegions();
    }
    public void SetFlatness()
    {
        Flatness = MapUtil.GetSlopeMap(Elevation);
        MapUtil.TransformMapMinMax(ref Flatness, MapUtil.dNormalize);
        MapUtil.TransformMap(ref Flatness, MapUtil.dInvert, 0f);
        MapUtil.TransformMap(ref Flatness, MapUtil.dExponentiate, 0.5f);
    }
    public void SetFertility()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                float fertility =  WaterFlux[x,y] * Flatness[x,y] * Mathf.Abs(0.5f - Temperature[x,y]);
                Fertility[x, y] = fertility;
            }
        }
        MapUtil.TransformMapMinMax(ref Fertility, MapUtil.dNormalize);
        MapUtil.TransformMap(ref Fertility, MapUtil.dExponentiate, 0.5f);
    }
    public void SetHarbor()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x, y] > seaLevel && WaterFlux[x, y] < riverLevel)
                {
                    int adjacentOceanCount = 0;
                    Dictionary<MapLoc, float> neighbors = MapUtil.GetValidNeighbors(Elevation, new MapLoc(x, y));
                    foreach (KeyValuePair<MapLoc, float> kvp in neighbors)
                    {
                        if (kvp.Value < seaLevel)
                        {
                            adjacentOceanCount++;
                        }
                    }
                    if (adjacentOceanCount == 0)
                    {
                        Harbor[x, y] = 0f;
                    }
                    else if (adjacentOceanCount == 1)
                    {
                        Harbor[x, y] = 1f;
                    }
                    else
                    {
                        Harbor[x, y] = 0.5f;
                    }
                }
            }
        }
    }
    
}
