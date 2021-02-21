using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBall : MonoBehaviour
{

    [SerializeField] private int energy = 20;

    private AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.AddEnergy(energy);
            audioSource.Play();
            gameObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.transform.parent.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
