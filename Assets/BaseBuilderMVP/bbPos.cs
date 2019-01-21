using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


    namespace BBMVP
{
    public class bbPos
    {
        public bbLoc gridLoc;
        public bbLoc mapLoc;
        public List<bbPos> neighbors;
        public BaseBuilderMVP game;
        public bbPos(bbLoc _loc, BaseBuilderMVP _game)
        {
            gridLoc = _loc;
            game = _game;
            setMapLoc(game.tileShape);
            neighbors = new List<bbPos>();
        }
        public void setMapLoc(TileShape _tileShape)
        {
            switch (_tileShape)
            {
                case TileShape.SQUARE:
                    mapLoc = new bbLoc(gridLoc.coordinates);
                    break;
                case TileShape.HEX:
                    float x = Mathf.Sqrt(3) * (gridLoc.x() - 0.5f * (gridLoc.y() % 2f)) / 1.9f;
                    float y = (3 / 2) * gridLoc.y() / 1.3f;
                    mapLoc = new bbLoc(x, y);
                    break;
            }
        }
        public static float DistanceMinkowski(bbPos p1, bbPos p2, float d = 2)
        {
            float pSum = 0f;
            for (int i = 0; i < p1.mapLoc.coordinates.Length; i++)
            {
                pSum += Mathf.Pow(Mathf.Abs(p1.mapLoc.coordinates[i] - p2.mapLoc.coordinates[i]), d);
            }
            return Mathf.Pow(pSum, (1 / d));
        }
        public static float DistancePath(bbPos p1, bbPos p2)
        {
            List<bbPos> path = bbPathfinder.findAStarPath(p1, p2);
            float d = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                d += path[i].getMoveToCost(path[i - 1]);
            }
            return d;
        }
        public float getMoveToCost(bbPos moveFrom)
        {
            if (game.currentIsland.lands[this].terrainType == bbTerrainType.MOUNTAIN || game.currentIsland.lands[this].terrainType == bbTerrainType.OCEAN)
            {
                return float.PositiveInfinity;
            }
            else if (neighbors.Contains(moveFrom))
            {
                float distance = DistanceMinkowski(this, moveFrom);
                return distance;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }
        public List<bbPos> findPath(bbPos pathTarget)
        {
            List<bbPos> path = bbPathfinder.findAStarPath(this, pathTarget);
            return path;
        }
        public bbStructure findClosestStructureByRange(bbStructureType structureType)
        {
            List<bbStructure> structuresOfType = getStructuresOfType(structureType);
            float minDistance = float.PositiveInfinity;
            bbStructure closestStructure = structuresOfType[0];
            foreach (bbStructure s in structuresOfType)
            {
                float sDistance = DistanceMinkowski(this, s.getPos());
                if (sDistance < minDistance)
                {
                    minDistance = sDistance;
                    closestStructure = s;
                }
            }
            return closestStructure;
        }
        public bool findClosestStructureByPath(bbStructureType structureType, ref bbStructure closestStructure)
        {
            List<bbStructure> structuresOfType = getStructuresOfType(structureType);
            if (structuresOfType.Count != 0)
            {
                closestStructure = structuresOfType[0];
                float minDistance = float.PositiveInfinity;
                foreach (bbStructure s in structuresOfType)
                {
                    float sDistance = DistancePath(this, s.getPos());
                    if (sDistance < minDistance)
                    {
                        minDistance = sDistance;
                        closestStructure = s;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        List<bbStructure> getStructuresOfType(bbStructureType structureType)
        {
            List<bbStructure> structuresOfType = new List<bbStructure>();
            foreach (bbStructure s in game.currentIsland.structures.Values)
            {
                if (s.getType() == structureType)
                {
                    structuresOfType.Add(s);
                }
            }
            return structuresOfType;
        }
        public void Write()
        {
            Debug.Log(getName());
        }
        public string getName()
        {
            return "[" + gridLoc.x() + "," + gridLoc.y() + "]";
        }
    }

    public static class bbPathfinder
    {
        public static List<bbPos> findAStarPath(bbPos start, bbPos end, int maxIter = 100000)
        {
            Dictionary<bbPos, float> DistanceFromStart = new Dictionary<bbPos, float>();
            Dictionary<bbPos, float> DistanceToEnd = new Dictionary<bbPos, float>();
            Dictionary<bbPos, bbPos> FastestPath = new Dictionary<bbPos, bbPos>();
            List<bbPos> Searched = new List<bbPos>();


            List<bbPos> path = new List<bbPos>();
            if (start != end)
            {
                // Create the queue of pos to check
                List<bbPos> nextStep = new List<bbPos>();
                // Add start pos' neighbors to queue
                foreach (bbPos p in start.neighbors)
                {
                    DistanceFromStart[p] = p.getMoveToCost(start);
                    DistanceToEnd[p] = bbPos.DistanceMinkowski(p, end);
                    FastestPath[p] = start;
                    nextStep.Add(p);
                }

                bool pathFound = false;
                int iter = 0;
                while (!pathFound && iter < maxIter)
                {
                    // Order queue by distance
                    nextStep = nextStep.OrderBy(p => DistanceFromStart[p] + DistanceToEnd[p]).ToList();
                    // Pull next pos to search
                    bbPos thisStep = nextStep[0];
                    //Debug.Log("thisStep is at " + thisStep.loc.x + " , " + thisStep.loc.y);
                    // Mark pos as searched
                    Searched.Add(thisStep);
                    if (thisStep.neighbors.Contains(end))
                    {
                        pathFound = true;
                        bbPos p = end;
                        float newPathCost = p.getMoveToCost(thisStep) + DistanceFromStart[thisStep];
                        if (!DistanceFromStart.ContainsKey(p) || newPathCost < DistanceFromStart[p])
                        {
                            DistanceFromStart[p] = newPathCost;
                            FastestPath[p] = thisStep;
                        }
                        if (DistanceToEnd.ContainsKey(p))
                        {
                            DistanceToEnd[p] = bbPos.DistanceMinkowski(p, end);
                        }
                    }
                    else
                    {
                        foreach (bbPos p in thisStep.neighbors)
                        {
                            float newPathCost = p.getMoveToCost(thisStep) + DistanceFromStart[thisStep];
                            if (!DistanceFromStart.ContainsKey(p) || newPathCost < DistanceFromStart[p])
                            {
                                DistanceFromStart[p] = newPathCost;
                                FastestPath[p] = thisStep;
                            }
                            if (!DistanceToEnd.ContainsKey(p))
                            {
                                DistanceToEnd[p] = bbPos.DistanceMinkowski(p, end);
                            }
                            if (!nextStep.Contains(p) && !Searched.Contains(p))
                            {
                                nextStep.Add(p);
                                //Debug.Log("Added to nextStep Pos at " + p.loc.x + " , " + p.loc.y);
                            }
                        }
                        nextStep.Remove(thisStep);
                        //Debug.Log("Removed from nextStep Pos at " + thisStep.loc.x + " , " + thisStep.loc.y);
                    }
                    iter++;
                }
                //Debug.Log("Completed with " + iter + " / " + maxIter + " iterations.");

                bbPos pathStep = end;
                while (pathStep != start)
                {
                    path.Add(pathStep);
                    if (FastestPath.ContainsKey(pathStep))
                    {
                        pathStep = FastestPath[pathStep];
                    }
                    else
                    {
                        return null;
                    }
                }
                path.Add(start);
                path.Reverse();

            }
            return path;
        }
    }

    public struct bbLoc
    {
        public float[] coordinates;
        public bbLoc(float _x, float _y, float _z = 0)
        {
            coordinates = new float[] { _x, _y, _z };
        }
        public bbLoc(float[] _coordinates)
        {
            coordinates = _coordinates;
        }
        public bbLoc(string _locKey)
        {
            coordinates = _locKey.Split(',').Select(x => float.Parse(x)).ToArray();
        }
        public string key()
        {
            //return coordinates.Select(a => a.ToString()).Aggregate((i, j) => i + "," + j);
            string[] k = coordinates.Select(x => x.ToString()).ToArray();
            return string.Join(",", k);
        }
        public float x()
        {
            try
            {
                return coordinates[0];
            }
            catch
            {
                return 0;
            }
        }
        public float y()
        {
            try
            {
                return coordinates[1];
            }
            catch
            {
                return 0;
            }
        }
        public float z()
        {
            try
            {
                return coordinates[2];
            }
            catch
            {
                return 0;
            }
        }
        public static bbLoc SquareToCube(bbLoc squareLoc)
        {
            float x = squareLoc.y() - (squareLoc.x() - (squareLoc.x() % 2f)) / 2f;
            float z = squareLoc.x();
            float y = -x - z;
            return new bbLoc(x, y, z);
        }
        public static bbLoc CubeToSquare(bbLoc cubeLoc)
        {
            float x = cubeLoc.z();
            float y = cubeLoc.x() + (cubeLoc.z() - (cubeLoc.z() % 2f)) / 2;
            return new bbLoc(x, y);
        }

    }



}