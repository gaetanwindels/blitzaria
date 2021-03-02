using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{

    private Camera cameraComp; // set this via inspector

    // Parameters
    [SerializeField] private float shake = 0f;
    [SerializeField] private float defaultShakeAmount = 0.1f;
    [SerializeField] private float decreaseFactor = 1.0f;

    private float zPos;
    private float shakeAmount;

    void Start()
    {
        cameraComp = GetComponent<Camera>();
        zPos = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (shake > 0)
        {
            cameraComp.transform.localPosition = Random.insideUnitSphere * shakeAmount;
            cameraComp.transform.localPosition = new Vector3(cameraComp.transform.localPosition.x, cameraComp.transform.localPosition.y, zPos);
            shake -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shake = 0f;
            cameraComp.transform.localPosition = new Vector3(0, 0, zPos);
        }
    }

    public void ShakeFor(float time)
    {
        shake = time;
        shakeAmount = defaultShakeAmount;
    }

    public void ShakeFor(float time, float shakeAmount)
    {
        this.shakeAmount = shakeAmount;
        shake = time;
    }
}
