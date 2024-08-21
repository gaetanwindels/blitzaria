using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{

    // Configuration parameters
    [SerializeField] float springconstant = 0.02f;
    [SerializeField] float damping = 0.04f;
    [SerializeField] float spread = 0.05f;
    [SerializeField] int edgeCountMultiplier = 10;
    [SerializeField] Material mat;
    [SerializeField] GameObject watermesh;
    [SerializeField] GameObject splash;
    [SerializeField] BuoyancyEffector2D effector;
    [SerializeField] Material material;

    // Cached variables
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private LineRenderer _lineRenderer;

    // State variables
    private float _z = -2f;
    private float[] _xPositions;
    private float[] _yPositions;
    private float[] _velocities;
    private float[] _accelerations;

    private float _baseHeight;
    private float _left;
    private float _bottom;
    
    private GameObject[] _meshObjects;
    private Mesh[] _meshes;
    private GameObject[] _colliders;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        Sprite sprite = _spriteRenderer.sprite;
        Vector4 border = sprite.border;
        Debug.Log(_spriteRenderer.bounds.size);
        Debug.Log(sprite.name);

        var position = transform.position;
        
        _z = position.z;

        var bounds = _spriteRenderer.bounds;
        
        var width = bounds.size.x;
        var height = bounds.size.y;
        var left = position.x - (width / 2);
        var top = position.y + (height / 2);
        var bottom = top - height;
        SpawnWater(left, width, top, bottom);
        Destroy(_spriteRenderer);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnWater(float left, float width, float top, float bottom)
    {
        var edgeCount = Mathf.RoundToInt(width) * edgeCountMultiplier;
        var nodeCount = edgeCount + 1;

        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = mat;
        _lineRenderer.material.renderQueue = 1000;
        _lineRenderer.positionCount = nodeCount;
        _lineRenderer.startWidth = 0.0625f;
        _lineRenderer.sortingLayerName = "Water";
        //body.sortingOrder = 3;

        _xPositions = new float[nodeCount];
        _yPositions = new float[nodeCount];
        _velocities = new float[nodeCount];
        _accelerations = new float[nodeCount];

        _meshObjects = new GameObject[edgeCount];
        _meshes = new Mesh[edgeCount];
        _colliders = new GameObject[edgeCount];

        _baseHeight = top;
        _bottom = bottom;
        _left = left;

        for (int i = 0; i < nodeCount; i++)
        {
            _yPositions[i] = top;
            _xPositions[i] = left + width * i / edgeCount;
            _accelerations[i] = 0;
            _velocities[i] = 0;
            _lineRenderer.SetPosition(i, new Vector3(_xPositions[i], _yPositions[i], _z));
        }

        Debug.Log("Edge count : " + edgeCount);

        // Creating meshes
        for (int i = 0; i < edgeCount; i++)
        {
            _meshes[i] = new Mesh();
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(_xPositions[i], _yPositions[i], _z);
            vertices[1] = new Vector3(_xPositions[i + 1], _yPositions[i + 1], _z);
            vertices[2] = new Vector3(_xPositions[i], bottom, _z);
            vertices[3] = new Vector3(_xPositions[i + 1], bottom, _z);

            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 1);
            uvs[1] = new Vector2(1, 1);
            uvs[2] = new Vector2(0, 0);
            uvs[3] = new Vector2(1, 0);

            int[] tris = { 0, 1, 3, 3, 2, 0 };

            _meshes[i].vertices = vertices;
            _meshes[i].uv = uvs;
            _meshes[i].triangles = tris;
            _meshObjects[i] = Instantiate(watermesh, Vector3.zero, Quaternion.identity) as GameObject;
            _meshObjects[i].GetComponent<MeshFilter>().mesh = _meshes[i];
            _meshObjects[i].GetComponent<MeshRenderer>().material.color = _spriteRenderer.color;
            //meshobjects[i].GetComponent<MeshRenderer>().material = material;
            _meshObjects[i].transform.parent = transform;

            //Create our colliders, set them be our child
            /*colliders[i] = new GameObject();
            colliders[i].name = "Trigger";
            colliders[i].AddComponent<BoxCollider2D>();
            colliders[i].transform.parent = transform;

            //Set the position and scale to the correct dimensions
            var height = Mathf.Abs(top - bottom);
            colliders[i].transform.position = new Vector3(left + width * (i + 0.5f) / edgecount, top - (height / 2), 0);
            colliders[i].transform.localScale = new Vector3(width / edgecount, 1, 1);

            //Add a WaterDetector and make sure they're triggers
            colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            colliders[i].AddComponent<WaterDetector>();*/

            // Set buoyancy effector
            /*colliders[i].GetComponent<BoxCollider2D>().usedByEffector = true;
            colliders[i].AddComponent<BuoyancyEffector2D>();
            BuoyancyEffector2D buoyancyToCreate = colliders[i].GetComponent<BuoyancyEffector2D>();
            buoyancyToCreate.density = this.effector.density;
            buoyancyToCreate.surfaceLevel = this.effector.surfaceLevel;
            buoyancyToCreate.linearDrag = this.effector.linearDrag;
            buoyancyToCreate.angularDrag = this.effector.angularDrag;
            buoyancyToCreate.flowAngle = this.effector.flowAngle;
            buoyancyToCreate.flowMagnitude = this.effector.flowMagnitude;
            buoyancyToCreate.flowVariation = this.effector.flowVariation;*/
        }
    }

    public void Splash(float xPosition, Rigidbody2D rigidbody)
    {
        //If the position is within the bounds of the water:
        if (xPosition >= _xPositions[0] && xPosition <= _xPositions[_xPositions.Length - 1])
        {
            //Offset the x position to be the distance from the left side
            xPosition -= _xPositions[0];

            //Find which spring we're touching
            int index = Mathf.RoundToInt((_xPositions.Length - 1) * (xPosition / (_xPositions[_xPositions.Length - 1] - _xPositions[0])));

            //Add the velocity of the falling object to the spring
            _velocities[index] += rigidbody.velocity.y / 40f;

            //Set the lifetime of the particle system.
            //float lifetime = 0.93f + Mathf.Abs(velocity) * 0.07f;

            //Set the splash to be between two values in Shuriken by setting it twice.

            if (splash == null) return; 

            var main = splash.GetComponent<ParticleSystem>().main;
            //main.startSpeed = 8 + 2 * Mathf.Pow(Mathf.Abs(velocity), 0.5f);
            //main.startSpeed = 9 + 2 * Mathf.Pow(Mathf.Abs(velocity), 0.5f);
            // main.startLifetime = lifetime;

            //Set the correct position of the particle system.
            Vector3 position = new Vector3(_xPositions[index], _yPositions[index], -3);

            //This line aims the splash towards the middle. Only use for small bodies of water:
            //Quaternion rotation = Quaternion.LookRotation(new Vector3(xpositions[Mathf.FloorToInt(xpositions.Length / 2)], baseheight + 8, 5) - position);

            //Create the splash and tell it to destroy itself.
            var angle = Quaternion.Euler(-90, 0, 0);
            GameObject splish = Instantiate(splash, position + new Vector3(0, 0.1f, 0), Quaternion.identity);
            var paticleSystem = splish.GetComponent<ParticleSystem>();
            var triggerModule = paticleSystem.trigger;
            triggerModule.inside = ParticleSystemOverlapAction.Kill;
            triggerModule.enabled = true;

            var normalizedVector = rigidbody.velocity.normalized;
            var velocityModule = paticleSystem.velocityOverLifetime;
            velocityModule.x =  new ParticleSystem.MinMaxCurve(rigidbody.velocity.y > 0 ? Mathf.Abs(normalizedVector.x) : -normalizedVector.x);
            velocityModule.y = new ParticleSystem.MinMaxCurve(normalizedVector.y);
            Debug.Log(normalizedVector.x +'/' + normalizedVector.y);
            
            var waterCollider = FindFirstObjectByType<WaterDetector>().gameObject.GetComponent<Collider2D>();
            
            triggerModule.SetCollider(0, waterCollider);
            Destroy(splish, 2f);
        }
    }

    //Same as the code from in the meshes before, set the new mesh positions
    void UpdateMeshes()
    {
        _lineRenderer.material = mat;

        for (int i = 0; i < _meshes.Length; i++)
        {
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(_xPositions[i], _yPositions[i], _z);
            //vertices[1] = new Vector3(_xPositions[i + 1], _yPositions[i + 1], _z);
            vertices[1] = new Vector3(_xPositions[i + 1], _yPositions[i], _z);
            vertices[2] = new Vector3(_xPositions[i], _bottom, _z);
            vertices[3] = new Vector3(_xPositions[i + 1], _bottom, _z);

            _meshes[i].vertices = vertices;
        }
    }

    //Called regularly by Unity
    void FixedUpdate()
    {

        //Here we use the Euler method to handle all the physics of our springs:
        for (int i = 0; i < _xPositions.Length; i++)
        {
            float force = springconstant * (_yPositions[i] - _baseHeight) + _velocities[i] * damping;
            _accelerations[i] = -force;
            _yPositions[i] += _velocities[i];
            _velocities[i] += _accelerations[i];
            //_lineRenderer.SetPosition(i, new Vector3(_xPositions[i], _yPositions[i], _z));
        }

        //Now we store the difference in heights:
        float[] leftDeltas = new float[_xPositions.Length];
        float[] rightDeltas = new float[_xPositions.Length];

        //We make 8 small passes for fluidity:
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < _xPositions.Length; i++)
            {
                //We check the heights of the nearby nodes, adjust velocities accordingly, record the height differences
                if (i > 0)
                {
                    leftDeltas[i] = spread * (_yPositions[i] - _yPositions[i - 1]);
                    _velocities[i - 1] += leftDeltas[i];
                }
                if (i < _xPositions.Length - 1)
                {
                    rightDeltas[i] = spread * (_yPositions[i] - _yPositions[i + 1]);
                    _velocities[i + 1] += rightDeltas[i];
                }
            }

            //Now we apply a difference in position
            for (int i = 0; i < _yPositions.Length; i++)
            {
                if (i > 0)
                    _yPositions[i - 1] += leftDeltas[i];
                if (i < _xPositions.Length - 1)
                    _yPositions[i + 1] += rightDeltas[i];
            }
        }

        DrawLine();
        //Finally we update the meshes to reflect this
        UpdateMeshes();
    }

    void DrawLine()
    {
        List<Vector3> positions = new(); 
        for (var linePos = 0; linePos < _xPositions.Length - 1; linePos++)
        {
            positions.Add(new Vector3(_xPositions[linePos], _yPositions[linePos], _z));
            positions.Add(new Vector3(_xPositions[linePos+1], _yPositions[linePos], _z));
        }

        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());
    }

}
