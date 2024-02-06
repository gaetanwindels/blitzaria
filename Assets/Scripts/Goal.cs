using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Goal : MonoBehaviour
{

    [SerializeField] private TeamEnum teamScoring = TeamEnum.Team1;
    [SerializeField] private int explosionForce = 1300;
    [SerializeField] private ParticleSystem goalParticleSystem;

    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip hitSound;
    
    [SerializeField] private EventChannel eventChannel;

    // Cached variable
    private GameManager gameManager;
    private AudioSource audioSource;

    private bool isReloading = false;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        
        if (ball != null && !isReloading)
        {
            if (audioSource != null && explosionSound != null)
            {
                audioSource.clip = explosionSound;
                audioSource.Play();
            }
            
            if (goalParticleSystem != null)
            {
                goalParticleSystem.gameObject.SetActive(true);
                goalParticleSystem.Play();
            }

            //ExplodePlayers();
            //var shaker = FindObjectOfType<CameraShaker>();
            //var gameManager = FindObjectOfType<GameManager>();
            //shaker.ShakeFor(0.5f, 0.3f);
            //gameManager.AddScore(teamScoring);
            //Destroy(collision.gameObject);
            //StartCoroutine(ReldoaScene());
            
            eventChannel.RaiseGoalScored(teamScoring);

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (ball != null)
        {
            if (audioSource != null && hitSound != null)
            {
                //audioSource.volume = 1f;
                audioSource.clip = hitSound;
                audioSource.Play();
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
        gameManager = GetComponent<GameManager>();
        audioSource = GetComponent<AudioSource>();

        if (goalParticleSystem != null)
        {
            goalParticleSystem.gameObject.SetActive(false);
            goalParticleSystem.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ReloadScene()
    {
        isReloading = true;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
