using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{
    public class bbIsland
    {
        public BaseBuilderMVP game;
        public Dictionary<string, bbPos> pathMap;
        public Dictionary<bbPos, bbStructure> structures;
        public Dictionary<bbPos, bbLand> lands;

        int N = 5; // DIM = 1+2^N
        public int dim;
        bool wrapEastWest, wrapNorthSouth;

        public bbIsland(BaseBuilderMVP _game)
        {
            wrapEastWest = true;
            wrapNorthSouth = true;
            dim = 1 + (int)Mathf.Pow(2, N);
            game = _game;
            pathMap = GenerateBasicMap(dim);
            lands = InitializeLandsFromMidpointDisplacement(N, pathMap);
            //lands = InitializeLandsFromMapGen(N, pathMap);
            structures = new Dictionary<bbPos, bbStructure>();
        }

        Dictionary<bbPos, bbLand> InitializeLandsFromMapGen(int _N, Dictionary<string, bbPos> pm)
        {
            Dictionary<bbPos, bbLand> landsDict = new Dictionary<bbPos, bbLand>();

            MapGen map = new MapGen(dim, dim);
            map.GenerateMap();
            float[,] elevation = map.Elevation;
            MapUtil.TransformMapMinMax(ref elevation, MapUtil.dNormalize);
            foreach (bbPos p in pm.Values)
            {
                bbLand newLand = new bbLand(p);
                newLand.setFromValue(elevation[(int)p.gridLoc.x(), (int)p.gridLoc.y()]);
                landsDict[p] = newLand;
            }

            return landsDict;

        }
        Dictionary<bbPos, bbLand> InitializeLandsFromMidpointDisplacement(int _N, Dictionary<string, bbPos> pm)
        {
            Dictionary<bbPos, bbLand> landsDict = new Dictionary<bbPos, bbLand>();

            MidpointDisplacement mpd = new MidpointDisplacement(N, wrapEastWest, wrapNorthSouth);
            float[,] elevation = mpd.Elevation;
            MapUtil.TransformMapMinMax(ref elevation, MapUtil.dNormalize);
            foreach (bbPos p in pm.Values)
            {
                bbLand newLand = new bbLand(p);
                newLand.setFromValue(elevation[(int)p.gridLoc.x(), (int)p.gridLoc.y()]);
                landsDict[p] = newLand;
            }

            return landsDict;

        }
        Dictionary<bbPos, bbLand> InitializeLandsRandom(Dictionary<string, bbPos> pm)
        {
            Dictionary<bbPos, bbLand> landsDict = new Dictionary<bbPos, bbLand>();
            foreach (bbPos p in pm.Values)
            {
                bbLand newLand = new bbLand(p);
                newLand.setRandom();
                landsDict[p] = newLand;
            }
            return landsDict;
        }
        void InitializeDummyStructures(int _dim)
        {
            structures = new Dictionary<bbPos, bbStructure>();
            AddStructure(bbStructureType.STRUCTURE_HQ, 1, 1, game.currentClickType);
            AddStructure(bbStructureType.STRUCTURE_MINE, _dim - 2, _dim - 2, game.currentClickType);
            AddStructure(bbStructureType.STRUCTURE_HOUSE, 1, _dim - 2, game.currentClickType);
            AddStructure(bbStructureType.STRUCTURE_HOUSE, 3, _dim - 4, game.currentClickType);
            AddStructure(bbStructureType.STRUCTURE_FARM, _dim - 2, 1, game.currentClickType);
        }
        public void AddStructure(bbStructureType t, float x, float y, ClickType _clickType)
        {
            bbLoc l = new bbLoc(x, y);
            bbStructure s = bbStructureFactory.GenerateStructure(t, pathMap[l.key()], _clickType);
            structures[s.getPos()] = s;
            //Debug.Log("Added Structure at " + s.getPos().getName());
        }

        public Dictionary<string, bbPos> GenerateBasicMap(int _dim)
        {
            Dictionary<string, bbPos> map = Generate2DGrid(_dim);
            map = SetNeighborsFor2DGrid(map, _dim, game.tileShape, wrapEastWest, wrapNorthSouth);
            return map;
        }
        private Dictionary<string, bbPos> Generate2DGrid(int _dim)
        {
            Dictionary<string, bbPos> map = new Dictionary<string, bbPos>();

            for (int x = 0; x < _dim; x++)
            {
                for (int y = 0; y < _dim; y++)
                {
                    bbLoc l = new bbLoc(x, y);
                    bbPos p = new bbPos(l, game);
                    map[p.gridLoc.key()] = p;
                }
            }

            return map;
        }
        List<bbPos> SetNeighborsSquare(bbPos p, Dictionary<string, bbPos> map, int _dim, bool _wrapEastWest = false, bool _wrapNorthSouth = false)
        {
            float x = p.gridLoc.x();
            float y = p.gridLoc.y();
            List<bbPos> neighbors = new List<bbPos>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {

                        float X = _wrapEastWest ? (_dim + x + i) % _dim : x + i;
                        float Y = _wrapNorthSouth ? (_dim + y + j) % _dim : y + j;

                        bbLoc l2 = new bbLoc(X, Y);
                        if (map.ContainsKey(l2.key()))
                        {
                            neighbors.Add(map[l2.key()]);
                        }
                        else
                        {
                            Debug.Log("Map doesn't contain " + l2.x() + "," + l2.y());
                        }
                    }
                }
            }
            return neighbors;
        }
        List<bbPos> SetNeighborsHex(bbPos p, Dictionary<string, bbPos> map, int _dim, bool _wrapEastWest = false, bool _wrapNorthSouth = false)
        {
            List<bbPos> neighbors = new List<bbPos>();

            List<int[]> hexNeighbors = new List<int[]>();
            if (p.gridLoc.y() % 2 == 0)
            {
                hexNeighbors.Add(new int[] { 1, 0 });
                hexNeighbors.Add(new int[] { 1, -1 });
                hexNeighbors.Add(new int[] { 0, -1 });
                hexNeighbors.Add(new int[] { -1, 0 });
                hexNeighbors.Add(new int[] { 0, 1 });
                hexNeighbors.Add(new int[] { 1, 1 });
            }
            else
            {

                hexNeighbors.Add(new int[] { 1, 0 });
                hexNeighbors.Add(new int[] { -1, -1 });
                hexNeighbors.Add(new int[] { 0, -1 });
                hexNeighbors.Add(new int[] { -1, 0 });
                hexNeighbors.Add(new int[] { 0, 1 });
                hexNeighbors.Add(new int[] { -1, 1 });
            }


            float x = p.gridLoc.x();
            float y = p.gridLoc.y();
            for (int k = 0; k < hexNeighbors.Count; k++)
            {
                int i = hexNeighbors[k][0];
                int j = hexNeighbors[k][1];
                float X = _wrapEastWest ? (_dim + x + i) % _dim : x + i;
                float Y = _wrapNorthSouth ? (_dim + y + j) % _dim : y + j;

                bbLoc l2 = new bbLoc(X, Y);
                if (map.ContainsKey(l2.key()))
                {
                    neighbors.Add(map[l2.key()]);
                }
                else
                {
                    Debug.Log("Map doesn't contain " + l2.x() + "," + l2.y());
                }
            }

            return neighbors;
        }
        private Dictionary<string, bbPos> SetNeighborsFor2DGrid(Dictionary<string, bbPos> map, int _dim, TileShape tileShape, bool _wrapEastWest = false, bool _wrapNorthSouth = false)
        {
            foreach (string k in map.Keys)
            {
                bbPos p = map[k];
                if (tileShape == TileShape.SQUARE)
                {
                    p.neighbors = SetNeighborsSquare(p, map, _dim, _wrapEastWest, _wrapNorthSouth);
                }
                else if (tileShape == TileShape.HEX)
                {
                    p.neighbors = SetNeighborsHex(p, map, _dim, _wrapEastWest, _wrapNorthSouth);
                }

            }
            return map;
        }
    }

}
