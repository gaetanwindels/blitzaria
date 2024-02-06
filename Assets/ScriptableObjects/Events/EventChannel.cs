
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "EventChannel", menuName = "Game/Event Channel")]
    public class EventChannel : ScriptableObject
    {
        public event UnityAction<TeamEnum> GoalScoredEvent;
        public event UnityAction TimeoutEvent;
        
        public event UnityAction CountdownOverEvent;
        
        public event UnityAction<TeamEnum> ScoreUpdatedEvent;

        public void RaiseCountdownOver()
        {
            if (CountdownOverEvent != null)
            {
                CountdownOverEvent.Invoke();
            }
        }
        
        public void RaiseGoalScored(TeamEnum teamEnum)
        {
            if (GoalScoredEvent != null)
            {
                GoalScoredEvent.Invoke(teamEnum);
            }
        }
        
        public void RaiseScoreUpdated(TeamEnum teamEnum)
        {
            if (ScoreUpdatedEvent != null)
            {
                ScoreUpdatedEvent.Invoke(teamEnum);
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
