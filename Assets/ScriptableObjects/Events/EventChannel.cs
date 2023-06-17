
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "EventChannel", menuName = "Game/Event Channel")]
    public class EventChannel : ScriptableObject
    {
        public event UnityAction<TeamEnum> GoalScoredEvent;
        public event UnityAction TimeoutEvent;

        public void RaiseGoalScored(TeamEnum teamEnum)
        {
            if (GoalScoredEvent != null)
            {
                GoalScoredEvent.Invoke(teamEnum);
            }
        }
        
        public void RaiseTimeoutEvent()
        {
            if (TimeoutEvent != null)
            {
                TimeoutEvent.Invoke();
            }
        }
        
    }
}
