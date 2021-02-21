using UnityEngine;
using System.Collections;

public class WaterDetector : MonoBehaviour
{

    [SerializeField] float massMitigator = 40f;
    [SerializeField] float maxMass = 0.2f;
    [SerializeField] float minVelocityExit = 7f;


    [SerializeField] AudioClip exitSound;
    [SerializeField] AudioClip enterSound;

    private bool gameJustStarted = true;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(EnterWaterRoutine());
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (gameJustStarted)
        {
            return;
        }

        var ball = hit.GetComponent<Ball>();

        var rigidBody = hit.GetComponent<Rigidbody2D>();
        var player = hit.GetComponent<Player>();
        
        if (player != null)
        {
            player.EnterWater();
            player.canGoUp = false;
        }

        if (rigidBody != null && rigidBody.velocity.magnitude > 0.4f && (ball == null || !ball.HasJustSpawned))
        {
            Debug.Log("magnitude" + rigidBody.velocity.magnitude);
            var mass = Mathf.Min(maxMass, rigidBody.mass);
            GetComponent<Water>().Splash(rigidBody.transform.position.x, rigidBody.velocity.y * mass / massMitigator);
            audioSource.volume = 0.3f;
            audioSource.clip = enterSound;
            audioSource.Play();
        }

        if (ball != null)
        {
            ball.HasJustSpawned = false;
        }

    }

    void OnTriggerExit2D(Collider2D hit)
    {
        if (gameJustStarted)
        {
            return;
        }

        var ball = hit.GetComponent<Ball>();

        var rigidBody = hit.GetComponent<Rigidbody2D>();

        if (rigidBody == null) {
            return;
        }

        if (rigidBody.velocity.magnitude <= minVelocityExit)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
            return;
        }

        Debug.Log(ball == null || !ball.HasJustSpawned);
        if (rigidBody != null && rigidBody.velocity.magnitude > 0.4f && (ball == null || !ball.HasJustSpawned))
        {
            var mass = Mathf.Min(maxMass, rigidBody.mass);
            GetComponent<Water>().Splash(rigidBody.transform.position.x, rigidBody.velocity.y * mass / massMitigator);
            audioSource.volume = 0.3f;
            audioSource.clip = exitSound;
            audioSource.Play();
        }

        if (ball != null)
        {
            ball.HasJustSpawned = false;
        }
    }

    IEnumerator EnterWaterRoutine()
    {
        gameJustStarted = true;
        yield return new WaitForSeconds(0.1f);
        gameJustStarted = false;
    }

    /*void OnTriggerStay2D(Collider2D Hit)
    {
        //print(Hit.name);
        if (Hit.rigidbody2D != null)
        {
            int points = Mathf.RoundToInt(Hit.transform.localScale.x * 15f);
            for (int i = 0; i < points; i++)
            {
                transform.parent.GetComponent<Water>().Splish(Hit.transform.position.x - Hit.transform.localScale.x + i * 2 * Hit.transform.localScale.x / points, Hit.rigidbody2D.mass * Hit.rigidbody2D.velocity.x / 10f / points * 2f);
            }
        }
    }*/

}