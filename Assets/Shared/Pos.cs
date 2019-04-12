using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace SamizdatEngine
{
    public enum TileShape { SQUARE, HEX };

    public class Pos
    {
        public Loc gridLoc { get; set; } // the location on a "square grid"
        public Loc mapLoc { get; set; } // the location in physical space ie same for squares, but hex' mapLoc is offset on odd rows, etc.
        public List<Pos> neighbors { get; set; }
        public Vector3 gameLoc
        {
            get
            {
                return (new Vector3(mapLoc.x(), mapLoc.z(), mapLoc.y()));
            }
            set
            {

            }
        }


        public static float DistanceMinkowski(Pos p1, Pos p2, float d = 2)
        {
            float pSum = 0f;
            for (int i = 0; i < p1.mapLoc.coordinates.Length; i++)
            {
                pSum += Mathf.Pow(Mathf.Abs(p1.mapLoc.coordinates[i] - p2.mapLoc.coordinates[i]), d);
            }
            return Mathf.Pow(pSum, (1 / d));
        }
        public static float DistancePath(Pos p1, Pos p2)
        {
            List<Pos> path = Pathfinder.findAStarPath(p1, p2);
            float d = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                d += path[i].getMoveToCost(path[i - 1]);
            }
            return d;
        }
        public static int EdgeCount(Pos p, int[] dims)
        {
            int count = 0;

            for (int i = 0; i < dims.Length; i++)
            {
                if (p.gridLoc.coordinates[i] == 0 || p.gridLoc.coordinates[i] == (dims[i]-1))
                {
                    count++;
                }
            }

            return count;
        }

        public Pos(Loc _loc, TileShape _tileShape)
        {
            gridLoc = _loc;
            neighbors = new List<Pos>();
            setMapLoc(_tileShape);
        }
        public void setMapLoc(TileShape _tileShape)
        {
            switch (_tileShape)
            {
                case TileShape.SQUARE:
                    mapLoc = new Loc(gridLoc.coordinates);
                    break;
                case TileShape.HEX:
                    float x = Mathf.Sqrt(3) * (gridLoc.x() - 0.5f * (gridLoc.y() % 2f)) / 1.9f;
                    float y = (3 / 2) * gridLoc.y() / 1.3f;
                    mapLoc = new Loc(x, y);
                    break;
            }
        }
        public float getMoveToCost(Pos moveFrom)
        {
            if (neighbors.Contains(moveFrom))
            {
                float distance = DistanceMinkowski(this, moveFrom);
                return distance;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }
        public List<Pos> findPath(Pos pathTarget)
        {
            List<Pos> path = Pathfinder.findAStarPath(this, pathTarget);
            return path;
        }
        public void Write()
        {
            Debug.Log(getName());
        }
        public string getName()
        {
            return "[" + gridLoc.x() + "," + gridLoc.y() + "]";
        }

        public Pos GetThisPos()
        {
            return this;
        }
    }

    public static class Pathfinder
    {
        public static List<Pos> findAStarPath(Pos start, Pos end, int maxIter = 100000)
        {
            Dictionary<Pos, float> DistanceFromStart = new Dictionary<Pos, float>();
            Dictionary<Pos, float> DistanceToEnd = new Dictionary<Pos, float>();
            Dictionary<Pos, Pos> FastestPath = new Dictionary<Pos, Pos>();
            List<Pos> Searched = new List<Pos>();


            List<Pos> path = new List<Pos>();
            if (start != end)
            {
                // Create the queue of pos to check
                List<Pos> nextStep = new List<Pos>();
                // Add start pos' neighbors to queue
                foreach (Pos p in start.neighbors)
                {
                    DistanceFromStart[p] = p.getMoveToCost(start);
                    DistanceToEnd[p] = Pos.DistanceMinkowski(p, end);
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
                    Pos thisStep = nextStep[0];
                    //Debug.Log("thisStep is at " + thisStep.loc.x + " , " + thisStep.loc.y);
                    // Mark pos as searched
                    Searched.Add(thisStep);
                    if (thisStep.neighbors.Contains(end))
                    {
                        pathFound = true;
                        Pos p = end;
                        float newPathCost = p.getMoveToCost(thisStep) + DistanceFromStart[thisStep];
                        if (!DistanceFromStart.ContainsKey(p) || newPathCost < DistanceFromStart[p])
                        {
                            DistanceFromStart[p] = newPathCost;
                            FastestPath[p] = thisStep;
                        }
                        if (DistanceToEnd.ContainsKey(p))
                        {
                            DistanceToEnd[p] = Pos.DistanceMinkowski(p, end);
                        }
                    }
                    else
                    {
                        foreach (Pos p in thisStep.neighbors)
                        {
                            float newPathCost = p.getMoveToCost(thisStep) + DistanceFromStart[thisStep];
                            if (!DistanceFromStart.ContainsKey(p) || newPathCost < DistanceFromStart[p])
                            {
                                DistanceFromStart[p] = newPathCost;
                                FastestPath[p] = thisStep;
                            }
                            if (!DistanceToEnd.ContainsKey(p))
                            {
                                DistanceToEnd[p] = Pos.DistanceMinkowski(p, end);
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

                Pos pathStep = end;
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

    public struct Loc
    {
        public float[] coordinates;
        public Loc(float _x, float _y, float _z)
        {
            coordinates = new float[] { _x, _y, _z };
        }
        public Loc(float _x, float _y)
        {
            coordinates = new float[] { _x, _y};
        }

        public Loc(float[] _coordinates)
        {
            coordinates = _coordinates;
        }
        public Loc(string _locKey)
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
        public static Loc SquareToCube(Loc squareLoc)
        {
            float x = squareLoc.y() - (squareLoc.x() - (squareLoc.x() % 2f)) / 2f;
            float z = squareLoc.x();
            float y = -x - z;
            return new Loc(x, y, z);
        }
        public static Loc CubeToSquare(Loc cubeLoc)
        {
            float x = cubeLoc.z();
            float y = cubeLoc.x() + (cubeLoc.z() - (cubeLoc.z() % 2f)) / 2;
            return new Loc(x, y);
        }

    }


}
