using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] float maxDistanceToTravel = 0.5f;
    [SerializeField] float speed = 1f;

    private Vector2 destinationPoint;

    // Start is called before the first frame update
    void Start()
    {
        PickNextDestination();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector2.MoveTowards(this.transform.localPosition, destinationPoint, speed * Time.deltaTime);
        if (transform.localPosition.x == destinationPoint.x)
        {
            PickNextDestination();
        }
    }

    private void PickNextDestination()
    {
        var rangeX = Random.Range(-1f, 1f);
        var rangeY = Random.Range(-1f, 1f);
        var direction = new Vector2(rangeX, rangeY);
        direction.Normalize();
        direction *= maxDistanceToTravel;

        destinationPoint = direction;

        if (destinationPoint == null)
        {
            
        } 
    }
}
