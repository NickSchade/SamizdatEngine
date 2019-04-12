using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;



public class DrawerBase : MonoBehaviour, IDrawer
{
    public IGame game { get; set; }
    public Dictionary<IPos, GameObject> goMap { get; set; }

    public bool init;

    public GameObject pfTileSquare, pfTileHex;
    private GameObject pfTile;

    // Start is called before the first frame update
    void Start()
    {
        init = false;
        goMap = new Dictionary<IPos, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetTile()
    {
        switch (game.tileShape)
        {
            case TileShape.SQUARE:
                pfTile = pfTileSquare;
                break;
            case TileShape.HEX:
                //pfTile = pfTileSquare;
                pfTile = pfTileHex;
                break;
        }
    }

    public void InitCamera()
    {
        IPos midPos = game.map.GetCenterPos();
        Vector3 p = goMap[midPos].transform.position;
        if (game.tileShape == TileShape.SQUARE)
        {
            Camera.main.transform.position = new Vector3(p.x, 1, p.z);
            Camera.main.orthographicSize = game.map.dim / 1.75f;
        }
        else
        {
            Camera.main.transform.position = new Vector3(p.x * 3 / 2, 1, p.z * 3 / 2);
            Camera.main.orthographicSize = (3 / 2) * game.map.dim / 1.75f;
        }
    }


    public void Init()
    {
        SetTile();
        DrawFrame();
        if (!init)
        {
            init = true;
            InitCamera();
        }

    }
    public void DrawTakeTurn()
    {
        DrawTerrainUpdate();
    }

    void DrawTerrainUpdate()
    {
        foreach (IPos p in game.map.lands.Keys)
        {
            if (!goMap.ContainsKey(p))
            {
                goMap[p] = InstantiateGo(pfTile, p.mapLoc, Color.white);
                goMap[p].GetComponentInChildren<Clickable>().setPos(p);
            }
            Renderer r = goMap[p].GetComponentInChildren<Renderer>();
            ILand thisLand = game.map.lands[p];
            r.material.color = thisLand.GetColor();
        }
    }
    GameObject InstantiateGo(GameObject pf, Loc l, Color c)
    {
        Vector3 pos = new Vector3(l.x(), l.z(), l.y());
        GameObject go = Instantiate(pf, pos, Quaternion.identity);
        go.GetComponentInChildren<Renderer>().material.color = c;
        return go;
    }

    public void DrawFrame()
    {
        //if (!game.paused)
        //{
        //    foreach (bbAgent a in game.playerAgents)
        //    {
        //        StartCoroutine(AnimateAgent(a));
        //    }
        //}
    }
    //IEnumerator AnimateAgent(bbAgent a)
    //{
    //    if (!a.animating)
    //    {
    //        if (goAgents.ContainsKey(a) && goMap.ContainsKey(a.pos))
    //        {
    //            a.animating = true;
    //            float nextTurn = game.nextTurn;
    //            float lastTurn = game.lastTurn;
    //            Transform agentTransform = goAgents[a].transform;
    //            Transform targetTransform = goMap[a.pos].transform;
    //            float percent = 0;

    //            while (percent < 1)
    //            {
    //                if (!goAgents.ContainsKey(a))
    //                {
    //                    break;
    //                }
    //                else
    //                {
    //                    percent = Mathf.InverseLerp(lastTurn, nextTurn, Time.time);
    //                    goAgents[a].transform.position = Vector3.Lerp(agentTransform.position, targetTransform.position, percent);
    //                    if (percent == 1)
    //                    {
    //                        a.animating = false;
    //                    }
    //                }


    //                yield return null;
    //            }
    //        }
    //    }

    //}
}

public class ExodusDrawer : DrawerBase, IDrawer
{

}
