using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectName { Exodus}

public class GameManagerBase : MonoBehaviour
{
    public IGame game { get; set; }
    public IDrawer drawer { get; set; }
    public InputHandler inputHandler;

    [System.NonSerialized] public ProjectName projectName = ProjectName.Exodus;


    // Start is called before the first frame update
    public void Start()
    {
        IGame _game = new GameBase();
        if (projectName == ProjectName.Exodus)
        {
            _game = new ExodusGame();
        }

        Init(_game);
    }

    public void Init(IGame _game)
    {
        game = _game;
        game.gameManager = this;
        drawer.game = game;
        inputHandler.game = game;
    }

    // Update is called once per frame
    public void Update()
    {
        if (game.turnNumber == 0)
        {
            game.turnNumber++;
            drawer.Init();
        }
        drawer.DrawFrame();

        if (inputHandler.HandleUserInput())
        {
           drawer.DrawTakeTurn();
        }
    }
}


