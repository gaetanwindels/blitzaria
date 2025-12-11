using System;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Shadow : MonoBehaviour
    {
        // Config parameters
        [SerializeField] private float offsetX = 10f;
        [SerializeField] private float offsetY = 10f;
        [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private GameObject objectToShadow;
        
        // Cached variables
        private SpriteRenderer _shadowSpriteRenderer;
        private SpriteRenderer _objectSpriteRenderer;
        private Vector3 _centerScreenWorldPos;
        
        void Awake()
        {
            _objectSpriteRenderer = objectToShadow.GetComponent<SpriteRenderer>();
            _shadowSpriteRenderer = GetComponent<SpriteRenderer>();
            _shadowSpriteRenderer.color = shadowColor;
            
            _centerScreenWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Screen.width / 2f, Screen.height / 2f, Camera.main.nearClipPlane)
            );
        }

        private void FixedUpdate()
        {
            _shadowSpriteRenderer.sprite = _objectSpriteRenderer.sprite;
            var diffX = _centerScreenWorldPos.x - objectToShadow.transform.position.x;
            var diffY = _centerScreenWorldPos.y - objectToShadow.transform.position.y;
            transform.position = new Vector3(objectToShadow.transform.position.x - diffX / offsetX, objectToShadow.transform.position.y - diffY / offsetY, transform.position.z);
            
        }
    }
}