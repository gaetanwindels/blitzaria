using enums;

public class PlayerInAirState : RootState {

    public PlayerInAirState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory)
    {
        StateName = "In Air";
    }

    public override void EnterState()
    {
        base.EnterState();
        Ctx.Animator.SetBool(AnimatorParameters.IsInAir, true);
        Ctx.RigidBody.gravityScale = 0.8f;
        Ctx.PreventYMove = true;
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(AnimatorParameters.IsInAir, false);
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.IsInWater())
        {
            SwitchState(StatePool.InWaterState);
        }
    }

    public override void InitializeSubState()
    {
        SetSubState(StatePool.HoveringState);
    }
}