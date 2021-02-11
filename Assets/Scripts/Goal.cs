﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{

    [SerializeField] private TeamEnum team = TeamEnum.Team1;
    [SerializeField] private int explosionForce = 1300;
    [SerializeField] private GameObject goalparticleSystem;
    [SerializeField] private AudioClip goalSound;

    // Cached variable
    GameManager gameManager;

    private bool isReloading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        
        if (ball != null && !isReloading)
        {
            ExplodePlayers();
            gameManager.AddScore(team);
            Destroy(collision.gameObject);
            StartCoroutine(ReloadScene());

            var audioSource = FindObjectOfType<AudioSource>();
            if (audioSource != null && goalSound != null)
            {
                audioSource.volume = 1f;
                audioSource.clip = goalSound;
                audioSource.Play();
            }
            
        }
    }

    private void ExplodePlayers()
    {
        var players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            player.DisableInputs();
            rb.freezeRotation = false;
            var explosion = team == TeamEnum.Team1 ? explosionForce : -explosionForce;
            rb.angularVelocity = 1000;
            var explosionY = Random.Range(-explosion / 2, explosion / 2);
            rb.AddForce(new Vector2(explosion, explosionY));

            ParticleSystem ps = goalparticleSystem.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Play();
            }
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        ParticleSystem ps = goalparticleSystem.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Stop();
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
