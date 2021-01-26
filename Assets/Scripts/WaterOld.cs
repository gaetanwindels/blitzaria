using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterOld : MonoBehaviour
{

    // Parameters
    [SerializeField] private float ejectFactor = 1.25f;

    // State variables
    List<Rigidbody2D> bodies = new List<Rigidbody2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log(collision.gameObject.name + " entered water!");
        Rigidbody2D body = collision.gameObject.GetComponent<Rigidbody2D>();
        Move move = collision.gameObject.GetComponent<Move>();

        WaterPushback waterPushback = collision.gameObject.GetComponent<WaterPushback>();
        if (waterPushback.OutOfWaterOnce)
        {
            waterPushback.StartWaterPushBack();
        }

        if (waterPushback.OutOfWaterOnce && move != null)
        {
            move.Lock();
        }
        
        if (body != null)
        {
            // Simulating water force
            body.gravityScale = 0;
            //StartCoroutine(StopFalling(body, move));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D body = collision.gameObject.GetComponent<Rigidbody2D>();
        Debug.Log(collision.gameObject.name + " left water!");

        Move move = collision.gameObject.GetComponent<Move>();
        WaterPushback waterPushback = collision.gameObject.GetComponent<WaterPushback>();
        waterPushback.OutOfWaterOnce = true;

        if (body != null)
        {
            body.gravityScale = 1;
            body.velocity = body.velocity * ejectFactor;
        }

        if (move != null)
        {
            move.SetIsJumping(true);
        }
    }

    IEnumerator StopFalling(Rigidbody2D body, Move move)
    {
        yield return new WaitForSeconds(0.5f);
        if (move != null)
        {
            move.SetIsJumping(false);
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
