using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OrbGenerator : MonoBehaviour
{
    // Config parameters
    [SerializeField] float timeToSpawn = 3f;
    [SerializeField] int orbsContained = 1;
    [SerializeField] float opacityWhenRespawning = 0.6f;
    

    // Cached variables
    private SpriteRenderer _spriteRenderer;
    
    // State variables
    private bool _isRespawning;
    private Color _spriteColor;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (_isRespawning)
        {
            return;
        }
        
        var player = other.GetComponent<Player>();
        if (player)
        {
            player.AddOrbs(orbsContained);
            StartCoroutine(RespawnRoutine());
        }
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteColor = _spriteRenderer.color;
    }

    IEnumerator RespawnRoutine()
    {
        _isRespawning = true;
        _spriteRenderer.color = new Color(_spriteColor.r, _spriteColor.g, _spriteColor.b, opacityWhenRespawning);
        yield return new WaitForSeconds(timeToSpawn);
        _isRespawning = false;
        _spriteRenderer.color = new Color(_spriteColor.r, _spriteColor.g, _spriteColor.b, 1);
    }

    
}
