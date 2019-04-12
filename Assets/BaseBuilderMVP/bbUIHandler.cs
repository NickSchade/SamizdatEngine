using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BBMVP;

public class bbUIHandler : MonoBehaviour {

    public BaseBuilderMVP game;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool HandleUserInput()
    {
        bool updateMouse = HandleMouse();
        bool updateKeys = HandleKeys();
        return updateMouse || updateKeys;
    }

    bool HandleClick()
    {
        bool updateClick = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            foreach (RaycastHit hit in hits)
            {
                bbClickable clicked = hit.transform.gameObject.GetComponentInParent<bbClickable>();

                if (clicked != null)
                {
                    //Debug.Log("Clicked " + clicked.pos.gridLoc.key());
                    updateClick = game.HandleClick(clicked.pos, Input.GetMouseButtonUp(0), Input.GetMouseButtonUp(1));
                }
                // Debug.Log("Hovering " + clicked.pos.gridLoc.key());
            }
        }
        return updateClick;
    }
    void HandleScroll()
    {
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
    public bool HandleMouse()
    {
        HandleScroll();
        return HandleClick();
    }
    public bool HandleKeys()
    {
        bool updateKey = false;
        if (Input.GetKeyUp("space"))
        {
            game.paused = !game.paused;
            updateKey = true;
        }
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
            Camera.main.transform.position = new Vector3(p.x-1, p.y, p.z);
        }
        if (Input.GetKey("d"))
        {
            Vector3 p = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(p.x + 1, p.y, p.z);
        }
        return updateKey;
    }
}
