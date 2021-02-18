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

    // Cached variables
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // State variables
    float z = -2f;
    float[] xpositions;
    float[] ypositions;
    float[] velocities;
    float[] accelerations;

    float baseheight;
    float left;
    float bottom;

    LineRenderer body;

    GameObject[] meshobjects;
    Mesh[] meshes;
    GameObject[] colliders;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        Sprite sprite = spriteRenderer.sprite;
        Vector4 border = sprite.border;
        Debug.Log(spriteRenderer.bounds.size);
        Debug.Log(sprite.name);

        this.z = transform.position.z;

        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;
        float left = transform.position.x - (width / 2);
        float top = transform.position.y + (height / 2);
        float bottom = top - height;
        SpawnWater(left, width, top, bottom);
        Destroy(spriteRenderer);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnWater(float left, float width, float top, float bottom)
    {
        int edgecount = Mathf.RoundToInt(width) * edgeCountMultiplier;
        int nodecount = edgecount + 1;

        body = gameObject.AddComponent<LineRenderer>();
        body.material = mat;
        body.material.renderQueue = 1000;
        body.positionCount = nodecount;
        body.startWidth = 0.025f;
        //body.sortingLayerName = "Water";
        //body.sortingOrder = 3;

        xpositions = new float[nodecount];
        ypositions = new float[nodecount];
        velocities = new float[nodecount];
        accelerations = new float[nodecount];

        meshobjects = new GameObject[edgecount];
        meshes = new Mesh[edgecount];
        colliders = new GameObject[edgecount];

        baseheight = top;
        this.bottom = bottom;
        this.left = left;

        for (int i = 0; i < nodecount; i++)
        {
            ypositions[i] = top;
            xpositions[i] = left + width * i / edgecount;
            accelerations[i] = 0;
            velocities[i] = 0;
            body.SetPosition(i, new Vector3(xpositions[i], ypositions[i], z));
        }

        Debug.Log("Edge count : " + edgecount);

        // Creating meshes
        for (int i = 0; i < edgecount; i++)
        {
            meshes[i] = new Mesh();
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(xpositions[i], ypositions[i], z);
            vertices[1] = new Vector3(xpositions[i + 1], ypositions[i + 1], z);
            vertices[2] = new Vector3(xpositions[i], bottom, z);
            vertices[3] = new Vector3(xpositions[i + 1], bottom, z);

            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 1);
            uvs[1] = new Vector2(1, 1);
            uvs[2] = new Vector2(0, 0);
            uvs[3] = new Vector2(1, 0);

            int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };

            meshes[i].vertices = vertices;
            meshes[i].uv = uvs;
            meshes[i].triangles = tris;
            meshobjects[i] = Instantiate(watermesh, Vector3.zero, Quaternion.identity) as GameObject;
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes[i];
            meshobjects[i].transform.parent = transform;

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

    public void Splash(float xpos, float velocity)
    {
        //If the position is within the bounds of the water:
        if (xpos >= xpositions[0] && xpos <= xpositions[xpositions.Length - 1])
        {
            //Offset the x position to be the distance from the left side
            xpos -= xpositions[0];

            //Find which spring we're touching
            int index = Mathf.RoundToInt((xpositions.Length - 1) * (xpos / (xpositions[xpositions.Length - 1] - xpositions[0])));

            //Add the velocity of the falling object to the spring
            velocities[index] += velocity;

            //Set the lifetime of the particle system.
            float lifetime = 0.93f + Mathf.Abs(velocity) * 0.07f;

            //Set the splash to be between two values in Shuriken by setting it twice.

            if (splash == null) return; 

            var main = splash.GetComponent<ParticleSystem>().main;
            //main.startSpeed = 8 + 2 * Mathf.Pow(Mathf.Abs(velocity), 0.5f);
            //main.startSpeed = 9 + 2 * Mathf.Pow(Mathf.Abs(velocity), 0.5f);
            main.startLifetime = lifetime;

            //Set the correct position of the particle system.
            Vector3 position = new Vector3(xpositions[index], ypositions[index], -3);

            //This line aims the splash towards the middle. Only use for small bodies of water:
            //Quaternion rotation = Quaternion.LookRotation(new Vector3(xpositions[Mathf.FloorToInt(xpositions.Length / 2)], baseheight + 8, 5) - position);

            //Create the splash and tell it to destroy itself.
            GameObject splish = Instantiate(splash, position, Quaternion.identity) as GameObject;
            Destroy(splish, lifetime + 0.3f);
        }
    }

    //Same as the code from in the meshes before, set the new mesh positions
    void UpdateMeshes()
    {
        body.material = mat;

        for (int i = 0; i < meshes.Length; i++)
        {
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(xpositions[i], ypositions[i], z);
            vertices[1] = new Vector3(xpositions[i + 1], ypositions[i + 1], z);
            vertices[2] = new Vector3(xpositions[i], bottom, z);
            vertices[3] = new Vector3(xpositions[i + 1], bottom, z);

            meshes[i].vertices = vertices;
        }
    }

    //Called regularly by Unity
    void FixedUpdate()
    {

        //Here we use the Euler method to handle all the physics of our springs:
        for (int i = 0; i < xpositions.Length; i++)
        {
            float force = springconstant * (ypositions[i] - baseheight) + velocities[i] * damping;
            accelerations[i] = -force;
            ypositions[i] += velocities[i];
            velocities[i] += accelerations[i];
            body.SetPosition(i, new Vector3(xpositions[i], ypositions[i], z));
        }

        //Now we store the difference in heights:
        float[] leftDeltas = new float[xpositions.Length];
        float[] rightDeltas = new float[xpositions.Length];

        //We make 8 small passes for fluidity:
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < xpositions.Length; i++)
            {
                //We check the heights of the nearby nodes, adjust velocities accordingly, record the height differences
                if (i > 0)
                {
                    leftDeltas[i] = spread * (ypositions[i] - ypositions[i - 1]);
                    velocities[i - 1] += leftDeltas[i];
                }
                if (i < xpositions.Length - 1)
                {
                    rightDeltas[i] = spread * (ypositions[i] - ypositions[i + 1]);
                    velocities[i + 1] += rightDeltas[i];
                }
            }

            //Now we apply a difference in position
            for (int i = 0; i < xpositions.Length; i++)
            {
                if (i > 0)
                    ypositions[i - 1] += leftDeltas[i];
                if (i < xpositions.Length - 1)
                    ypositions[i + 1] += rightDeltas[i];
            }
        }
        //Finally we update the meshes to reflect this
        UpdateMeshes();
    }

}
