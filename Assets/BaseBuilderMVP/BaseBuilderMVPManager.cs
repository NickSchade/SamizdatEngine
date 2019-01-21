using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using BBMVP;

public class BaseBuilderMVPManager : MonoBehaviour {

    // Game
    BaseBuilderMVP game;
    // GameRenderer
    public bbDrawer drawer;
    // UI
    public bbUIHandler uihandler;

    // Constants
    

    // Use this for initialization
    void Start () {
        game = new BaseBuilderMVP();
        game.gameManager = this;
        drawer.game = game;
        uihandler.game = game;
    }

    // Update is called once per frame
    void Update () {
        if (game.turnNumber == 0)
        {
            game.turnNumber++;
            drawer.DrawInit();
        }
        drawer.Animate();
        if (NeedsUpdate())
        {
            drawer.DrawTakeTurn();
        }
    }

    bool NeedsUpdate()
    {
        bool updateRealtime = game.RealTimeTurn();
        bool updateClick = uihandler.HandleMouse();
        bool updateKey = uihandler.HandleKeys();
        return updateRealtime || updateClick || updateKey;
    }
    
    
    // UI
    public void Button_TakeTurn()
    {
        game.TakeTurn();
        drawer.DrawTakeTurn();
    }
    public void Button_TogglePause()
    {
        game.paused = !game.paused;
        drawer.UpdateButtons();
    }
    public void Button_ToggleSelectedAction()
    {
        game.ToggleAction();
        drawer.UpdateButtons();
    }

}
