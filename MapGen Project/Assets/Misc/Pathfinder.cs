using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

//https://www.codeproject.com/Articles/1221034/Pathfinding-Algorithms-in-Csharp

public class Node
{
    public Loc loc;
    public int x, y;
    public Dictionary<Node, float> neighborWeights;
    public List<Node> neighbors;
    public Dictionary<Loc, float> distanceTo;
    public Dictionary<Loc, float> distanceFrom;
    public Dictionary<Loc, Node> NearestTo;
    public Dictionary<PathOD, float> distanceBetween;
    public bool Visited;
    public int iRegion;
    public Node(int _x, int _y)
    {
        x = _x;
        y = _y;
        loc = new Loc(_x, _y);
        distanceTo = new Dictionary<Loc, float>();
        distanceFrom = new Dictionary<Loc, float>();
        distanceBetween = new Dictionary<PathOD, float>();
        neighbors = new List<Node>();
        NearestTo = new Dictionary<Loc, Node>();
        neighborWeights = new Dictionary<Node, float>();
    }
}

public struct PathOD
{
    public Loc origin, destination;

    public PathOD(Loc _origin, Loc _destination)
    {
        origin = _origin;
        destination = _destination;
    }
}

public struct Path
{
    public Loc originLoc, destinationLoc;
    public float distancePath, distanceEuclidean;
    public List<Node> path;
    public Path(Loc _origin, Loc _destination, List<Node> _path, float _distancePath, float _distanceEuclidean)
    {
        originLoc = _origin;
        destinationLoc = _destination;
        distancePath = _distancePath;
        distanceEuclidean = _distanceEuclidean;
        path = _path;
    }
    
}

public class Pathfinder {
    List<Node> nodelist = new List<Node>();
    public Dictionary<Loc, Node> nodeDict = new Dictionary<Loc, Node>();
    public Dictionary<PathOD, Path> PathDict = new Dictionary<PathOD, Path>();
    public Pathfinder()
    {

    }
    public void SetNodeList(float[,] _elevation, float _seaLevel, delCost fCost, float _seaTravelCost = 0f)
    {
        int xDim = _elevation.GetLength(0);
        int yDim = _elevation.GetLength(1);
        Node[,] nodes = new Node[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                nodes[x, y] = new Node(x, y);
                nodeDict[new Loc(x, y)] = nodes[x, y];
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Node n = nodes[x, y];
                n.Visited = false;
                Loc l = new Loc(x, y);
                Dictionary<Loc, float> d = MapUtil.GetValidNeighbors(_elevation, l);
                foreach (KeyValuePair<Loc, float> kvp in d)
                {
                    n.neighborWeights[nodes[kvp.Key.x, kvp.Key.y]] = fCost(l,kvp.Key,_elevation,_seaLevel,_seaTravelCost);
                    n.neighbors.Add(nodes[kvp.Key.x, kvp.Key.y]);
                }
                nodelist.Add(n);
            }
        }
    }
    public void SetNodeList(List<Node> _nodes)
    {
        nodelist = _nodes;
        foreach (Node n in nodelist)
        {
            nodeDict[n.loc] = n;
        }
    }
    public Path GetPath(Loc origin, Loc destination)
    {
        PathOD p = new PathOD(origin, destination);
        bool pathFound = false;
        if (!PathDict.ContainsKey(p))
        {
            pathFound = AStarSearch(origin, destination);
        }
        return PathDict[p];
    }
    private bool AStarSearch(Loc origin, Loc destination)
    {
        List<Node> prioQueue;
        bool PathFound = false;
        AStarInit(out prioQueue, origin, destination);
        while (prioQueue.Any())
        {
            AStarWorkhorse(ref prioQueue, ref PathFound, origin, destination);
        }
        if (PathFound)
        {
            AddPath(origin, destination);
        }
        return PathFound;
    }
    private void AStarInit(out List<Node> prioQueue, Loc originLoc, Loc destinationLoc)
    {
        foreach (Node n in nodelist)
        {
            n.distanceTo[destinationLoc] = MapUtil.Distance(n.loc, destinationLoc);
        }
        nodeDict[originLoc].distanceFrom[originLoc] = 0f;
        prioQueue = new List<Node>();
        prioQueue.Add(nodeDict[originLoc]);
    }
    private void AStarWorkhorse(ref List<Node> prioQueue, ref bool PathFound, Loc originLoc, Loc destinationLoc)
    {
        prioQueue = prioQueue.OrderBy(x => x.distanceFrom[originLoc] + x.distanceTo[destinationLoc]).ToList();
        Node n = prioQueue.First();
        prioQueue.Remove(n);
        foreach (Node neighbor in n.neighbors)
        {
            if (!neighbor.Visited)
            {
                float newCost = n.distanceFrom[originLoc] + n.neighborWeights[neighbor];
                if (!neighbor.distanceFrom.ContainsKey(originLoc) || newCost < neighbor.distanceFrom[originLoc]) // this will cause problems if there is ever no cost between nodes!
                {
                    neighbor.distanceFrom[originLoc] = newCost;
                    neighbor.NearestTo[originLoc] = n;
                    if (!prioQueue.Contains(neighbor))
                    {
                        prioQueue.Add(neighbor);
                    }
                }

            }
        }

        n.Visited = true;
        if (n.loc.Equals(destinationLoc))
        {
            PathFound = true;
        }
    }
    private void AddPath(Loc origin, Loc destination, bool addBetweenPaths = false)
    {
        float dP = 0f;
        List<Node> shortestPath = new List<Node>();
        Node thisNode = nodeDict[destination];
        shortestPath.Add(thisNode);
        while (!thisNode.loc.Equals(origin))
        {
            dP += thisNode.neighborWeights[thisNode.NearestTo[origin]];
            thisNode = thisNode.NearestTo[origin];
            shortestPath.Add(thisNode);
        }
        shortestPath.Reverse();
        float dE = MapUtil.Distance(origin, destination);
        Path p = new Path(origin, destination, shortestPath, dP, dE);
        PathDict[new PathOD(origin, destination)] = p;
        if (addBetweenPaths)
        {
            List<PathOD> betweenPaths = new List<PathOD>();
            int iWindow = shortestPath.Count - 2;
            while (iWindow > 0)
            {
                for (int i = 0; i < (shortestPath.Count - iWindow); i++)
                {
                    PathOD betweenPath = new PathOD(shortestPath[i].loc, shortestPath[i + iWindow].loc);
                    betweenPaths.Add(betweenPath);
                }
                iWindow--;
            }
            foreach (PathOD pOD in betweenPaths)
            {
                AddPath(pOD.origin, pOD.destination, false);
            }
        }
    }
    public delegate float delCost(Loc l, Loc n, float[,] _elevation, float _seaLevel, float _seaTravelCost);
    public static float CostStandard(Loc l, Loc n, float[,] _elevation, float _seaLevel, float _seaTravelCost)
    {
        float nelev = _elevation[n.x, n.y];
        float lelev = _elevation[l.x, l.y];
        float diff = (nelev > _seaLevel && lelev > _seaLevel) ? Math.Abs(nelev - lelev) : _seaTravelCost;
        float dist = MapUtil.Distance(n, l);
        diff *= dist;
        diff += dist;
        return diff;
    }
}
