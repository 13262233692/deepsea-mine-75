using System.Collections.Generic;
using UnityEngine;

public class PlumeManager : MonoBehaviour
{
    public static PlumeManager Instance { get; private set; }

    [Header("Global Plume Settings")]
    public float baseEmissionRate = 100f;
    public float driftSpeed = 1f;
    public Vector3 globalDriftDirection = new Vector3(0, 0.3f, 0.2f);

    [Header("Sediment Level")]
    public float globalSedimentLevel = 0f;
    public float sedimentDecayRate = 0.01f;
    public float sedimentIncreaseRate = 0.02f;
    public float maxSedimentLevel = 0.8f;

    [Header("Global Memory Protection")]
    [Tooltip("全局粒子总数硬限制")]
    public int globalMaxParticles = 3000;
    [Tooltip("全局软限制阈值")]
    [Range(0.5f, 0.9f)]
    public float globalSoftLimitThreshold = 0.7f;
    [Tooltip("全局监控间隔（秒）")]
    public float globalMonitoringInterval = 1f;
    [Tooltip("严重超限时强制清空所有粒子")]
    public bool emergencyClearEnabled = true;

    [Header("Adaptive LOD")]
    [Tooltip("高沉积物水平时自动降低粒子生命周期")]
    public bool adaptiveLifetime = true;
    [Tooltip("粒子生命周期最低比例")]
    [Range(0.3f, 1f)]
    public float minLifetimeRatio = 0.4f;

    [Header("Debug")]
    public int totalActiveParticles;
    public int globalSoftLimit;
    public bool isGlobalLimited;
    public float globalLimitMultiplier = 1f;

    [Header("References")]
    public List<SedimentPlume> plumes = new List<SedimentPlume>();

    private float lastGlobalMonitorTime;
    private float originalSedimentIncreaseRate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        originalSedimentIncreaseRate = sedimentIncreaseRate;
        globalSoftLimit = Mathf.FloorToInt(globalMaxParticles * globalSoftLimitThreshold);
    }

    private void Update()
    {
        UpdateSedimentLevel();
        UpdatePlumeIntensity();

        if (Time.time - lastGlobalMonitorTime >= globalMonitoringInterval)
        {
            GlobalParticleMonitoring();
            lastGlobalMonitorTime = Time.time;
        }
    }

    private void UpdateSedimentLevel()
    {
        float miningIntensity = MiningManager.Instance?.GetTotalMiningIntensity() ?? 0f;
        float actualIncreaseRate = sedimentIncreaseRate * globalLimitMultiplier;

        if (miningIntensity > 0f)
        {
            globalSedimentLevel += miningIntensity * actualIncreaseRate * Time.deltaTime;

            if (globalSedimentLevel >= maxSedimentLevel)
            {
                globalSedimentLevel = maxSedimentLevel;
                if (miningIntensity > 0.5f)
                {
                    ForceReduceEmission();
                }
            }
        }
        else
        {
            globalSedimentLevel -= sedimentDecayRate * Time.deltaTime;
        }

        globalSedimentLevel = Mathf.Clamp01(globalSedimentLevel);
        GameManager.Instance?.UpdateSedimentLevel(globalSedimentLevel);

        if (adaptiveLifetime)
        {
            UpdateAdaptiveLifetime();
        }
    }

    private void UpdateAdaptiveLifetime()
    {
        float lifetimeRatio = Mathf.Lerp(1f, minLifetimeRatio, globalSedimentLevel);

        foreach (var plume in plumes)
        {
            if (plume != null && plume.particleSystem != null)
            {
                var main = plume.particleSystem.main;
                main.startLifetime = plume.particleLifetime * lifetimeRatio;
            }
        }
    }

    private void UpdatePlumeIntensity()
    {
        float intensity = MiningManager.Instance?.GetTotalMiningIntensity() ?? 0f;
        float actualIntensity = intensity * globalLimitMultiplier;

        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                plume.SetEmissionIntensity(actualIntensity);
            }
        }
    }

    private void GlobalParticleMonitoring()
    {
        totalActiveParticles = 0;
        int limitedPlumes = 0;

        foreach (var plume in plumes)
        {
            if (plume != null && plume.particleSystem != null)
            {
                totalActiveParticles += plume.particleSystem.particleCount;
                if (plume.isLimited)
                {
                    limitedPlumes++;
                }
            }
        }

        if (totalActiveParticles >= globalMaxParticles)
        {
            isGlobalLimited = true;
            globalLimitMultiplier = 0.2f;

            if (emergencyClearEnabled && totalActiveParticles >= globalMaxParticles * 1.2f)
            {
                EmergencyClearAllParticles();
            }
            else
            {
                ForceReduceEmission();
            }

            Debug.LogWarningFormat(
                "[PlumeManager] 全局粒子超限！总数: {0}/{1}, 受限羽流: {2}",
                totalActiveParticles, globalMaxParticles, limitedPlumes
            );
        }
        else if (totalActiveParticles > globalSoftLimit)
        {
            isGlobalLimited = true;
            float overRatio = (float)(totalActiveParticles - globalSoftLimit) / (globalMaxParticles - globalSoftLimit);
            globalLimitMultiplier = Mathf.Lerp(1f, 0.3f, overRatio);

            if (overRatio > 0.7f)
            {
                ForceCleanupOldParticles();
            }
        }
        else
        {
            isGlobalLimited = false;
            globalLimitMultiplier = Mathf.Lerp(globalLimitMultiplier, 1f, 2f * Time.deltaTime);
        }
    }

    private void ForceReduceEmission()
    {
        foreach (var plume in plumes)
        {
            if (plume != null && plume.particleSystem != null)
            {
                var emission = plume.particleSystem.emission;
                emission.rateOverTime *= 0.3f;
            }
        }
    }

    private void ForceCleanupOldParticles()
    {
        foreach (var plume in plumes)
        {
            if (plume != null && plume.particleSystem != null)
            {
                plume.ForceCleanupParticles();
            }
        }
    }

    private void EmergencyClearAllParticles()
    {
        Debug.LogErrorFormat(
            "[PlumeManager] 紧急清空！粒子数 {0} 严重超出限制 {1}",
            totalActiveParticles, globalMaxParticles
        );

        foreach (var plume in plumes)
        {
            if (plume != null && plume.particleSystem != null)
            {
                plume.particleSystem.Clear();
            }
        }

        totalActiveParticles = 0;
    }

    public void RegisterPlume(SedimentPlume plume)
    {
        if (!plumes.Contains(plume))
        {
            plumes.Add(plume);
            UpdateGlobalLimits();
        }
    }

    public void UnregisterPlume(SedimentPlume plume)
    {
        plumes.Remove(plume);
        UpdateGlobalLimits();
    }

    private void UpdateGlobalLimits()
    {
        if (plumes.Count > 0)
        {
            int perPlumeMax = Mathf.FloorToInt(globalMaxParticles / plumes.Count);
            foreach (var plume in plumes)
            {
                if (plume != null)
                {
                    plume.maxParticles = Mathf.Min(plume.maxParticles, perPlumeMax);
                }
            }
        }
    }

    public float GetSedimentDensityAtPosition(Vector3 position, float radius)
    {
        float totalDensity = 0f;
        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                totalDensity += plume.GetParticleDensityInArea(position, radius);
            }
        }
        return Mathf.Clamp01(totalDensity);
    }

    public void UpdateGlobalDrift(Vector3 direction)
    {
        globalDriftDirection = direction.normalized;
        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                plume.UpdateDriftDirection(globalDriftDirection * driftSpeed);
            }
        }
    }

    public void StopAllPlumes()
    {
        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                plume.StopEmission();
            }
        }
    }

    public void StartAllPlumes()
    {
        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                plume.StartEmission();
            }
        }
    }

    public void ClearAllParticles()
    {
        foreach (var plume in plumes)
        {
            if (plume != null)
            {
                plume.ClearAllParticles();
            }
        }
        totalActiveParticles = 0;
    }
}
