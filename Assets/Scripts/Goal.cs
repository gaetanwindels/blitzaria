using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{

    [SerializeField] TeamEnum team = TeamEnum.Team1;

    // Cached variable
    GameManager gameManager;

    private bool isReloading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        
        if (ball != null && !isReloading)
        {
            gameManager.AddScore(team);
            StartCoroutine(ReloadScene());
            
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ReloadScene()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(0);
    }
}
