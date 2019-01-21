using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public struct Loc
{
    public int x, y;
    public float v;
    public Loc(int _x, int _y)
    {
        x = _x;
        y = _y;
        v = 0f;
    }
}

public static class MapUtil  {

    public static void GetMapMaxMinValues(float[,] Elevation, out float maxVal, out float minVal)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        minVal = float.PositiveInfinity;
        maxVal = float.NegativeInfinity;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                maxVal = Elevation[x, y] > maxVal ? Elevation[x, y] : maxVal;
                minVal = Elevation[x, y] < minVal ? Elevation[x, y] : minVal;
            }
        }
    }
    public static void GetMapMaxIndices(float[,] Elevation, out int xMaxIndex, out int yMaxIndex)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float maxVal = float.NegativeInfinity;
        xMaxIndex = 0;
        yMaxIndex = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x, y] > maxVal)
                {
                    maxVal = Elevation[x, y];
                    xMaxIndex = x;
                    yMaxIndex = y;
                }
            }
        }
    }
    public static void GetMapMinIndices(float[,] Elevation, out int xMinIndex, out int yMinIndex)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float maxVal = float.PositiveInfinity;
        xMinIndex = 0;
        yMinIndex = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x, y] < maxVal)
                {
                    maxVal = Elevation[x, y];
                    xMinIndex = x;
                    yMinIndex = y;
                }
            }
        }
    }

    public static void TransformMap(ref float[,] Elevation, dTransform dFunc, float c, Benchmark bench = null)
    {
        if (bench != null)
        {
            bench.StartBenchmark("TransformMap:" + dFunc.ToString());
        }
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float[,] dElev = new float[xDim, yDim];
        float f;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                f = Elevation[x, y];
                f = dFunc(f,c);
                dElev[x, y] = f;
            }
        }
        Elevation = dElev;
        if (bench != null)
        {
            bench.EndBenchmark("TransformMap:" + dFunc.ToString());
        }
    }
    public delegate float dTransform(float f, float c);
    public static float dInvert(float f, float c)
    {
        return 1.0f - f;
    }
    public static float dExponentiate(float f, float c)
    {
        return (float)Math.Pow(f, c);
    }

    public static void TransformMapSpecialNormalize(ref float[,] Elevation, float[,] WaterFlux)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float[,] dElev = new float[xDim, yDim];
        float f;
        float minVal, maxVal;
        GetMapMaxMinValues(Elevation, out maxVal, out minVal);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                f = Elevation[x, y];
                f = dNormalize(f, minVal, maxVal);
                if (WaterFlux[x,y] > 0.0f)
                {
                    dElev[x, y] = f;
                }
            }
        }
        Elevation = dElev;
    }

    public static void TransformMapMinMax(ref float[,] Elevation, dTransformMinMax dFunc, Benchmark bm = null)
    {
        if (!(bm == null))
        {
            bm.StartBenchmark("TransformMapMinMax");
        }
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float[,] dElev = new float[xDim, yDim];
        float f;
        float minVal, maxVal;
        GetMapMaxMinValues(Elevation, out maxVal, out minVal);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                f = Elevation[x, y];
                f = dFunc(f, minVal, maxVal);
                dElev[x, y] = f;
            }
        }
        Elevation = dElev;
        if (!(bm == null))
        {
            bm.EndBenchmark("TransformMapMinMax");
        }
    }
    public delegate float dTransformMinMax(float f, float fMin, float fMax, float newMinVal = 0f, float newMaxVal = 1f);
    public static float dNormalize(float f, float oldMinVal, float oldMaxVal, float newMinVal = 0f, float newMaxVal = 1f)
    {
        return (((f - oldMinVal) * (newMaxVal - newMinVal)) / (oldMaxVal - oldMinVal)) + newMinVal;
    }

    public static void TransformMapWithMap(ref float[,] Elevation, dTransformWithMap dFunc)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float f;
        float minVal, maxVal;
        GetMapMaxMinValues(Elevation, out maxVal, out minVal);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                f = dFunc(x,y,Elevation);
                Elevation[x, y] = f;
            }
        }
    }
    public delegate float dTransformWithMap(int xIndex, int yIndex, float[,] Elevation);
    public static float dSmooth(int xIndex, int yIndex, float[,] Elevation)
    {
        float coeff = 0.5f; // value between 0 and 1; 0 doesnt change anything
        float nAvg = GetNeighborAverage(Elevation, new Loc(xIndex, yIndex));
        float r = coeff * nAvg + (1 - coeff) * Elevation[xIndex, yIndex];
        return r;
    }
    public static float dSmoothKeep1(int xIndex, int yIndex, float[,] Elevation)
    {
        float coeff = 0.5f; // value between 0 and 1; 0 doesnt change anything
        float nAvg = GetNeighborAverage(Elevation, new Loc(xIndex, yIndex));
        float r = coeff * nAvg + (1 - coeff) * Elevation[xIndex, yIndex];
        r = Elevation[xIndex, yIndex] == 1f ? 1f : r;
        return Math.Max(r, Elevation[xIndex, yIndex]);
    }
    public static void TransformMapWithMapLoop(ref float[,] Elevation, int iterations, dTransformWithMap dFunc)
    {
        for (int i = 0; i < iterations; i++)
        {
            TransformMapWithMap(ref Elevation, dFunc);
        }
    }

    public static void TransformEqualizeMapByLevel(ref float seaLevel, ref float[,] Elevation, float target, Benchmark bm = null, float tolerance = 0.01f, bool equalizeByExponentiation = false)
    {
        if (!(bm == null))
        {
            bm.StartBenchmark("Equalize Level to " + target);
        }
        float percentAbove = GetPercentAbove(Elevation, seaLevel);
        int iter = 0;
        while (Math.Abs(percentAbove - target) > tolerance && iter < 10000)
        {
            if (percentAbove > target + tolerance)
            {
                if ( seaLevel > 0.0f)
                {
                    if (equalizeByExponentiation)
                    {
                        TransformMap(ref Elevation, dExponentiate, 2f);
                    }
                    else
                    {
                        seaLevel += tolerance / 2f;
                    }
                }
                else
                {
                    break;
                }
            }
            if (percentAbove < target - tolerance)
            {
                if (seaLevel < 1.0f)
                {
                    if (equalizeByExponentiation)
                    {
                        TransformMap(ref Elevation, dExponentiate, 0.5f);

                    }
                    else
                    {
                        seaLevel -= tolerance / 2f;
                    }
                }
                else
                {
                    break;
                }
            }
            percentAbove = GetPercentAbove(Elevation, seaLevel);
            iter++;
        }
        if (!(bm == null))
        {
            bm.EndBenchmark("Equalize Level to " + target);
        }
    }
    public static void TransformEqualizeMapByLevelAboveSea(ref float riverLevel, ref float[,] WaterFlux, float target, float[,] Elevation, float seaLevel, float tolerance = 0.01f, bool equalizeByExponentiation = false)
    {
        float percentAbove = GetPercentAboveSeaLevel(WaterFlux, riverLevel, Elevation, seaLevel);
        int iter = 0;
        while (Math.Abs(percentAbove - target) > tolerance && iter < 1000)
        {
            //Debug.Log("Percent Above SeaLevel is " + percentAbove);
            if (percentAbove > target + tolerance)
            {
                if (riverLevel < 1.0f)
                {
                    if (equalizeByExponentiation)
                    {
                        TransformMap(ref WaterFlux, dExponentiate, 2f);
                    }
                    else
                    {
                        riverLevel += tolerance;
                    }
                }
                else
                {
                    break;
                }
            }
            if (percentAbove < target - tolerance)
            {
                if (riverLevel > 0f)
                {
                    if (equalizeByExponentiation)
                    {
                        TransformMap(ref WaterFlux, dExponentiate, 0.5f);
                    }
                    else
                    {
                        riverLevel -= tolerance;
                    }
                }
                else
                {
                    break;
                }
            }
            percentAbove = GetPercentAboveSeaLevel(WaterFlux, riverLevel, Elevation, seaLevel);
            iter++;
        }
    }
    public static float GetPercentAboveSeaLevel(float[,] WaterFlux, float thresh, float[,] Elevation, float seaLevel)
    {
        List<float> listAbove = new List<float>();
        int xDim = WaterFlux.GetLength(0);
        int yDim = WaterFlux.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x, y] > seaLevel)
                {
                    if (WaterFlux[x, y] > thresh)
                    {
                        listAbove.Add(1.0f);
                    }
                    else
                    {
                        listAbove.Add(0.0f);
                    }
                }
            }
        }
        float avg = listAbove.Count > 0 ? listAbove.Average() : 0f;
        return avg;
    }
    public static int OceanNeighbors(Loc l, float[,] Elevation, float seaLevel)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(Elevation, l);
        int numberOceanNeighbors = 0;
        foreach (KeyValuePair<Loc, float> kvp in neighbors)
        {
            if (kvp.Value < seaLevel)
            {
                numberOceanNeighbors++;
            } 
        }
        return numberOceanNeighbors;
    }
    public static int RiverNeighbors(Loc l, float[,] WaterFlux, float riverLevel)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(WaterFlux, l);
        int numberRiverNeighbors = 0;
        foreach (KeyValuePair<Loc, float> kvp in neighbors)
        {
            if (kvp.Value > riverLevel)
            {
                numberRiverNeighbors++;
            }
        }
        return numberRiverNeighbors;
    }
    public static List<Loc> GetCoastLocs(float[,] Elevation, float seaLevel)
    {
        List<Loc> coast = new List<Loc>();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x, y] > seaLevel)
                {
                    Loc l = new Loc(x, y);
                    int numberOfOceanNeighbors = OceanNeighbors(l, Elevation, seaLevel);
                    if (numberOfOceanNeighbors > 0)
                    {
                        coast.Add(l);
                    }
                }
            }
        }
        return coast;
    }

    public static bool IsBorder(Loc l, float[,] Elevation)
    {
        int x = l.x;
        int y = l.y;
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);

        bool isBorder = (x == 0 || y == 0 || x == xDim - 1 || y == yDim - 1) ? true : false;
        return isBorder;
    }
    public static float GetMapBorderAverage(float[,] Elevation)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        List<float> listBorder = new List<float>();
        Loc l;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                l = new Loc(x, y);
                if (IsBorder(l, Elevation))
                {
                    listBorder.Add(Elevation[x, y]);
                }
            }
        }
        return listBorder.Average();
    }
    public static float GetMapMedian(float[,] Elevation)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        List<float> listValues = new List<float>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                listValues.Add(Elevation[x, y]);
            }
        }
        return GetMedian(listValues); // change it to median
    }
    public static float GetMedian(List<float> x)
    {
        x.Sort();
        int i = (x.Count - 1) / 2;
        return x[i];
    }
    public static float GetPercentAbove(float[,] Elevation, float thresh)
    {
        List<float> listAbove = new List<float>();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (Elevation[x,y] > thresh)
                {
                    listAbove.Add(1.0f);
                }
                else
                {
                    listAbove.Add(0.0f);
                }
            }
        }
        return listAbove.Average();
    }
    public static float GetSlope(float[,] Elevation, Loc l)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(Elevation, l); // Get Slope
        float maxDiff = float.NegativeInfinity;
        float localDiff;
        foreach (KeyValuePair<Loc, float> n in neighbors)
        {
            localDiff = Math.Abs(Elevation[l.x, l.y] - Elevation[n.Key.x, n.Key.y]);
            if (localDiff > maxDiff)
            {
                maxDiff = localDiff;
            }
        }
        return maxDiff;
    }
    public static float[,] GetSlopeMap(float[,] Elevation)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        float[,] Slopes = new float[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Slopes[x, y] = GetSlope(Elevation, new Loc(x, y));
            }
        }
        return Slopes;
    }

    public static void GetCoordinates(int xDim, int yDim, int _X, int _Y, ref int X, ref int Y, bool Wrapped = true)
    {
        if (Wrapped)
        {
            X = (1000 * xDim + _X) % xDim;
            Y = (1000 * yDim + _Y) % yDim;
        }
        else
        {
            X = _X;
            Y = _Y;
        }
    }
    public static Dictionary<Loc,float> GetValidNeighbors(float[,] Elevation, Loc l, bool excludeSelf = true, bool includeDiagonal = false, bool Wrap = true)
    {
        Dictionary<Loc, float> neighbors = new Dictionary<Loc, float>();
        int x = 0;
        int y = 0;
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (excludeSelf && i == 0 && j == 0)
                {

                }
                else
                {
                    GetCoordinates(xDim, yDim, l.x+i, l.y+j, ref x, ref y, Wrap);
                    if (x >= 0 && y >= 0 && x < xDim && y < yDim)
                    {
                        if (includeDiagonal || Math.Sqrt(i*i+j*j) <= 1)
                        {
                            neighbors[new Loc(x, y)] = Elevation[x, y];
                        }
                    }
                }
            }
        }
        return neighbors;
    }
    public static Loc GetLowestNeighbor(float[,] Elevation, Loc l, bool excludeSelf)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(Elevation, l, excludeSelf); // Get Lowest Neighbor
        Loc minLoc = new Loc(0, 0);
        float minVal = float.PositiveInfinity;
        foreach (KeyValuePair<Loc,float> kvp in neighbors)
        {
            if (kvp.Value < minVal)
            {
                minVal = kvp.Value;
                minLoc = kvp.Key;
            }
        }
        return minLoc;
    }
    public static Loc GetHighestNeighbor(float[,] Elevation, Loc l, bool includeSelf)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(Elevation, l, includeSelf); // Get Highest Neighbor
        Loc maxLoc = new Loc(0, 0);
        float maxVal = float.NegativeInfinity;
        foreach (KeyValuePair<Loc, float> kvp in neighbors)
        {
            if (kvp.Value > maxVal)
            {
                maxVal = kvp.Value;
                maxLoc = kvp.Key;
            }
        }
        return maxLoc;
    }
    public static float GetNeighborAverage(float[,] Elevation, Loc l)
    {
        Dictionary<Loc, float> neighbors = GetValidNeighbors(Elevation, l); // GetNeighborAversge
        List<float> NeighborHeights = new List<float>();
        foreach (KeyValuePair<Loc, float> kvp in neighbors)
        {
            NeighborHeights.Add(kvp.Value);
        }
        return NeighborHeights.Average();
    }
    public static Loc[,] GetDownhillMap(float[,] Elevation)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        Loc[,] Downhill = new Loc[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Loc ln = GetLowestNeighbor(Elevation, new Loc(x, y), false);
                Downhill[x, y] = ln;
                //Debug.Log("Lowest Neighbor of [" + x + "," + y + "] at " + Elevation[x, y] + " is [" + ln.x + "," + ln.y + "] at " + Elevation[ln.x, ln.y]);
            }
        }
        return Downhill;
    }
    public static Vec[,] GetDownhillVectors(float[,] Elevation)
    {
        Loc[,] Downhills = GetDownhillMap(Elevation);
        int xDim = Downhills.GetLength(0);
        int yDim = Downhills.GetLength(1);
        Vec[,] dhVectors = new Vec[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (x == Downhills[x, y].x && y == Downhills[x, y].y)
                {

                }
                else
                {
                    Vec v = VectorBetween(new Loc(x, y), Downhills[x, y]);
                    dhVectors[x, y] = v;
                    //Debug.Log("Vector at [" + x + "," + y + "] is [" + v.x + "," + v.y + "]");
                }
            }
        }
        return dhVectors;
    }
    public static Vec VectorBetween(Loc l1, Loc l2)
    {
        float x = l2.x - l1.x;
        float y = l1.y - l2.y;
        return new Vec(x, y);
    }
    public static List<Loc> GetLocsDescending(float[,] Elevation, Benchmark bench = null)
    {
        if (bench != null)
        {
            bench.StartBenchmark("GetLocsDescending");
        }
        List<Loc> dLocs = new List<Loc>();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Loc l = new Loc(x, y);
                l.v = Elevation[x, y];
                dLocs.Add(l);
            }
        }
        dLocs.OrderBy(l => l.v);
        dLocs.Reverse();

        if (bench != null)
        {
            bench.EndBenchmark("GetLocsDescending");
        }
        return dLocs;
    }

    public static void TransformApplyPreferencesWaterBody(ref float[,] Elevation, float seaLevel, WaterBodyPrefence prefWaterBody, Benchmark bench = null)
    {
        if (!(bench == null))
        {
            bench.StartBenchmark("Water Bodies");
        }
        if (prefWaterBody == WaterBodyPrefence.Islands)
        {
            if (GetMapBorderAverage(Elevation) > 0.5f)
            {
                TransformMap(ref Elevation, dInvert, 0f);
            }
        }
        else if (prefWaterBody == WaterBodyPrefence.Continent)
        {
            AddMaskIsland(ref Elevation, 0.1f);
            if (GetMapBorderAverage(Elevation) > 0.5f)
            {
                TransformMap(ref Elevation, dInvert, 0f);
            }
        }
        else if (prefWaterBody == WaterBodyPrefence.Lakes)
        {
            if (GetMapBorderAverage(Elevation) < 0.5f)
            {
                TransformMap(ref Elevation, dInvert, 0f);
            }
        }
        else if (prefWaterBody == WaterBodyPrefence.Coast)
        {
            AddMaskCoast(ref Elevation);
            if (GetMapBorderAverage(Elevation) > 0.5f)
            {
                TransformMap(ref Elevation, dInvert, 0f);
            }
        }
        if (!(bench == null))
        {
            bench.EndBenchmark("Water Bodies");
        }

        TransformMapMinMax(ref Elevation, dNormalize);
        TransformEqualizeMapByLevel(ref seaLevel, ref Elevation, 0.5f, bench);
    }
    public static void TransformResolveDepressions(ref float[,] Elevation, int maxIter = 1000, Benchmark bench = null, bool normalize = true)
    {
        if (!(bench == null))
        {
            bench.StartBenchmark("Resolve Depression" + maxIter);
        }
        List<Loc> depressedLocs = GetListOfDepressedLocs(Elevation);
        int iter = 0;
        while (depressedLocs.Count > 0 && iter < maxIter) 
        {
            SortLocs(ref depressedLocs, Elevation);
            depressedLocs.Reverse();
            foreach (Loc l in depressedLocs)
            {
                Elevation[l.x, l.y] = GetNeighborAverage(Elevation, l);
            }

            depressedLocs = GetListOfDepressedLocs(Elevation);
            iter++;
        }
        if (!(bench == null))
        {
            bench.EndBenchmark("Resolve Depression" + maxIter);
        }
        if (normalize)
        {
            TransformMapMinMax(ref Elevation, dNormalize, bench);
        }
    }
    public static List<Loc> GetListOfDepressedLocs(float[,] Elevation)
    {
        List<Loc> depressedLocs = new List<Loc>();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);

        Loc l;
        float e;
        bool lIsDepressed;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                l = new Loc(x, y);
                e = Elevation[x, y];
                lIsDepressed = true;
                Dictionary<Loc,float> lNeighbors = GetValidNeighbors(Elevation, l); // Get Depressed Locs
                foreach (KeyValuePair<Loc,float> n in lNeighbors)
                {
                    if (n.Value <= e)
                    {
                        lIsDepressed = false;
                        break;
                    }
                }
                if (lIsDepressed)
                {
                    depressedLocs.Add(l);
                }
            }
        }

        return depressedLocs;
    }
    public static void SortLocs(ref List<Loc> locs, float[,] Elevation)
    {
        locs.OrderBy(p => Elevation[p.x, p.y]);
    }

    public static List<Loc> GetCircleLocs(float[,] Elevation, Loc center, float radius)
    {
        List<Loc> circleLocs = new List<Loc>();
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                float d = Distance(center, new Loc(x, y));
                if (d < radius)
                {
                    circleLocs.Add(new Loc(x, y));
                }
            }
        }
        return circleLocs;
    }
    public static void AddMaskCircle(ref float[,] Elevation, Loc center, float radius, float height)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                float d = Distance(center, new Loc(x, y));
                if (d < radius)
                {
                    Elevation[x, y] += height * (float)(1 - Math.Pow(d / radius,2));
                }
            }
        }
        TransformMapMinMax(ref Elevation, dNormalize);
    }
    public static void AddMaskIsland(ref float[,] Elevation, float height = 0.5f)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        int xi = (xDim - 1) / 2;
        int yi = (yDim - 1) / 2;
        int ri = (int)Math.Min(xi, yi);
        AddMaskCircle(ref Elevation, new Loc(xi, yi), ri, height);
    }
    public static void AddMaskCoast(ref float[,] Elevation, float height = 0.5f)
    {
        int xDim = Elevation.GetLength(0);
        int yDim = Elevation.GetLength(1);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Elevation[x, y] += height * x / xDim;
            }
        }
        TransformMapMinMax(ref Elevation, dNormalize);
    }

    public static float Distance(Loc l1, Loc l2)
    {
        float d = (float)Math.Sqrt(Math.Pow(l1.x - l2.x, 2) + Math.Pow(l1.y - l2.y, 2));
        return d;
    }
    public static int nFromDims(int xDim, int yDim)
    {
        int N = 1;
        int resultDim = (int)Mathf.Pow(2, N) + 1;
        int maxDim = Mathf.Max(xDim, yDim);
        while (resultDim < maxDim)
        {
            N++;
            resultDim = (int)Mathf.Pow(2, N) + 1;
        }
        return N;
    }
    public static float DegreesToRadians(float degrees)
    {
        return (float)(degrees * (Math.PI / 180));
    }
    public static float RadiansToDegrees(float radians)
    {
        return (float)(radians * (180 / Math.PI));
    }
    public static float VectorToRadians(float x, float y)
    {
        return (float)Math.Atan2(y,x);
    }

    public static void PrintElevationToDebug(float[,] Elevation)
    {
        int Dim = Elevation.GetLength(0);
        string sLine;
        for (int y = 0; y < Dim; y++)
        {
            sLine = "";
            for (int x = 0; x < Dim; x++)
            {
                sLine += Math.Round(Elevation[x, y], 2).ToString() + ",";
            }
            sLine = sLine.TrimEnd(',');
            Debug.Log(sLine);
        }
    }
    

}
