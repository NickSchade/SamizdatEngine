using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine;
using SamizdatEngine.GE;
using SamizdatEngine.GE.Basic;

public class TestScript : MonoBehaviour {

    public GameObject goDrawer;
    GEDrawerBasic drawer;
	// Use this for initialization
	void Start () {
        TileShape tileShape = TileShape.SQUARE;
        Loc loci = new Loc(0, 0);
        Loc locj = new Loc(5, 5);
        Intersection i = new IntersectionBasic(loci, tileShape);
        Intersection j = new IntersectionBasic(locj, tileShape);
        drawer = goDrawer.GetComponentInChildren<GEDrawerBasic>();
        //Vector3 vi = new Vector3(i.pos.gameLoc.x, i.pos.gameLoc.y+1, i.pos.gameLoc.z);
        //Vector3 vj = new Vector3(j.pos.gameLoc.x, j.pos.gameLoc.y+1, j.pos.gameLoc.z);
        Vector3 vi = i.pos.gameLoc;
        Vector3 vj = j.pos.gameLoc;
        drawer.InstantiateGo(drawer.pfIntersection, vi, Color.black);
        drawer.InstantiateGo(drawer.pfIntersection, vj, Color.black);
        drawer.DrawLine(i.pos.gameLoc, j.pos.gameLoc, Color.white);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
