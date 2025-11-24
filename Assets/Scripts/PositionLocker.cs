using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLocker : MonoBehaviour
{

    [SerializeField] GameObject objectToLock;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] public TeamEnum team;
    [SerializeField] float slotNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (objectToLock != null)
        {
            rigidBody = objectToLock.GetComponent<Rigidbody2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (objectToLock != null)
        {
            if (rigidBody != null)
            {
                rigidBody.linearVelocity = Vector3.zero;
            }

            objectToLock.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, objectToLock.transform.position.z);
            objectToLock.transform.localScale = this.transform.localScale;
        }
    }

    public void SetObjectToLock(GameObject objectToLock)
    {
        this.objectToLock = objectToLock;
    }

    public GameObject GetObjectTolock()
    {
        return this.objectToLock;
    }


    public void UnlockObject()
    {
        this.objectToLock = null;
    }
}
