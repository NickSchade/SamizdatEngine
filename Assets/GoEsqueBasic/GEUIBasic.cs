using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine.GE.Basic;


namespace SamizdatEngine.GE
{

    public class GEUIBasic : MonoBehaviour, GEUI
    {
        public GoEsque game { get; set; }
        public void HandleUI()
        {
            HandleMouse();
            HandleKeys();
        }
        void HandleMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                foreach (RaycastHit hit in hits)
                {
                    GEClickable clicked = hit.transform.gameObject.GetComponentInParent<GEClickable>();

                    if (clicked != null)
                    {

                        //Debug.Log("Clicked " + clicked.pos.gridLoc.key());
                        game.ClickPos(clicked.pos);
                        game.drawer.Draw();
                    }
                    // Debug.Log("Hovering " + clicked.pos.gridLoc.key());
                }
            }
            var d = Input.GetAxis("Mouse ScrollWheel");
            if (d > 0f)
            {
                Camera.main.GetComponent<Camera>().orthographicSize -= 1;
            }
            else if (d < 0f)
            {
                Camera.main.GetComponent<Camera>().orthographicSize += 1;
            }
        }
        void HandleKeys()
        {
            if (Input.GetKeyUp("space")) { }
            if (Input.GetKey("w"))
            {
                Vector3 p = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(p.x, p.y, p.z + 1);
            }
            if (Input.GetKey("s"))
            {
                Vector3 p = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(p.x, p.y, p.z - 1);
            }
            if (Input.GetKey("a"))
            {
                Vector3 p = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(p.x - 1, p.y, p.z);
            }
            if (Input.GetKey("d"))
            {
                Vector3 p = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(p.x + 1, p.y, p.z);
            }
        }
        public void ClickPass()
        {
            game.Pass();
        }
        public void TerritoryView()
        {
            game.TerritoryView();
        }
    }
}