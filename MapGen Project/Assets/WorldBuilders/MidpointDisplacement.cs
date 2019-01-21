using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MidpointDisplacement{
    
    public int N, xDim, yDim, Dim, featureSize;
    public float RandomInfluence;
    public float[,] Elevation;

	public MidpointDisplacement(int N)
    {
        Dim = (int)Mathf.Pow(2, N) + 1;         // This indicates the height and width of the map
        xDim = Dim;
        yDim = xDim;
        RandomInfluence = 1.0f;                    // R should be between 0.0 and 1.0
        featureSize = xDim;                        // This indicates the intricacy of detail on the map
        Elevation = new float[xDim, yDim];        // Most functions implicitly refer to this array
        BuildMap();                             // The constructor calls this function to build the map
    }

    public void BuildMap()
    {
        //RandomizePoints(featureSize);
        int sampleSize = featureSize;
        float scale = RandomInfluence;
        while (sampleSize > 1)
        {
            DiamondSquare(sampleSize, scale);
            sampleSize /= 2;
            scale /= 2f;
        }
    }

    public void DiamondSquare(int stepsize, float scale)
    {
        int halfstep = stepsize / 2;
        //Debug.Log("Diamond Square on stepsize = " + stepsize + " and scale = " + scale + " and halfstep = " + halfstep);

        for (int x = halfstep; x < xDim - halfstep; x += stepsize)
        {
            for (int y = halfstep; y < yDim - halfstep; y += stepsize)
            {
                SquareStep(x, y, stepsize, GetRandom(scale));
            }
        }
        for (int x = halfstep; x < xDim - halfstep; x += stepsize)
        {
            for (int y = halfstep; y < yDim - halfstep; y += stepsize)
            {
                DiamondStep(x + halfstep, y, stepsize, GetRandom(scale));
                DiamondStep(x, y + halfstep, stepsize, GetRandom(scale));
            }
        }
    }
    public void SquareStep(int x, int y, int size, float randomValue)
    {
        //Debug.Log("Square Step on [" + x + "," + y + "]");
        int hs = size / 2;

        // a     b 
        //
        //    x
        //
        // c     d

        List<float> validPoints = new List<float>();
        validPoints.Add(GetValueAt(x - hs, y - hs));
        validPoints.Add(GetValueAt(x + hs, y - hs));
        validPoints.Add(GetValueAt(x - hs, y + hs));
        validPoints.Add(GetValueAt(x + hs, y + hs));
        float midpointDisplacedValue = validPoints.Average() + randomValue;

        SetValue(x, y, midpointDisplacedValue);
    }
    public void DiamondStep(int x, int y, int size, float randomValue)
    {
        //Debug.Log("Diamond Step on [" + x + "," + y + "]");
        int hs = size / 2;

        //   c
        //
        //a  x  b
        //
        //   d
        List<float> validPoints = new List<float>();
        validPoints.Add(GetValueAt(x - hs, y));
        validPoints.Add(GetValueAt(x + hs, y));
        validPoints.Add(GetValueAt(x, y - hs));
        validPoints.Add(GetValueAt(x, y + hs));
        float midpointDisplacedValue = validPoints.Average() + randomValue;

        SetValue(x, y, midpointDisplacedValue);
    }

    public static void GetCoordinates(int xDim, int yDim, int _X, int _Y, out int X, out int Y)
    {
        X = (1000 * xDim + _X) % xDim;
        Y = (1000 * yDim + _Y) % yDim;
    }
    public float GetValueAt(int _X, int _Y)
    {
        int X, Y;
        GetCoordinates(xDim, yDim, _X, _Y, out X, out Y);
        return Elevation[X, Y];
    }
    public void SetValue(int _X, int _Y, float v)
    {
        int X, Y;
        GetCoordinates(xDim, yDim, _X, _Y, out X, out Y);
        Elevation[X, Y] = v;
        //Debug.Log("Setting [" + X + "," + Y + "] to " + v);
    }
    public static float GetRandom(float coeff)
    {
        float r = (Random.Range(0, 100) - 50) / 100f;
        return r * coeff;
    }

}
