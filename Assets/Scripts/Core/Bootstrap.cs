using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        Debug.Log("Deep Sea Mining Simulation - Starting...");
    }

    private void Start()
    {
        SceneInitializer initializer = FindObjectOfType<SceneInitializer>();
        if (initializer == null)
        {
            GameObject initObj = new GameObject("Scene Initializer");
            initObj.AddComponent<SceneInitializer>();
        }
    }
}
