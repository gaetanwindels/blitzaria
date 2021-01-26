using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [SerializeField] public Player player;
    [SerializeField] public float impulseSpeedFactor = 0.5f;

    private bool hasJustSpawned = true;

    public bool HasJustSpawned { get => hasJustSpawned; set => hasJustSpawned = value; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Ball collided " + collision.gameObject.name);

        var tagName = collision.gameObject.tag;
        Player player = collision.gameObject.GetComponent<Player>();
        Debug.Log(Input.GetButton("Grab"));
        if (this.player == null && player != null && tagName == "Player")
        {
            if (Input.GetButton("Grab")) {
                Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
                Debug.Log("Velocity collided " + tagName);
                Collider2D collider = GetComponent<Collider2D>();
                WaterPushback waterPushback = GetComponent<WaterPushback>();
                this.player = player;
                Destroy(rigidbody);
                Destroy(collider);
                Destroy(waterPushback);
            } else
            {
                var rigidBodyPlayer = player.GetComponent<Rigidbody2D>();
                var rigidBodyBall = GetComponent<Rigidbody2D>();
                rigidBodyBall.velocity = rigidBodyPlayer.velocity * impulseSpeedFactor;

            }
            
        }        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Ball collided " + collision.gameObject.name);
        var tagName = collision.gameObject.tag;
        var parentGO = collision.gameObject.transform.parent;

        if (parentGO == null)
        {
            return;
        }

        Player player = parentGO.gameObject.GetComponent<Player>();

        if (player != null && tagName == "ShotHitbox")
        {
            Debug.Log(player);
            Debug.Log(player);
            player.Shoot();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (rigidbody != null)
        {
            rigidbody.gravityScale = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? 0 : 1;
            rigidbody.mass = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? 1f : 0.25f;
        }

        if (this.player != null)
        {
            this.transform.position = player.BallPoint.position;
        }
    }
}
