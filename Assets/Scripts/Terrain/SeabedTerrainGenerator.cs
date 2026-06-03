using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SeabedTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainSize = 200;
    public float terrainHeight = 20f;
    public float noiseScale = 0.03f;
    public int octaves = 4;
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int seed = 0;
    public Vector2 offset;

    [Header("Nodule Settings")]
    public int noduleCount = 50;
    public float noduleRadius = 1f;
    public float noduleMinSpacing = 8f;
    public GameObject nodulePrefab;

    private Mesh terrainMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    void Start()
    {
        GenerateTerrain();
        GenerateNodules();
    }

    public void GenerateTerrain()
    {
        terrainMesh = new Mesh();
        terrainMesh.name = "Seabed Terrain";
        GetComponent<MeshFilter>().mesh = terrainMesh;

        CreateShape();
        UpdateMesh();
        AddMeshCollider();
    }

    void CreateShape()
    {
        vertices = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        colors = new Color[vertices.Length];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int z = 0, i = 0; z <= terrainSize; z++)
        {
            for (int x = 0; x <= terrainSize; x++, i++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x - terrainSize / 2f) * noiseScale * frequency + octaveOffsets[o].x;
                    float sampleZ = (z - terrainSize / 2f) * noiseScale * frequency + octaveOffsets[o].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                vertices[i] = new Vector3(
                    x - terrainSize / 2f,
                    noiseHeight * terrainHeight,
                    z - terrainSize / 2f
                );

                float normalizedHeight = (vertices[i].y + terrainHeight) / (terrainHeight * 2f);
                colors[i] = GetTerrainColor(normalizedHeight);
            }
        }

        triangles = new int[terrainSize * terrainSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + terrainSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + terrainSize + 1;
                triangles[tris + 5] = vert + terrainSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    Color GetTerrainColor(float height)
    {
        if (height < 0.3f)
            return new Color(0.1f, 0.15f, 0.2f);
        else if (height < 0.5f)
            return new Color(0.15f, 0.2f, 0.25f);
        else if (height < 0.7f)
            return new Color(0.2f, 0.25f, 0.3f);
        else
            return new Color(0.25f, 0.3f, 0.35f);
    }

    void UpdateMesh()
    {
        terrainMesh.Clear();
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.colors = colors;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
    }

    void AddMeshCollider()
    {
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null)
            collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = terrainMesh;
    }

    public void GenerateNodules()
    {
        Transform noduleParent = new GameObject("Nodules").transform;
        noduleParent.parent = transform;

        System.Random rnd = new System.Random(seed + 1000);
        int placedNodules = 0;
        int maxAttempts = noduleCount * 10;
        int attempts = 0;

        while (placedNodules < noduleCount && attempts < maxAttempts)
        {
            attempts++;
            float x = (float)(rnd.NextDouble() * terrainSize - terrainSize / 2f);
            float z = (float)(rnd.NextDouble() * terrainSize - terrainSize / 2f);

            bool validPosition = true;
            foreach (Transform existing in noduleParent)
            {
                if (Vector3.Distance(new Vector3(x, 0, z), new Vector3(existing.position.x, 0, existing.position.z)) < noduleMinSpacing)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                float y = GetTerrainHeight(x, z);
                if (y > -terrainHeight * 0.5f)
                {
                    GameObject nodule = CreateNodule(new Vector3(x, y, z));
                    nodule.transform.parent = noduleParent;
                    placedNodules++;
                }
            }
        }
    }

    GameObject CreateNodule(Vector3 position)
    {
        GameObject nodule = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodule.name = "Nodule";
        nodule.transform.position = position;
        nodule.transform.localScale = Vector3.one * noduleRadius * Random.Range(0.8f, 1.5f);

        Renderer renderer = nodule.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.4f, 0.35f, 0.25f);
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Glossiness", 0.2f);
        renderer.material = mat;

        nodule.AddComponent<NoduleController>();

        return nodule;
    }

    public float GetTerrainHeight(float worldX, float worldZ)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int o = 0; o < octaves; o++)
        {
            float sampleX = worldX * noiseScale * frequency + octaveOffsets[o].x;
            float sampleZ = worldZ * noiseScale * frequency + octaveOffsets[o].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noiseHeight * terrainHeight;
    }
}
