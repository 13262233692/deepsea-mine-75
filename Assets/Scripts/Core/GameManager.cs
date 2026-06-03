using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Simulation Settings")]
    public float simulationSpeed = 1f;
    public float miningRate = 0.5f;
    public float environmentHealth = 100f;
    public float sedimentLevel = 0f;

    [Header("Events")]
    public event System.Action<float> OnMiningRateChanged;
    public event System.Action<float> OnEnvironmentHealthChanged;
    public event System.Action<float> OnSedimentLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        Time.timeScale = simulationSpeed;
    }

    public void SetMiningRate(float rate)
    {
        miningRate = Mathf.Clamp01(rate);
        OnMiningRateChanged?.Invoke(miningRate);
    }

    public void UpdateEnvironmentHealth(float health)
    {
        environmentHealth = Mathf.Clamp(health, 0f, 100f);
        OnEnvironmentHealthChanged?.Invoke(environmentHealth);
    }

    public void UpdateSedimentLevel(float level)
    {
        sedimentLevel = Mathf.Clamp01(level);
        OnSedimentLevelChanged?.Invoke(sedimentLevel);
    }
}
