public class PlayerStatePool {
	
	public PlayerStateMachine Ctx { get; }

	public RootState InWaterState { get; }
	
	public RootState InAirState { get; }
	
	public SubState HoveringState { get; }

	public SubState SwimState { get; }
	
	public PlayerStatePool(PlayerStateMachine playerStateMachine) {
		Ctx = playerStateMachine;
		SwimState = new PlayerSwimState(playerStateMachine, this);
		InWaterState = new PlayerInWaterState(playerStateMachine, this);
		InAirState = new PlayerInAirState(playerStateMachine, this);
		HoveringState = new PlayerHoveringState(playerStateMachine, this);
	}
	
}