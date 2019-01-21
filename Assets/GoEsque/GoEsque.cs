using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SamizdatEngine;
using UnityEngine;

namespace SamizdatEngine.GE
{
    public interface GoEsque
    {
        TileShape tileShape { get; set; }
        List<Player> players { get; set; }
        Player currentPlayer { get; set; }
        int turn { get; set; }
        int[] dims { get; set; }
        Dictionary<string, Intersection> intersections { get; set; }
        Dictionary<Intersection, Stone> stones { get; set; }
        GEDrawer drawer { get; set; }
        GEUI uihandler { get; set; }
        void ClickPos(Pos clicked);
        void Pass();
        void TerritoryView();
    }
    
    public interface Intersection
    {
        Stone occupant { get; set; }
        List<Pos> neighbors { get; set; }
        Pos pos { get; }
        string serialize { get; }
        List<Intersection> GetArea();
        void GetAreaWorkhorse(ref List<Intersection> area);
    }

    public interface Player
    {
        string name { get; set; }
        Color color { get; set; }
        List<Stone> capturedStones { get; set; }
        bool passed { get; set; }
    }
   
    public interface Stone
    {
        Player player { get; set; }
        Intersection intersection { get; set; }
        bool IsAlive();
        bool StoneIsAlive(List<Stone> attackers);
        void GetGroup(ref List<Stone> Group);
        void RemoveStoneFromGame();
        int Attack(Stone s);
        List<Stone> GetAttackers();
    }
    
    public interface RuleManager
    {
        void ClickPos(Pos p);
        void Pass();
    }
    
}