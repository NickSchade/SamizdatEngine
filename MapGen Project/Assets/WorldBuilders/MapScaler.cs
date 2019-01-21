using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScaler {
	public MapScaler()
    {
    }
    public Dictionary<string,float[,]> BuildSurfaces(MapGen mg)
    {
        Dictionary<string, float[,]> surfaces = new Dictionary<string, float[,]> {
            { "Elevation", mg.Elevation },
            { "Rain", mg.Rain },
            { "WaterFlux", mg.WaterFlux },
            { "Temperature", mg.Temperature },
            { "WindMagnitude", mg.WindMagnitude  },
            { "Flatness", mg.Flatness },
            { "Fertility", mg.Fertility },
            { "Harbor", mg.Harbor}
                    };
        return surfaces;
    }
    public void Crop(ref MapGen inputMap, int xDim, int yDim, int xTL, int yTL, bool bNormalize = false)
    {
        MapGen outputMap = new MapGen(xDim, yDim);
        outputMap.seaLevel = inputMap.seaLevel;
        outputMap.riverLevel = inputMap.riverLevel;
        Dictionary<string, float[,]> inputSurfaces = BuildSurfaces(inputMap);
        Dictionary<string, float[,]> outputSurfaces = BuildSurfaces(outputMap);

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                foreach (KeyValuePair<string, float[,]> kvp in inputSurfaces)
                {
                    string k = kvp.Key;
                    outputSurfaces[k][x, y] = inputSurfaces[k][x + xTL, y + yTL];
                }
            }
        }
        if (bNormalize)
        {
            Normalize(ref outputMap, outputSurfaces);
        }
        inputMap = outputMap;
    }
    public void ExpandSquare(ref MapGen inputMap, int xE, int yE, bool bNormalize = true)
    {
        int xDim = inputMap.Elevation.GetLength(0);
        int yDim = inputMap.Elevation.GetLength(1);
        int xl = xDim * xE;
        int yl = yDim * yE;
        MapGen outputMap = new MapGen(xl, yl);
        outputMap.seaLevel = inputMap.seaLevel;
        outputMap.riverLevel = inputMap.riverLevel;

        Dictionary<string, float[,]> inputSurfaces = BuildSurfaces(inputMap);
        Dictionary<string, float[,]> outputSurfaces = BuildSurfaces(outputMap);
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                for (int xi = 0; xi < xE; xi++)
                {
                    for (int yi = 0; yi < yE; yi++)
                    {
                        foreach (KeyValuePair<string,float[,]> kvp in inputSurfaces)
                        {
                            string k = kvp.Key;
                            int xp1 = x < xDim - 1 ? x + 1 : x;
                            int yp1 = y < yDim - 1 ? y + 1 : y;
                            float xLerpTop = Mathf.Lerp(inputSurfaces[k][x, y], inputSurfaces[k][xp1, y], (float)xi / (float)xE);
                            float xLerpBot = Mathf.Lerp(inputSurfaces[k][x, yp1], inputSurfaces[k][xp1, yp1], (float)xi / (float)xE);
                            float lerpVal = Mathf.Lerp(xLerpTop, xLerpBot, yi / yE);
                            int xind = x * xE + xi;
                            int yind = y * yE + yi;
                            outputSurfaces[k][xind, yind] = lerpVal;
                        }
                    }
                }
            }
        }
        if (bNormalize)
        {
            Normalize(ref outputMap, outputSurfaces);
        }
        inputMap = outputMap;
    }
    public void Smooth(ref MapGen inputMap, bool bNormalize = true)
    {
        int xDim = inputMap.Elevation.GetLength(0);
        int yDim = inputMap.Elevation.GetLength(1);
        MapGen outputMap = new MapGen(xDim, yDim);
        outputMap.seaLevel = inputMap.seaLevel;
        outputMap.riverLevel = inputMap.riverLevel;

        Dictionary<string, float[,]> inputSurfaces = BuildSurfaces(inputMap);
        Dictionary<string, float[,]> outputSurfaces = BuildSurfaces(outputMap);
        foreach (KeyValuePair<string, float[,]> kvp in inputSurfaces)
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    outputSurfaces[kvp.Key][x, y] = MapUtil.dSmooth(x, y, inputSurfaces[kvp.Key]);    
                }
            }

        }
        if (bNormalize)
        {
            Normalize(ref outputMap, outputSurfaces);
        }
        inputMap = outputMap;
    }
    private void Normalize(ref MapGen inputMap, Dictionary<string, float[,]> inputSurfaces)
    {
        foreach (KeyValuePair<string, float[,]> kvp in inputSurfaces)
        {
            float[,] surface = kvp.Value;
            MapUtil.TransformMapMinMax(ref surface, MapUtil.dNormalize);
        }
        MapUtil.TransformEqualizeMapByLevel(ref inputMap.seaLevel, ref inputMap.Elevation, 0.5f);
        MapUtil.TransformEqualizeMapByLevelAboveSea(ref inputMap.riverLevel, ref inputMap.WaterFlux, 0.05f, inputMap.Elevation, inputMap.seaLevel);
    }
}
