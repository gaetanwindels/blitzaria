﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour
{
    // Parameters
    [Header("Stuff")]
    [SerializeField] private Transform ballPoint;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject ballPrefab;

    [Header("Tackle")]
    [SerializeField] private float tackleSpeed = 10f;
    [SerializeField] private float tackleDuration = 0.1f;
    [SerializeField] private float tackleWindUp = 0.1f;

    [Header("Shoot")]
    [SerializeField] private float maxShotPower = 10f;
    [SerializeField] private float minShotPower = 5f;
    [SerializeField] private float releasePower = 2f;
    [SerializeField] private float timeToBuildUp = 1f;
    [SerializeField] private Collider2D shotHitbox;
    [SerializeField] private float shotHitboxTime = 0.3f;

    [Header("Move")]
    [SerializeField] float speed = 7f;
    [SerializeField] float airSpeed = 0.02f;
    [SerializeField] float boostSpeed = 10f;
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float grabSpeedFactor = 0.8f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;

    [Header("Player Config")]
    [SerializeField] public int playerNumber = 0;
    private Rewired.Player rwPlayer;

    // Cached variables
    private Rigidbody2D rigidBody;
    private Animator animator;
    private Collider2D bodyCollider;

    // State variable
    [Header("State")]
    public float builtupPower = 0;
    public Ball ballGrabbed = null;

    [Header("Timers")]
    public float dashTimer = 0f;
    public float tackleTimer = 0f;
    public float tackleWindupTimer = 0f;

    public Transform BallPoint { get => ballPoint; set => ballPoint = value; }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("IsShooting", false);
        bodyCollider = GetComponent<Collider2D>();
        rwPlayer = ReInput.players.GetPlayer(playerNumber);

        if (this.shotHitbox != null)
        {
            this.shotHitbox.enabled = false;
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        ManageGravity();
        ManageShoot();
        ManageMove();
        ManageTackle();
        ManageDash();
        ManageRotation();
        ManageAnimation();
    }

    private void ManageGravity()
    {
        rigidBody.gravityScale = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Water Area")) ? 0 : 1;
    }

    private void ManageTackle()
    {
        if (IsTackling())
        {
           tackleTimer -= Time.deltaTime;
            return;
        }

        var hasPressedTackle = rwPlayer.GetButtonDown("Tackle");
        if (hasPressedTackle)
        {
            tackleTimer = tackleDuration;
            tackleWindupTimer = tackleWindUp;
            this.rigidBody.velocity = ComputeMoveSpeed(this.tackleSpeed);
        }
    }

    private void ManageDash()
    {
        if (IsDashing())
        {
            dashTimer -= Time.deltaTime;
            return;
        }

        var hasPressedDash = rwPlayer.GetButtonDown("Dash");
        if (hasPressedDash)
        {
            dashTimer = dashDuration;
            this.rigidBody.velocity = ComputeMoveSpeed(this.dashSpeed);
        }
    }

    private void ManageShoot()
    {
        if (rwPlayer.GetButton("Shoot"))
        {
            builtupPower += Time.deltaTime;
            builtupPower = Mathf.Min(timeToBuildUp, builtupPower);
        }

        if (rwPlayer.GetButtonUp("Shoot") || rwPlayer.GetButtonUp("Grab"))
        {
            GameObject newBall = null;
            if (IsGrabbing())
            {
                Debug.Log("SHOOT");
                animator.SetBool("IsShooting", true);
                Destroy(FindObjectOfType<Ball>().gameObject);
                newBall = Instantiate(ballPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

                var combinedVelocity = Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.y);

                var computedShotPower = GetSpeed() + releasePower;

                if (rwPlayer.GetButtonUp("Shoot"))
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
                    var speed = ComputeShotSpeed(computedShotPower);
                    velocityX = speed.x;
                    velocityY = speed.y;
                }

                newBallBody.velocity = new Vector2(velocityX, velocityY);
                builtupPower = 0;
                Debug.Log(newBallBody.velocity);

            } else if (!IsGrabbing() && rwPlayer.GetButtonUp("Shoot")) {
                Debug.Log("enabling");
                this.shotHitbox.enabled = true;
                StartCoroutine("EnableShotHitbox");
            }

            if (newBall != null)
            {
                StartCoroutine(DisableBody(newBall));
            }
            
            animator.SetBool("IsShooting", false);
        }
    }

    private void ManageMove()
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        float computedSpeed = speed;

        if (IsTackling() || IsDashing())
        {
            return;
        }

        if (rwPlayer.GetButton("Turbo"))
        {
            computedSpeed = boostSpeed;
        }

        if (IsGrabbing())
        {
            computedSpeed *= grabSpeedFactor;
        }

        float speedX = rwPlayer.GetAxis("Move Horizontal") * computedSpeed;
        float speedY = rwPlayer.GetAxis("Move Vertical") * computedSpeed;
        var speedVector = new Vector2(rwPlayer.GetAxis("Move Horizontal"), rwPlayer.GetAxis("Move Vertical"));

        if ((speedX != 0 || speedY != 0) && isTouchingWater)
        {
            this.rigidBody.velocity = ComputeMoveSpeed(speedVector.magnitude * computedSpeed);
        }
        else if (!isTouchingWater)
        {
            var counterForce = this.airSpeed * rwPlayer.GetAxis("Move Horizontal");
            this.rigidBody.velocity = new Vector2(Mathf.Clamp(this.rigidBody.velocity.x + counterForce, -this.speed, this.speed), this.rigidBody.velocity.y);
        }
    }

    private void ManageAnimation()
    {
        var isImmobile = rwPlayer.GetAxis("Move Horizontal") == 0 && rwPlayer.GetAxis("Move Vertical") == 0;
        var isInWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        var isUp = rigidBody.velocity.y >= 0;
        animator.SetBool("IsSwimming", isInWater && !isImmobile);
        animator.SetBool("IsIdle", isInWater && isImmobile);
        animator.SetBool("IsInAir", !isInWater);
        animator.SetBool("IsDashing", IsDashing());
        animator.SetBool("IsTackling", IsTackling());
        animator.SetBool("IsUp", isUp);
        animator.SetBool("IsDown", !isUp);
        animator.SetBool("IsLoadingShoot", rwPlayer.GetButton("Shoot"));
        animator.SetBool("IsShooting", rwPlayer.GetButton("Shoot"));
    }

    private void ManageRotation()
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        var isMoving = (Math.Abs(this.rigidBody.velocity.x) > 0 || Math.Abs(this.rigidBody.velocity.y) > 0);
        float speedX = rwPlayer.GetAxis("Move Horizontal");
        float speedY = rwPlayer.GetAxis("Move Vertical");

        var scaleX = this.rigidBody.velocity.x < 0 ? -1 : 1;
        if (!isMoving || (!isTouchingWater && !IsDashing()))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            transform.localScale = new Vector3(scaleX, 1, 1);
        }
        else if (speedX != 0 || speedY != 0)
        {
            // Manage rotation
            float angle = Mathf.Atan2(this.rigidBody.velocity.y, this.rigidBody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 270));
            transform.localScale = new Vector3(scaleX, 1, 1);
        }
    }

    public void Grab(Ball ball)
    {
        ball.player = this;
        ball.HasJustSpawned = true;
        ballGrabbed = ball;
    }

    public bool IsTackling()
    {
        return tackleTimer > 0 && tackleWindUp > 0;
    }

    public bool IsGrabbing()
    {
        return ballGrabbed != null;
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
            var speed = ComputeShotSpeed(computedShotPower);
            velocityX = speed.x;
            velocityY = speed.y;
        }

        rigidBodyBall.velocity = new Vector2(velocityX, velocityY);
        StartCoroutine("DisableBody");
        Debug.Log(rigidBodyBall.velocity);
    }

    IEnumerator EnableShotHitbox()
    {
        yield return new WaitForSeconds(this.shotHitboxTime);
        this.shotHitbox.enabled = false;
        builtupPower = 0;
    }

    IEnumerator DisableBody(GameObject newBall)
    {

        Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        yield return new WaitForSeconds(0.15f);
        Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
    }

    private Vector2 ComputeMoveSpeed(float speedWanted)
    {
        float speedX = rwPlayer.GetAxis("Move Horizontal");
        float speedY = rwPlayer.GetAxis("Move Vertical");
        float totalFactor = Math.Abs(speedX) + Math.Abs(speedY);
        float speedSquare = speedWanted * speedWanted;

        var newSpeedX = Mathf.Sqrt((speedSquare * Math.Abs(speedX)) / totalFactor);
        var newSpeedY = Mathf.Sqrt((speedSquare * Math.Abs(speedY)) / totalFactor);

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

    private Vector2 ComputeShotSpeed(float speedWanted)
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));

        float speedX = rwPlayer.GetAxis("Move Horizontal 2");
        float speedY = rwPlayer.GetAxis("Move Vertical 2");

        Debug.Log("SPEEDX" + speedX);
        Debug.Log("SPEEDY" + speedY);

        if (speedX == 0 && speedY == 0)
        {
            speedX = rwPlayer.GetAxis("Move Horizontal");
            speedY = rwPlayer.GetAxis("Move Vertical");
        }
        
        if (Mathf.Abs(speedX) + Mathf.Abs(speedY) == 0)
        {
            speedX = rigidBody.velocity.x;
            speedY = rigidBody.velocity.y;
        }

        float totalFactor = Mathf.Abs(speedX) + Mathf.Abs(speedY);
        float speedSquare = speedWanted * speedWanted;

        Debug.Log("TitalFactor" + totalFactor);
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

    private bool IsDashing()
    {
        return dashTimer > 0f;
    }

    private float GetSpeed()
    {
        return rigidBody.velocity.magnitude;
    }
}
