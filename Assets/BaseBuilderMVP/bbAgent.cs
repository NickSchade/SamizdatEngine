using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{
    public class bbAgent
    {
        public string name;
        public bbPos pos;
        public bbPos lastPos;
        public List<bbJob> jobQueue;
        public BaseBuilderMVP game;
        public Dictionary<ItemType, int> needTimer;
        public Dictionary<ItemType, int> needQuantity;
        public Dictionary<ItemType, int> inventory;
        public List<ItemType> needsAddressing;
        public bbStructure myHouse;
        public bool alive;
        public bool animating;
        public bbAgent()
        {
            alive = false;
        }
        public bbAgent(string _name, bbPos _pos, BaseBuilderMVP _game, bbStructure _myHouse)
        {
            alive = true;
            animating = false;
            name = _name;
            //Debug.Log("Created Agent named " + name);
            pos = _pos;
            game = _game;
            myHouse = _myHouse;
            jobQueue = new List<bbJob>();
            needTimer = new Dictionary<ItemType, int> { { ItemType.ITEM_FOOD, 50 } };
            needQuantity = new Dictionary<ItemType, int> { { ItemType.ITEM_FOOD, 1 } };
            inventory = new Dictionary<ItemType, int> { { ItemType.ITEM_FOOD, 0 }, { ItemType.ITEM_GOLD, 0 } };
            needsAddressing = new List<ItemType>();
        }
        void AssertNeed(ItemType needType)
        {
            //Debug.Log("Need for " + needType.ToString() + " assigned");
            List<bbJob> needJobs = new List<bbJob>();
            needsAddressing.Add(needType);
            bbJobMoveTo moveToHouse = new bbJobMoveTo(myHouse.getPos());
            needJobs.Add(moveToHouse);
            bbJobUseStructure useHouse = new bbJobUseStructure(myHouse);
            needJobs.Add(useHouse);
            needJobs.AddRange(jobQueue);
            jobQueue = needJobs;
        }
        public void takeTurn()
        {
            if (alive)
            {
                CheckNeeds();
                // Do current job
                if (jobQueue.Count != 0)
                {
                    bbJob currentJob = jobQueue[0];
                    currentJob.doJobTurn(this);
                    if (currentJob.checkJobComplete(this))
                    {
                        jobQueue.Remove(currentJob);
                    }
                }
                else
                {
                    // Assign new job
                    if (pos.game.playerJobQueue.Count == 0)
                    {

                    }
                    else
                    {
                        List<bbJob> jobs = pos.game.playerJobQueue[0];
                        jobQueue = jobs;
                        pos.game.playerJobQueue.Remove(jobs);
                    }
                }
            }
        }
        void CheckNeeds()
        {
            // Adjust Need Timer
            List<ItemType> needs = new List<ItemType>(needTimer.Keys);
            foreach (ItemType needType in needs)
            {
                if (needTimer[needType] > 0)
                {
                    needTimer[needType] -= 1;
                }
            }
            // Check to see if any Needs are triggered
            foreach (ItemType needType in needs)
            {
                if (needTimer[needType] <= 0)
                {
                    if (!needsAddressing.Contains(needType))
                    {
                        AssertNeed(needType);
                    }
                }
            }
        }
        void AssignDefaultJob(bbStructure assignedStructure)
        {
            bbStructure mine = assignedStructure;
            // Move To Mine
            bbJobMoveTo moveToMine = new bbJobMoveTo(mine.getPos());
            jobQueue.Add(moveToMine);
            // Use Mine
            bbJobUseStructure useMine = new bbJobUseStructure(mine);
            jobQueue.Add(useMine);
            // Move To HQ
            bbStructure trystruct = new bbStructureDummy();
            if (pos.findClosestStructureByPath(bbStructureType.STRUCTURE_HQ, ref trystruct))
            {
                bbStructure hq = trystruct;
                bbJobMoveTo moveToHQ = new bbJobMoveTo(hq.getPos());
                jobQueue.Add(moveToHQ);
                // Use HQ
                bbJobUseStructure useHQ = new bbJobUseStructure(hq);
                jobQueue.Add(useHQ);
            }
        }
        public Color getColor()
        {
            return Color.black;
        }

        public bool Deposit()
        {
            List<ItemType> invItems = new List<ItemType>(inventory.Keys);
            foreach (ItemType i in invItems)
            {
                pos.game.playerResources[i] += inventory[i];
                inventory[i] = 0;
            }
            return true;
        }
        public bool Collect(ItemType type, int quantity = 1)
        {
            inventory[type] += quantity;
            return true;
        }

    }

}

