using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerAttributes", menuName = "PlayerAttributes", order = 0)]
    public class PlayerAttributes : ScriptableObject
    {
        public GameObject playerPrefab;
        public Sprite selectSprite;
        public Color sourcePrimaryColor;
        public Color sourceSecondaryColor;
        public Color[] primaryColors;
        public Color[] secondaryColors;
        public string name;
    }
}