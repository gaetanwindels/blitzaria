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
    [SerializeField] private float tackleSpeed = 8f;
    [SerializeField] private float tackleReturnSpeed = 8f;
    [SerializeField] private float tackleDuration = 0.1f;
    [SerializeField] private float tackleWindUp = 0.1f;

    [Header("Shoot")]
    [SerializeField] private float maxShotPower = 10f;
    [SerializeField] private float minShotPower = 5f;
    [SerializeField] private float releasePower = 2f;
    [SerializeField] private float timeToBuildUp = 1f;
    [SerializeField] private float shootSpeedFactor = 0.3f;
    [SerializeField] private Collider2D shotHitbox;
    [SerializeField] private float shotHitboxTime = 0.3f;
    [SerializeField] private BarSlider barSlider;

    [Header("Move")]
    [SerializeField] float speed = 7f;
    [SerializeField] float airSpeed = 0.02f;
    [SerializeField] float airThrust = 0.02f;
    [SerializeField] float boostSpeed = 10f;
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float grabSpeedFactor = 0.8f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;


    [Header("Player Config")]
    [SerializeField] public int playerNumber = 0;
    [SerializeField] public bool isAI = false;
    [SerializeField] public bool isActive = false;

    private Rewired.Player rwPlayer;
    public Brain brain;

    public InputManager inputManager;

    // Cached variables
    private Rigidbody2D rigidBody;
    private Animator animator;
    private Collider2D bodyCollider;
    private GameManager gameManager;

    // State variable
    [Header("State")]
    public float builtupPower = 0;
    public Ball ballGrabbed = null;
    public Vector2 tackleStartPoint;
    public bool IsReturnFromTackle = false;
    public float currentEnergy;
    public bool isReplenishing = false;
    public IEnumerator replenishRoutine;
    public bool moveEnabled = true;

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
        gameManager = FindObjectOfType<GameManager>();

        rwPlayer = ReInput.players.GetPlayer(playerNumber);
        inputManager = new RewiredInputManager(playerNumber);

        currentEnergy = GameSettings.energyAmount;

        if (isAI)
        {
            inputManager = new SimulatedInputManager(playerNumber);
        }

        if (this.shotHitbox != null)
        {
            this.shotHitbox.enabled = false;
        }
       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var go = collision.gameObject;
        var player = go.GetComponent<Player>();

        if (player != null && IsTackling())
        {
            player.ReceiveTackle(this);
        }

        Debug.Log("Ball collided " + collision.gameObject.name);

        // BALL GRAB COLLISION
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (this.ballGrabbed == null && ball != null)
        {
            if (inputManager.GetButton("Grab"))
            {
                var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
                Collider2D collider = ball.GetComponent<Collider2D>();
                Grab(ball);
                Destroy(rigidBodyBall);
                Destroy(collider);
            }
            else
            {
                var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
                rigidBodyBall.velocity = rigidBody.velocity * ball.impulseSpeedFactor;

            }

        }
    }

    public void ReceiveTackle(Player player)
    {
        if (IsGrabbing())
        {
            var otherRigidBody = player.GetComponent<Rigidbody2D>();
            this.ballGrabbed.player = null;
            Destroy(FindObjectOfType<Ball>().gameObject);
            var newBall = Instantiate(ballPrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

            var otherVelocity = otherRigidBody.velocity;
            var velocity = rigidBody.velocity;
            newBallBody.velocity = (-velocity - otherVelocity) / 2;
            DisableBallCollision(newBall);
        }
    }

    public void DisableMove()
    {
        this.moveEnabled = false;
    }

    public void EnableMove()
    {
        this.moveEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {        

        if (gameManager.IsGameOver())
        {
            return;
        }

        ManageGravity();
        ManageShoot();
        ManageMove();
        ManageTackle();
        ManageDash();
        ManageRotation();
        ManageAnimation();

        barSlider.SetValue(builtupPower / timeToBuildUp);

        if (isReplenishing)
        {
            AddEnergy(Time.deltaTime * GameSettings.replenishEnergyPerSecond);
        }
    }

    public void NotifyShootingFinish()
    {
        animator.SetBool("IsShooting", false);
        this.EnableMove();
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

        var hasPressedTackle = inputManager.GetButtonDown("Tackle");
        if (hasPressedTackle)
        {
            tackleTimer = tackleDuration;
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

        var hasPressedDash = inputManager.GetButtonDown("Dash");
        if (hasPressedDash && currentEnergy >= GameSettings.dashEnergyCost)
        {
            RemoveEnergy(GameSettings.dashEnergyCost);
            dashTimer = dashDuration;
            this.rigidBody.velocity = ComputeMoveSpeed(this.dashSpeed);
        }
    }

    private void ManageShoot()
    {
        if (inputManager.GetButton("Shoot"))
        {
            builtupPower += Time.deltaTime;
            builtupPower = Mathf.Min(timeToBuildUp, builtupPower);

        }

        if (inputManager.GetButtonUp("Shoot") || inputManager.GetButtonUp("Grab"))
        {
            animator.SetBool("IsShooting", true);
            GameObject newBall = null;
            if (IsGrabbing())
            {
                Debug.Log("SHOOT");
                
                Destroy(FindObjectOfType<Ball>().gameObject);
                newBall = Instantiate(ballPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

                var combinedVelocity = Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.y);

                var computedShotPower = GetSpeed() + releasePower;

                if (inputManager.GetButtonUp("Shoot"))
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

            } else if (!IsGrabbing() && inputManager.GetButtonUp("Shoot")) {
                this.DisableMove();
                this.shotHitbox.enabled = true;
                StartCoroutine("EnableShotHitbox");
            }

            if (newBall != null)
            {
                StartCoroutine(DisableBody(newBall));
            }
     
        }
    }

    private void ManageMove()
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        float computedSpeed = speed;

        if (IsTackling() || IsDashing() || !moveEnabled)
        {
            return;
        }

        var currentAirThrust = 0f; 
        if (inputManager.GetButton("Turbo") && currentEnergy > 0)
        {
            RemoveEnergy(Time.deltaTime * GameSettings.turboEnergyCostPerSecond);
            currentAirThrust = airThrust;
            computedSpeed = boostSpeed;
        }

        if (IsGrabbing())
        {
            computedSpeed *= grabSpeedFactor;
        } else if (inputManager.GetButton("Shoot"))
        {
            computedSpeed *= shootSpeedFactor;
        }

        float speedX = inputManager.GetAxis("Move Horizontal") * computedSpeed;
        float speedY = inputManager.GetAxis("Move Vertical") * computedSpeed;
        var speedVector = new Vector2(inputManager.GetAxis("Move Horizontal"), inputManager.GetAxis("Move Vertical"));

        if ((speedX != 0 || speedY != 0) && isTouchingWater)
        {
            this.rigidBody.velocity = ComputeMoveSpeed(speedVector.magnitude * computedSpeed);
        }
        else if (!isTouchingWater)
        {
            var counterForce = this.airSpeed * inputManager.GetAxis("Move Horizontal");
            this.rigidBody.velocity = new Vector2(Mathf.Clamp(this.rigidBody.velocity.x + counterForce, -this.speed, this.speed), this.rigidBody.velocity.y + currentAirThrust);
        }
    }

    private void RemoveEnergy(float energy)
    {
        currentEnergy = Mathf.Max(0, currentEnergy - energy);
        if (replenishRoutine != null)
        {
            StopCoroutine(replenishRoutine);
        }
        replenishRoutine = DisableEnergyReplenish();
        StartCoroutine(replenishRoutine);
    }

    public void AddEnergy(float energy)
    {
        currentEnergy = Mathf.Min(currentEnergy + energy, GameSettings.energyAmount);
    }

    IEnumerator DisableEnergyReplenish()
    {
        isReplenishing = false;
        yield return new WaitForSeconds(GameSettings.replenishAfter);
        isReplenishing = true;
    }

    private void ManageAnimation()
    {
        var isImmobile = inputManager.GetAxis("Move Horizontal") == 0 && inputManager.GetAxis("Move Vertical") == 0;
        var isInWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        var isUp = rigidBody.velocity.y >= 0;
        animator.SetBool("IsSwimming", isInWater && !isImmobile);
        animator.SetBool("IsIdle", isInWater && isImmobile);
        animator.SetBool("IsInAir", !isInWater);
        animator.SetBool("IsDashing", IsDashing());
        animator.SetBool("IsTackling", IsTackling());
        animator.SetBool("IsUp", isUp);
        animator.SetBool("IsDown", !isUp);
        animator.SetBool("IsLoadingShoot", inputManager.GetButton("Shoot"));
        //animator.SetBool("IsShooting", inputManager.GetButtonUp("Shoot"));
    }

    private void ManageRotation()
    {
        var isTouchingWater = bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        var isMoving = (Math.Abs(this.rigidBody.velocity.x) > 0 || Math.Abs(this.rigidBody.velocity.y) > 0);
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");

        var scaleX = this.rigidBody.velocity.x < 0 ? - Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);
        if (!isMoving || (!isTouchingWater && !IsDashing()))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            if (this.rigidBody.velocity.x != 0)
            {
                transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
            }
        }
        else if (speedX != 0 || speedY != 0)
        {
            // Manage rotation
            float angle = Mathf.Atan2(this.rigidBody.velocity.y, this.rigidBody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 270));
            if (this.rigidBody.velocity.x != 0)
            {
                transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
            }
            
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
        return tackleTimer > 0;
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

    public void DisableBallCollision(GameObject newBall)
    {
        StartCoroutine(DisableBody(newBall));
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
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");
        float totalFactor = Math.Abs(speedX) + Math.Abs(speedY);
        float speedSquare = speedWanted * speedWanted;

        if (totalFactor == 0)
        {
            return new Vector2(0, 0);
        }

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

        float speedX = inputManager.GetAxis("Move Horizontal 2");
        float speedY = inputManager.GetAxis("Move Vertical 2");

        Debug.Log("SPEEDX" + speedX);
        Debug.Log("SPEEDY" + speedY);

        if (speedX == 0 && speedY == 0)
        {
            speedX = inputManager.GetAxis("Move Horizontal");
            speedY = inputManager.GetAxis("Move Vertical");
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
