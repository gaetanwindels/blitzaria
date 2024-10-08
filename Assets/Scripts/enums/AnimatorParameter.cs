﻿using UnityEngine;

namespace enums
{
    public class AnimatorParameters
    {
        // Player
        public static readonly int IsSwimming = Animator.StringToHash("IsSwimming");
        public static readonly int IsIdle = Animator.StringToHash("IsIdle");
        public static readonly int IsInAir = Animator.StringToHash("IsInAir");
        public static readonly int IsDashing = Animator.StringToHash("IsDashing");
        public static readonly int IsTackling = Animator.StringToHash("IsTackling");
        public static readonly int IsUp = Animator.StringToHash("IsUp");
        public static readonly int IsDown = Animator.StringToHash("IsDown");
        public static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");
        public static readonly int IsRolling = Animator.StringToHash("IsRolling");
        public static readonly int IsLoadingShoot = Animator.StringToHash("IsLoadingShoot");
        public static readonly int IsTackled = Animator.StringToHash("IsTackled");
        public static readonly int IsShooting = Animator.StringToHash("IsShooting");
        public static readonly int IsHovering = Animator.StringToHash("IsHovering");
        public static readonly int IsLoadingKick = Animator.StringToHash("IsLoadingKick");
        public static readonly int IsLoadingShoulder = Animator.StringToHash("IsLoadingShoulder");
        public static readonly int IsShouldering = Animator.StringToHash("IsShouldering");
        
        // Middle Text
        public static readonly int TriggerText = Animator.StringToHash("TriggerText");
        
        // Goal Score
        public static readonly int TriggerGoalScored = Animator.StringToHash("TriggerGoalScored");

    }
}