using System;
using System.Collections;
using System.Collections.Generic;
using enums;
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
    
    [Header("Move")]
    [SerializeField] private float rotationSpeed = 900f;
    [SerializeField] private float maxAngleLoadingShot = 45;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float airSpeed = 0.02f;
    [SerializeField] private float airThrust = 0.02f;
    [SerializeField] private float downAirThrust = 10f;
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float grabSpeedFactor = 0.8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float grabWithoutBallSpeedFactor = 0.2f;
    [SerializeField] private float lockedTime = 0.2f;

    [Header("Rollover")]
    [SerializeField] private float rollOverSpeed = 5f;
    [SerializeField] private float rollOverDuration = 1f;
    [SerializeField] private float rollOverCountdown = 1f;

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

    [Header("VFX")]
    [SerializeField] private ParticleSystem turboParticles;
    [SerializeField] private GameObject dashParticles;
    [SerializeField] private GameObject ballImpact;

    public Brain brain;
    public InputManager inputManager;

    // Cached variables
    private Rigidbody2D _rigidBody;
    private AudioSource _audioSource;
    private Rewired.Player _rwPlayer;
    private Animator _animator;
    private Collider2D _bodyCollider;
    private GameManager _gameManager;
    private AudioLowPassFilter _audioFilter;
    private OrbsManager _orbManager;
    private InputBuffer _inputBuffer;

    // State variable
    [Header("State")]
    public float builtupPower;
    public float builtUpCurl;
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

    // Routines
    private IEnumerator _enteredWaterRoutine;
    private IEnumerator _disableBodyRoutine;
    private IEnumerator _disableGrabbingRoutine;
    private IEnumerator _replenishRoutine;
    private IEnumerator _rollingOverRoutine;

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
        Debug.Log("GRAB 2" + playerParent);
        Debug.Log("GRAB 3" + tagName);
        if (!isInvicible && playerParent != null && 
            (tagName == "DashHitbox" || tagName == "ShotHitbox") 
            && !playerParent.IsGrabbing() && playerParent.team != team)
        {
            _audioSource.clip = hitPlayerSound;
            AudioUtils.PlaySound(gameObject);
            StartCoroutine(TriggerInvicibility());
            Debug.Log("GRAB LOL");
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

        Debug.Log("Body collided " + collision.gameObject.name);

        // BALL GRAB COLLISION
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (ballGrabbed == null && ball != null)
        {
            if (inputManager.GetButton("Grab"))
            {
                /*var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
                Collider2D collider = ball.GetComponent<Collider2D>();
                Grab(ball);
                Destroy(rigidBodyBall);
                Destroy(collider);*/
            }
            else
            {
                var impulseSpeed = IsDashing() ? ball.impulseSpeedFactor * 1.5f : ball.impulseSpeedFactor;

                var rigidBodyBall = ball.GetComponent<Rigidbody2D>();
                rigidBodyBall.velocity = _rigidBody.velocity * impulseSpeed;

            }

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
        _audioFilter = GetComponent<AudioLowPassFilter>();
        _orbManager = GetComponentInChildren<OrbsManager>();
        _rwPlayer = ReInput.players.GetPlayer(playerNumber);
        _inputBuffer = GetComponent<InputBuffer>();
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

        ManageState();
        ManageWater();
        ManageGravity();
        //ManageShoot();
        ManageAutoShoot();
        ManageThrow();
        ManageMove();
        ManageRollOver();
        ManageCurl();
        ManageDash();
        ManageRotation();
        ManageAnimation();
        ManageGrab();

        if (isReplenishing)
        {
            AddEnergy(Time.deltaTime * GameSettings.replenishEnergyPerSecond);
        }
    }

    private void ManageState()
    {
        playerStates = new();

        // var isInWater = IsInWater();
        // var isImmobile = inputManager.GetAxis("Move Horizontal") == 0 && inputManager.GetAxis("Move Vertical") == 0;
        //
        // if (isInWater && !isImmobile) playerStates.Add(PlayerState.Swimming);
        // if (isImmobile) playerStates.Add(PlayerState.Idle);
        // if (!isInWater) playerStates.Add(PlayerState.InAir);
        // if (isLoadingAutoShoot) playerStates.Add(PlayerState.LoadingShoot);
        // if (IsLoadingShoot()) playerStates.Add(PlayerState.LoadingThrow);
    }

    private IEnumerator TriggerInvicibility()
    {
        isInvicible = true;
        yield return new WaitForSeconds(invicibilityFrames);
        isInvicible = false;
    }

    public void ReceiveTackle(Player player)
    {
        if (ballGrabbed != null)
        {
            ballGrabbed.player = null;
            Destroy(FindObjectOfType<Ball>().gameObject);
            var newBall = Instantiate(ballPrefab, GetThrowPoint().position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();
            
            var velX = Random.Range(-0.5f, 0.5f);
            newBallBody.velocity = new Vector2(velX, 4f);
            DisableBallCollision(newBall);
            player.DisableBallCollision(newBall);
        }

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
                axis = -xAxis;
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
        
        if (!IsInWater() || (!isUp && !isDown))
        {
            return;
        }

        if (isRollingOver || isRollingCountDown)
        {
            return;
        }

        var adjusted = Vector2.Perpendicular(_rigidBody.velocity);
        adjusted.Normalize();

        var computedSpeed = isUp ? -rollOverSpeed : rollOverSpeed;
        if (transform.localScale.x > 0 && isUp)
        {
           // computedSpeed *= -1;
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
        //_rigidBody.gravityScale =  gravityScale;
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
            _rigidBody.velocity = ComputeMoveSpeed(tackleSpeed);
        }
    }

    private void ManageDash()
    {
        if (IsDashing())
        {
            _rigidBody.velocity = currentDashVelocity;
            dashTimer -= Time.deltaTime;
            return;
        }

        var hasPressedDash = inputManager.GetButtonDown("Dash");
        /*if (hasPressedDash && currentEnergy >= GameSettings.dashEnergyCost && !IsLoadingShoot())
        {
            var go = Instantiate(dashParticles, transform.position, Quaternion.identity, transform);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            Destroy(go, dashDuration);
            RemoveEnergy(GameSettings.dashEnergyCost);
            dashTimer = dashDuration;
            rigidBody.velocity = ComputeMoveSpeed(dashSpeed);
        }*/

        if (hasPressedDash && !IsLoadingShoot() && _orbManager.ConsumeOrbs(1))
        {
            var go = Instantiate(dashParticles, transform.position, transform.rotation, transform);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = dashDuration;
            dashParticlesObject = go;
            Destroy(go, 5f);
            RemoveEnergy(GameSettings.dashEnergyCost);
            dashTimer = dashDuration;
            _rigidBody.velocity = ComputeMoveSpeed(dashSpeed);
            currentDashVelocity = _rigidBody.velocity;
        }
    }

    private void ManageGrab()
    {
        var hasPressedGrab = inputManager.GetButton("Grab");

        if (hasPressedGrab && !IsGrabbing() && !IsLoadingShoot() && !IsTackling())
        {
            EnableGrabbingMotion();

            if (IsDashing())
            {
                CancelDash();
            }
        } else
        {
            DisableGrabbingMotion();
        }
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
            return;
        }

        if (inputManager.GetButtonDown("tackle"))
        {
            _animator.SetBool(AnimatorParameters.IsLoadingKick, true);
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
            windupAutoShootTimer += Time.deltaTime;
            isShooting = true;
            isLoadingAutoShoot = false;
            _animator.SetBool(AnimatorParameters.IsLoadingKick, false);
            return;
        }

        if (!isShooting)
        {
            return;
        }

        // Is ball from reachable distance?
        var ball = FindObjectOfType<Ball>();
        if (ball == null)
        {
            return;
        }
        
        var ballPosition = ball.gameObject.transform.position;

        var feetBodyDistance = Vector2.Distance(feetPoint.position, transform.position);
        var bodyBallDistance = Vector2.Distance(ballPosition, transform.position);

        if (bodyBallDistance < feetBodyDistance + shootTolerance)
        {
            var ballPosition2 = new Vector3(ballPosition.x, ballPosition.y);
            var position2 = new Vector3(transform.position.x, transform.position.y);
            var direction = -(ballPosition2 - position2).normalized;
            
            var truc = ballPosition2 - position2;
            float angle = Mathf.Atan2(truc.y, truc.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
            
            var shotPower = minShotPower + ((maxShotPower - minShotPower) * (loadingAutoShootTimer / timeToBuildUp));
            StartCoroutine(ShootingRoutine(shotPower));
            StartCoroutine(DisableAutoShoot());
            StartCoroutine(DisableGrabbing(0.3f));
        }
        else
        {
            float speedX = inputManager.GetAxis("Move Horizontal");
            float speedY = inputManager.GetAxis("Move Vertical");
            float angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg + 90;
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
        
        if (IsGrabbing() && (inputManager.GetButtonUp("Tackle") || inputManager.GetButtonDown("Grab")))
        {
            GameObject newBall = null;
            Destroy(FindObjectOfType<Ball>().gameObject);
            DisableGrabbingMotion();
            var trueThrowPoint = inputManager.GetButtonUp("Tackle") ? throwPointLoading : throwPoint;
            newBall = Instantiate(ballPrefab, trueThrowPoint.position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

            var combinedVelocity = Mathf.Abs(_rigidBody.velocity.x) + Mathf.Abs(_rigidBody.velocity.y);

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

            newBallBody.velocity = new Vector2(velocityX, velocityY);
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

            _audioSource.clip = launchBallSound;
            AudioUtils.PlaySound(gameObject);

            if (_disableGrabbingRoutine != null)
            {
                StopCoroutine(_disableGrabbingRoutine);
            }
            _disableGrabbingRoutine = DisableGrabbing(0.6f);

            StartCoroutine(_disableGrabbingRoutine);

            if (newBall != null)
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

    private void ManageShoot()
    {
        if (isShootCoolDown)
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
            var ball = FindObjectOfType<Ball>();
            StartCoroutine(CooldownShooting());
            isTackling = true;
            CancelDash();
            //animator.SetBool("IsShooting", true);
            // Either release or shoot the ball
        } else if (IsGrabbing() && (inputManager.GetButtonUp("Tackle") || inputManager.GetButtonDown("Grab")))
        {
            GameObject newBall = null;
            Destroy(FindObjectOfType<Ball>().gameObject);
            DisableGrabbingMotion();
            var trueThrowPoint = inputManager.GetButtonUp("Tackle") ? throwPointLoading : throwPoint;
            newBall = Instantiate(ballPrefab, trueThrowPoint.position, Quaternion.identity);
            Rigidbody2D newBallBody = newBall.GetComponent<Rigidbody2D>();

            var combinedVelocity = Mathf.Abs(_rigidBody.velocity.x) + Mathf.Abs(_rigidBody.velocity.y);

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

            newBallBody.velocity = new Vector2(velocityX, velocityY);
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

            _audioSource.clip = launchBallSound;
            AudioUtils.PlaySound(gameObject);

            if (_disableGrabbingRoutine != null)
            {
                StopCoroutine(_disableGrabbingRoutine);
            }
            _disableGrabbingRoutine = DisableGrabbing(0.6f);

            StartCoroutine(_disableGrabbingRoutine);

            if (newBall != null)
            {
                if (_disableBodyRoutine != null)
                {
                    StopCoroutine(_disableBodyRoutine);
                }
                _disableBodyRoutine = DisableBody(newBall);

                StartCoroutine(_disableBodyRoutine);
            }
        }
    }

    private void ManageMove()
    {
        float computedSpeed = speed;
        if (isShooting || isTackling || IsDashing() || isRollingOver || isLoadingAutoShoot)
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
        } else if (inputManager.GetButton("Grab"))
        {
            computedSpeed *= grabWithoutBallSpeedFactor;
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
            _rigidBody.velocity = speedVector;
        }
        else if (!isTouchingWater)
        {
            var counterForce = airSpeed * inputManager.GetAxis("Move Horizontal");
            var downAirForce = 0f;
            if (!IsLoadingShoot() && inputManager.GetAxis("Move Vertical") < 0) {
                downAirForce = Time.deltaTime * downAirThrust * inputManager.GetAxis("Move Vertical");
                _animator.SetBool("IsDiving", true);
            }
            _rigidBody.velocity = new Vector2(Mathf.Clamp(_rigidBody.velocity.x + counterForce, -speed, speed), _rigidBody.velocity.y + downAirForce);
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
        var collider = ball.GetComponent<Collider2D>();
        grabHitbox.enabled = false;
        if (collider != null)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        }
        
        isTackling = true;
    }

    public void DisableIsTackling()
    {
        var ball = FindObjectOfType<Ball>();
        var collider = ball.GetComponent<Collider2D>();
        grabHitbox.enabled = true;
        if (collider != null)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
        }
        isTackling = false;
    }
    
    private bool IsInWater()
    {
        var bounds = _bodyCollider.bounds;
        var waterBounds = FindObjectOfType<Water>().GetComponent<Collider2D>().bounds;
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
        var adjustedPower = Mathf.Max(accelerationBall * rigidBodyBall.velocity.magnitude, power);
        
        Time.timeScale = 0;
        _audioSource.clip = hitBallSound;
        AudioUtils.PlaySound(gameObject);
        isShooting = true;
        var percentPower = Math.Min(1f, (adjustedPower - minShotPower) / (maxShotPower - minShotPower));
        var shootFreezeTime = minShootFreezeTime + (maxShootFreezeTime - minShootFreezeTime) * percentPower;
        yield return new WaitForSecondsRealtime(shootFreezeTime);
        var impactVfx = Instantiate(ballImpact, ball.transform.position, Quaternion.identity);
        Destroy(impactVfx, 0.5f);
        //FindObjectOfType<CameraShaker>().ShakeFor(0.1f, 0.2f * percentPower);
        Time.timeScale = 1;

        Shoot(power);
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
        _animator.SetBool(AnimatorParameters.IsTackled, false);
        EnableInputs();
    }

    private void ManageAnimation()
    {
        var isImmobile = inputManager.GetAxis("Move Horizontal") == 0 && inputManager.GetAxis("Move Vertical") == 0;
        var isUp = _rigidBody.velocity.y >= 0;
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

        var scaleX = _rigidBody.velocity.x < 0 ? - Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);
        var adjustedRotationSpeed = isTackling || IsDashing() ? 3000 : rotationSpeed;

        Quaternion currentAngle = transform.rotation;
        float angle = Mathf.Atan2(_rigidBody.velocity.y, _rigidBody.velocity.x) * Mathf.Rad2Deg;
        
        if (!IsTouchingWater() && !IsDashing())
        {
            angle = 0f;
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle)), adjustedRotationSpeed * Time.deltaTime);
        } else
        {
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }

        // Manage rotation
        if (IsLoadingShoot())
        {
            angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg;
            
            if (scaleX > 0)
            {
                angle = Mathf.Clamp(angle, -maxAngleLoadingShot, maxAngleLoadingShot);
            }
            else
            {
                angle = ((angle + 360) % 360) - 180;
                Debug.Log(angle);
                angle = Mathf.Clamp(angle, -maxAngleLoadingShot, maxAngleLoadingShot);
            }
            
            transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle)), adjustedRotationSpeed * Time.deltaTime);
        }

        if (isTackling)
        {
            angle = Mathf.Atan2(speedY, speedX) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 270));
        }

        if (IsLoadingShoot())
        {
            //angle = scaleX > 0 ? angle + 90 : angle - 90;
            //transform.rotation = Quaternion.RotateTowards(currentAngle, Quaternion.Euler(new Vector3(0, 0, angle + 270)), adjustedRotationSpeed * Time.deltaTime);
        }


        if (_rigidBody.velocity.x != 0)
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
        Destroy(collider);
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

        shotSpeed = Mathf.Max(accelerationBall * rigidBodyBall.velocity.magnitude, shotSpeed);

        float velocityX;
        float velocityY;

        var speed = ComputeShotSpeed(shotSpeed);
        velocityX = speed.x;
        velocityY = speed.y;

        rigidBodyBall.velocity = new Vector2(velocityX, velocityY);
        Debug.DrawRay(transform.position, Vector2.down * 0.8f);
        Vector3 endLine = new Vector3(rigidBodyBall.velocity.x, rigidBodyBall.velocity.y, 0);
        Debug.DrawLine(rigidBodyBall.transform.position, rigidBodyBall.transform.position + endLine);

        if (_disableBodyRoutine != null)
        {
            StopCoroutine(_disableBodyRoutine);
        }

        _disableBodyRoutine = DisableBody(rigidBodyBall.gameObject);
        StartCoroutine(_disableBodyRoutine);
        DisableShotHitbox();
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
        StartCoroutine(DisableBody(newBall));
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
        if (newBall != null)
        {
            Debug.Log("Disable body" + GetComponent<Collider2D>());
            Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
            yield return new WaitForSeconds(0.6f);
            if (newBall != null)
            {
                Debug.Log("Enable body");
                Physics2D.IgnoreCollision(newBall.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
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
            speedX = _rigidBody.velocity.x;
            speedY = _rigidBody.velocity.y;
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
        return _rigidBody.velocity.magnitude;
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
