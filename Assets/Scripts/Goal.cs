using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Goal : MonoBehaviour
{
    // Config parameters
    [SerializeField] private TeamEnum teamScoring = TeamEnum.Team1;
    [SerializeField] private int explosionForce = 1300;
    [SerializeField] private ParticleSystem goalParticleSystem;

    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip hitSound;
    
    [SerializeField] private EventChannel eventChannel;

    // Cached variables
    private GameManager _gameManager;
    private AudioSource _audioSource;
    private Collider2D _collider;

    // State variables
    private bool _isReloading = false;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (ball)
        {
            Destroy(ball.gameObject);
            ScoreGoal();
        }
    }

    private void ScoreGoal()
    {
        if (!_isReloading)
        {
            if (_audioSource && explosionSound)
            {
                _audioSource.clip = explosionSound;
                _audioSource.Play();
            }

            if (goalParticleSystem)
            {
                goalParticleSystem.gameObject.SetActive(true);
                goalParticleSystem.Play();
            }

            eventChannel.RaiseGoalScored(teamScoring);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        
        if (ball)
        {
            if (_audioSource != null && hitSound != null)
            {
                //audioSource.volume = 1f;
                _audioSource.clip = hitSound;
                _audioSource.Play();
            }
            
        }
    }

    private void ExplodePlayers()
    {
        var players = FindObjectsOfType<Player>();

        ParticleSystem ps = goalParticleSystem.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            Debug.Log("GOGO");
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        foreach (Player player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            player.DisableInputs();
            rb.freezeRotation = false;
            var explosion = teamScoring == TeamEnum.Team1 ? explosionForce : -explosionForce;
            rb.angularVelocity = 1000;
            var explosionY = Random.Range(-explosion / 2, explosion / 2);
            rb.AddForce(new Vector2(explosion, explosionY));            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GetComponent<GameManager>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider2D>();

        if (goalParticleSystem != null)
        {
            goalParticleSystem.gameObject.SetActive(false);
            goalParticleSystem.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var ball = FindFirstObjectByType<Ball>();
        if (ball)
        {
            var ballPosition = new Vector3(ball.transform.position.x, ball.transform.position.y, transform.position.z);
            if (_collider.bounds.Contains(ballPosition))
            {
                Destroy(ball.gameObject);
                ScoreGoal();
            }
        }
    }

    IEnumerator ReloadScene()
    {
        _isReloading = true;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
