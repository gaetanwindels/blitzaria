
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "EventChannel", menuName = "Game/Event Channel")]
    public class EventChannel : ScriptableObject
    {
        public event UnityAction<TeamEnum> goalScoredEvent;

        public void RaiseGoalScored(TeamEnum teamEnum)
        {
            if (goalScoredEvent != null)
            {
                goalScoredEvent.Invoke(teamEnum);
            }
        }
        
    }
}
