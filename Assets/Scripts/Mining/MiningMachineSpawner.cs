using UnityEngine;

public class MiningMachineSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0f, 5f, 0f);

    [Header("References")]
    public SeabedTerrainGenerator terrainGenerator;
    public CameraController cameraController;

    private GameObject miningMachine;

    private void Start()
    {
        SpawnMiningMachine();
    }

    public void SpawnMiningMachine()
    {
        miningMachine = new GameObject("Mining Machine");
        miningMachine.transform.position = spawnPosition;

        Rigidbody rb = miningMachine.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        MiningMachineController controller = miningMachine.AddComponent<MiningMachineController>();
        controller.terrainLayer = LayerMask.GetMask("Default");
        controller.hoverHeight = 3f;
        controller.moveSpeed = 8f;

        CreateMachineVisuals(miningMachine);
        CreateCollectionPoint(miningMachine, controller);
        CreatePlumeEmitter(miningMachine);

        if (terrainGenerator != null)
        {
            float terrainHeight = terrainGenerator.GetTerrainHeight(spawnPosition.x, spawnPosition.z);
            miningMachine.transform.position = new Vector3(spawnPosition.x, terrainHeight + controller.hoverHeight, spawnPosition.z);
        }

        if (cameraController != null)
        {
            cameraController.target = miningMachine.transform;
        }
    }

    private void CreateMachineVisuals(GameObject parent)
    {
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.parent = parent.transform;
        visuals.transform.localPosition = Vector3.zero;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = visuals.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(6f, 2f, 4f);
        body.name = "Body";

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.3f, 0.3f, 0.35f);
        bodyMat.SetFloat("_Metallic", 0.8f);
        bodyMat.SetFloat("_Glossiness", 0.5f);
        bodyRenderer.material = bodyMat;

        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.transform.parent = visuals.transform;
        cabin.transform.localPosition = new Vector3(0f, 1.5f, 0.5f);
        cabin.transform.localScale = new Vector3(3f, 1.5f, 2f);
        cabin.name = "Cabin";

        Renderer cabinRenderer = cabin.GetComponent<Renderer>();
        Material cabinMat = new Material(Shader.Find("Standard"));
        cabinMat.color = new Color(0.2f, 0.4f, 0.6f);
        cabinMat.SetFloat("_Metallic", 0.3f);
        cabinMat.SetFloat("_Glossiness", 0.8f);
        cabinRenderer.material = cabinMat;

        GameObject trackLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trackLeft.transform.parent = visuals.transform;
        trackLeft.transform.localPosition = new Vector3(0f, -0.8f, 2.2f);
        trackLeft.transform.localScale = new Vector3(7f, 0.6f, 0.8f);
        trackLeft.name = "TrackLeft";

        GameObject trackRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trackRight.transform.parent = visuals.transform;
        trackRight.transform.localPosition = new Vector3(0f, -0.8f, -2.2f);
        trackRight.transform.localScale = new Vector3(7f, 0.6f, 0.8f);
        trackRight.name = "TrackRight";

        Material trackMat = new Material(Shader.Find("Standard"));
        trackMat.color = new Color(0.15f, 0.15f, 0.15f);
        trackLeft.GetComponent<Renderer>().material = trackMat;
        trackRight.GetComponent<Renderer>().material = trackMat;

        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.transform.parent = visuals.transform;
        arm.transform.localPosition = new Vector3(3.5f, 0f, 0f);
        arm.transform.localScale = new Vector3(3f, 0.5f, 1f);
        arm.name = "Arm";

        GameObject drill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drill.transform.parent = visuals.transform;
        drill.transform.localPosition = new Vector3(5f, -1f, 0f);
        drill.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
        drill.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        drill.name = "Drill";

        Renderer drillRenderer = drill.GetComponent<Renderer>();
        Material drillMat = new Material(Shader.Find("Standard"));
        drillMat.color = new Color(0.5f, 0.45f, 0.4f);
        drillMat.SetFloat("_Metallic", 0.9f);
        drillMat.SetFloat("_Glossiness", 0.7f);
        drillRenderer.material = drillMat;

        Collider[] colliders = parent.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }

        BoxCollider mainCollider = parent.AddComponent<BoxCollider>();
        mainCollider.size = new Vector3(8f, 3f, 5f);
        mainCollider.center = new Vector3(0f, 0.5f, 0f);
    }

    private void CreateCollectionPoint(GameObject parent, MiningMachineController controller)
    {
        GameObject collectionPoint = new GameObject("CollectionPoint");
        collectionPoint.transform.parent = parent.transform;
        collectionPoint.transform.localPosition = new Vector3(5f, -2f, 0f);
        controller.collectionPoint = collectionPoint.transform;
    }

    private void CreatePlumeEmitter(GameObject parent)
    {
        GameObject plumeEmitter = new GameObject("PlumeEmitter");
        plumeEmitter.transform.parent = parent.transform;
        plumeEmitter.transform.localPosition = new Vector3(5f, -3f, 0f);

        SedimentPlume plume = plumeEmitter.AddComponent<SedimentPlume>();
        plume.emissionRate = 60f;
        plume.particleLifetime = 6f;
        plume.particleSpeed = 1.5f;
        plume.spreadAngle = 60f;
        plume.driftDirection = new Vector3(0f, 0.8f, 0.2f);
        plume.particleSize = 0.4f;
        plume.startColor = new Color(0.45f, 0.4f, 0.3f, 0.7f);
        plume.endColor = new Color(0.25f, 0.22f, 0.18f, 0f);

        plume.maxParticles = 500;
        plume.softLimitThreshold = 0.7f;
        plume.monitoringInterval = 0.3f;
        plume.autoCleanup = true;
        plume.cullWhenInvisible = true;

        PlumeManager.Instance?.RegisterPlume(plume);
    }

    public GameObject GetMiningMachine()
    {
        return miningMachine;
    }
}
