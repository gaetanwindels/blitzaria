using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Move : MonoBehaviour
{
    // Parameters
    [SerializeField] int speed = 7;
    [SerializeField] float airSpeed = 0.02f;
    [SerializeField] int boostSpeed = 10;
    [SerializeField] int dashSpeed = 20;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;

    // Cached variables
    private Rigidbody2D rigidBody;
    private Collider2D collider;
    private Animator animator;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private WaterPushback waterPushback;
    public float dashTimer = 0f;

    // State variable
    bool isJumping = false;
    public float lockedTimeTracker;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        waterPushback = GetComponent<WaterPushback>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.playerNumber != 1)
        {
            return;
        }

        ManageMove();
        ManageAnimation();
    }

    private void ManageAnimation()
    {
        var isImmobile = Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0;
        var isInWater = collider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        animator.SetBool("IsSwimming", isInWater && !isImmobile);
        animator.SetBool("IsIdle", isInWater && isImmobile);
        animator.SetBool("IsInAir", !isInWater);
    }

    public void SetIsJumping(bool isJumping)
    {
        this.isJumping = isJumping;
    }

    public void Lock()
    {
        this.lockedTimeTracker = lockedTime;
    }

    private void ManageMove()
    {
        var isTouchingWater = collider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        var computedSpeed = speed;

        if (isDashing())
        {
            dashTimer -= Time.deltaTime;
            return;
        }

        var hasPressedDash = Input.GetButtonDown("Fire2");
        if (hasPressedDash)
        {
            computedSpeed = dashSpeed;
            dashTimer = dashDuration;
        }

        if (Input.GetButton("Jump"))
        {
            computedSpeed = boostSpeed;
        }

        float speedX = Input.GetAxis("Horizontal") * computedSpeed;
        float speedY = Input.GetAxis("Vertical") * computedSpeed;

        //lockedTimeTracker -= Time.deltaTime;

        if ((speedX != 0 || speedY != 0) && (isTouchingWater || hasPressedDash))
        {
            this.rigidBody.velocity = ComputeSpeed(computedSpeed);
        } else if (!isTouchingWater)
        {
            var counterForce = this.airSpeed * Input.GetAxis("Horizontal");
            this.rigidBody.velocity = new Vector2(Mathf.Clamp(this.rigidBody.velocity.x + counterForce, -this.speed, this.speed), this.rigidBody.velocity.y);
        }

        var isMoving = (Math.Abs(this.rigidBody.velocity.x) > 0 || Math.Abs(this.rigidBody.velocity.y) > 0);

        var scaleX = this.rigidBody.velocity.x < 0 ? -1 : 1;
        if (!isMoving || (!isTouchingWater && !isDashing()))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            transform.localScale = new Vector3(scaleX, 1, 1);
        } else if (speedX != 0 || speedY != 0)
        {
            // Manage rotation
            float angle = Mathf.Atan2(this.rigidBody.velocity.y, this.rigidBody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 270));
            transform.localScale = new Vector3(scaleX, 1, 1);
        }
    }

    private Vector2 ComputeSpeed(float speedWanted)
    {
        float speedX = Input.GetAxis("Horizontal");
        float speedY = Input.GetAxis("Vertical");
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

    private bool isDashing()
    {
        return dashTimer > 0f;
    }
}
