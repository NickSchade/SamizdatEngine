using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{
    public enum ItemType { ITEM_GOLD, ITEM_FOOD };
    public enum ClickType { NONE, BUILD_MINE, BUILD_FARM, BUILD_HOUSE, SETTLE_HQ, SETTLE_HOUSE };
    public enum TileShape { SQUARE, HEX };

    public class BaseBuilderMVP
    {
        public int STARTING_FOOD = 10;
        public int STARTING_GOLD = 100;
        int STRUCTURE_COST = 10;
        public float timePerTurn = 0.1f;

        public bbIsland currentIsland;
        public List<bbAgent> playerAgents;
        public Dictionary<ItemType, int> playerResources;
        public List<List<bbJob>> playerJobQueue;
        public BaseBuilderMVPManager gameManager;

        //public Agent currentAgent;
        public ClickType currentClickType;
        public List<ClickType> availableActions;

        public bool paused = true;
        public float nextTurn;
        public float lastTurn;
        public int turnNumber;

        public TileShape tileShape = TileShape.SQUARE;

        public BaseBuilderMVP()
        {
            turnNumber = 0;
            lastTurn = 0;
            nextTurn = Time.time + timePerTurn;
            currentIsland = new bbIsland(this);
            playerAgents = new List<bbAgent>();
            playerResources = new Dictionary<ItemType, int> { { ItemType.ITEM_FOOD, STARTING_FOOD }, { ItemType.ITEM_GOLD, STARTING_GOLD } };
            playerJobQueue = new List<List<bbJob>>();
            availableActions = new List<ClickType> { ClickType.BUILD_FARM, ClickType.BUILD_MINE, ClickType.BUILD_HOUSE };
            currentClickType = availableActions[0];
            InitializeSetup();
        }
        public void InitializeSetup()
        {
            currentClickType = ClickType.SETTLE_HQ;
        }
        public void TakeTurn()
        {
            List<bbAgent> dead = new List<bbAgent>();
            for (int i = 0; i < playerAgents.Count; i++)
            {
                bbAgent agent = playerAgents[i];
                agent.takeTurn();
                if (!agent.alive)
                {
                    dead.Add(agent);
                }
            }
            foreach (bbAgent a in dead)
            {
                playerAgents.Remove(a);
            }
            foreach (bbStructure s in currentIsland.structures.Values)
            {
                s.TakeTurn();
            }
            turnNumber++;
        }
        public bool RealTimeTurn()
        {
            bool needsUpdate = false;
            if (!paused)
            {
                //Debug.Log(Time.time + " < " + nextTurn.ToString() + " ? ");
                if (Time.time > nextTurn)
                {
                    TakeTurn();
                    lastTurn = nextTurn;
                    nextTurn = Time.time + timePerTurn;
                    needsUpdate = true;
                    gameManager.drawer.Animate();
                }
            }
            return needsUpdate;
        }
        public void ToggleAction()
        {
            int index = availableActions.FindIndex(x => x == currentClickType);
            index = (index + 1) % availableActions.Count;
            currentClickType = availableActions[index];
        }
        public bool HandleClick(bbPos pos, bool leftClick, bool rightClick)
        {
            bool clickDidSomething = false;

            if (leftClick)
            {
                if (!currentIsland.structures.ContainsKey(pos))
                {
                    if (CheckBuildable(pos))
                    {
                        if (currentClickType == ClickType.SETTLE_HQ || currentClickType == ClickType.SETTLE_HOUSE)
                        {
                            if (currentClickType == ClickType.SETTLE_HQ)
                            {
                                currentIsland.AddStructure(bbStructureType.STRUCTURE_HQ, pos.gridLoc.x(), pos.gridLoc.y(), currentClickType);
                                currentClickType = ClickType.SETTLE_HOUSE;
                            }
                            else if (currentClickType == ClickType.SETTLE_HOUSE)
                            {
                                currentIsland.AddStructure(bbStructureType.STRUCTURE_HOUSE, pos.gridLoc.x(), pos.gridLoc.y(), currentClickType);
                                currentClickType = ClickType.NONE;
                                paused = false;
                            }
                            clickDidSomething = true;
                        }
                        else if (playerResources[ItemType.ITEM_GOLD] >= STRUCTURE_COST)
                        {
                            playerResources[ItemType.ITEM_GOLD] -= STRUCTURE_COST;
                            ClickType buildingType = ClickType.BUILD_HOUSE;
                            buildingType = currentIsland.lands[pos].terrainFeature == bbTerrainFeature.ARABLE ? ClickType.BUILD_FARM : buildingType;
                            buildingType = currentIsland.lands[pos].terrainFeature == bbTerrainFeature.MINEABLE ? ClickType.BUILD_MINE : buildingType;
                            currentIsland.AddStructure(bbStructureType.STRUCTURE_CONSTRUCTION, pos.gridLoc.x(), pos.gridLoc.y(), buildingType);
                            List<bbJob> jobs = new List<bbJob>();
                            bbStructure site = currentIsland.structures[pos];
                            bbJobMoveTo moveToMine = new bbJobMoveTo(site.getPos());
                            jobs.Add(moveToMine);
                            bbJobUseStructure useMine = new bbJobUseStructure(site);
                            jobs.Add(useMine);
                            playerJobQueue.Add(jobs);
                            clickDidSomething = true;
                        }
                    }
                }
            }


            if (rightClick)
            {
                gameManager.drawer.HighlightTile(pos);
            }

            return clickDidSomething;
        }
        bool SelectAgent(bbPos pos, out bbAgent selectedAgent)
        {
            bool clickDidSomething = false;
            bbAgent agentAtPos;
            if (GetAgentAtPos(pos, out agentAtPos))
            {
                selectedAgent = agentAtPos;
                clickDidSomething = true;
            }
            else
            {
                selectedAgent = null;
            }
            return clickDidSomething;
        }
        bool CheckBuildable(bbPos pos)
        {
            bool buildable = false;
            if (currentIsland.lands[pos].terrainType == bbTerrainType.LAND)
            {
                buildable = true;
            }
            return buildable;
        }
        bool GetAgentAtPos(bbPos pos, out bbAgent myAgent)
        {
            foreach (bbAgent agent in playerAgents)
            {
                if (agent.pos == pos)
                {
                    myAgent = agent;
                    return true;
                }
            }
            myAgent = new bbAgent();
            return false;
        }
    }

}
