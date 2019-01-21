using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{

    public enum bbStructureType { STRUCTURE_HQ, STRUCTURE_MINE, STRUCTURE_FARM, STRUCTURE_HOUSE, STRUCTURE_CONSTRUCTION };

    public interface bbStructure
    {
        Color getColor();
        bbPos getPos();
        bbStructureType getType();
        bool UseStructure(bbAgent a);
        void TakeTurn();
    }

    public static class bbStructureFactory
    {
        public static bbStructure GenerateStructure(bbStructureType _type, bbPos _pos, ClickType _clickType)
        {
            if (_type == bbStructureType.STRUCTURE_HQ)
            {
                return new bbStructureHQ(_type, _pos);
            }
            else if (_type == bbStructureType.STRUCTURE_MINE)
            {
                return new bbStructureMine(_type, _pos);
            }
            else if (_type == bbStructureType.STRUCTURE_FARM)
            {
                return new bbStructureFarm(_type, _pos);
            }
            else if (_type == bbStructureType.STRUCTURE_HOUSE)
            {
                return new bbStructureHouse(_type, _pos);
            }
            else if (_type == bbStructureType.STRUCTURE_CONSTRUCTION)
            {
                return new bbStructureConstruction(_type, _pos, _clickType);
            }
            else
            {
                return new bbStructureEx(_type, _pos);
            }
        }
    }

    public abstract class bbStructureBasic
    {
        public bbPos pos;
        public bbStructureType structureType;
        public bbPos getPos()
        {
            return pos;
        }
        public bbStructureType getType()
        {
            return structureType;
        }
        public virtual Color getColor()
        {
            return Color.black;
        }
        public virtual void TakeTurn()
        {

        }
    }

    public class bbStructureDummy : bbStructureBasic, bbStructure
    {
        public bbStructureDummy()
        {

        }
        public bool UseStructure(bbAgent a)
        {
            return true;
        }
    }

    public class bbStructureConstruction : bbStructureBasic, bbStructure
    {
        ClickType clickType;
        public bbStructureConstruction(bbStructureType _type, bbPos _pos, ClickType _clickType)
        {
            pos = _pos;
            structureType = _type;
            clickType = _clickType;
        }
        public override Color getColor()
        {
            return Color.magenta;
        }
        public bool UseStructure(bbAgent a)
        {
            // replace this with the structure
            pos.game.currentIsland.structures.Remove(pos);
            pos.game.currentIsland.AddStructure(getStructureTypeFromClickType(), pos.gridLoc.x(), pos.gridLoc.y(), clickType);
            return true;
        }
        bbStructureType getStructureTypeFromClickType()
        {
            if (clickType == ClickType.BUILD_FARM)
            {
                return bbStructureType.STRUCTURE_FARM;
            }
            else if (clickType == ClickType.BUILD_MINE)
            {
                return bbStructureType.STRUCTURE_MINE;
            }
            else if (clickType == ClickType.BUILD_HOUSE)
            {
                return bbStructureType.STRUCTURE_HOUSE;
            }
            else
            {
                return bbStructureType.STRUCTURE_CONSTRUCTION;
            }
        }
    }

    public class bbStructureHQ : bbStructureBasic, bbStructure
    {
        public bbStructureHQ(bbStructureType _type, bbPos _pos)
        {
            pos = _pos;
            structureType = _type;
        }
        public bool UseStructure(bbAgent a)
        {
            return a.Deposit();
        }
        public override Color getColor()
        {
            return Color.blue;
        }
    }

    public class bbStructureFarm : bbStructureResource, bbStructure
    {
        public bbStructureFarm(bbStructureType _type, bbPos _pos)
        {
            pos = _pos;
            structureType = _type;
            hasResource = false;
        }
        public override bool UseStructure(bbAgent a)
        {
            hasResource = false;
            jobQueued = false;
            return a.Collect(ItemType.ITEM_FOOD);
        }
    }

    public class bbStructureMine : bbStructureResource, bbStructure
    {
        public bbStructureMine(bbStructureType _type, bbPos _pos)
        {
            pos = _pos;
            structureType = _type;
            hasResource = false;
        }
        public override bool UseStructure(bbAgent a)
        {
            hasResource = false;
            jobQueued = false;
            return a.Collect(ItemType.ITEM_GOLD);
        }
    }

    public abstract class bbStructureResource : bbStructureBasic, bbStructure
    {
        public bool hasResource;
        public bool jobQueued;
        int resourceTimer = 0;
        int resourceCooldown = 50;
        public abstract bool UseStructure(bbAgent a);
        public override void TakeTurn()
        {
            if (!hasResource)
            {
                resourceTimer++;
                if (resourceTimer >= resourceCooldown)
                {
                    resourceTimer = 0;
                    hasResource = true;
                    jobQueued = false;
                }
            }
            if (hasResource && !jobQueued)
            {
                pos.game.gameManager.drawer.DrawEffectResource(this);
                List<bbJob> jobQueue = new List<bbJob>();
                bbJobMoveTo moveToHere = new bbJobMoveTo(getPos());
                jobQueue.Add(moveToHere);
                bbJobUseStructure useThis = new bbJobUseStructure(this);
                jobQueue.Add(useThis);
                bbStructure trystruct = new bbStructureDummy();
                if (pos.findClosestStructureByPath(bbStructureType.STRUCTURE_HQ, ref trystruct))
                {
                    bbStructure hq = trystruct;
                    bbJobMoveTo moveToHQ = new bbJobMoveTo(hq.getPos());
                    jobQueue.Add(moveToHQ);
                    bbJobUseStructure useHQ = new bbJobUseStructure(hq);
                    jobQueue.Add(useHQ);
                }
                pos.game.playerJobQueue.Add(jobQueue);
                jobQueued = true;
            }
        }
        public override Color getColor()
        {
            return hasResource ? Color.black : Color.white;
        }

    }

    public class bbStructureHouse : bbStructureBasic, bbStructure
    {
        bool spawned;
        public bbStructureHouse(bbStructureType _type, bbPos _pos)
        {
            pos = _pos;
            structureType = _type;
            spawned = false;
        }
        public override void TakeTurn()
        {
            if (!spawned)
            {
                spawned = true;
                bbAgent newAgent = new bbAgent("Agent" + (1 + pos.game.playerAgents.Count), pos, pos.game, this);
                pos.game.playerAgents.Add(newAgent);
            }
        }
        public bool UseStructure(bbAgent a)
        {
            if (pos.game.playerResources[ItemType.ITEM_FOOD] >= a.needQuantity[ItemType.ITEM_FOOD])
            {
                pos.game.playerResources[ItemType.ITEM_FOOD] -= a.needQuantity[ItemType.ITEM_FOOD];
                a.needTimer[ItemType.ITEM_FOOD] = 50;
                a.needsAddressing.Remove(ItemType.ITEM_FOOD);
                return true;
            }
            else
            {
                a.alive = false;
                Debug.Log(a.name + " died of starvation");
                return false;
            }
        }
        public override Color getColor()
        {
            return Color.red;
        }
    }

    public class bbStructureEx : bbStructureBasic, bbStructure
    {

        public bbStructureEx(bbStructureType _structureType, bbPos _pos)
        {
            structureType = _structureType;
            pos = _pos;
        }
        public bool UseStructure(bbAgent a)
        {
            return true;
        }
    }

}