using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

//https://www.codeproject.com/Articles/1221034/Pathfinding-Algorithms-in-Csharp

public class Node
{
    public MapLoc loc;
    public int x, y;
    public Dictionary<Node, float> neighborWeights;
    public List<Node> neighbors;
    public Dictionary<MapLoc, float> distanceTo;
    public Dictionary<MapLoc, float> distanceFrom;
    public Dictionary<MapLoc, Node> NearestTo;
    public Dictionary<PathOD, float> distanceBetween;
    public bool Visited;
    public int iRegion;
    public Node(int _x, int _y)
    {
        x = _x;
        y = _y;
        loc = new MapLoc(_x, _y);
        distanceTo = new Dictionary<MapLoc, float>();
        distanceFrom = new Dictionary<MapLoc, float>();
        distanceBetween = new Dictionary<PathOD, float>();
        neighbors = new List<Node>();
        NearestTo = new Dictionary<MapLoc, Node>();
        neighborWeights = new Dictionary<Node, float>();
    }
}

public struct PathOD
{
    public MapLoc origin, destination;

    public PathOD(MapLoc _origin, MapLoc _destination)
    {
        origin = _origin;
        destination = _destination;
    }
}

public struct Path
{
    public MapLoc originMapLoc, destinationMapLoc;
    public float distancePath, distanceEuclidean;
    public List<Node> path;
    public Path(MapLoc _origin, MapLoc _destination, List<Node> _path, float _distancePath, float _distanceEuclidean)
    {
        originMapLoc = _origin;
        destinationMapLoc = _destination;
        distancePath = _distancePath;
        distanceEuclidean = _distanceEuclidean;
        path = _path;
    }
    
}

public class MapPathfinder {
    List<Node> nodelist = new List<Node>();
    public Dictionary<MapLoc, Node> nodeDict = new Dictionary<MapLoc, Node>();
    public Dictionary<PathOD, Path> PathDict = new Dictionary<PathOD, Path>();
    public MapPathfinder()
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
                nodeDict[new MapLoc(x, y)] = nodes[x, y];
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Node n = nodes[x, y];
                n.Visited = false;
                MapLoc l = new MapLoc(x, y);
                Dictionary<MapLoc, float> d = MapUtil.GetValidNeighbors(_elevation, l);
                foreach (KeyValuePair<MapLoc, float> kvp in d)
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
    public Path GetPath(MapLoc origin, MapLoc destination)
    {
        PathOD p = new PathOD(origin, destination);
        bool pathFound = false;
        if (!PathDict.ContainsKey(p))
        {
            pathFound = AStarSearch(origin, destination);
        }
        return PathDict[p];
    }
    private bool AStarSearch(MapLoc origin, MapLoc destination)
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
    private void AStarInit(out List<Node> prioQueue, MapLoc originMapLoc, MapLoc destinationMapLoc)
    {
        foreach (Node n in nodelist)
        {
            n.distanceTo[destinationMapLoc] = MapUtil.Distance(n.loc, destinationMapLoc);
        }
        nodeDict[originMapLoc].distanceFrom[originMapLoc] = 0f;
        prioQueue = new List<Node>();
        prioQueue.Add(nodeDict[originMapLoc]);
    }
    private void AStarWorkhorse(ref List<Node> prioQueue, ref bool PathFound, MapLoc originMapLoc, MapLoc destinationMapLoc)
    {
        prioQueue = prioQueue.OrderBy(x => x.distanceFrom[originMapLoc] + x.distanceTo[destinationMapLoc]).ToList();
        Node n = prioQueue.First();
        prioQueue.Remove(n);
        foreach (Node neighbor in n.neighbors)
        {
            if (!neighbor.Visited)
            {
                float newCost = n.distanceFrom[originMapLoc] + n.neighborWeights[neighbor];
                if (!neighbor.distanceFrom.ContainsKey(originMapLoc) || newCost < neighbor.distanceFrom[originMapLoc]) // this will cause problems if there is ever no cost between nodes!
                {
                    neighbor.distanceFrom[originMapLoc] = newCost;
                    neighbor.NearestTo[originMapLoc] = n;
                    if (!prioQueue.Contains(neighbor))
                    {
                        prioQueue.Add(neighbor);
                    }
                }

            }
        }

        n.Visited = true;
        if (n.loc.Equals(destinationMapLoc))
        {
            PathFound = true;
        }
    }
    private void AddPath(MapLoc origin, MapLoc destination, bool addBetweenPaths = false)
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
    public delegate float delCost(MapLoc l, MapLoc n, float[,] _elevation, float _seaLevel, float _seaTravelCost);
    public static float CostStandard(MapLoc l, MapLoc n, float[,] _elevation, float _seaLevel, float _seaTravelCost)
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
