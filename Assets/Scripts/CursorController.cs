using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnScreenPositionChanged(Vector2 screenPosition)
    {
        Debug.Log("SCREEN CHANGED");
        Debug.Log(screenPosition);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        Debug.Log(worldPos);
        transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    }
}
