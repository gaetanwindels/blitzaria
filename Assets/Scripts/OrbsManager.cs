using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbsManager : MonoBehaviour
{

    [SerializeField] int maxOrbs = 3;
    [SerializeField] float timeToGenerateOrb = 2f;
    [SerializeField] GameObject orb;

    private int orbsPending = 0;
    private Coroutine orbRoutine;
    private bool isGenerating = false;

    // Start is called before the first frame update
    void Start()
    {
        //GenerateOrbs();
    }

    private void GenerateOrbs()
    {
        for (var i = 0; i < maxOrbs;  i++)
        {
            Instantiate(orb, transform.position, Quaternion.identity, transform.GetChild(i));
        }
    }

    public void AddOrbs(int number)
    {
        var orbs = GetComponentsInChildren<Orb>();
        var orbsToGenerate = Mathf.Min(maxOrbs - orbs.Length, number); 
        for (var i = 0; i < orbsToGenerate; i++)
        {
            Instantiate(orb, transform.position, Quaternion.identity, transform.GetChild(orbs.Length));
        }
    }

    public bool ConsumeOrbs(int number)
    {
        var orbs = GetComponentsInChildren<Orb>();
        if (number > orbs.Length)
        {
            return false;
        }

        for (var i = 0; i < number; i++)
        {
            Destroy(orbs[orbs.Length - 1 - i].gameObject);
        }

        orbsPending++;

        //StartCoroutine(GenerateOrb());
        
        if (!isGenerating)
        {
            
        }
        
        return true;
    }

    IEnumerator GenerateOrb()
    {
        isGenerating = true;
        yield return new WaitForSeconds(timeToGenerateOrb);
        isGenerating = false;
        orbsPending--;

        var orbs = GetComponentsInChildren<Orb>();
        Instantiate(orb, transform.position, Quaternion.identity, transform.GetChild(orbs.Length));

        if (orbsPending > 0)
        {
            //StartCoroutine(GenerateOrb());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
