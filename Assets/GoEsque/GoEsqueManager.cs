using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;
using SamizdatEngine.GE;
using SamizdatEngine.GE.Basic;


public class GoEsqueManager : MonoBehaviour
{
    public GameObject goDrawer, goUI;
    GoEsque game;
    GEDrawer drawer;
    GEUI ui;
    // Use this for initialization
    void Start()
    {
        drawer = goDrawer.GetComponentInChildren<GEDrawerBasic>();
        ui = goUI.GetComponentInChildren<GEUIBasic>();
        game = new GoEsqueBasic(drawer, ui);
        ui.game = game;
        drawer.DrawInit(game);
    }

    // Update is called once per frame
    void Update()
    {
        ui.HandleUI();
    }
}
