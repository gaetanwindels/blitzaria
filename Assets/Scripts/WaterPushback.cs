using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPushback : MonoBehaviour
{

    // Parameters
    [SerializeField] private float initialPushbackFactor = 2.5f;
    [SerializeField] private float pushbackFactor = 1f;
    [SerializeField] private float lockedTime = 1f;

    // Cached variables
    Rigidbody2D body;

    // State variables
    bool isPushBack = false;
    public bool isLocked = false;
    public float pushbackTracker;
    public float lockedTimeTracker;
    bool outOfWaterOnce = false;

    public bool OutOfWaterOnce { get => outOfWaterOnce; set => outOfWaterOnce = value; }
    public bool IsPushBack { get => isPushBack; set => isPushBack = value; }
    public bool IsLocked { get => isLocked; set => isLocked = value; }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (this.isPushBack)
        {
            pushbackTracker += pushbackFactor;
            pushbackTracker *= 1.15f;

            lockedTimeTracker += Time.deltaTime;
            IsLocked = lockedTimeTracker < lockedTime;

            // Manage pushback on y
            if (body.linearVelocity.y < 0)
            {
                var newVelocityY = pushbackTracker * Time.deltaTime;
                body.linearVelocity += new Vector2(0, newVelocityY);
            }

            if (body.linearVelocity.y > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            }

            // Manage pushback on x
            if (body.linearVelocity.x < 0)
            {
                var newVelocityX = pushbackTracker * Time.deltaTime;
                body.linearVelocity += new Vector2(newVelocityX, 0);

                if (body.linearVelocity.x > 0)
                {
                    body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                }
            }
            else if (body.linearVelocity.x > 0)
            {
                var newVelocityX = pushbackTracker * Time.deltaTime;
                body.linearVelocity -= new Vector2(newVelocityX, 0);

                if (body.linearVelocity.x < 0)
                {
                    body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                }
            }

            if (body.linearVelocity.y == 0 && body.linearVelocity.x == 0)
            {
                this.isPushBack = false;
                IsLocked = false;
            }

        }
    }

    public void StartWaterPushBack()
    {
        pushbackTracker = initialPushbackFactor;
        lockedTimeTracker = 0;
        IsLocked = false;
        this.isPushBack = true;
    }

    public void StopWaterPushBack()
    {
        this.isPushBack = false;
        IsLocked = false;
    }
}
