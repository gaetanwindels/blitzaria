using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBallSpawner : MonoBehaviour
{

    [SerializeField] GameObject energyBallPrefab;

    [SerializeField] float timeToGenerate = 2;

    GameObject energyBallInstance = null;

    private bool isRunning;

    // Start is called before the first frame update
    void Start()
    {
        energyBallInstance = Instantiate(energyBallPrefab, transform.position, Quaternion.identity, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (energyBallInstance == null && !isRunning)
        {    
            StartCoroutine(SpawnEnergyBall());
        }
    }

    IEnumerator SpawnEnergyBall()
    {
        isRunning = true;
        yield return new WaitForSeconds(timeToGenerate);
        energyBallInstance = Instantiate(energyBallPrefab, transform.position, Quaternion.identity, this.transform);
        isRunning = false;
    }
}
