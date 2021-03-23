using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{

    private AudioSource audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null && audioSource != null)
        {
            audioSource.Play();
            var body = ball.GetComponent<Rigidbody2D>();

            if (body != null)
            {
                body.angularVelocity *= 0.5f;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
