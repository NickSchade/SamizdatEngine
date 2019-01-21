using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ElevationBuilder  {
    public int Dim, N;
    public float[,] Elevation;
	public ElevationBuilder(int _N)
    {
        // Currently, Dimensions must be W = H and W = 2^N + 1 to make midpoint displacement simpler
        // Later, we should build the midpoint displacement map, then stretch it to the appropriate shape 
        // OR build a bigger MPD map, then truncate to fit w and h
        N = _N;
        Dim = (int)Mathf.Pow(2, N) + 1;
        Elevation = new float[Dim, Dim];
    }
    public void SetElevationWithMidpointDisplacement(int iExpand, Benchmark bench = null)
    {
        if (!(bench == null))
        {
            bench.StartBenchmark("Midpoint Displacement");
        }
        MidpointDisplacement mpd = new MidpointDisplacement(N);
        Elevation = mpd.Elevation;
        MapUtil.TransformMapMinMax(ref Elevation, MapUtil.dNormalize);
        if (iExpand > 1)
        {
            ExpandSquare(iExpand, iExpand);
            Smooth();
        }
        if (!(bench == null))
        {
            bench.EndBenchmark("Midpoint Displacement");
        }
    }
    public void TrimToDimensions(int xDim, int yDim)
    {
        float[,] dElev = new float[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                dElev[x,y] = Elevation[x,y];
            }
        }
        Elevation = dElev;
    }
    public void ExpandSquare(int xE, int yE, bool bNormalize = true)
    {
        PrintDims();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        int xl = xDim * xE;
        int yl = yDim * yE;
        float[,] newElevation = new float[xl, yl];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                for (int xi = 0; xi < xE; xi++)
                {
                    for (int yi = 0; yi < yE; yi++)
                    {
                        int xp1 = x < xDim - 1 ? x + 1 : x;
                        int yp1 = y < yDim - 1 ? y + 1 : y;
                        float xLerpTop = Mathf.Lerp(Elevation[x, y], Elevation[xp1, y], (float)xi / (float)xE);
                        float xLerpBot = Mathf.Lerp(Elevation[x, yp1], Elevation[xp1, yp1], (float)xi / (float)xE);
                        float lerpVal = Mathf.Lerp(xLerpTop, xLerpBot, yi / yE);
                        int xind = x * xE + xi;
                        int yind = y * yE + yi;
                        newElevation[xind, yind] = lerpVal;   
                    }
                }
            }
        }
        if (bNormalize)
        {
            MapUtil.TransformMapMinMax(ref newElevation, MapUtil.dNormalize);
        }
        Elevation = newElevation;
        PrintDims();
    }
    public void Smooth(bool bNormalize = true)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float[,] newElevation = new float[xDim, yDim];
        
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                newElevation[x, y] = MapUtil.dSmooth(x, y, Elevation);
            }
        }        
        if (bNormalize)
        {
            MapUtil.TransformMapMinMax(ref newElevation, MapUtil.dNormalize);
        }
        Elevation = newElevation;
    }
    public void PrintDims()
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);

        Debug.Log("Elevation[" + xDim + "," + yDim + "]");
    }
}
