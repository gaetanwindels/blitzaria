public class PlayerSwimState : SubState {

    public PlayerSwimState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory)
    {
        StateName = "Swimming";
    }

    public override void EnterState()
    {
        Ctx.Animator.SetBool(AnimatorParameters.IsSwimming, true);
        Ctx.SpeedMultiplier = 1f;
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(AnimatorParameters.IsSwimming, false);
    }

    public override void CheckSwitchStates()
    {
        
    }

}