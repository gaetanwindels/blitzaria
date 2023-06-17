using UnityEngine;

public class PlayerInWaterState : RootState {

    public PlayerInWaterState(PlayerStateMachine ctx, PlayerStatePool factory) : base(ctx, factory)
    {
        StateName = "In Water";
    }

    public override void EnterState()
    {
        base.EnterState();
        Ctx.Animator.SetBool(AnimatorParameters.IsSwimming, true);
        Ctx.RigidBody.gravityScale = 0f;
        Ctx.PreventYMove = false;
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(AnimatorParameters.IsSwimming, false);
    }

    public override void CheckSwitchStates()
    {
        if (!Ctx.IsInWater())
        {
            SwitchState(StatePool.InAirState);
        }
    }

    public override void InitializeSubState()
    {
        SetSubState(StatePool.SwimState);
    }
}