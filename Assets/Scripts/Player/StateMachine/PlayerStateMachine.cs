using System;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour {
	
	// Cached variables
	private Collider2D _bodyCollider;
	private Bounds _waterBounds;
	private Rigidbody2D _rigidBody;
	private InputManager _inputManager;

	// State Variables
	private RootState _currentState;
	private PlayerStatePool _stateFactory;
	private float _speedMultiplier = 1f;
	private bool _preventYMove = true;
	
	// Getters and setters
	public Animator Animator { get; private set; }
	public Rigidbody2D RigidBody { get => _rigidBody; }
	public RootState CurrentState { get => _currentState; set => _currentState = value; }
	public float SpeedMultiplier { get => _speedMultiplier; set => _speedMultiplier = value; }
	public bool PreventYMove { get => _preventYMove; set => _preventYMove = value; }

	private void Start()
	{
		_bodyCollider = GetComponent<Collider2D>();
		_waterBounds = FindObjectOfType<Water>().GetComponent<Collider2D>().bounds;
		Animator = GetComponent<Animator>();
		_rigidBody = GetComponent<Rigidbody2D>();
		_inputManager = new RewiredInputManager(0);
		_stateFactory = new PlayerStatePool(this);
		
		CurrentState = new PlayerInWaterState(this, _stateFactory);
		CurrentState.EnterState();
	}

	public bool IsInWater()
	{
		var bounds = _bodyCollider.bounds;
		return _waterBounds.Contains(bounds.min) && _waterBounds.Contains(bounds.max);
	}

	private void Update()
	{
		CurrentState.UpdateStates();
		HandleMove();
	}

	private void HandleMove()
	{
		float speedX = _inputManager.GetAxis("Move Horizontal");
		float speedY = _inputManager.GetAxis("Move Vertical");
		
		if (PreventYMove)
		{
			speedY = RigidBody.velocity.y;
		}
		
		_rigidBody.velocity = new Vector2(speedX, speedY).normalized * (12 * SpeedMultiplier);
	}

	private void RegisterInputs()
	{

	}
	
}