using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;


public interface IGame
{
    GameManagerBase gameManager { get; set; }
    bool HandleClick(IPos _pos, bool _leftClick, bool _rightClick);
    int turnNumber { get; set; }

    IMap map { get; set; }
    TileShape tileShape { get; set; }
    PosType posType { get; set; }
    LandType landType { get; set; }

}

public class GameBase : IGame
{
    public int turnNumber { get; set; }
    public TileShape tileShape { get; set; }
    public PosType posType { get; set; }
    public LandType landType { get; set; }
    public IMap map { get; set; }
    public GameManagerBase gameManager { get; set; }
    public bool HandleClick(IPos _pos, bool _leftClick, bool _rightClick)
    {
        return true;
    }
    public GameBase(TileShape _tileShape = TileShape.SQUARE,
                        PosType _posType = PosType.GridPos,
                        LandType _landType = LandType.BasicLand)
    {
        turnNumber = 0;
        tileShape = _tileShape;
        posType = _posType;
        landType = _landType;

    }
}

public class ExodusGame : GameBase, IGame
{
    public ExodusGame(TileShape _tileShape = TileShape.SQUARE,
                        PosType _posType = PosType.GridPos,
                        LandType _landType = LandType.BasicLand) : base(_tileShape, _posType, _landType)
    {
        map = new ExodusMap(this);
    }
    
}
