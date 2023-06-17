public abstract class SubState: AbstractState
{
	
	protected SubState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory) {
	}
	
	protected void SetSuperState(RootState state) {}
	
	protected void SwitchState(SubState newState) {
		ExitState();
		newState.EnterState();
	}
	
}