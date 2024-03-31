using System.Collections;
using UnityEngine;

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
    
    [SerializeField] public float minSpeedCharged = 3f;
    
    [SerializeField] public float minSpeedTrail = 0.5f;
    [SerializeField] public AnimationCurve trailIntensity;

    [Header("Curl")]
    [SerializeField] public float velocityDragFactor = 0.5f;
    [SerializeField] public float minAngularVelocityCurl = 10f;
    [SerializeField] public float baseCurlRotation = 3000;

    [Header("Vfx")]
    [SerializeField] public ParticleSystem trailParticles;
    
    [SerializeField] AudioClip hitSound;

    // Cached variables
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private GameObject _perfectShotLight;

    private bool _hasJustSpawned = true;
    private float _curlRotation = 0f;

    public bool HasJustSpawned { get => _hasJustSpawned; set => _hasJustSpawned = value; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tagName = collision.gameObject.tag;
        Player player = collision.gameObject.GetComponentInParent<Player>();
        
        if (collision.GetComponent<Goal>())
        {
            Destroy(gameObject);
        }
        
        if (player != null)
        {
            if (tagName == "ShotHitbox")
            {
                if (tagName == "ShotHitbox")
                {
                    //player.Shoot();
                } else
                {
                    //player.DashShoot();
                }
            } else if (tagName == "GrabHitbox")
            {
                OnGrab();
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
        _perfectShotLight.SetActive(true);
        yield return new WaitForSecondsRealtime(0.15f);
        _perfectShotLight.SetActive(false);
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
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        //var light = GetComponentInChildren<Light2D>();
        //perfectShotLight = light.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (rigidbody)
        {
            var isTouchingWater = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area"));
            rigidbody.gravityScale = isTouchingWater ? 0 : gravityScale;
            rigidbody.mass = isTouchingWater ? massInWater : mass;

            // if (isTouchingWater && rigidbody.velocity.magnitude < minSpeedCharged)
            // {
            //     rigidbody.velocity = rigidbody.velocity.normalized * minSpeedCharged;
            // }
            
            ManageTrail();
            
            if (Mathf.Abs(rigidbody.angularVelocity) > minAngularVelocityCurl) 
            {
                ManageCurl();
            } else
            {
                ManageScale();
                ManageRotation();
            }
        }
    
        if (player != null)
        {
            transform.position = player.GetBallPoint().position;
            ManageRotation();
        }
    }

    private void OnGrab()
    {
        ResetTrail();
        ResetScale();
    }
    
    private void ResetScale()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void ResetTrail()
    {
        if (trailParticles != null)
        {
            var em = trailParticles.emission;
            em.rateOverTimeMultiplier = 0;
        }
    }
    
    private void ManageTrail()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        if (trailParticles != null && GetComponent<Rigidbody2D>() != null)
        {
            if (GetComponent<Rigidbody2D>() != null)
            {
                var em = trailParticles.emission;
                em.rateOverTimeMultiplier = trailIntensity.Evaluate(rigidbody.velocity.magnitude);
                //Debug.Log(em.rateOverTimeMultiplier);
                
            }
            
        }
    }

    public void ApplyRotation(float percentage)
    {
        _curlRotation = this.baseCurlRotation * percentage;
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

        if (player != null)
        {
            transform.Rotate(Vector3.forward * _curlRotation * Time.deltaTime);
        } else if (rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody2D>();
            float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        } else
        {
            _curlRotation = 0;
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
