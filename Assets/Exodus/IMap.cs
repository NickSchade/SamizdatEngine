using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;

public interface IMap
{
    IGame game { get; set; }
    int dim { get; set; }
    bool wrapEastWest { get; set; }
    bool wrapNorthSouth { get; set; }
    Dictionary<IPos, ILand> lands { get; set; }
    Dictionary<IPos, ILand> MakeLands(int _N);
    IPos GetCenterPos();
}

public abstract class MapAbstract
{
    public IGame game { get; set; }
    public bool wrapEastWest { get; set; }
    public bool wrapNorthSouth { get; set; }
    public Dictionary<IPos, ILand> lands { get; set; }
    public Dictionary<string, IPos> pathMap { get; set; }

    public int dim { get; set; }
    public TileShape tileShape;
    public Dictionary<IPos, IOccupant> occupants { get; set; }

    public MapAbstract(IGame _game, int _N = 5, bool _wrapEastWest = true, bool _wrapNorthSouth = true)
    {
        game = _game;
        wrapEastWest = _wrapEastWest;
        wrapNorthSouth = _wrapNorthSouth;
        tileShape = game.tileShape;

        occupants = new Dictionary<IPos, IOccupant>();

        dim = 1 + (int)Mathf.Pow(2, _N);
        lands = MakeLands(_N);
    }

    public abstract Dictionary<IPos, ILand> MakeLands(int _N);

    public float[,] NoiseGrid(int _N)
    {
        MidpointDisplacement mpd = new MidpointDisplacement(_N, wrapEastWest, wrapNorthSouth);
        float[,] elevation = mpd.Elevation;
        return elevation;
    }

    #region BUILD GRID
    public Dictionary<string, IPos> GenerateBasicMap(int _N)
    {
        int _dim = 1 + (int)Mathf.Pow(2, _N);
        Dictionary<string, IPos> map = Generate2DGrid(_dim);
        map = SetNeighborsFor2DGrid(map);
        return map;
    }
    public Dictionary<string, IPos> Generate2DGrid(int _dim)
    {
        Dictionary<string, IPos> map = new Dictionary<string, IPos>();

        for (int x = 0; x < _dim; x++)
        {
            for (int y = 0; y < _dim; y++)
            {
                Loc l = new Loc(x, y);
                IPos p = PosFactory.CreatePos(l, game);
                map[p.gridLoc.key()] = p;
            }
        }

        return map;
    }
    public Dictionary<string, IPos> SetNeighborsFor2DGrid(Dictionary<string, IPos> map)
    {
        foreach (string k in map.Keys)
        {
            IPos p = map[k];
            if (tileShape == TileShape.SQUARE)
            {
                p.neighbors = SetNeighborsSquare(p, map);
            }
            else if (tileShape == TileShape.HEX)
            {
                p.neighbors = SetNeighborsHex(p, map);
            }

        }
        return map;
    }
    public List<IPos> SetNeighborsSquare(IPos p, Dictionary<string, IPos> map)
    {
        float x = p.gridLoc.x();
        float y = p.gridLoc.y();
        List<IPos> neighbors = new List<IPos>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {

                    float X = wrapEastWest ? (dim + x + i) % dim : x + i;
                    float Y = wrapNorthSouth ? (dim + y + j) % dim : y + j;

                    Loc l2 = new Loc(X, Y);
                    if (map.ContainsKey(l2.key()))
                    {
                        neighbors.Add(map[l2.key()]);
                    }
                    else
                    {
                        Debug.Log("Map doesn't contain " + l2.x() + "," + l2.y());
                    }
                }
            }
        }
        return neighbors;
    }
    public List<IPos> SetNeighborsHex(IPos p, Dictionary<string, IPos> map)
    {
        List<IPos> neighbors = new List<IPos>();

        List<int[]> hexNeighbors = new List<int[]>();
        if (p.gridLoc.y() % 2 == 0)
        {
            hexNeighbors.Add(new int[] { 1, 0 });
            hexNeighbors.Add(new int[] { 1, -1 });
            hexNeighbors.Add(new int[] { 0, -1 });
            hexNeighbors.Add(new int[] { -1, 0 });
            hexNeighbors.Add(new int[] { 0, 1 });
            hexNeighbors.Add(new int[] { 1, 1 });
        }
        else
        {

            hexNeighbors.Add(new int[] { 1, 0 });
            hexNeighbors.Add(new int[] { -1, -1 });
            hexNeighbors.Add(new int[] { 0, -1 });
            hexNeighbors.Add(new int[] { -1, 0 });
            hexNeighbors.Add(new int[] { 0, 1 });
            hexNeighbors.Add(new int[] { -1, 1 });
        }


        float x = p.gridLoc.x();
        float y = p.gridLoc.y();
        for (int k = 0; k < hexNeighbors.Count; k++)
        {
            int i = hexNeighbors[k][0];
            int j = hexNeighbors[k][1];
            float X = wrapEastWest ? (dim + x + i) % dim : x + i;
            float Y = wrapNorthSouth ? (dim + y + j) % dim : y + j;

            Loc l2 = new Loc(X, Y);
            if (map.ContainsKey(l2.key()))
            {
                neighbors.Add(map[l2.key()]);
            }
            else
            {
                Debug.Log("Map doesn't contain " + l2.x() + "," + l2.y());
            }
        }

        return neighbors;
    }
    #endregion

    #region INITIALIZE LANDS
    public Dictionary<IPos, ILand> InitializeLandsFromMidpointDisplacement(int _N, Dictionary<string, IPos> pm)
    {
        Dictionary<IPos, ILand> landsDict = new Dictionary<IPos, ILand>();

        float[,] elevation = NoiseGrid(_N);
        MapUtil.TransformMapMinMax(ref elevation, MapUtil.dNormalize);
        foreach (IPos p in pm.Values)
        {
            Dictionary<string, float> _val = new Dictionary<string, float>() { { "elevation", elevation[(int)p.gridLoc.x(), (int)p.gridLoc.y()] } };

            ILand newLand = LandFactory.CreateLand(p, _val, game.landType);
            landsDict[p] = newLand;
        }

        return landsDict;

    }
    #endregion

    public IPos GetCenterPos()
    {
        Loc MidLoc = new Loc(game.map.dim / 2 - 1, game.map.dim / 2 - 1);
        return pathMap[MidLoc.key()];
    }
}

public class MapBasic : MapAbstract, IMap
{

    public MapBasic(IGame _game, int _N = 5, bool _wrapEastWest = true, bool _wrapNorthSouth = true) : base(_game, _N, _wrapEastWest, _wrapNorthSouth)
    {
        
    }

    public override Dictionary<IPos, ILand> MakeLands(int _N)
    {
        pathMap = GenerateBasicMap(_N);
        return InitializeLandsFromMidpointDisplacement(_N, pathMap);
    }
    

}

public class ExodusMap : MapAbstract, IMap
{
    public ExodusMap(IGame _game, int _N = 5, bool _wrapEastWest = true, bool _wrapNorthSouth = true) : base(_game, _N, _wrapEastWest, _wrapNorthSouth)
    {

    }

    public override Dictionary<IPos, ILand> MakeLands (int _N)
    {
        Dictionary<string, IPos> pm = GenerateBasicMap(_N);
        Dictionary<IPos, ILand> landsDict = new Dictionary<IPos, ILand>();

        float[,] elevation = NoiseGrid(_N);
        float[,] temperature = NoiseGrid(_N);

        MapUtil.TransformMapMinMax(ref elevation, MapUtil.dNormalize);
        MapUtil.TransformMapMinMax(ref temperature, MapUtil.dNormalize);

        foreach (IPos p in pm.Values)
        {
            float elev = elevation[(int)p.gridLoc.x(), (int)p.gridLoc.y()];
            float temp = temperature[(int)p.gridLoc.x(), (int)p.gridLoc.y()];
            Dictionary<string, float> _val = new Dictionary<string, float>(){ { "elevation", elev }, { "temperature", temp } };
            ILand newLand = LandFactory.CreateLand(p, _val, game.landType);
            landsDict[p] = newLand;
        }

        return landsDict;
    }
}

public interface IOccupant
{

}

