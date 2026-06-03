using UnityEngine;

[ExecuteInEditMode]
public class MainSceneSetup : MonoBehaviour
{
    public bool setupOnStart = true;

    private void Start()
    {
        if (setupOnStart && Application.isPlaying)
        {
            SetupScene();
        }
    }

    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        RemoveExisting();
        CreateSceneRoot();
    }

    private void RemoveExisting()
    {
        SceneInitializer[] initializers = FindObjectsOfType<SceneInitializer>();
        foreach (var init in initializers)
        {
            if (Application.isPlaying)
            {
                Destroy(init.gameObject);
            }
            else
            {
                DestroyImmediate(init.gameObject);
            }
        }
    }

    private void CreateSceneRoot()
    {
        GameObject root = new GameObject("=== Deep Sea Mining Simulation ===");
        root.transform.position = Vector3.zero;

        GameObject managers = new GameObject("_Managers");
        managers.transform.parent = root.transform;

        GameObject systems = new GameObject("_Systems");
        systems.transform.parent = root.transform;

        GameObject gameManager = new GameObject("GameManager");
        gameManager.transform.parent = managers.transform;
        gameManager.AddComponent<GameManager>();

        GameObject miningManager = new GameObject("MiningManager");
        miningManager.transform.parent = managers.transform;
        miningManager.AddComponent<MiningManager>();

        GameObject plumeManager = new GameObject("PlumeManager");
        plumeManager.transform.parent = managers.transform;
        plumeManager.AddComponent<PlumeManager>();

        GameObject ecologyManager = new GameObject("EcologyManager");
        ecologyManager.transform.parent = managers.transform;
        ecologyManager.AddComponent<EcologyManager>();

        GameObject foodWebSystem = new GameObject("FoodWebSystem");
        foodWebSystem.transform.parent = managers.transform;
        foodWebSystem.AddComponent<FoodWebSystem>();

        GameObject initializer = new GameObject("SceneInitializer");
        initializer.transform.parent = systems.transform;
        initializer.AddComponent<SceneInitializer>();

        Bootstrap[] bootstraps = FindObjectsOfType<Bootstrap>();
        if (bootstraps.Length == 0)
        {
            GameObject bootstrapObj = new GameObject("Bootstrap");
            bootstrapObj.transform.parent = root.transform;
            bootstrapObj.AddComponent<Bootstrap>();
        }
    }

    [ContextMenu("Clean Scene")]
    public void CleanScene()
    {
        SceneInitializer[] initializers = FindObjectsOfType<SceneInitializer>();
        foreach (var init in initializers)
        {
            DestroyImmediate(init.gameObject);
        }

        GameManager[] gameManagers = FindObjectsOfType<GameManager>();
        foreach (var gm in gameManagers)
        {
            DestroyImmediate(gm.gameObject);
        }

        MiningManager[] miningManagers = FindObjectsOfType<MiningManager>();
        foreach (var mm in miningManagers)
        {
            DestroyImmediate(mm.gameObject);
        }

        PlumeManager[] plumeManagers = FindObjectsOfType<PlumeManager>();
        foreach (var pm in plumeManagers)
        {
            DestroyImmediate(pm.gameObject);
        }

        EcologyManager[] ecologyManagers = FindObjectsOfType<EcologyManager>();
        foreach (var em in ecologyManagers)
        {
            DestroyImmediate(em.gameObject);
        }

        Debug.Log("Scene cleaned!");
    }
}
