using UnityEngine;

[ExecuteInEditMode]
public class SedimentPlume : MonoBehaviour
{
    [Header("Plume Settings")]
    public float emissionRate = 50f;
    public float particleLifetime = 8f;
    public float particleSpeed = 2f;
    public float spreadAngle = 45f;
    public Vector3 driftDirection = new Vector3(0, 0.5f, 0.3f);
    public float particleSize = 0.3f;

    [Header("Memory Protection")]
    [Tooltip("最大粒子数量上限（硬限制）")]
    public int maxParticles = 1000;
    [Tooltip("软限制阈值，超过后开始减少发射")]
    [Range(0.5f, 0.95f)]
    public float softLimitThreshold = 0.75f;
    [Tooltip("粒子数监控间隔（秒）")]
    public float monitoringInterval = 0.5f;
    [Tooltip("超出限制时是否自动清理过期粒子")]
    public bool autoCleanup = true;

    [Header("Performance")]
    [Tooltip("不可见时暂停发射")]
    public bool cullWhenInvisible = true;

    [Header("Color Settings")]
    public Color startColor = new Color(0.5f, 0.45f, 0.35f, 0.8f);
    public Color endColor = new Color(0.3f, 0.28f, 0.22f, 0f);

    [Header("References")]
    public Transform emissionSource;
    public ParticleSystem particleSystem;

    [Header("Debug")]
    public int currentParticleCount;
    public bool isLimited;
    public float currentIntensityMultiplier = 1f;

    private ParticleSystem.Particle[] particles;
    private float lastEmissionTime;
    private float lastMonitorTime;
    private float originalEmissionRate;
    private bool isInitialized;

    private void Start()
    {
        InitializeParticleSystem();
    }

    private void InitializeParticleSystem()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = gameObject.AddComponent<ParticleSystem>();
            }
        }

        int calculatedMax = Mathf.CeilToInt(emissionRate * particleLifetime * 1.5f);
        maxParticles = Mathf.Min(maxParticles, Mathf.Max(calculatedMax, 200));

        var main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startColor = startColor;
        main.gravityModifier = -0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = maxParticles;

        if (cullWhenInvisible)
        {
            main.cullingMode = ParticleSystemCullingMode.Pause;
        }
        else
        {
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        }

        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
        emission.enabled = true;

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = spreadAngle;
        shape.radius = 1f;
        shape.arc = 360f;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 2f);

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = driftDirection.x;
        velocityOverLifetime.y = driftDirection.y;
        velocityOverLifetime.z = driftDirection.z;

        var collision = particleSystem.collision;
        collision.enabled = false;

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial();

        originalEmissionRate = emissionRate;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || particleSystem == null) return;

        currentParticleCount = particleSystem.particleCount;

        if (Time.time - lastMonitorTime >= monitoringInterval)
        {
            MonitorAndControlParticles();
            lastMonitorTime = Time.time;
        }
    }

    private void MonitorAndControlParticles()
    {
        int count = particleSystem.particleCount;
        float usageRatio = (float)count / maxParticles;
        int softLimit = Mathf.FloorToInt(maxParticles * softLimitThreshold);

        if (count >= maxParticles)
        {
            isLimited = true;
            currentIntensityMultiplier = 0.3f;

            if (autoCleanup)
            {
                ForceCleanupParticles();
            }

            var emission = particleSystem.emission;
            emission.rateOverTime = originalEmissionRate * currentIntensityMultiplier;

            if (count >= maxParticles * 1.1f)
            {
                particleSystem.Clear();
                Debug.LogWarningFormat(
                    "[SedimentPlume] 粒子严重超限 ({0}/{1})，已强制清空！",
                    count, maxParticles
                );
            }
        }
        else if (count > softLimit)
        {
            isLimited = true;
            float overRatio = (float)(count - softLimit) / (maxParticles - softLimit);
            currentIntensityMultiplier = Mathf.Lerp(1f, 0.4f, overRatio);

            var emission = particleSystem.emission;
            emission.rateOverTime = originalEmissionRate * currentIntensityMultiplier;
        }
        else
        {
            isLimited = false;
            currentIntensityMultiplier = 1f;

            var emission = particleSystem.emission;
            emission.rateOverTime = originalEmissionRate * currentIntensityMultiplier;
        }
    }

    public void ForceCleanupParticles()
    {
        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        int particleCount = particleSystem.GetParticles(particles);
        int removedCount = 0;

        for (int i = 0; i < particleCount; i++)
        {
            float remainingLifetime = particles[i].remainingLifetime;
            float totalLifetime = particles[i].startLifetime;
            float ageRatio = 1f - (remainingLifetime / totalLifetime);

            if (ageRatio > 0.6f || remainingLifetime < particleLifetime * 0.4f)
            {
                particles[i].remainingLifetime = 0f;
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            particleSystem.SetParticles(particles, particleCount);
            Debug.LogFormat("[SedimentPlume] 清理了 {0} 个过期粒子", removedCount);
        }
    }

    private Material CreateParticleMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", Color.white);
        mat.SetFloat("_InvFade", 1f);
        return mat;
    }

    public void SetEmissionIntensity(float intensity)
    {
        if (particleSystem == null || !isInitialized) return;

        float actualIntensity = intensity * currentIntensityMultiplier;
        originalEmissionRate = emissionRate * intensity;

        var main = particleSystem.main;
        int targetMaxParticles = Mathf.CeilToInt(emissionRate * intensity * particleLifetime * 1.5f);
        main.maxParticles = Mathf.Min(targetMaxParticles, maxParticles);

        var emission = particleSystem.emission;
        emission.rateOverTime = originalEmissionRate * currentIntensityMultiplier;
    }

    public void UpdateDriftDirection(Vector3 newDrift)
    {
        driftDirection = newDrift;
        if (particleSystem != null)
        {
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.x = driftDirection.x;
            velocityOverLifetime.y = driftDirection.y;
            velocityOverLifetime.z = driftDirection.z;
        }
    }

    public float GetParticleDensityInArea(Vector3 position, float radius)
    {
        if (particleSystem == null) return 0f;

        if (particles == null || particles.Length < particleSystem.main.maxParticles)
        {
            particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        }

        int particleCount = particleSystem.GetParticles(particles);
        if (particleCount == 0) return 0f;

        int particlesInArea = 0;
        float radiusSquared = radius * radius;

        for (int i = 0; i < particleCount; i++)
        {
            if ((particles[i].position - position).sqrMagnitude < radiusSquared)
            {
                particlesInArea++;
            }
        }

        return (float)particlesInArea / particleCount;
    }

    public void StopEmission()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void StartEmission()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    public void ClearAllParticles()
    {
        if (particleSystem != null)
        {
            particleSystem.Clear();
        }
    }

    private void OnDestroy()
    {
        if (particles != null)
        {
            particles = null;
        }
    }

    private void OnValidate()
    {
        if (particleSystem != null && Application.isPlaying && isInitialized)
        {
            InitializeParticleSystem();
        }
    }
}
