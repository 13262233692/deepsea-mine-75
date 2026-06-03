using UnityEngine;
using UnityEngine.UI;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI References")]
    public GameObject monitorPanel;
    public Text fpsText;
    public Text particleCountText;
    public Text memoryText;
    public Text sedimentLevelText;
    public Text healthText;

    [Header("Settings")]
    public float updateInterval = 0.5f;
    public KeyCode toggleKey = KeyCode.F3;

    [Header("Debug")]
    public bool showMonitor = false;

    private float deltaTime = 0f;
    private float lastUpdateTime = 0f;
    private float fps;
    private float allocatedMemory;

    private void Start()
    {
        if (monitorPanel != null)
        {
            monitorPanel.SetActive(showMonitor);
        }
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMonitor();
        }

        if (showMonitor && Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMonitorDisplay();
            lastUpdateTime = Time.time;
        }
    }

    private void ToggleMonitor()
    {
        showMonitor = !showMonitor;
        if (monitorPanel != null)
        {
            monitorPanel.SetActive(showMonitor);
        }
    }

    private void UpdateMonitorDisplay()
    {
        fps = 1.0f / deltaTime;
        allocatedMemory = (float)System.GC.GetTotalMemory(false) / (1024f * 1024f);

        if (fpsText != null)
        {
            fpsText.text = $"FPS: {fps:F1}";
            fpsText.color = fps < 30f ? Color.red : fps < 60f ? Color.yellow : Color.green;
        }

        if (particleCountText != null && PlumeManager.Instance != null)
        {
            int totalParticles = PlumeManager.Instance.totalActiveParticles;
            int maxParticles = PlumeManager.Instance.globalMaxParticles;
            float ratio = (float)totalParticles / maxParticles;

            particleCountText.text = $"粒子: {totalParticles}/{maxParticles}";
            particleCountText.color = ratio > 0.9f ? Color.red : ratio > 0.7f ? Color.yellow : Color.white;
        }

        if (memoryText != null)
        {
            memoryText.text = $"内存: {allocatedMemory:F1} MB";
            memoryText.color = allocatedMemory > 1000f ? Color.red : allocatedMemory > 500f ? Color.yellow : Color.white;
        }

        if (sedimentLevelText != null && PlumeManager.Instance != null)
        {
            float sediment = PlumeManager.Instance.globalSedimentLevel * 100f;
            sedimentLevelText.text = $"沉积物: {sediment:F1}%";
        }

        if (healthText != null && EcologyManager.Instance != null)
        {
            float health = EcologyManager.Instance.overallHealth;
            healthText.text = $"环境健康: {health:F1}%";
        }
    }
}
