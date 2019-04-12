using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public IGame game;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool HandleUserInput()
    {
        HandleScrollWheel();
        HandleScrollKeys();
        bool updateClick = HandleClick();
        bool updateKeys = HandleSpace();
        return updateClick || updateKeys;
    }

    void HandleScrollWheel()
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
    void HandleScrollKeys()
    {
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
    bool HandleClick()
    {
        bool updateClick = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            foreach (RaycastHit hit in hits)
            {
                Clickable clicked = hit.transform.gameObject.GetComponentInParent<Clickable>();

                if (clicked != null)
                {
                    updateClick = game.HandleClick(clicked.pos, Input.GetMouseButtonUp(0), Input.GetMouseButtonUp(1));
                }
            }
        }
        return updateClick;
    }
    bool HandleSpace()
    {
        bool updateKey = false;
        if (Input.GetKeyUp("space"))
        {
            //game.paused = !game.paused;
            updateKey = true;
        }
        return updateKey;
    }

    
}
public class Clickable : MonoBehaviour
{
    public IPos pos;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setPos(IPos _pos)
    {
        pos = _pos;
    }
}
