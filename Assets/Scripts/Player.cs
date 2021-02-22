using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour
{
    // Parameters
    [Header("Stuff")]
    [SerializeField] private Transform ballPointNormal;
    [SerializeField] private Transform ballPointLoading;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Transform throwPointLoading;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Collider2D waterRayCast;

    [Header("Tackle")]
    [SerializeField] private float tackleSpeed = 8f;
    [SerializeField] private float invicibilityFrames = 1.5f;
    [SerializeField] private float tackleReturnSpeed = 8f;
    [SerializeField] private float tackleDuration = 0.1f;
    [SerializeField] private float tackleWindUp = 0.1f;
    [SerializeField] private float tackleStunDuration = 0.2f;

    [Header("Shoot")]
    [SerializeField] private float maxShotPower = 10f;
    [SerializeField] private float minShotPower = 5f;
    [SerializeField] private float releasePower = 2f;
    [SerializeField] private float timeToBuildUp = 1f;
    [SerializeField] private float shootSpeedFactor = 0.3f;
    [SerializeField] private Collider2D shotHitbox;
    [SerializeField] private float shotHitboxTime = 0.3f;
    [SerializeField] private float shootCoolDown = 0.8f;
    [SerializeField] private BarSlider barSlider;
    [SerializeField] private float accelerationBall = 1.4f;
    [SerializeField] private float curlPower = 20000f;

    [Header("Move")]
    [SerializeField] float rotationSpeed = 900f;
    [SerializeField] float speed = 7f;
    [SerializeField] float airSpeed = 0.02f;
    [SerializeField] float airThrust = 0.02f;
    [SerializeField] private float downAirThrust = 10f;
    [SerializeField] float boostSpeed = 10f;
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float grabSpeedFactor = 0.8f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;

    [Header("Player Config")]
    [SerializeField] public int playerNumber = 0;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;
    [SerializeField] public bool isAI = false;
    [SerializeField] public bool isActive = false;
    [SerializeField] public bool disableOnStart = true;

    [Header("Sounds")]
    [SerializeField] public AudioClip hitPlayerSound;
    [SerializeField] public AudioClip launchBallSound;

    [Header("Particles")]
    [SerializeField] private ParticleSystem turboParticles;


    public Brain brain;
    public InputManager inputManager;

    // Cached variables
    private Rigidbody2D rigidBody;
    private AudioSource audioSource;
    private Rewired.Player rwPlayer;
    private Animator animator;
    private Collider2D bodyCollider;
    private GameManager gameManager;
    private AudioLowPassFilter audioFilter;

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
    public bool isShootCoolDown = false;
    public bool isTackling = false;
    public bool hasJustEnteredWater = false;
    public IEnumerator enteredWaterRoutine;
    private bool isInvicible = false;

    // Water management
    public bool canGoUp = true;

    [Header("Timers")]
    public float dashTimer = 0f;
    public float tackleTimer = 0f;
    public float tackleWindupTimer = 0f;

    public Transform GetBallPoint()
    {
        return IsLoadingShoot() ? ballPointLoading : ballPointNormal;
    }

    public Transform GetThrowPoint()
    {
        return IsGrabbing() ? throwPointLoading : throwPoint;
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("IsShooting", false);
        bodyCollider = GetComponent<Collider2D>();
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        audioFilter = GetComponent<AudioLowPassFilter>();

        rwPlayer = ReInput.players.GetPlayer(playerNumber);

        inputManager = new RewiredInputManager(playerNumber);
        
        currentEnergy = GameSettings.energyAmount;

        if (turboParticles != null)
        {
            turboParticles.Stop();
        }

        if (isAI)
        {
            inputManager = new SimulatedInputManager(playerNumber);
        }

        if (disableOnStart)
        {
            DisableInputs();
        }

        if (this.shotHitbox != null)
        {
            this.shotHitbox.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tagName = collision.gameObject.tag;
        var parentGO = collision.gameObject.transform.parent;
        Player playerParent = null;
        if (parentGO != null)
        {
            playerParent = parentGO.gameObject.GetComponent<Player>();
        }

        if (!isInvicible && playerParent != null && tagName == "ShotHitbox" && playerParent.team != team)
        {
            audioSource.clip = hitPlayerSound;
            AudioUtils.PlaySound(gameObject);
            StartCoroutine(TriggerInvicibility());
            ReceiveTackle(this);
        }
    }

    private IEnumerator TriggerInvicibility()
    {
        isInvicible = true;
        yield return new WaitForSeconds(invicibilityFrames);
        isInvicible = false; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var go = collision.gameObject;
        var player = go.GetComponent<Player>();

        if (!isInvicible && player != null && player.team != team && player.IsDashing())
        {
            audioSource.clip = hitPlayerSound;
            audioSource.Play();
            StartCoroutine(TriggerInvicibility());
            ReceiveTackle(this);
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
                var impulseSpeed = IsDashing() ? ball.impulseSpeedFactor * 1.5f : ball.impulseSpeedFactor;

                var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
                rigidBodyBall.velocity = rigidBody.velocity * impulseSpeed;

            }

        }
    }

    public void ReceiveTackle(Player player)
    {
        if (!IsDashing())
        {
            var otherRigidBody = player.GetComponent<Rigidbody2D>();
            if (this.ballGrabbed != null)
            {
                this.ballGrabbed.player = null;
                Destroy(FindObjectOfType<Ball>().gameObject);
                var newBall = Instantiate(ballPrefab, GetThrowPoint().position, Quaternion.identity);
                Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

                var otherVelocity = otherRigidBody.velocity;
                var velocity = rigidBody.velocity;
                newBallBody.velocity = (-velocity - otherVelocity) / 2;
                DisableBallCollision(newBall);
            }

            //rigidBody.velocity = Vector2.zero;

            StartCoroutine(FinishBeingTackled()); 
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

        if (gameManager != null && gameManager.IsGameOver())
        {
            return;
        }

        if (inputManager.GetButtonDown("Start"))
        {
            gameManager.ManagePause();
        }

        ManageGravity();
        ManageShoot();
        ManageMove();
        //ManageTackle();
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
        rigidBody.gravityScale = IsTouchingWater() ? 0 : 0.6f; 
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
        if (hasPressedDash && currentEnergy >= GameSettings.dashEnergyCost && !IsLoadingShoot())
        {
            RemoveEnergy(GameSettings.dashEnergyCost);
            dashTimer = dashDuration;
            this.rigidBody.velocity = ComputeMoveSpeed(this.dashSpeed);
        }
    }

    private void ManageShoot()
    {
        if (this.isShootCoolDown)
        {
            return;
        }

        if (IsLoadingShoot())
        {
            builtupPower += Time.deltaTime;
            builtupPower = Mathf.Min(timeToBuildUp, builtupPower);
        }

        // Generate Shot Hitbox
        if (!IsGrabbing() && inputManager.GetButtonDown("Tackle"))
        {
            this.shotHitbox.enabled = true;
            StartCoroutine(EnableShotHitbox());
            StartCoroutine(CooldownShooting());
            StartCoroutine(StartTackleAnimation());
            //animator.SetBool("IsShooting", true);
        // Either release or shoot the ball
        } else if (IsGrabbing() && (inputManager.GetButtonUp("Tackle") || inputManager.GetButtonDown("Grab")))
        {
            Debug.Log("OPT2");
            GameObject newBall = null;
            Destroy(FindObjectOfType<Ball>().gameObject);
            newBall = Instantiate(ballPrefab, GetThrowPoint().position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

            var combinedVelocity = Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.y);

            var computedShotPower = GetSpeed() + releasePower;

            if (inputManager.GetButtonUp("Tackle"))
            {
                StartCoroutine(ManageShootingAnimation());
                computedShotPower = minShotPower + ((maxShotPower - minShotPower) * (builtupPower / timeToBuildUp));
            }

            float velocityX;
            float velocityY;
            if (combinedVelocity == 0)
            {
                velocityX = this.transform.localScale.x * computedShotPower;
                velocityY = 0f;
            }
            else
            {
                var speed = ComputeShotSpeed(computedShotPower);
                velocityX = speed.x;
                velocityY = speed.y;
            }

            newBallBody.velocity = new Vector2(velocityX, velocityY);
            builtupPower = 0;

            if (inputManager.GetAxis("Curled Right") > 0)
            {
                Debug.Log("Curl right" + inputManager.GetAxis("Curled right"));
                newBallBody.angularVelocity = 5000;
            } else if (inputManager.GetAxis("Curled Left") > 0)
            {
                Debug.Log("Curl left" + inputManager.GetAxis("Curled Left"));
                newBallBody.angularVelocity = -5000;
            }

            audioSource.clip = launchBallSound;
            AudioUtils.PlaySound(gameObject);
            //audioSource.Play();

            if (newBall != null)
            {
                StartCoroutine(DisableBody(newBall));
            }
        }
    }

    public bool IsLoadingShoot()
    {
        return IsGrabbing() && inputManager.GetButton("Tackle");
    }

    private void ManageMove()
    {
        bool isTouchingWater = IsTouchingWater() && !hasJustEnteredWater;
        float computedSpeed = speed;

        if (this.isTackling || IsDashing())
        {
            return;
        }

        var currentAirThrust = 0f;
        if (inputManager.GetButton("Turbo") && currentEnergy > 0)
        {
            RemoveEnergy(Time.deltaTime * GameSettings.turboEnergyCostPerSecond);
            currentAirThrust = airThrust;
            computedSpeed = boostSpeed;
            animator.SetFloat("Speed Multiplier", 1.5f);

            if (!this.turboParticles.isPlaying)
            {
                this.turboParticles.Play();
            }
        }
        else
        {
            this.turboParticles.Stop();
            animator.SetFloat("Speed Multiplier", 1f);
        }

        if (IsGrabbing())
        {
            computedSpeed *= grabSpeedFactor;
        }
        else if (IsLoadingShoot())
        {
            computedSpeed *= shootSpeedFactor;
        }

        //float speedX = inputManager.GetAxis("Move Horizontal") * computedSpeed;
        //float speedY = inputManager.GetAxis("Move Vertical") * computedSpeed;

        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");
        var speedVector = new Vector2(speedX, speedY);
        if (speedVector.magnitude > 1)
        {
            speedVector.Normalize();
        }
        speedVector = speedVector * computedSpeed;
        //var speedVector = new Vector2(inputManager.GetAxis("Move Horizontal"), inputManager.GetAxis("Move Vertical"));

        if ((speedX != 0 || speedY != 0) && isTouchingWater)
        {
            this.rigidBody.velocity = speedVector;
        }
        else if (!isTouchingWater)
        {
            var counterForce = this.airSpeed * inputManager.GetAxis("Move Horizontal");
            var downAirForce = 0f;
            if (!IsLoadingShoot() && inputManager.GetAxis("Move Vertical") < 0) {
                downAirForce = Time.deltaTime * this.downAirThrust * inputManager.GetAxis("Move Vertical");
            }
            this.rigidBody.velocity = new Vector2(Mathf.Clamp(this.rigidBody.velocity.x + counterForce, -this.speed, this.speed), this.rigidBody.velocity.y + downAirForce);
        }
    }

    public void EnterWater()
    {
        if (enteredWaterRoutine != null)
            StopCoroutine(enteredWaterRoutine);
        enteredWaterRoutine = EnterWaterRoutine();
        StartCoroutine(enteredWaterRoutine);
    }

    IEnumerator EnterWaterRoutine()
    {
        hasJustEnteredWater = true;
        yield return new WaitForSeconds(0.3f);
        hasJustEnteredWater = false;
    }

    private bool IsTouchingWater()
    {
        return bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
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
    IEnumerator CooldownShooting()
    {
        isShootCoolDown = true;
        yield return new WaitForSeconds(shootCoolDown);
        isShootCoolDown = false;
    }

    IEnumerator StartTackleAnimation()
    {
        isTackling = true;
        yield return new WaitForSeconds(shotHitboxTime);
        isTackling = false;
    }

    IEnumerator ManageShootingAnimation()
    {
        animator.SetBool("IsShooting", true);
        yield return new WaitForSeconds(shotHitboxTime);
        animator.SetBool("IsShooting", false);
    }

    IEnumerator DisableEnergyReplenish()
    {
        isReplenishing = false;
        yield return new WaitForSeconds(GameSettings.replenishAfter);
        isReplenishing = true;
    }

    IEnumerator FinishBeingTackled()
    {
        animator.SetBool("IsTackled", true);
        DisableInputs();
        yield return new WaitForSeconds(tackleStunDuration);
        animator.SetBool("IsTackled", false);
        EnableInputs();
    }

    private void ManageAnimation()
    {
        var isImmobile = inputManager.GetAxis("Move Horizontal") == 0 && inputManager.GetAxis("Move Vertical") == 0;
        var isInWater = IsTouchingWater();
        var isUp = rigidBody.velocity.y >= 0;
        animator.SetBool("IsSwimming", isInWater && !isImmobile);
        animator.SetBool("IsIdle", isInWater && isImmobile);
        animator.SetBool("IsInAir", !isInWater);
        animator.SetBool("IsDashing", IsDashing());
        animator.SetBool("IsTackling", this.isTackling);
        animator.SetBool("IsUp", isUp);
        animator.SetBool("IsDown", !isUp);
        animator.SetBool("IsLoadingShoot", IsLoadingShoot());
        //animator.SetBool("IsShooting", inputManager.GetButtonUp("Shoot"));
    }

    private void ManageRotation()
    {
        var isTouchingWater = IsTouchingWater();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.8f);
        Debug.DrawRay(transform.position, Vector2.down * 0.8f);
        bool fullyOutOfWater = !(hit != null && hit.collider != null && hit.collider.gameObject.tag == "Water");
        
        var isMoving = (Math.Abs(this.rigidBody.velocity.x) > 0 || Math.Abs(this.rigidBody.velocity.y) > 0);
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");

        var scaleX = this.rigidBody.velocity.x < 0 ? - Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);
        var adjustedRotationSpeed = isTackling || IsDashing() ? 3000 : rotationSpeed;

        Quaternion currentAngle = transform.rotation;
        float angle = Mathf.Atan2(this.rigidBody.velocity.y, this.rigidBody.velocity.x) * Mathf.Rad2Deg;

        if (!isTouchingWater && !IsDashing() && fullyOutOfWater)
        {
            angle = 0f;
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle)), adjustedRotationSpeed * Time.deltaTime);
        } else
        {
            //transform.rotation = ;
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }

        // Manage rotation
        if (isTackling || IsLoadingShoot())
        {
            
            angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }

        if (IsLoadingShoot())
        {
            //angle = scaleX > 0 ? angle + 90 : angle - 90;
            //transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }


        if (this.rigidBody.velocity.x != 0)
        {
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
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
        return this.isShootCoolDown;
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
        Debug.Log("acc" + rigidBodyBall.velocity.magnitude + "computed" + computedShotPower);
        computedShotPower = Math.Max(accelerationBall * rigidBodyBall.velocity.magnitude, computedShotPower);

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
        StartCoroutine(DisableBody(null));
        this.shotHitbox.enabled = false;
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
        if (newBall != null)
        {
            Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
            yield return new WaitForSeconds(0.6f);
            if (newBall != null)
            {
                Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
            }   
        }
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

        var newSpeed = new Vector2(speedX, speedY);
        newSpeed.Normalize();
        newSpeed *= speedWanted;

        return newSpeed;
    }

    private bool IsDashing()
    {
        return dashTimer > 0f;
    }

    private float GetSpeed()
    {
        return rigidBody.velocity.magnitude;
    }

    public void DisableInputs()
    {
        if (this.inputManager != null)
        {
            this.inputManager.UnregisterInputEvents();
        }
        
    }

    public void EnableInputs()
    {
        if (this.inputManager != null)
        {
            this.inputManager.RegisterInputEvents();
        }
    }
}
