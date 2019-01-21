using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SamizdatEngine.GE.Basic
{
    public class GEDrawerBasic : MonoBehaviour, GEDrawer
    {
        public GameObject pfStone, pfIntersection, pfStarPoint;
        public GoEsque game { get; set; }
        public Dictionary<Stone, GameObject> stones { get; set; }
        public Dictionary<Intersection, GameObject> intersections { get; set; }

        public void DrawInit(GoEsque _game)
        {
            game = _game;
            stones = new Dictionary<Stone, GameObject>();
            intersections = new Dictionary<Intersection, GameObject>();
            Draw();
            Loc MidLoc = new Loc(Mathf.Ceil(game.dims[0] / 2) - 1, Mathf.Ceil(game.dims[1] / 2) - 1);
           // Debug.Log(MidLoc.key());
            Vector3 p = intersections[game.intersections[MidLoc.key()]].transform.position;
            if (game.tileShape == TileShape.SQUARE)
            {
                Camera.main.transform.position = new Vector3(p.x, 10, p.z);
                Camera.main.orthographicSize = game.dims[0] / 1.75f;
            }
            else
            {
                Camera.main.transform.position = new Vector3(p.x * 3 / 2, 10, p.z * 3 / 2);
                Camera.main.orthographicSize = (3 / 2) * game.dims[0] / 1.75f;
            }
        }

        public void Draw()
        {
            DrawStones();
            DrawIntersections();
        }

        void DrawStones()
        {
            //Debug.Log(game);
            if (game.stones.Count != 0)
            {
                Debug.Log("There are " + game.stones.Count + " stones to draw");
                foreach (Stone s in game.stones.Values)
                {
                    if (!stones.ContainsKey(s))
                    {
                        Color c = s.player.color;
                        GameObject go = InstantiateGo(pfStone, s.intersection.pos.mapLoc, c);
                        stones[s] = go;
                    }
                }
            }
            List<Stone> CheckStones = new List<Stone>(stones.Keys);
            foreach (Stone s in CheckStones)
            {
                if (!game.stones.ContainsKey(s.intersection))
                {
                    Destroy(stones[s]);
                    stones.Remove(s);
                }
            }
        }
        void DrawIntersections()
        {
            drawIntersectionsBase();
            drawIntersectionLines();
            drawIntersectionStarPoints();
        }
        void drawIntersectionStarPoints()
        {
            int c = game.dims[0] > 11 ? 3 : 2;
            List<Loc> StarPoints = new List<Loc>();
            StarPoints.Add(new Loc(c, c));
            StarPoints.Add(new Loc(c, game.dims[1] - c - 1));
            StarPoints.Add(new Loc(game.dims[0] - c - 1, game.dims[1] - c - 1));
            StarPoints.Add(new Loc(game.dims[0] - c - 1, c));
            StarPoints.Add(new Loc(Mathf.Ceil(game.dims[1] / 2), c));
            StarPoints.Add(new Loc(c, Mathf.Ceil(game.dims[1] / 2)));
            StarPoints.Add(new Loc(game.dims[0] - c - 1, Mathf.Ceil(game.dims[1] / 2)));
            StarPoints.Add(new Loc(Mathf.Ceil(game.dims[0] / 2), game.dims[1] - c - 1));
            StarPoints.Add(new Loc(Mathf.Ceil(game.dims[0] / 2), Mathf.Ceil(game.dims[1] / 2)));

            foreach (Loc l in StarPoints)
            {
                Vector3 p = game.intersections[l.key()].pos.gameLoc;
                p = new Vector3(p.x, p.y + 0.01f, p.z);
                GameObject go = InstantiateGo(pfStarPoint, p, Color.black);
            }
        }
        void drawIntersectionLines()
        {
            foreach (Intersection i in game.intersections.Values)
            {
                //Debug.Log(i + "'s neighbors count = " + i.neighbors.Count);
                foreach (Intersection j in i.neighbors)
                {
                    //Debug.Log("Drawing");
                    Vector3 posi = intersections[i].transform.position;
                    Vector3 posj = intersections[j].transform.position;
                    posi = new Vector3(posi.x, posi.y+0.01f, posi.z);
                    posj = new Vector3(posj.x, posj.y+0.01f, posj.z);
                    DrawLine(posi, posj, Color.black);
                }
            }
        }
        void drawIntersectionsBase()
        {
            foreach (Intersection i in game.intersections.Values)
            {
                if (!intersections.ContainsKey(i))
                {
                    intersections[i] = InstantiateGo(pfIntersection, i.pos.mapLoc, Color.gray);
                    intersections[i].GetComponentInChildren<GEClickable>().pos = i.pos;
                }
            }
        }
        public GameObject InstantiateGo(GameObject pf, Loc l, Color c)
        {
            Vector3 pos = new Vector3(l.x(), l.z(), l.y());
            GameObject go = Instantiate(pf, pos, Quaternion.identity);
            go.GetComponentInChildren<Renderer>().material.color = c;
            return go;
        }
        public GameObject InstantiateGo(GameObject pf, Vector3 pos, Color c)
        {
            GameObject go = Instantiate(pf, pos, Quaternion.identity);
            go.GetComponentInChildren<Renderer>().material.color = c;
            return go;
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.startColor = color;
            lr.endColor = color;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        public void SetBasicColors()
        {
            foreach (GameObject go in intersections.Values)
            {
                go.GetComponentInChildren<Renderer>().material.color = Color.gray;
            }
        }
        public void SetTerritoryColors(Dictionary<Intersection,Player> territory)
        {
            foreach (Intersection i in territory.Keys)
            {
                Player p = territory[i];
                GameObject go = intersections[i];
                go.GetComponentInChildren<Renderer>().material.color = p.color;
            }
        }
    }
}
