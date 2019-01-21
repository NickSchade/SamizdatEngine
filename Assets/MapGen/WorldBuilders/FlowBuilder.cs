using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowBuilder {

    public float[,] Elevation;
    public float[,] Rain;
    public float[,] Flow;
    public float[,] Water;
    int xDim, yDim;

    public FlowBuilder(float[,] _Elevation, float[,] _Rain)
    {
        Elevation = _Elevation;
        Rain = _Rain;
        xDim = Elevation.GetLength(0);
        yDim = Elevation.GetLength(1);
        Flow = new float[xDim, yDim];
        Water = new float[xDim, yDim];
    }

    public void FlowStep(int iterations, Benchmark bench = null)
    {
        if (bench != null)
        {
            bench.StartBenchmark("Flow Step");
        }
        RainStep();
        for (int i = 0; i < iterations; i++)
        {
            AddWaterToFlow();
            float[,] flowStep = new float[xDim, yDim];
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    FlowProportional(ref flowStep, new MapLoc(x, y));
                }
            }
            Water = flowStep;
        }
        if (bench != null)
        {
            bench.EndBenchmark("Flow Step");
        }
    }
    public void FlowProportional(ref float[,] flowStep, MapLoc l)
    {
        Dictionary<MapLoc, float> neighbors = MapUtil.GetValidNeighbors(Elevation, l, false); // Flow Proportional
        float localHeight = Elevation[l.x, l.y];
        Dictionary<MapLoc, float> lowerNeighbors = new Dictionary<MapLoc, float>();
        float fDiff;
        float totalDiff = 0f;
        foreach (KeyValuePair<MapLoc, float> n in neighbors)
        {
            if (n.Value < localHeight)
            {
                fDiff = localHeight - n.Value;
                lowerNeighbors[n.Key] = fDiff;
                totalDiff += fDiff;
            }
        }
        
        if (lowerNeighbors.Count > 0)
        {
            foreach (KeyValuePair<MapLoc, float> n in lowerNeighbors)
            {
                flowStep[n.Key.x, n.Key.y] += Water[l.x, l.y] * n.Value / totalDiff;
            }
        }
        else
        {
            float newElev = MapUtil.GetNeighborAverage(Elevation, l);
            newElev = (newElev + Elevation[l.x, l.y]) / 2f;
            Elevation[l.x, l.y] = newElev;
        }
    }
    private void AddWaterToFlow()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Flow[x, y] += Water[x, y];
            }
        }
    }
    private void RainStep()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Water[x, y] += Rain[x, y];
            }
        }
    }

}
