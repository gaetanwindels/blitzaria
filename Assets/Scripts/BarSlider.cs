using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSlider : MonoBehaviour
{

    public float value = 0;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(1f - value, transform.localScale.y, transform.localScale.z);
    }

    public void SetValue(float value)
    {
        this.value = value;
    }
}
