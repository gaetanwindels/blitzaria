public abstract class AbstractState
{
	public string StateName { get; set; }
	protected PlayerStateMachine Ctx { get; }
	protected PlayerStatePool StatePool { get; }

	protected AbstractState(PlayerStateMachine ctx, PlayerStatePool factory) {
		Ctx = ctx;
		StatePool = factory;
	}

	public abstract void EnterState();

	public virtual void Update()
	{
		CheckSwitchStates();
	}

	public abstract void ExitState();

	public abstract void CheckSwitchStates();

}