using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rewired;
using System;
using UnityEngine.Experimental.Rendering.Universal;

public class Ball : MonoBehaviour
{

    [SerializeField] public Player player;
    [SerializeField] public float impulseSpeedFactor = 0.5f;
    [SerializeField] public float gravityScale = 0.8f;

    [SerializeField] public float minScaleY = 0.5f;
    [SerializeField] public float minVelocityTransform = 5f;
    [SerializeField] public float maxVelocityTransform = 10f;

    [SerializeField] public float mass = 1f;
    [SerializeField] public float massInWater = 0.5f;

    [Header("Curl")]
    [SerializeField] public float velocityDragFactor = 0.5f;
    [SerializeField] public float minAngularVelocityCurl = 10f;
    [SerializeField] public float baseCurlRotation = 3000;

    [SerializeField] AudioClip hitSound;

    // Cached variables
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private GameObject perfectShotLight;

    private bool hasJustSpawned = true;
    private float curlRotation = 0f;

    public bool HasJustSpawned { get => hasJustSpawned; set => hasJustSpawned = value; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Ball collided " + collision.gameObject.name);
        var tagName = collision.gameObject.tag;

        Player player = collision.gameObject.GetComponentInParent<Player>();

        if (player != null)
        {
            if (tagName == "ShotHitbox")
            {
                Debug.Log("BALL HIT SHOT");
                //FindObjectOfType<CameraShaker>().ShakeFor(0.1f);
                //audioSource.clip = hitSound;
                //AudioUtils.PlaySound(gameObject);
                if (tagName == "ShotHitbox")
                {
                    //player.Shoot();
                } else
                {
                    //player.DashShoot();
                }
            } else if (tagName == "GrabHitbox")
            {
                player.Grab(this);
            } else if (tagName == "PerfectShotHitbox")
            {
                Debug.Log("Blink");
                //StartCoroutine(Blink());
            }
            
        }
    }

    IEnumerator Blink()
    {
        perfectShotLight.SetActive(true);
        yield return new WaitForSecondsRealtime(0.15f);
        perfectShotLight.SetActive(false);
    }


    IEnumerator Freeze(float freezeTime)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(freezeTime);
        Time.timeScale = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //var light = GetComponentInChildren<Light2D>();
        //perfectShotLight = light.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (rigidbody != null)
        {
            rigidbody.gravityScale = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? 0 : gravityScale;
            rigidbody.mass = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? massInWater : mass;

            if (Mathf.Abs(rigidbody.angularVelocity) > minAngularVelocityCurl) 
            {
                ManageCurl();
            } else
            {
                ManageScale();
                ManageRotation();
            }
        }
    
        if (this.player != null)
        {
            this.transform.position = player.GetBallPoint().position;
            ManageRotation();
        }
    }

    public void ApplyRotation(float percentage)
    {
        curlRotation = this.baseCurlRotation * percentage;
    }

    private void ManageCurl()
    {
        transform.localScale = new Vector3(1, 1, 1);
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        var angVelocity = rigidbody.angularVelocity;
        var adjusted = Vector2.Perpendicular(rigidbody.velocity);
        adjusted *= (angVelocity * velocityDragFactor * Time.deltaTime);
        rigidbody.AddForce(adjusted);
    }

    private void ManageRotation()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (this.player != null)
        {
            transform.Rotate(Vector3.forward * curlRotation * Time.deltaTime);
        } else if (rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        } else
        {
            curlRotation = 0;
        }
    }

    private void ManageScale()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (rigidbody == null)
        {
            transform.localScale = new Vector3(1, 1, 1);
            return;
        }
        
        var speed = rigidbody.velocity.magnitude;

        if (speed < minVelocityTransform)
        {
            transform.localScale = new Vector3(1, 1, 1);
        } else
        {
            Debug.Log(speed);
            var adjustedSpeed = Mathf.Min(speed, maxVelocityTransform);
            var scaleInterval = 1 - minScaleY;
            var scaleY = Mathf.Min(1, minScaleY + (scaleInterval * minScaleY / adjustedSpeed));
            transform.localScale = new Vector3(1, scaleY, 1);

            Debug.Log(scaleY);
        }
    }
}
