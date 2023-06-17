public class PlayerHoveringState : SubState {

    public PlayerHoveringState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory)
    {
        StateName = "Hovering";
    }

    public override void EnterState()
    {
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
       
    }

}