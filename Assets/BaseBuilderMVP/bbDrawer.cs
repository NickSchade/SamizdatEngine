using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BBMVP;

public class bbDrawer : MonoBehaviour
{
    public GameObject pfTileSquare, pfTileHex, pfAgent, pfStructure, pfButton;
    public GameObject ScoreGold, ScoreFood;
    public GameObject buttonPaused, buttonTurn;
    public GameObject panelAgentStats;
    public GameObject effectResource;

    public BaseBuilderMVP game;

    private GameObject pfTile;

    Dictionary<bbPos, GameObject> goMap;
    Dictionary<bbAgent, GameObject> goAgents;
    Dictionary<bbAgent, GameObject> goAgentStats;
    Dictionary<bbStructure, GameObject> goStructures;

    bool init;

    // Use this for initialization
    void Start () {
        goMap = new Dictionary<bbPos, GameObject>();
        goAgents = new Dictionary<bbAgent, GameObject>();
        goAgentStats = new Dictionary<bbAgent, GameObject>();
        goStructures = new Dictionary<bbStructure, GameObject>();
        init = false;
	}
    

    public void DrawInit()
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
        DrawTakeTurn();
        bbLoc MidLoc = new bbLoc(game.currentIsland.dim / 2 - 1, game.currentIsland.dim / 2 - 1);
        Vector3 p = goMap[game.currentIsland.pathMap[MidLoc.key()]].transform.position;
        if (!init)
        {
            init = true;
            if (game.tileShape == TileShape.SQUARE)
            {
                Camera.main.transform.position = new Vector3(p.x, 1, p.z);
                Camera.main.orthographicSize = game.currentIsland.dim / 1.75f;
            }
            else
            {
                Camera.main.transform.position = new Vector3(p.x * 3/2, 1, p.z * 3/2);
                Camera.main.orthographicSize = (3/2) * game.currentIsland.dim / 1.75f;
            }

        }
        
    }
    public void DrawTakeTurn()
    {
        DrawAgentsUpdate();
        DrawStructuresUpdate();
        DrawTerrainUpdate();
        UpdateResources();
        UpdateButtons();
    }

    void DrawTerrainUpdate()
    {
        foreach (string k in game.currentIsland.pathMap.Keys)
        {
            bbPos p = game.currentIsland.pathMap[k];
            if (!goMap.ContainsKey(p))
            {
                goMap[p] = InstantiateGo(pfTile, p.mapLoc, Color.white);
                goMap[p].GetComponentInChildren<bbClickable>().setPos(game.currentIsland.pathMap[k]);
            }
            Renderer r = goMap[p].GetComponentInChildren<Renderer>();
            bbLand thisLand = game.currentIsland.lands[p];
            if (thisLand.terrainType == bbTerrainType.OCEAN)
            {
                r.material.color = Color.blue;
            }
            else if (thisLand.terrainType == bbTerrainType.MOUNTAIN)
            {
                r.material.color = Color.black;
            }
            else
            {
                r.material.color = Color.gray;
            }
            if (thisLand.terrainFeature == bbTerrainFeature.ARABLE)
            {
                r.material.color = Color.green;
            }
            else if (thisLand.terrainFeature == bbTerrainFeature.MINEABLE)
            {
                r.material.color = Color.yellow;
            }
        }
    }
    void DrawAgentsUpdate()
    {
        foreach (bbAgent a in game.playerAgents)
        {
            if (!goAgents.ContainsKey(a))
            {
                goAgents[a] = InstantiateGo(pfAgent, a.pos.mapLoc, a.getColor());
            }
            Vector3 pos = new Vector3(a.pos.mapLoc.x(), a.pos.mapLoc.z(), a.pos.mapLoc.y());
            GameObject go = goAgents[a];
            go.transform.position = pos;
        }
        foreach (bbAgent a in game.playerAgents)
        {
            if (!goAgentStats.ContainsKey(a))
            {
                goAgentStats[a] = Instantiate(pfButton, panelAgentStats.transform);
            }
            goAgentStats[a].GetComponentInChildren<Text>().text = a.name.ToString() + a.pos.getName();

        }
        List<bbAgent> agents = new List<bbAgent>(goAgents.Keys);
        foreach (bbAgent a in agents)
        {
            if (!game.playerAgents.Contains(a))
            {
                GameObject go = goAgents[a];
                Destroy(go);
                goAgents.Remove(a);
            }
        }
        agents = new List<bbAgent>(goAgentStats.Keys);
        foreach (bbAgent a in agents)
        {
            if (!game.playerAgents.Contains(a))
            {
                GameObject go = goAgentStats[a];
                Destroy(go);
                goAgentStats.Remove(a);
            }
        }
    }
    void DrawStructuresUpdate()
    {
        foreach (bbStructure s in game.currentIsland.structures.Values)
        {
            if (!goStructures.ContainsKey(s))
            {
                goStructures[s] = InstantiateGo(pfStructure, s.getPos().mapLoc, s.getColor());
            }
            goStructures[s].GetComponent<Renderer>().material.color = s.getColor();
        }
        List<bbStructure> structures = new List<bbStructure>(goStructures.Keys);
        foreach (bbStructure s in structures)
        {
            if (!game.currentIsland.structures.ContainsValue(s))
            {
                GameObject go = goStructures[s];
                Destroy(go);
                goStructures.Remove(s);
            }
        }
    }
    GameObject InstantiateGo(GameObject pf, bbLoc l, Color c)
    {
        Vector3 pos = new Vector3(l.x(), l.z(), l.y());
        GameObject go = Instantiate(pf, pos, Quaternion.identity);
        go.GetComponentInChildren<Renderer>().material.color = c;
        return go;
    }

    public void UpdateResources()
    {
        ScoreGold.GetComponentInChildren<Text>().text = "GOLD:"+game.playerResources[ItemType.ITEM_GOLD].ToString();
        ScoreFood.GetComponentInChildren<Text>().text = "FOOD:"+game.playerResources[ItemType.ITEM_FOOD].ToString();
    }
    public void UpdateButtons()
    {
        buttonPaused.GetComponentInChildren<Text>().text = game.paused ? "Paused" : "Running";
        buttonTurn.GetComponentInChildren<Text>().text = "Turn " + game.turnNumber;
    }

    public void DrawEffect(GameObject _effect, Transform t)
    {
        GameObject effect = Instantiate(_effect, t) as GameObject;
        Destroy(effect, 1f);
    }
    public void DrawEffectResource(bbStructure s)
    {
        DrawEffect(effectResource, goStructures[s].transform);
    }

    public void Animate()
    {
        if (!game.paused)
        {
            foreach (bbAgent a in game.playerAgents)
            {
                StartCoroutine(AnimateAgent(a));
            }
        }
    }
    IEnumerator AnimateAgent(bbAgent a)
    {
        if (!a.animating)
        {
            if (goAgents.ContainsKey(a) && goMap.ContainsKey(a.pos))
            {
                a.animating = true;
                float nextTurn = game.nextTurn;
                float lastTurn = game.lastTurn;
                Transform agentTransform = goAgents[a].transform;
                Transform targetTransform = goMap[a.pos].transform;
                float percent = 0;

                while (percent < 1)
                {
                    if (!goAgents.ContainsKey(a))
                    {
                        break;
                    }
                    else
                    {
                        percent = Mathf.InverseLerp(lastTurn, nextTurn, Time.time);
                        goAgents[a].transform.position = Vector3.Lerp(agentTransform.position, targetTransform.position, percent);
                        if (percent == 1)
                        {
                            a.animating = false;
                        }
                    }
                    

                    yield return null;
                }
            }
        }
        
    }

    public void HighlightTile(bbPos p)
    {
        DrawTerrainUpdate();

        goMap[p].GetComponent<Renderer>().material.color = Color.red;
        foreach (bbPos n in p.neighbors)
        {
            goMap[n].GetComponent<Renderer>().material.color = Color.red;

        }
    }
}
