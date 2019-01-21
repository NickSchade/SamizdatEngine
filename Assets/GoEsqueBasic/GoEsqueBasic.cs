using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;
using SamizdatEngine.GE;
using System.Linq;

namespace SamizdatEngine.GE.Basic
{
    public class GoEsqueBasic : GoEsque
    {
        enum TurnOutcome { SUCESS, FAILURE };

        public TileShape tileShape { get; set; }
        public List<Player> players { get; set; }
        public Player currentPlayer { get; set; }
        public GEDrawer drawer { get; set; }
        public GEUI uihandler { get; set; }
        //public RuleManager ruler { get; set; }
        public int turn { get; set; }
        public int[] dims { get; set; }
        public Dictionary<int, string> serialization;
        public Dictionary<string, Intersection> intersections { get; set; }
        public Dictionary<Intersection, Stone> stones { get; set; }
        List<string> serializations = new List<string>();
        bool gameRunning = true;
        bool territoryView = false;
        Dictionary<Player,bool> playerPassed;

        public void Pass()
        {
            if (gameRunning)
            {
                Debug.Log(currentPlayer.name + " Passed");
                if (AllPlayersPassed())
                {
                    EndGame();
                }
                else
                {
                    playerPassed[currentPlayer] = true;
                    AdvanceTurn();
                }
            }            
        }
        bool AllPlayersPassed()
        {
            bool allPassed = true;
            foreach (Player p in players)
            {
                if (playerPassed[p] == false)
                {
                    allPassed = false;
                    break;
                }
            }
            return allPassed;
        }
        void EndGame()
        {
            Debug.Log("End Game");
            gameRunning = false;
        }
        public void ClickPos(Pos p)
        {
            if (gameRunning)
            {
                Intersection i = intersections[p.gridLoc.key()];
                TurnOutcome outcome = TryPlay(i);
                if (outcome == TurnOutcome.SUCESS)
                {
                    playerPassed[currentPlayer] = false;
                    AdvanceTurn();
                }
            }
        }
        public GoEsqueBasic(GEDrawer _drawer, GEUI _geui)
        {
            //ruler = new RuleManagerBasic(this);
            intersections = new Dictionary<string, Intersection>();
            stones = new Dictionary<Intersection, Stone>();
            serialization = new Dictionary<int, string>();
            tileShape = TileShape.SQUARE;
            drawer = _drawer;
            uihandler = _geui;
            //InitializeGame(new int[] { 19, 19 });
            InitializeGame(new int[] { 9, 9 });
        }

        void InitializeGame(int[] _dims)
        {
            dims = _dims;
            SetIntersections();
            SetNeighbors();
            SetPlayers();
        }
        void SetPlayers()
        {
            players = new List<Player>();
            players.Add(new PlayerBasic("B", Color.black));
            players.Add(new PlayerBasic("W", Color.white));
            currentPlayer = players[0];
            playerPassed = new Dictionary<Player, bool>();
            foreach (Player p in players)
            {
                playerPassed[p] = false;
            }
        }
        void SetIntersections()
        {
            for (int x = 0; x < dims[0]; x++)
            {
                for (int y = 0; y < dims[1]; y++)
                {
                    Loc newLoc = new Loc(x, y);
                    IntersectionBasic initIntersection = new IntersectionBasic(newLoc, tileShape);
                    intersections[newLoc.key()] = initIntersection;
                }
            }
        }
        void SetNeighbors()
        {
            foreach (Intersection i in intersections.Values)
            {
                if (tileShape == TileShape.SQUARE)
                {
                    i.neighbors = SetNeighborsSquare(i, intersections, false);
                }
            }
        }

        public void TerritoryView()
        {
            territoryView = !territoryView;
            if (territoryView)
            {
                //Debug.Log("Territory View Toggled On");
                Dictionary<Intersection, Player> territories = GetTerritories();
                Dictionary<Player, int> score = new Dictionary<Player, int>();
                foreach (Player p in players)
                {
                    score[p] = p.capturedStones.Count;
                }
                foreach (Player p in territories.Values)
                {
                    score[p]++;
                }
                foreach (Player p in score.Keys)
                {
                    Debug.Log(p.name + " has " + score[p] + "points");
                }
                drawer.SetTerritoryColors(territories);
                //Debug.Log("There are " + territories.Count + " territories with owners");
            }
            else
            {
                drawer.SetBasicColors();
                //Debug.Log("Territory View Toggled Off");
            }
        }
        
        List<Stone> GetDeadStones()
        {
            //Debug.Log("Getting Dead Stones");
            List<Stone> deadStones = new List<Stone>();
            foreach (Stone s in stones.Values)
            {
                if (!s.IsAlive()) { deadStones.Add(s); }
            }
            return deadStones;
        }
        void AdvanceTurn()
        {
            int ci = (1 + players.IndexOf(currentPlayer)) % players.Count;
            currentPlayer = players[ci];
        }
        TurnOutcome TryPlay(Intersection p)
        {
            if (!IllegalSpace(p))
            {
                StoneBasic tryStone = new StoneBasic(p, this);
                if (IllegalMove(tryStone, p))
                {
                    tryStone.RemoveStoneFromGame();
                    Debug.Log("Failure");
                    return TurnOutcome.FAILURE;
                }
                else
                {
                    PlaceStone(tryStone);
                    //Debug.Log("Success");
                    return TurnOutcome.SUCESS;
                }
            }
            else
            {
                Debug.Log("Illegal Space");
                return TurnOutcome.FAILURE;
            }

        }
        bool IllegalMove(Stone s, Intersection p)
        {
            Debug.Log("Checking illegal");
            if (SuicidalMove(s))
            {
                Debug.Log("Suicidal Move");
                return true;
            }
            else if (KoViolation())
            {
                Debug.Log("Ko Violation");
                return true;
            }
            else
            {
                Debug.Log("Legal Move");
                return false;
            }
        }
        bool IllegalSpace(Intersection _intersection)
        {
            return _intersection.occupant != null;
        }
        bool SuicidalMove(Stone s)
        {
            List<Stone> killedStones = GetDeadStones();
            Dictionary<Player, List<Stone>> playerDeadStones = new Dictionary<Player, List<Stone>>();
            foreach (Player p in players)
            {
                playerDeadStones[p] = killedStones.Where(k => k.player == p).ToList();
            }
            if (!playerDeadStones.ContainsKey(currentPlayer) || playerDeadStones[currentPlayer].Count == 0)
            {
                return false;
            }
            foreach (Player p in players)
            {
                if (p != currentPlayer)
                {
                    if (playerDeadStones.ContainsKey(p) && playerDeadStones[p].Count > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        bool KoViolation()
        {
            bool violation = false;
            string s = SerializeState();
            if (serializations.Count > 0)
            {
                Debug.Log("Checking for " + s + " in " + serializations[serializations.Count - 1]);
            }
            violation = serializations.Contains(s);
            serializations.Add(SerializeState());
            return violation;
        }
        string SerializeState()
        {
            List<string> series = new List<string>();
            int xDim = dims[0];
            int yDim = dims[1];
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    Loc l = new Loc(x, y);
                    Intersection i = intersections[l.key()];
                    if (i.occupant != null)
                    {
                        series.Add(i.serialize);
                    }
                }
            }
            return string.Join(",", series.ToArray());
        }
        void PlaceStone(Stone s)
        {
            //Debug.Log("Placing Stone" + s.player + ";"+ s.intersection.serialize);
            List<Stone> killedStones = GetDeadStones().Where((x) => x.player != s.player).ToList();
            Debug.Log("After " + s.intersection.serialize + ", #DeadStones = " + killedStones.Count.ToString());

            foreach (Stone k in killedStones)
            {
                k.RemoveStoneFromGame();
            }
        }
        List<Pos> SetNeighborsSquare(Intersection p, Dictionary<string, Intersection> map, bool _allowDiagonals, bool _wrapEastWest = false, bool _wrapNorthSouth = false)
        {
            float x = p.pos.gridLoc.x();
            float y = p.pos.gridLoc.y();
            List<Pos> neighbors = new List<Pos>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (((i != 0 || j != 0) && _allowDiagonals) || ((i != 0 || j != 0) && !(i != 0 && j != 0)))
                    {

                        float X = _wrapEastWest ? (dims[0] + x + i) % dims[0] : x + i;
                        float Y = _wrapNorthSouth ? (dims[1] + y + j) % dims[1] : y + j;

                        Loc l2 = new Loc(X, Y);
                        if (map.ContainsKey(l2.key()))
                        {
                            neighbors.Add(map[l2.key()].pos);
                        }
                        else
                        {
                            //Debug.Log("Map doesn't contain " + l2.x() + "," + l2.y());
                        }
                    }
                }
            }
            return neighbors;
        }

        public Dictionary<Intersection, Player> GetTerritories()
        {
            Dictionary<Intersection, Player> territories = new Dictionary<Intersection, Player>();


            foreach (Intersection i in intersections.Values)
            {
                List<Intersection> area = i.GetArea();
                Player p = GetTerritoryOwner(area);
                if (p!= null)
                {
                    territories[i] = p;
                }
            }

            return territories;
        }
        Player GetTerritoryOwner(List<Intersection> area)
        {
            Player owner = null;
            List<Player> stoneNeighbors = new List<Player>();
            foreach (Intersection i in area)
            {
                foreach (Intersection j in i.neighbors)
                {
                    if (j.occupant != null && !stoneNeighbors.Contains(j.occupant.player))
                    {
                        stoneNeighbors.Add(j.occupant.player);
                    }
                }
            }
            if (stoneNeighbors.Count == 1)
            {
                owner = stoneNeighbors[0];
            }
            return owner;
        }
    }
    public class PlayerBasic : Player
    {
        public string name { get; set; }
        public Color color { get; set; }
        public List<Stone> capturedStones { get; set; }
        public bool passed { get; set; }
        public PlayerBasic(string _name, Color _color)
        {
            name = _name;
            color = _color;
            capturedStones = new List<Stone>();
            passed = false;
        }
    }

    public class IntersectionBasic : Pos, Intersection
    {
        public Pos pos
        {
            get
            {
                return GetThisPos();
            }
        }
        public Stone occupant { get; set; }

        public IntersectionBasic(Loc _loc, TileShape _tileShape) : base(_loc, _tileShape)
        {
            occupant = null;
        }
        public string serialize
        {
            get
            {
                string s = "[" + gridLoc.x() + "," + gridLoc.y() + ":" + (occupant != null ? occupant.player.name : "NULL") + "]";

                return s;
            }
        }
        public List<Intersection> GetArea()
        {
            List<Intersection> area = new List<Intersection>();
            this.GetAreaWorkhorse(ref area);
            return area;
        }
        public void GetAreaWorkhorse(ref List<Intersection> area)
        {
            if (!area.Contains(this) && occupant == null)
            {
                area.Add(this);
                foreach (Intersection i in this.neighbors)
                {
                    if (i.occupant == null)
                    {
                        i.GetAreaWorkhorse(ref area);
                    }
                }
            }
        }
    }
    public class StoneBasic : Stone
    {
        public Player player { get; set; }
        int liberties;
        public Intersection intersection { get; set; }
        public string serialize { get { return intersection.serialize; } }
        GoEsque game;
        public bool IsAlive()
        {
            List<Stone> Group = new List<Stone>();
            GetGroup(ref Group);
            Debug.Log(serialize + "'s group has #" + Group.Count);
            bool Alive = false;

            foreach (Stone s in Group)
            {
                if (s.StoneIsAlive(s.GetAttackers()))
                {
                    Alive = true;
                }
            }
            return Alive;
        }
        public bool StoneIsAlive(List<Stone> attackers)
        {
            int d = 0;
            foreach (Stone attacker in attackers)
            {
                d += attacker.Attack(this);
            }
            bool alive = d < liberties ? true : false;
            //Debug.Log(intersection.serialize + " is " + (alive ? "alive" : "dead") + ":" + d + " < "+liberties+"?");
            return alive;
        }
       
        public void GetGroup(ref List<Stone> Group)
        {
            if (!Group.Contains(this))
            {
                //Debug.Log("Adding " + serialize + " to the group");
                Group.Add(this);
                foreach (Intersection i in intersection.neighbors)
                {
                    if (i.occupant != null && i.occupant.player == this.player)
                    {
                        i.occupant.GetGroup(ref Group);
                    }
                }
            }
        }
        public int Attack(Stone s)
        {
            return 1;
        }
        public List<Stone> GetAttackers()
        {
            List<Stone> attackers = new List<Stone>();
            foreach (Intersection i in intersection.neighbors)
            {
                if (i.occupant != null)
                {
                    attackers.Add(i.occupant);
                }
            }
            return attackers;
        }
        public StoneBasic(Intersection _intersection, GoEsque _game)
        {
            intersection = _intersection;
            liberties = 4 - Pos.EdgeCount(intersection.pos, _game.dims);
            Stone stone = this;
            _game.stones[intersection] = stone;
            _game.intersections[intersection.pos.gridLoc.key()].occupant = stone;
            game = _game;
            player = game.currentPlayer;
        }
        public void RemoveStoneFromGame()
        {
            Debug.Log("Removing " + intersection.serialize);
            game.stones.Remove(intersection);
            game.intersections[intersection.pos.gridLoc.key()].occupant = null;
            game.currentPlayer.capturedStones.Add(this);
        }
    }
}
