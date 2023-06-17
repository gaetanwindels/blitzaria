public abstract class RootState: AbstractState
{
	public SubState CurrentSubState { get; set; }
	
	protected RootState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory)  {
	}

	public void UpdateStates()
	{
		Update();
		if (CurrentSubState != null)
		{
			CurrentSubState.Update();
		}
	}

	public override void EnterState()
	{
		InitializeSubState();
	}

	public void SetSubState(SubState state)
	{
		if (CurrentSubState != null)
		{
			CurrentSubState.ExitState();
		}
		
		CurrentSubState = state;
		state.EnterState();
	}

	public abstract void InitializeSubState();
	
	protected void SwitchState(RootState newState) {
		ExitState();
		newState.EnterState();
		Ctx.CurrentState = newState;
	}
}