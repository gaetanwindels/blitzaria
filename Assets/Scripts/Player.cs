using System;
using System.Collections;
using System.Collections.Generic;
using enums;
using Extensions;
using UnityEngine;
using Rewired;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    // Parameters
    [Header("Stuff")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float gravityScale = 0.8f;

    [Header("Reference Points")]
    [SerializeField] private Transform ballPointNormal;
    [SerializeField] private Transform ballPointLoading;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Transform throwPointLoading;
    [SerializeField] private Transform feetPoint;

    [Header("Hitboxes")]
    [SerializeField] private Collider2D shotHitbox;
    [SerializeField] private Collider2D dashHitbox;
    [SerializeField] private Collider2D grabHitbox;
    [SerializeField] private Collider2D shoulderHitbox;
    [SerializeField] private Collider2D controlHitbox;

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
    [SerializeField] private float dashShotPower = 5f;
    [SerializeField] private float releasePower = 2f;
    [SerializeField] private float timeToBuildUp = 1f;
    [SerializeField] private float shootSpeedFactor = 0.3f;
    [SerializeField] private float shotHitboxTime = 0.3f;
    [SerializeField] private float shootCoolDown = 0.8f;
    [SerializeField] private BarSlider barSlider;
    [SerializeField] private float accelerationBall = 1.4f;
    [SerializeField] private float curlPower = 20000f;
    [SerializeField] private float shotSpeedCurlFactor = 0.8f;
    [SerializeField] private float timeToMaxCurl = 0.5f;
    [SerializeField] private float shootTolerance = 0.1f;
    [SerializeField] private float minShootFreezeTime = 0.2f;
    [SerializeField] private float maxShootFreezeTime = 0.5f;
    [SerializeField] private float anticipationShotTime = 0.4f;
    [SerializeField] private float minFreezePower = 6f;
    [SerializeField] private float maxFreezePower = 20f;
    [SerializeField] private float shootSpeedFactorPerChargeIntensityLevel = 0.35f;
    [SerializeField] private AnimationCurve powerShotCameraShake;
    
    [Header("Move")]
    [SerializeField] private float rotationSpeed = 900f;
    [SerializeField] private float maxAngleLoadingShot = 45;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float airSpeed = 0.02f;
    [SerializeField] private float airThrust = 0.02f;
    [SerializeField] private float downAirThrust = 10f;
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private AnimationCurve dashSpeedCurve;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float grabSpeedFactor = 0.8f;
    [SerializeField] private float grabWithoutBallSpeedFactor = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;
    [SerializeField] private float dashSpeedFactorPerChargeIntensityLevel = 0.2f;

    [Header("Rollover")]
    [SerializeField] private float rollOverSpeed = 5f;
    [SerializeField] private float rollOverDuration = 1f;
    [SerializeField] private float rollOverCountdown = 1f;
    
    [Header("Charge")]
    [SerializeField] private float chargeDuration = 1.4f;
    
    [Header("Shoulder")]
    [SerializeField] private AnimationCurve shoulderPower;
    [SerializeField] private AnimationCurve shoulderDuration;

    [Header("Player Config")]
    [SerializeField] public int playerNumber = 0;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;
    [SerializeField] public bool isAI = false;
    [SerializeField] public bool isActive = false;
    [SerializeField] public bool disableOnStart = true;

    [Header("Sounds")]
    [SerializeField] public AudioClip hitPlayerSound;
    [SerializeField] public AudioClip launchBallSound;
    [SerializeField] public AudioClip hitBallSound;
    [SerializeField] public AudioClip dashSound;
    [SerializeField] public AudioClip chargeSound;
    [SerializeField] public AudioClip impactBallSound;

    [Header("VFX")]
    [SerializeField] private ParticleSystem turboParticles;
    [SerializeField] private GameObject dashParticles;
    [SerializeField] private GameObject ballImpact;
    [SerializeField] private ParticleSystem loadingShootParticles;
    [SerializeField] private ChargeVfx chargeVfx;
    [SerializeField] private GameObject whiteFlashVfx;

    public Brain brain;
    public InputManager inputManager;

    // Cached variables
    private Rigidbody2D _rigidBody;
    private AudioSource _audioSource;
    private Rewired.Player _rwPlayer;
    private Animator _animator;
    private Collider2D _bodyCollider;
    private GameManager _gameManager;
    private OrbsManager _orbManager;
    private InputBuffer _inputBuffer;
    private CameraShaker _cameraShaker;

    // State variable
    [Header("State")]
    public float builtupPower;
    public float builtUpCurl;
    public bool isDribbling;
    public Ball ballGrabbed;
    public Vector2 tackleStartPoint;
    public Vector2 currentDashVelocity;
    public bool IsReturnFromTackle;
    public float currentEnergy;
    public bool isReplenishing;
    public bool moveEnabled = true;
    public bool isShootCoolDown;
    public bool isTackling;
    public bool isShooting;
    public bool isThrowing;
    public bool isStunned;
    public bool hasJustEnteredWater;
    private bool isInvicible;
    public bool isMotionGrabbing;
    public bool isRollingOver;
    public bool isRollingCountDown;
    public bool isGrabbingDisabled;
    public GameObject dashParticlesObject;
    public bool isInWater = true;
    public bool isTouchingWater = true;
    public bool isAutoShootDisabled;
    public bool isLoadingAutoShoot;
    public List<PlayerState> playerStates = new();
    private int _chargeLevel = 0;
    
    // Shoulder state
    private bool _isShouldering;
    private float _shoulderTimer = -1;
    private Vector2 _currentShoulderVelocity;

    // Routines
    private IEnumerator _enteredWaterRoutine;
    private IEnumerator _disableBodyRoutine;
    private IEnumerator _disableGrabbingRoutine;
    private IEnumerator _replenishRoutine;
    private IEnumerator _rollingOverRoutine;
    private IEnumerator _chargeRoutine;

    // Water management
    public bool canGoUp = true;

    [Header("Timers")]
    public float dashTimer = 0f;
    public float loadingAutoShootTimer;
    public float windupAutoShootTimer;
    public float freezeAutoShootTimer;
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
    
     private void OnTriggerEnter2D(Collider2D collision)
    {
        var tagName = collision.gameObject.tag;
        var parentGO = collision.gameObject.transform.parent;
        Player playerParent = null;
        if (parentGO != null)
        {
            playerParent = collision.gameObject.GetComponentInParent<Player>();
        }
        
        if (shoulderHitbox.IsTouching(collision) && playerParent)
        {
            playerParent.ReceiveTackle(this);
            _shoulderTimer = -1;
            _isShouldering = false;
        }

        if (!isInvicible && playerParent != null && 
            (tagName == "DashHitbox" || tagName == "ShotHitbox") 
            && !playerParent.IsGrabbing() && playerParent.team != team)
        {
            _audioSource.PlayClipWithRandomPitch(hitPlayerSound, isTouchingWater);
            StartCoroutine(TriggerInvicibility());
            ReceiveTackle(this);
            DisableShotHitbox();
            DisableDashHitbox();
            playerParent.DisableShotHitbox();
            playerParent.DisableDashHitbox();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var go = collision.gameObject;
        var player = go.GetComponent<Player>();

        // BALL GRAB COLLISION
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (!ballGrabbed && ball)
        {  
            _audioSource.PlayClipWithRandomPitch(impactBallSound, isTouchingWater);
            var impulseSpeed = IsDashing() ? ball.impulseSpeedFactor * 1.5f : ball.impulseSpeedFactor;

            var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
            //rigidBodyBall.velocity = _rigidBody.velocity * impulseSpeed;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _animator.SetBool(AnimatorParameters.IsShooting, false);
        _bodyCollider = GetComponent<Collider2D>();
        _gameManager = FindObjectOfType<GameManager>();
        _audioSource = GetComponent<AudioSource>();
        _orbManager = GetComponentInChildren<OrbsManager>();
        _rwPlayer = ReInput.players.GetPlayer(playerNumber);
        _inputBuffer = GetComponent<InputBuffer>();
        inputManager = new RewiredInputManager(playerNumber);
        _cameraShaker = FindFirstObjectByType<CameraShaker>();
        
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

        if (shotHitbox != null)
        {
            shotHitbox.enabled = false;
        }

        if (dashHitbox != null)
        {
            dashHitbox.enabled = false;
        }

        if (grabHitbox != null)
        {
            //grabHitbox.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inputManager.GetButtonDown("Start"))
        {
            _gameManager.ManagePause();
        }

        ManageWater();
        ManageGravity();
        ManageAutoShoot();
        ManageThrow();
        ManageMove();
        ManageRollOver();
        ManageCurl();
        ManageDash();
        ManageDribble();
        ManageCharge();
        ManageShoulder();
        ManageRotation();
        ManageAnimation();

        if (isReplenishing)
        {
            AddEnergy(Time.deltaTime * GameSettings.replenishEnergyPerSecond);
        }
    }
    private IEnumerator TriggerInvicibility()
    {
        isInvicible = true;
        yield return new WaitForSeconds(invicibilityFrames);
        isInvicible = false;
    }

    private void ReceiveTackle(Player player)
    {
        isStunned = true;
        grabHitbox.enabled = false;
        if (ballGrabbed)
        {
            ballGrabbed.player = null;
            Destroy(FindFirstObjectByType<Ball>().gameObject);
            var newBall = Instantiate(ballPrefab, GetThrowPoint().position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();
            var velX = Random.Range(-0.5f, 0.5f);
            newBallBody.linearVelocity = new Vector2(velX, 4f);
            DisableBallCollision(newBall);
            player.DisableBallCollision(newBall);
        }

        CancelAutoShoot();
        
        StartCoroutine(FinishBeingTackled());
    }

    public void DisableMove()
    {
        moveEnabled = false;
    }

    public void EnableMove()
    {
        moveEnabled = true;
    }

    private void ManageWater()
    {
        var previousTouchingWater = isTouchingWater;
        isTouchingWater = IsTouchingWater();
        isInWater = IsInWater();

        if (!previousTouchingWater && isTouchingWater)
        {
            hasJustEnteredWater = true;
        }

        if (isInWater)
        {
            hasJustEnteredWater = false;
        }

    }
    
    private void ManageRollOver()
    {
        if (IsDashing() || IsLoadingShoot() || IsTackling())
        {
            return;
        }

        var xAxis = inputManager.GetAxis("Move Horizontal 2");
        var yAxis = inputManager.GetAxis("Move Vertical 2");
        float axis;

        if (Mathf.Abs(xAxis) > Mathf.Abs(yAxis))
        {
            axis = xAxis;
            if (transform.localScale.x < 0)
            {
                //axis = -xAxis;
            }
        } else
        {
            axis = yAxis;
            if (transform.localScale.x > 0)
            {
                axis = -yAxis;
            }
        }

        var isUp = axis > 0;
        var isDown = axis < 0;
        
        if (!isTouchingWater || (!isUp && !isDown))
        {
            return;
        }

        if (isRollingOver || isRollingCountDown)
        {
            return;
        }

        var adjusted = Vector2.Perpendicular(_rigidBody.linearVelocity);
        adjusted.Normalize();

        var computedSpeed = isUp ? -rollOverSpeed : rollOverSpeed;
        if (transform.localScale.x < 0 && isUp)
        {
           //computedSpeed *= -1;
        } else if (transform.localScale.x < 0 && isDown)
        {
            //computedSpeed *= -1;
        }
        adjusted *= computedSpeed;

        _rigidBody.AddForce(adjusted);
        //rigidBody.velocity = adjusted;
        Debug.DrawRay(transform.position, new Vector3(adjusted.x, adjusted.y, 0));
        isRollingOver = true;
        _rollingOverRoutine = StartRollingOver();
        StartCoroutine(StartRollingOver());
        StartCoroutine(StartRollingCountDown());
    }
    IEnumerator DisableGrabbing(float seconds)
    {
        isGrabbingDisabled = true;
        yield return new WaitForSeconds(seconds);
        isGrabbingDisabled = false;
    }

    IEnumerator StartRollingCountDown()
    {
        isRollingCountDown = true;
        yield return new WaitForSeconds(rollOverCountdown);
        isRollingCountDown = false;
    }

    IEnumerator StartRollingOver()
    {
        isRollingOver = true;
        yield return new WaitForSeconds(rollOverDuration);
        isRollingOver = false;
    }

    private void ManageCurl()
    {
        var curlingLeft = inputManager.GetAxis("Curl Left") > 0;
        var curlingRight = inputManager.GetAxis("Curl Right") > 0;
        if (IsLoadingShoot() && (curlingLeft || curlingRight))
        {
            builtUpCurl = Mathf.Min(1f, builtUpCurl + (Time.deltaTime / timeToMaxCurl));

            float direction;

            if (curlingLeft)
            {
                direction = transform.localScale.x < 0 ? 1 : -1;
            } else
            {
                direction = transform.localScale.x < 0 ? -1 : 1;
            }

            ballGrabbed.ApplyRotation(direction * builtUpCurl);
        } else if (IsLoadingShoot())
        {
            builtUpCurl = 0f;
            ballGrabbed.ApplyRotation(builtUpCurl);
        }
    }

    private void ManageGravity()
    {
        _rigidBody.gravityScale = IsTouchingWater() ? 0 : gravityScale;
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
            _rigidBody.linearVelocity = ComputeMoveSpeed(tackleSpeed);
        }
    }

    public void AddOrbs(int number)
    {
        _orbManager.AddOrbs(number);
    }

    #region ChargeState

    private void ManageCharge()
    {
        // TODO RENAME ACTION ?
        var hasPressedCharge = inputManager.GetButtonDown("Grab");

        if (hasPressedCharge && _orbManager.HasNumbersOrbs(1))
        {
            ConsumeCharge();
        }
        
        if (chargeVfx)
        {
            chargeVfx.SetIntensity(_chargeLevel);
        }
    }

    private void ConsumeCharge()
    {
        _orbManager.ConsumeOrbs(1);
        _chargeLevel++;
            
        if (_chargeRoutine != null)
        {
            StopCoroutine(_chargeRoutine);
        }

        _audioSource.PlayClipWithRandomPitch(chargeSound, isTouchingWater, 1 + 0.3f * _chargeLevel, 1 + 0.3f * _chargeLevel);
        _chargeRoutine = ChargeCoRoutine();
        StartCoroutine(_chargeRoutine);
    }
    
    #endregion
    
    #region ShoulderState

    private void ManageShoulder()
    {
        if (_shoulderTimer > -1)
        {
            _shoulderTimer += Time.deltaTime;
        }

        if (_isShouldering)
        {
            _rigidBody.linearVelocity = _currentShoulderVelocity;
        }
        
        if (isShooting || IsDashing() || _isShouldering)
        {
            return;
        }

        if (inputManager.GetButtonDown("Shoulder"))
        {
            _shoulderTimer = 0;
        }
        
        if (inputManager.GetButtonUp("Shoulder") && _shoulderTimer > -1)
        {
            var speedX = inputManager.GetAxis("Move Horizontal");
            var speedY = inputManager.GetAxis("Move Vertical");
            _currentShoulderVelocity = shoulderPower.Evaluate(_shoulderTimer) * new Vector2(speedX, speedY).normalized;
            StartCoroutine(ShoulderingRoutine(shoulderDuration.Evaluate(_shoulderTimer)));
            _shoulderTimer = -1;
        }
    }

    IEnumerator ShoulderingRoutine(float duration)
    {
        _isShouldering = true;
        shoulderHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        _isShouldering = false;
        shoulderHitbox.gameObject.SetActive(false);
    }
    
    #endregion

    #region DribbleState
    
    private void ManageDribble()
    {
        inputManager.ForceRegisterInputEvents();
        if (inputManager.GetButtonDown("Dribble"))
        {
            isDribbling = true;
            controlHitbox.gameObject.SetActive(true);

            if (IsGrabbing())
            {
                ballGrabbed.player = null;
                Destroy(ballGrabbed.gameObject);
                var newBall = Instantiate(ballPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();
                newBallBody.linearVelocity = _rigidBody.linearVelocity.normalized * (_rigidBody.linearVelocity.magnitude + releasePower);
                DisableBallCollision(newBall);
            }
        }
        if (inputManager.GetButtonUp("Dribble"))
        {
            isDribbling = false;
            controlHitbox.gameObject.SetActive(false);
        }
            
        var ball = FindFirstObjectByType<Ball>();
    
        if (ball && !isStunned)
        {
            var ballCollider = ball.GetComponent<Collider2D>();
            if (ballCollider)
            {
               Physics2D.IgnoreCollision(ballCollider, _bodyCollider, !isDribbling);
            }
            
            grabHitbox.enabled = !isDribbling;
        }
        
        inputManager.UnForceRegisterInputEvents();
    }

    IEnumerator ChargeCoRoutine()
    {
        yield return new WaitForSeconds(chargeDuration);
        _chargeLevel = 0;
    }

    #endregion
    
    private void ManageDash()
    {
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");

        if (IsDashing())
        {
            _rigidBody.linearVelocity = currentDashVelocity.normalized * dashSpeedCurve.Evaluate(dashDuration - dashTimer);
            dashTimer -= Time.deltaTime;
            return;
        }
        
        if (speedX == 0 && speedY == 0)
        {
            return;
        }
        
        var hasPressedDash = inputManager.GetButtonDown("Dash");

        if (hasPressedDash && !IsLoadingShoot() && !isLoadingAutoShoot && _orbManager.HasNumbersOrbs(1))
        {
            // if (IsLoadingShoot())
            // {
            //     // TODO PROPER SHOOT STATE
            //     //CancelAutoShoot();
            // }
            //
            // if (isLoadingAutoShoot)
            // {
            //     CancelAutoShoot();
            // }
            ConsumeCharge();
            var go = Instantiate(dashParticles, transform.position, transform.rotation, transform);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            //main.duration = dashDuration;
            dashParticlesObject = go;
            Destroy(go, 5f);
            RemoveEnergy(GameSettings.dashEnergyCost);
            dashTimer = dashDuration;
            _rigidBody.linearVelocity = ComputeMoveSpeed(dashSpeedCurve.Evaluate(0) * (1 + _chargeLevel * dashSpeedFactorPerChargeIntensityLevel));
            currentDashVelocity = _rigidBody.linearVelocity;
            _audioSource.PlayClip(dashSound, isTouchingWater);
        }
    }

    private void CancelShoot()
    {
        builtupPower = 0;
        isThrowing = false;
    }
    
    private void CancelAutoShoot()
    {
        isShooting = false;
        isAutoShootDisabled = false;
        loadingAutoShootTimer = 0;
        isLoadingAutoShoot = false;
        windupAutoShootTimer = 0;
        loadingShootParticles.Stop();
        _animator.SetBool(AnimatorParameters.IsLoadingKick, false);
    }
    
    private void ManageAutoShoot()
    {
        if (windupAutoShootTimer != 0 && windupAutoShootTimer < anticipationShotTime)
        {
            windupAutoShootTimer += Time.deltaTime;
            return;
        }
        
        if (IsGrabbing() || isShootCoolDown || isAutoShootDisabled)
        {
            isLoadingAutoShoot = false;
            loadingShootParticles.Stop();
            return;
        }

        if (inputManager.GetButtonDown("tackle"))
        {
            _animator.SetBool(AnimatorParameters.IsLoadingKick, true);
            loadingShootParticles.Play();
            isLoadingAutoShoot = true;
            return;
        }

        if (inputManager.GetButton("tackle"))
        {
            loadingAutoShootTimer += Time.deltaTime;
            loadingAutoShootTimer = Mathf.Min(maxShotPower, loadingAutoShootTimer);
        }

        if (_inputBuffer.GetButtonUp("tackle"))
        {
            CancelDash();
            windupAutoShootTimer += Time.deltaTime;
            isShooting = true;
            loadingShootParticles.Stop();
            isLoadingAutoShoot = false;
            _animator.SetBool(AnimatorParameters.IsLoadingKick, false);
            return;
        }

        if (!isShooting)
        {
            return;
        }

        // Is ball from reachable distance?
        var ball = FindFirstObjectByType<Ball>();
        if (!ball)
        {
            return;
        }
        
        var ballPosition = ball.gameObject.transform.position;

        var feetBodyDistance = Vector2.Distance(feetPoint.position, transform.position);
        var bodyBallDistance = Vector2.Distance(ballPosition, transform.position);

        if (bodyBallDistance < feetBodyDistance + shootTolerance)
        {
            var ballPosition2d = new Vector3(ballPosition.x, ballPosition.y);
            var position2d = new Vector3(transform.position.x, transform.position.y);
            
            var distanceFromBall = ballPosition2d - position2d;
            var angle = Mathf.Atan2(distanceFromBall.y, distanceFromBall.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));

            var chargeFactor = 1 + _chargeLevel * 0.6f;

            if (whiteFlashVfx && _chargeLevel == 3)
            {
                var ballPositionWithZ = new Vector3(ballPosition2d.x, ballPosition2d.y, -5);
                Instantiate(whiteFlashVfx, ballPositionWithZ, Quaternion.identity, ball.transform);
                
            }
            
            var shotPower = chargeFactor * (minShotPower + (maxShotPower - minShotPower) * (loadingAutoShootTimer / timeToBuildUp));
            StartCoroutine(ShootingRoutine(shotPower));
            StartCoroutine(DisableAutoShoot());
            StartCoroutine(DisableGrabbing(0.3f));
        }
        else
        {
            var speedX = inputManager.GetAxis("Move Horizontal");
            var speedY = inputManager.GetAxis("Move Vertical");
            var angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg + 90;
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, angle));
        }
        
        StartCoroutine(CooldownShooting());
        loadingAutoShootTimer = 0;
        isLoadingAutoShoot = false;
        windupAutoShootTimer = 0;
        
        _animator.SetBool(AnimatorParameters.IsLoadingKick, false);
    }

    private void ManageThrow()
    {
        if (isShooting)
        {
            return;
        }
        
        if (IsLoadingShoot())
        {
            builtupPower += Time.deltaTime;
            builtupPower = Mathf.Min(timeToBuildUp, builtupPower);
        }
        
        if (IsGrabbing() && inputManager.GetButtonUp("Tackle"))
        {
            GameObject newBall;
            Destroy(FindFirstObjectByType<Ball>().gameObject);
            DisableGrabbingMotion();
            var trueThrowPoint = inputManager.GetButtonUp("Tackle") ? throwPointLoading : throwPoint;
            newBall = Instantiate(ballPrefab, trueThrowPoint.position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

            var combinedVelocity = Mathf.Abs(_rigidBody.linearVelocity.x) + Mathf.Abs(_rigidBody.linearVelocity.y);

            var computedShotPower = GetSpeed() + releasePower;

            if (inputManager.GetButtonUp("Tackle"))
            {
                //StartCoroutine(ManageShootingAnimation());
                computedShotPower = minShotPower + ((maxShotPower - minShotPower) * (builtupPower / timeToBuildUp));
            }

            var curlingLeft = inputManager.GetAxis("Curl Left") > 0;
            var curlingRight = inputManager.GetAxis("Curl Right") > 0;

            float velocityX;
            float velocityY;

            computedShotPower *= 1 + shootSpeedFactorPerChargeIntensityLevel * _chargeLevel;
            if (curlingLeft || curlingRight)
            {
                computedShotPower *= shotSpeedCurlFactor;
            }

            if (combinedVelocity == 0)
            {
                velocityX = transform.localScale.x * computedShotPower;
                velocityY = 0f;
            }
            else
            {
                var speed = ComputeShotSpeed(computedShotPower);
                velocityX = speed.x;
                velocityY = speed.y;
            }

            newBallBody.linearVelocity = new Vector2(velocityX, velocityY);
            builtupPower = 0;

            float computedCurlPower;
            if (curlingRight)
            {
                computedCurlPower = transform.localScale.x < 0 ? -curlPower : curlPower;
                computedCurlPower *= builtUpCurl;
                newBallBody.angularVelocity = (computedCurlPower * inputManager.GetAxis("Curl Right"));
            } else if (curlingLeft)
            {
                computedCurlPower = transform.localScale.x < 0 ? -curlPower : curlPower;
                computedCurlPower *= builtUpCurl;
                newBallBody.angularVelocity = (-computedCurlPower * inputManager.GetAxis("Curl Left"));
            }

            builtUpCurl = 0;
            _audioSource.PlayClipWithRandomPitch(launchBallSound, isTouchingWater);

            if (_disableGrabbingRoutine != null)
            {
                StopCoroutine(_disableGrabbingRoutine);
            }
            _disableGrabbingRoutine = DisableGrabbing(0.6f);

            StartCoroutine(_disableGrabbingRoutine);

            if (newBall)
            {
                if (_disableBodyRoutine != null)
                {
                    StopCoroutine(_disableBodyRoutine);
                }
                _disableBodyRoutine = DisableBody(newBall);

                StartCoroutine(_disableBodyRoutine);
            }

            StartCoroutine(DisableAutoShoot());
        }
    }

    private void ManageMove()
    {
        float computedSpeed = speed;
        
        if (isShooting || isTackling || IsDashing() || isRollingOver || isLoadingAutoShoot || _isShouldering || _shoulderTimer > -1)
        {
            return;
        }

        var currentAirThrust = 0f;
        if (inputManager.GetButton("Turbo") && currentEnergy > 0)
        {
            RemoveEnergy(Time.deltaTime * GameSettings.turboEnergyCostPerSecond);
            currentAirThrust = airThrust;
            computedSpeed = boostSpeed;
            _animator.SetFloat("Speed Multiplier", 1.5f);

            if (!turboParticles.isPlaying)
            {
                turboParticles.Play();
            }
        }
        else
        {
            turboParticles.Stop();
            _animator.SetFloat("Speed Multiplier", 1f);
        }

        if (IsLoadingShoot())
        {
            computedSpeed *= shootSpeedFactor;
        } if (IsGrabbing()) {
             computedSpeed *= grabSpeedFactor;
        } 
        
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");

        if (hasJustEnteredWater)
        {
            //speedY = Mathf.Clamp(speedY, -1, 0);
        }
        
        var speedVector = new Vector2(speedX, speedY);
        if (speedVector.magnitude > 1)
        {
            speedVector.Normalize();
        }
        speedVector *= computedSpeed;
        _animator.SetBool("IsDiving", false);
        if ((speedX != 0 || speedY != 0) && isTouchingWater)
        {
            _rigidBody.linearVelocity = speedVector;
        }
        else if (!isTouchingWater)
        {
            var counterForce = airSpeed * inputManager.GetAxis("Move Horizontal");
            var downAirForce = 0f;
            if (!IsLoadingShoot() && inputManager.GetAxis("Move Vertical") < 0) {
                downAirForce = Time.deltaTime * downAirThrust * inputManager.GetAxis("Move Vertical");
                _animator.SetBool("IsDiving", true);
            }
            _rigidBody.linearVelocity = new Vector2(Mathf.Clamp(_rigidBody.linearVelocity.x + counterForce, -speed, speed), _rigidBody.linearVelocity.y + downAirForce);
        }
    }
    public bool IsLoadingShoot()
    {
        return IsGrabbing() && inputManager.GetButton("Tackle");
    }

    private void CancelDash()
    {
        dashTimer = 0f;
        if (dashParticlesObject != null)
        {
            Destroy(dashParticlesObject);
        }
    }

    public void EnableIsTackling()
    {
        var ball = FindObjectOfType<Ball>();
        if (ball != null)
        {
            var collider = ball.GetComponent<Collider2D>(); 
            if (collider != null)
            {
                Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
            }
        };
        grabHitbox.enabled = false;
        isTackling = true;
    }

    public void DisableIsTackling()
    {
        var ball = FindObjectOfType<Ball>();

        Collider2D collider2D = null;
        if (ball != null)
        {
            collider2D = ball.GetComponent<Collider2D>();
        }
        
        grabHitbox.enabled = true;
        if (collider2D != null)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
        }
        
        isTackling = false;
    }
    
    private bool IsInWater()
    {
        var bounds = _bodyCollider.bounds;
        var waterBounds = FindFirstObjectByType<Water>().GetComponent<Collider2D>().bounds;
        return waterBounds.Contains(bounds.min) && waterBounds.Contains(bounds.max);
    }

    private bool IsTouchingWater()
    {
        return _bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
    }

    private void RemoveEnergy(float energy)
    {
        currentEnergy = Mathf.Max(0, currentEnergy - energy);
        if (_replenishRoutine != null)
        {
            StopCoroutine(_replenishRoutine);
        }
        _replenishRoutine = DisableEnergyReplenish();
        StartCoroutine(_replenishRoutine);
    }

    public void AddEnergy(float energy)
    {
        currentEnergy = Mathf.Min(currentEnergy + energy, GameSettings.energyAmount);
    }
    
    IEnumerator DisableAutoShoot()
    {
        isAutoShootDisabled = true;
        yield return new WaitForSeconds(shootCoolDown);
        isAutoShootDisabled = false;
    }
    
    IEnumerator CooldownShooting()
    {
        isShooting = true;
        isAutoShootDisabled = true;
        yield return new WaitForSeconds(shootCoolDown);
        isShooting = false;
        isAutoShootDisabled = false;
    }

    IEnumerator ShootingRoutine(float power)
    {
        var ball = FindObjectOfType<Ball>();
        
        var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
        
        if (rigidBodyBall == null)
        {
            yield return new WaitForSecondsRealtime(0);
        }
        else
        {
            var adjustedPower = Mathf.Max(accelerationBall * rigidBodyBall.linearVelocity.magnitude, power);
        
            Time.timeScale = 0;
            _audioSource.PlayClipWithRandomPitch(hitBallSound, isTouchingWater);
            isShooting = true;
            var percentPower = Math.Min(1f, (adjustedPower - minShotPower) / (maxShotPower - minShotPower));
            var shootFreezeTime = 0;
            yield return new WaitForSecondsRealtime(_chargeLevel == 3 ? 0 : 0);
            
            float speedX = inputManager.GetAxis("Move Horizontal");
            float speedY = inputManager.GetAxis("Move Vertical");
            float angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg;
            var impactVfx = Instantiate(ballImpact, ball.transform.position, Quaternion.Euler(new Vector3(0, 0, angle)));
            Destroy(impactVfx, 0.5f);
            Time.timeScale = 1;
            
            if (_cameraShaker)
            {
                _cameraShaker.ShakeFor(0.3f, powerShotCameraShake.Evaluate(power) / 10);
            }

            Shoot(power);
        }
    }

    IEnumerator DisableEnergyReplenish()
    {
        isReplenishing = false;
        yield return new WaitForSeconds(GameSettings.replenishAfter);
        isReplenishing = true;
    }

    IEnumerator FinishBeingTackled()
    {
        _animator.SetBool(AnimatorParameters.IsTackled, true);
        DisableInputs();
        yield return new WaitForSeconds(tackleStunDuration);
        grabHitbox.enabled = true;
        isStunned = false;
        _animator.SetBool(AnimatorParameters.IsTackled, false);
        EnableInputs();
    }

    private void ManageAnimation()
    {
        var isImmobile = inputManager.GetAxis("Move Horizontal") == 0 && inputManager.GetAxis("Move Vertical") == 0;
        var isUp = _rigidBody.linearVelocity.y >= 0;
        _animator.SetBool(AnimatorParameters.IsSwimming, isTouchingWater && !isImmobile);
        _animator.SetBool(AnimatorParameters.IsIdle, isTouchingWater && isImmobile);
        _animator.SetBool(AnimatorParameters.IsInAir, !isTouchingWater);
        _animator.SetBool(AnimatorParameters.IsDashing, IsDashing());
        _animator.SetBool(AnimatorParameters.IsTackling, isShooting);
        _animator.SetBool(AnimatorParameters.IsUp, isUp);
        _animator.SetBool(AnimatorParameters.IsDown, !isUp);
        _animator.SetBool(AnimatorParameters.IsLoadingShoot, IsLoadingShoot());
        _animator.SetBool(AnimatorParameters.IsGrabbing, isMotionGrabbing);
        _animator.SetBool(AnimatorParameters.IsRolling, isRollingOver);
        _animator.SetBool(AnimatorParameters.IsLoadingShoulder, _shoulderTimer > -1);
        _animator.SetBool(AnimatorParameters.IsShouldering, _isShouldering);
        //animator.SetBool("IsShooting", inputManager.GetButtonUp("Shoot"));
    }

    private void ManageRotation()
    {
        if (isRollingOver || isShooting)
        {
            return;
        }

        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");

        var scaleX = _rigidBody.linearVelocity.x < 0 ? - Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);

        if (isLoadingAutoShoot)
        {
            scaleX = speedX < 0 ? - Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);
        }
        
        var adjustedRotationSpeed = isTackling || IsDashing() ? 3000 : rotationSpeed;

        Quaternion currentAngle = transform.rotation;
        float angle = Mathf.Atan2(_rigidBody.linearVelocity.y, _rigidBody.linearVelocity.x) * Mathf.Rad2Deg;
        
        if (!IsTouchingWater() && !IsDashing())
        {
            angle = 0f;
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle)), adjustedRotationSpeed * Time.deltaTime);
        } else
        {
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }

        // Manage rotation
        if (IsLoadingShoot() || isLoadingAutoShoot || _shoulderTimer > -1)
        {
            angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg;
            
            if (scaleX > 0)
            {
                angle = Mathf.Clamp(angle, -maxAngleLoadingShot, maxAngleLoadingShot);
            }
            else
            {
                angle = ((angle + 360) % 360) - 180;
                angle = Mathf.Clamp(angle, -maxAngleLoadingShot, maxAngleLoadingShot);
            }
            
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle)), adjustedRotationSpeed * Time.deltaTime);
        }

        if (_rigidBody.linearVelocity.x != 0)
        {
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }

    }

    public void Grab(Ball ball)
    {
        if (isGrabbingDisabled || isShooting || isLoadingAutoShoot)
        {
            return;
        }

        var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
        Collider2D collider = ball.GetComponent<Collider2D>();
        Destroy(rigidBodyBall);
        //Destroy(collider);
        collider.isTrigger = true;
        DisableGrabbingMotion();

        ball.player = this;
        ball.HasJustSpawned = true;
        ballGrabbed = ball;
    }

    public bool IsTackling()
    {
        return isTackling;
    }

    public bool IsGrabbing()
    {
        return ballGrabbed != null;
    }

    public void Shoot(float shotSpeed)
    {
        var ball = FindObjectOfType<Ball>();
        var rigidBodyBall = ball.GetComponent<Rigidbody2D>();

        shotSpeed = Mathf.Max(accelerationBall * rigidBodyBall.linearVelocity.magnitude, shotSpeed);

        float velocityX;
        float velocityY;

        var comutedSpeed = ComputeShotSpeed(shotSpeed);
        velocityX = comutedSpeed.x;
        velocityY = comutedSpeed.y;

        rigidBodyBall.linearVelocity = new Vector2(velocityX, velocityY);
        Vector3 endLine = new Vector3(rigidBodyBall.linearVelocity.x, rigidBodyBall.linearVelocity.y, 0);

        if (_disableBodyRoutine != null)
        {
            StopCoroutine(_disableBodyRoutine);
        }

        _disableBodyRoutine = DisableBody(rigidBodyBall.gameObject);
        StartCoroutine(_disableBodyRoutine);
        CancelDash();
    }

    public void Shoot()
    {
        Shoot(minShotPower);
    }
    public void DashShoot()
    {
        Shoot(dashShotPower);
    }

    public void DisableBallCollision(GameObject newBall)
    {
        if (_disableBodyRoutine != null)
        {
            StopCoroutine(_disableBodyRoutine);
        }
        
        _disableBodyRoutine = DisableBody(newBall);
        StartCoroutine(_disableBodyRoutine);
    }

    public void EnableShotHitbox()
    {
        shotHitbox.enabled = true;
        builtupPower = 0;
    }

    public void DisableShotHitbox()
    {
        shotHitbox.enabled = false;
        builtupPower = 0;
    }

    public void EnableDashHitbox()
    {
        Debug.Log("Enabling");
        dashHitbox.enabled = true;
    }

    public void DisableDashHitbox()
    {
        dashHitbox.enabled = false;
    }

    public void EnableGrabbingMotion()
    {
        isMotionGrabbing = true;
        //grabHitbox.enabled = true;
    }

    public void DisableGrabbingMotion()
    {
        isMotionGrabbing = false;
        //grabHitbox.enabled = false;
    }

    IEnumerator DisableBody(GameObject newBall)
    {
        var ballBody = !newBall ? null : newBall.GetComponent<Collider2D>();
        if (ballBody && grabHitbox && _bodyCollider)
        {
            Physics2D.IgnoreCollision(ballBody, grabHitbox, true);
            Physics2D.IgnoreCollision(ballBody, _bodyCollider, true);
            yield return new WaitForSeconds(0.6f);
            if (ballBody)
            {
                Physics2D.IgnoreCollision(ballBody, grabHitbox, false);
                Physics2D.IgnoreCollision(ballBody, _bodyCollider, false);
            }   
        }
    }

    private Vector2 ComputeMoveSpeed(float speedWanted)
    {
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");
        float totalFactor = Mathf.Abs(speedX) + Mathf.Abs(speedY);
        float speedSquare = speedWanted * speedWanted;

        if (totalFactor == 0)
        {
            return new Vector2(0, 0);
        }

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

    private Vector2 ComputeShotSpeed(float speedWanted)
    {
        float speedX = inputManager.GetAxis("Move Horizontal");
        float speedY = inputManager.GetAxis("Move Vertical");
        
        if (Mathf.Abs(speedX) + Mathf.Abs(speedY) == 0)
        {
            speedX = _rigidBody.linearVelocity.x;
            speedY = _rigidBody.linearVelocity.y;
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
        return _rigidBody.linearVelocity.magnitude;
    }

    public void DisableInputs()
    {
        if (inputManager != null)
        {
            inputManager.UnregisterInputEvents();
        }
        
    }
        public void EnableInputs()
    {
        if (inputManager != null)
        {
            inputManager.RegisterInputEvents();
        }
    }
}
