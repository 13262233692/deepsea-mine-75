using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [Header("Managers")]
    public GameObject gameManagerPrefab;
    public GameObject miningManagerPrefab;
    public GameObject plumeManagerPrefab;
    public GameObject ecologyManagerPrefab;

    [Header("System Prefabs")]
    public GameObject terrainPrefab;
    public GameObject miningMachineSpawnerPrefab;
    public GameObject marineLifeSpawnerPrefab;
    public GameObject cameraPrefab;
    public GameObject uiPrefab;

    private void Awake()
    {
        InitializeManagers();
        InitializeSystems();
    }

    private void InitializeManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManager = new GameObject("Game Manager");
            gameManager.AddComponent<GameManager>();
        }

        if (MiningManager.Instance == null)
        {
            GameObject miningManager = new GameObject("Mining Manager");
            miningManager.AddComponent<MiningManager>();
        }

        if (PlumeManager.Instance == null)
        {
            GameObject plumeManager = new GameObject("Plume Manager");
            plumeManager.AddComponent<PlumeManager>();
        }

        if (EcologyManager.Instance == null)
        {
            GameObject ecologyManager = new GameObject("Ecology Manager");
            ecologyManager.AddComponent<EcologyManager>();
        }

        if (FoodWebSystem.Instance == null)
        {
            GameObject foodWebSystem = new GameObject("Food Web System");
            foodWebSystem.AddComponent<FoodWebSystem>();
        }
    }

    private void InitializeSystems()
    {
        CreateTerrain();
        CreateCamera();
        CreateMiningMachine();
        CreateMarineLife();
        CreateUI();
        CreatePerformanceMonitor();
    }

    private void CreatePerformanceMonitor()
    {
        GameObject monitorObj = new GameObject("Performance Monitor");
        monitorObj.AddComponent<PerformanceMonitor>();
    }

    private void CreateTerrain()
    {
        GameObject terrainObj = new GameObject("Seabed Terrain");
        terrainObj.transform.position = Vector3.zero;

        MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();

        Material terrainMat = new Material(Shader.Find("Standard"));
        terrainMat.color = new Color(0.15f, 0.18f, 0.2f);
        meshRenderer.material = terrainMat;

        SeabedTerrainGenerator terrain = terrainObj.AddComponent<SeabedTerrainGenerator>();
        terrain.terrainSize = 150;
        terrain.terrainHeight = 15f;
        terrain.noiseScale = 0.04f;
        terrain.octaves = 5;
        terrain.persistance = 0.5f;
        terrain.lacunarity = 2f;
        terrain.seed = 42;
        terrain.noduleCount = 80;
        terrain.noduleRadius = 1.2f;
    }

    private void CreateCamera()
    {
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.transform.position = new Vector3(0f, 30f, -40f);
        cameraObj.transform.rotation = Quaternion.Euler(30f, 0f, 0f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;

        CameraController controller = cameraObj.AddComponent<CameraController>();
        controller.underwaterColor = new Color(0.05f, 0.15f, 0.2f);
        controller.fogDensity = 0.015f;
    }

    private void CreateMiningMachine()
    {
        GameObject spawnerObj = new GameObject("Mining Machine Spawner");
        MiningMachineSpawner spawner = spawnerObj.AddComponent<MiningMachineSpawner>();
        spawner.spawnPosition = new Vector3(0f, 10f, 0f);

        SeabedTerrainGenerator terrain = FindObjectOfType<SeabedTerrainGenerator>();
        if (terrain != null)
        {
            spawner.terrainGenerator = terrain;
        }

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            spawner.cameraController = cameraController;
        }
    }

    private void CreateMarineLife()
    {
        GameObject spawnerObj = new GameObject("Marine Life Spawner");
        MarineLifeSpawner spawner = spawnerObj.AddComponent<MarineLifeSpawner>();
        spawner.fishCount = 20;
        spawner.coralCount = 15;
        spawner.shrimpCount = 10;
        spawner.jellyfishCount = 8;
        spawner.spongeCount = 5;
        spawner.seaCucumberCount = 12;
        spawner.tubeWormCount = 10;
        spawner.seaAnemoneCount = 6;
        spawner.starfishCount = 8;
        spawner.deepSeaCrabCount = 5;
        spawner.spawnArea = new Vector3(120f, 40f, 120f);

        SeabedTerrainGenerator terrain = FindObjectOfType<SeabedTerrainGenerator>();
        if (terrain != null)
        {
            spawner.terrainGenerator = terrain;
        }
    }

    private void CreateUI()
    {
        GameObject uiObj = new GameObject("UI Manager");
        MainUI ui = uiObj.AddComponent<MainUI>();
    }
}
