using UnityEngine;
using System.Collections;

public class WaterDetector : MonoBehaviour
{

    [SerializeField] float massMitigator = 40f;
    [SerializeField] float maxMass = 0.2f;
    [SerializeField] float minVelocityExit = 7f;
    
    [SerializeField] AudioClip exitSound;
    [SerializeField] AudioClip enterSound;

    private bool _gameJustStarted = true;
    private AudioSource _audioSource;
    private Water _water;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _water = GetComponent<Water>();
        StartCoroutine(EnterWaterRoutine());
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (_gameJustStarted)
        {
            return;
        }

        var ball = hit.GetComponent<Ball>();

        var rigidBody = hit.GetComponent<Rigidbody2D>();
        var player = hit.GetComponent<Player>();
        
        if (player != null)
        {
            player.canGoUp = false;
        }

        if (rigidBody != null && rigidBody.linearVelocity.magnitude > 0.4f && (ball == null || !ball.HasJustSpawned))
        {
            var mass = Mathf.Min(maxMass, rigidBody.mass);
            _water.Splash(rigidBody.transform.position.x, rigidBody);
            _audioSource.volume = 0.3f;
            _audioSource.clip = enterSound;
            _audioSource.Play();
        }

        if (ball != null)
        {
            ball.HasJustSpawned = false;
        }

    }

    void OnTriggerExit2D(Collider2D hit)
    {
        if (_gameJustStarted)
        {
            return;
        }

        var ball = hit.GetComponent<Ball>();
        var player = hit.GetComponent<Player>();

        var rigidBody = hit.GetComponent<Rigidbody2D>();

        if (rigidBody == null) {
            return;
        }

        if (player && rigidBody.linearVelocity.magnitude <= minVelocityExit)
        {
            return;
        }
        
        if (rigidBody && rigidBody.linearVelocity.magnitude > 0.4f && (!ball || !ball.HasJustSpawned))
        {
            var mass = Mathf.Min(maxMass, rigidBody.mass);
            GetComponent<Water>().Splash(rigidBody.transform.position.x, rigidBody);
            _audioSource.volume = 0.3f;
            _audioSource.clip = exitSound;
            _audioSource.Play();
        }

        if (ball)
        {
            ball.HasJustSpawned = false;
        }
    }

    IEnumerator EnterWaterRoutine()
    {
        _gameJustStarted = true;
        yield return new WaitForSeconds(0.1f);
        _gameJustStarted = false;
    }

}