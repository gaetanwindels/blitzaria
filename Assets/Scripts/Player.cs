using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Parameters
    [SerializeField] private Transform ballPoint;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float maxShotPower = 10f;
    [SerializeField] private float minShotPower = 5f;
    [SerializeField] private float releasePower = 2f;
    [SerializeField] private float timeToBuildUp = 1f;
    [SerializeField] private Collider2D shotHitbox;
    [SerializeField] private float shotHitboxTime = 0.3f;

    public int playerNumber = 1;

    // Cached variables
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    private Ball ball;
    private Animator animator;
    private Collider2D bodyCollider;

    // State variable
    public float builtupPower = 0;

    public Transform BallPoint { get => ballPoint; set => ballPoint = value; }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        ball = FindObjectOfType<Ball>();
        animator = GetComponent<Animator>();
        animator.SetBool("IsShooting", false);
        bodyCollider = GetComponent<Collider2D>();

        if (this.shotHitbox != null)
        {
            this.shotHitbox.enabled = false;
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.gravityScale = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? 0 : 1;
        ManageShoot();
    }


    private void ManageShoot()
    {
        if (playerNumber != 1)
        {
            return;
        }

        ball = FindObjectOfType<Ball>();
        if (Input.GetButton("Fire1"))
        {
            builtupPower += Time.deltaTime;
            builtupPower = Mathf.Min(timeToBuildUp, builtupPower);
        }

        if (Input.GetButtonUp("Fire1") || Input.GetButtonUp("Grab"))
        {
            if (ball != null && ball.player == this)
            {
                Debug.Log("SHOOT");
                animator.SetBool("IsShooting", true);
                Destroy(FindObjectOfType<Ball>().gameObject);
                GameObject newBall = Instantiate(ballPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

                var combinedVelocity = Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.y);

                var computedShotPower = GetSpeed() + releasePower;

                if (Input.GetButtonUp("Fire1"))
                {
                    computedShotPower = minShotPower + ((maxShotPower - minShotPower) * (builtupPower / timeToBuildUp));
                }
                
                float velocityX;
                float velocityY;
                if (combinedVelocity == 0)
                {
                    velocityX = this.transform.localScale.x * computedShotPower;
                    velocityY = 0f;
                    Debug.Log(velocityX);
                } else
                {
                    var speed = ComputeSpeed(computedShotPower);
                    velocityX = speed.x;
                    velocityY = speed.y;
                }

                newBallBody.velocity = new Vector2(velocityX, velocityY);
                builtupPower = 0;
                Debug.Log(newBallBody.velocity);

            } else if (ball.player != this && Input.GetButtonUp("Fire1")) {
                Debug.Log("enabling");
                this.shotHitbox.enabled = true;
                StartCoroutine("EnableShotHitbox");
            }

            animator.SetBool("IsShooting", false);
        }
    }

    public void Shoot()
    {
        Debug.Log("SHOOT");
        var ball = FindObjectOfType<Ball>();

        var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
        var combinedVelocity = Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.y);

        var computedShotPower = minShotPower + ((maxShotPower - minShotPower) * (builtupPower / timeToBuildUp));

        float velocityX;
        float velocityY;
        if (combinedVelocity == 0)
        {
            velocityX = this.transform.localScale.x * computedShotPower;
            velocityY = 0f;
            Debug.Log(velocityX);
        }
        else
        {
            var speed = ComputeSpeed(computedShotPower);
            velocityX = speed.x;
            velocityY = speed.y;
        }

        rigidBodyBall.velocity = new Vector2(velocityX, velocityY);
        Debug.Log(rigidBodyBall.velocity);
    }

    IEnumerator EnableShotHitbox()
    {
        yield return new WaitForSeconds(this.shotHitboxTime);
        this.shotHitbox.enabled = false;
        builtupPower = 0;
    }

    private float GetSpeed()
    {
        var speed = rigidBody.velocity;
        return Mathf.Sqrt(speed.x * speed.x + speed.y + speed.y);
    }

    private Vector2 ComputeSpeed(float speedWanted)
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        float speedX = Input.GetAxis("Horizontal");
        float speedY = Input.GetAxis("Vertical");

        if (Mathf.Abs(speedX) + Mathf.Abs(speedY) == 0)
        {
            speedX = rigidBody.velocity.x;
            speedY = rigidBody.velocity.y;
        }

        float totalFactor = Mathf.Abs(speedX) + Mathf.Abs(speedY);
        float speedSquare = speedWanted * speedWanted;

        var newSpeedX = Mathf.Sqrt((speedSquare * Mathf.Abs(speedX)) / totalFactor);
        var newSpeedY = Mathf.Sqrt((speedSquare * Mathf.Abs(speedY)) / totalFactor);

        if (speedX < 0)
        {
            newSpeedX *= -1;
        }

        if (speedY < 0)
        {
            newSpeedY *= -1;
        }

        return new Vector2(newSpeedX, newSpeedY);
    }

}
