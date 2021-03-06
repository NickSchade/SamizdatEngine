﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPainter {

    List<Node> nodelist = new List<Node>();
    Dictionary<Loc, Node> nodedict = new Dictionary<Loc, Node>();

    Dictionary<int, List<Loc>> areas = new Dictionary<int, List<Loc>>();
    List<Loc>[] landbodies;
    List<Loc>[] waterbodies;
    int xDim, yDim;
    float[,] elevation;
    float seaLevel;

    public MapPainter(float[,] _elevation, float _seaLevel)
    {
        nodelist = new List<Node>();
        elevation = _elevation;
        seaLevel = _seaLevel;
        xDim = _elevation.GetLength(0);
        yDim = _elevation.GetLength(1);
        Node[,] nodes = new Node[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                nodes[x, y] = new Node(x, y);
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
                    if ((_elevation[x, y] > _seaLevel && _elevation[kvp.Key.x, kvp.Key.y] > _seaLevel)
                        || (_elevation[x, y] < _seaLevel && _elevation[kvp.Key.x, kvp.Key.y] < _seaLevel))
                    {
                        n.neighborWeights[nodes[kvp.Key.x, kvp.Key.y]] = MapUtil.Distance(kvp.Key, l);
                        n.neighbors.Add(nodes[kvp.Key.x, kvp.Key.y]);
                    }
                }
                nodelist.Add(n);
                nodedict[n.loc] = n;
            }
        }
        Paint();
    }
    
    public void Paint()
    {
        int iColor = 0;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Loc l = new Loc(x, y);
                Node n = nodedict[l];
                iColor = n.Visited ? iColor : iColor + 1;
                AssignColorToNodeAndNeighbors(n, iColor);
            }
        }
        BuildAreasDictionaries();
    }
    public void AssignColorToNodeAndNeighbors(Node n, int iColor)
    {
        if (!n.Visited)
        {
            n.Visited = true;
            n.iRegion = iColor;
            foreach (Node neighbor in n.neighbors)
            {
                AssignColorToNodeAndNeighbors(neighbor, iColor);
            }
        }
    }

    public void BuildAreasDictionaries()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                Loc l = new Loc(x, y);
                Node n = nodedict[l];
                int iColor = n.iRegion;
                if (!areas.ContainsKey(iColor))
                {
                    areas[iColor] = new List<Loc>();
                }
                areas[iColor].Add(l);
            }
        }
        List<List<Loc>> waterBodies = new List<List<Loc>>();
        List<List<Loc>> landBodies = new List<List<Loc>>();
        foreach (KeyValuePair<int,List<Loc>> kvp in areas)
        {
            int iColor = kvp.Key;
            List<Loc> locsInArea = kvp.Value;
            Loc l0 = locsInArea[0];
            float e = elevation[l0.x, l0.y];
            if (e > seaLevel)
            {
                landBodies.Add(locsInArea);
            }
            else
            {
                waterBodies.Add(locsInArea);
            }
        }
        waterbodies = waterBodies.ToArray();
        landbodies = landBodies.ToArray();
    }
    public float[,] BuildRegions()
    {
        float[,] regions = new float[xDim, yDim];
        foreach (Node n in nodelist)
        {
            regions[n.loc.x, n.loc.y] = (float)n.iRegion;
        }
        return regions;
    }
}
