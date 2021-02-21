using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLimiter : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log("I CAN GO UP");
            player.canGoUp = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
