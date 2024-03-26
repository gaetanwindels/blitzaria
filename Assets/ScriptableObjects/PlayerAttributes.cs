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
        public Color sourceTertiaryColor;
        public SkinColor[] colors;
        public string name;
        
        [System.Serializable]
        public struct SkinColor {
    
            public Color primaryColor;
            public Color secondaryColor;
            public Color tertiaryColor;
    
        }
    }
}