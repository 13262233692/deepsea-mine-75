using System.Collections.Generic;
using UnityEngine;

public class EcologyManager : MonoBehaviour
{
    public static EcologyManager Instance { get; private set; }

    [Header("Environment Health")]
    public float overallHealth = 100f;
    public float recoveryRate = 0.5f;
    public float damageRate = 1f;

    [Header("Indicators")]
    public float waterQuality = 100f;
    public float biodiversity = 100f;
    public float habitatIntegrity = 100f;
    public float carbonSequestration = 100f;
    public float foodWebStability = 100f;

    [Header("Damage Factors")]
    public float sedimentDamageFactor = 0.5f;
    public float miningDamageFactor = 0.3f;
    public float checkRadius = 50f;
    public float foodWebImpactWeight = 0.25f;

    [Header("Events")]
    public event System.Action<float> OnOverallHealthChanged;
    public event System.Action<float> OnWaterQualityChanged;
    public event System.Action<float> OnBiodiversityChanged;
    public event System.Action<float> OnHabitatIntegrityChanged;
    public event System.Action<float> OnCarbonSequestrationChanged;
    public event System.Action<float> OnFoodWebStabilityChanged;

    private List<MarineLife> marineLife = new List<MarineLife>();

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
        UpdateEcologyIndicators();
    }

    private void UpdateEcologyIndicators()
    {
        float sedimentLevel = PlumeManager.Instance?.globalSedimentLevel ?? 0f;
        float miningIntensity = MiningManager.Instance?.GetTotalMiningIntensity() ?? 0f;

        float totalDamage = (sedimentLevel * sedimentDamageFactor + miningIntensity * miningDamageFactor) * damageRate * Time.deltaTime;

        waterQuality = Mathf.Max(0f, waterQuality - totalDamage * 0.8f);
        habitatIntegrity = Mathf.Max(0f, habitatIntegrity - totalDamage * 0.5f);
        carbonSequestration = Mathf.Max(0f, carbonSequestration - totalDamage * 0.3f);

        UpdateBiodiversity();

        if (FoodWebSystem.Instance != null)
        {
            foodWebStability = FoodWebSystem.Instance.foodWebStability;
        }

        if (sedimentLevel < 0.1f && miningIntensity < 0.1f)
        {
            waterQuality = Mathf.Min(100f, waterQuality + recoveryRate * Time.deltaTime);
            biodiversity = Mathf.Min(100f, biodiversity + recoveryRate * 0.5f * Time.deltaTime);
            habitatIntegrity = Mathf.Min(100f, habitatIntegrity + recoveryRate * 0.3f * Time.deltaTime);
            carbonSequestration = Mathf.Min(100f, carbonSequestration + recoveryRate * 0.2f * Time.deltaTime);
        }

        overallHealth = (waterQuality + biodiversity + habitatIntegrity + carbonSequestration + foodWebStability) / 5f;

        GameManager.Instance?.UpdateEnvironmentHealth(overallHealth);

        OnOverallHealthChanged?.Invoke(overallHealth);
        OnWaterQualityChanged?.Invoke(waterQuality);
        OnBiodiversityChanged?.Invoke(biodiversity);
        OnHabitatIntegrityChanged?.Invoke(habitatIntegrity);
        OnCarbonSequestrationChanged?.Invoke(carbonSequestration);
        OnFoodWebStabilityChanged?.Invoke(foodWebStability);

        UpdateMarineLifeStress(sedimentLevel);
    }

    private void UpdateBiodiversity()
    {
        if (FoodWebSystem.Instance == null)
        {
            biodiversity = Mathf.Max(0f, biodiversity);
            return;
        }

        var speciesData = FoodWebSystem.Instance.GetAllSpeciesData();
        if (speciesData.Count == 0) return;

        float totalRatio = 0f;
        int speciesCount = 0;

        foreach (var kvp in speciesData)
        {
            totalRatio += kvp.Value.populationRatio;
            speciesCount++;
        }

        float targetBiodiversity = speciesCount > 0 ? (totalRatio / speciesCount) * 100f : 0f;
        biodiversity = Mathf.Lerp(biodiversity, targetBiodiversity, 0.5f * Time.deltaTime);

        biodiversity = Mathf.Max(0f, biodiversity);
    }

    private void UpdateMarineLifeStress(float sedimentLevel)
    {
        for (int i = marineLife.Count - 1; i >= 0; i--)
        {
            if (marineLife[i] == null)
            {
                marineLife.RemoveAt(i);
                continue;
            }

            float localSediment = PlumeManager.Instance?.GetSedimentDensityAtPosition(marineLife[i].transform.position, checkRadius) ?? sedimentLevel;
            marineLife[i].UpdateStressLevel(localSediment);
        }
    }

    public void RegisterMarineLife(MarineLife life)
    {
        if (!marineLife.Contains(life))
        {
            marineLife.Add(life);

            if (FoodWebSystem.Instance != null)
            {
                FoodWebSystem.Instance.RegisterInitialPopulation(life.lifeType, 1);
            }
        }
    }

    public void UnregisterMarineLife(MarineLife life)
    {
        marineLife.Remove(life);
    }

    public List<MarineLife> GetAllMarineLife()
    {
        return new List<MarineLife>(marineLife);
    }

    public int GetAliveCount()
    {
        int count = 0;
        foreach (var life in marineLife)
        {
            if (life != null && life.IsAlive()) count++;
        }
        return count;
    }

    public int GetStressedCount()
    {
        int count = 0;
        foreach (var life in marineLife)
        {
            if (life != null && life.state == MarineLife.LifeState.Stressed) count++;
        }
        return count;
    }

    public int GetMigratingCount()
    {
        int count = 0;
        foreach (var life in marineLife)
        {
            if (life != null && life.isMigrating) count++;
        }
        return count;
    }

    public void ResetEcology()
    {
        overallHealth = 100f;
        waterQuality = 100f;
        biodiversity = 100f;
        habitatIntegrity = 100f;
        carbonSequestration = 100f;
        foodWebStability = 100f;

        OnOverallHealthChanged?.Invoke(overallHealth);
        OnWaterQualityChanged?.Invoke(waterQuality);
        OnBiodiversityChanged?.Invoke(biodiversity);
        OnHabitatIntegrityChanged?.Invoke(habitatIntegrity);
        OnCarbonSequestrationChanged?.Invoke(carbonSequestration);
        OnFoodWebStabilityChanged?.Invoke(foodWebStability);
    }

    public string GetHealthStatus()
    {
        if (overallHealth >= 80f) return "优秀";
        if (overallHealth >= 60f) return "良好";
        if (overallHealth >= 40f) return "一般";
        if (overallHealth >= 20f) return "较差";
        return "危急";
    }
}
