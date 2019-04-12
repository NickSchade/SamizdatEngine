using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;


public interface IDrawer
{
    void Init();
    void DrawFrame();
    void DrawTakeTurn();
    IGame game { get; set; }
}



public interface IPos
{
    Loc gridLoc { get; set; } // the location on a "square grid"
    Loc mapLoc { get; set; } // the location in physical space ie same for squares, but hex' mapLoc is offset on odd rows, etc.
    Vector3 gameLoc { get; set; } // the location in game space
    List<IPos> neighbors { get; set; }
}

public class GridPos : IPos
{
    public Loc gridLoc { get; set; } // the location on a "square grid"
    public Loc mapLoc { get; set; } // the location in physical space ie same for squares, but hex' mapLoc is offset on odd rows, etc.
    public Vector3 gameLoc { get; set; } // the location in game space
    public List<IPos> neighbors { get; set; }
    public IGame game { get; set; }
    public GridPos(Loc _gridLoc, IGame _game)
    {
        game = _game;
        gridLoc = _gridLoc;
    }

}

public enum PosType { GridPos}

public static class PosFactory
{
    public static IPos CreatePos(Loc _gridLoc, IGame _game)
    {
        if (_game.posType == PosType.GridPos)
        {
            return new GridPos(_gridLoc, _game);
        }
        else
        {
            return new GridPos(_gridLoc, _game);
        }
    }
}

public interface ILand
{
    IPos pos { get; set; }
    Color GetColor();
}

public enum TerrainType { LAND, OCEAN, MOUNTAIN}

public abstract class LandAbstract
{
    public IPos pos { get; set; }
    public LandAbstract(IPos _pos)
    {
        pos = _pos;
    }
}

public class LandBasic : LandAbstract, ILand
{
    public Color GetColor()
    {
        Color c = Color.magenta;
        if (terrainType == TerrainType.OCEAN)
        {
            c = Color.blue;
        }
        else if (terrainType == TerrainType.MOUNTAIN)
        {
            c = Color.black;
        }
        else
        {
            c = Color.gray;
        }
        return c;
    }
    TerrainType terrainType { get; set; }

    public LandBasic(IPos _pos, float val) : base(_pos)
    {
        SetTerrainType(val);
    }
    public void SetTerrainType(float val, float TERRAIN_LEVEL = 0.25f)
    {
        terrainType = TerrainType.LAND;
        if (val < TERRAIN_LEVEL)
        {
            terrainType = TerrainType.OCEAN;
        }
        else if (val > (1 - TERRAIN_LEVEL))
        {
            terrainType = TerrainType.MOUNTAIN;
        }
    }
}

public class ExodusLand : LandAbstract , ILand
{
    public IPos pos { get; set; }

    int iLand;

    public ExodusLand(IPos _pos, float _elevation, float _temperature) : base(_pos)
    {
        if (_elevation < 0.5f)
        {
            if (_temperature < 0.5f)
            {
                iLand = 1;
            }
            else
            {
                iLand = 2;
            }
        }
        else
        {
            if (_temperature < 0.5f)
            {
                iLand = 3;
            }
            else
            {
                iLand = 4;
            }
        }
    }
    public Color GetColor()
    {
        Color c = Color.magenta;
        if (iLand == 1)
        {
            c = Color.red;
        }
        else if (iLand == 2)
        {
            c = Color.blue;
        }
        else if (iLand == 3)
        {
            c = Color.green;
        }
        else if (iLand == 4)
        {
            c = Color.yellow;
        }
        return c;

    }

}

public enum LandType { BasicLand, ExodusLand};

public static class LandFactory
{
    public static ILand CreateLand(IPos _pos, Dictionary<string,float> _val, LandType _landType)
    {
        if (_landType == LandType.BasicLand)
        {
            return new LandBasic(_pos, _val["elevation"]);
        }
        else if (_landType == LandType.ExodusLand)
        {
            return new ExodusLand(_pos, _val["elevation"], _val["temperature"]);
        }
        else
        {
            return new LandBasic(_pos, _val["elevation"]);
        }
    }
}


