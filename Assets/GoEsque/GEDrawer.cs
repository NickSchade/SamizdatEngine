using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SamizdatEngine.GE
{
    public interface GEDrawer
    {
        GoEsque game { get; set; }
        Dictionary<Stone, GameObject> stones { get; set; }
        Dictionary<Intersection, GameObject> intersections { get; set; }

        void Draw();
        void DrawInit(GoEsque game);


        void SetBasicColors();
        void SetTerritoryColors(Dictionary<Intersection, Player> territory);
    }
}