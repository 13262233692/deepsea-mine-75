using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 15f, -20f);

    [Header("Movement Settings")]
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 50f;

    [Header("Free Look")]
    public bool freeLookEnabled = false;
    public KeyCode freeLookKey = KeyCode.LeftShift;
    public float freeLookSpeed = 10f;

    [Header("Underwater Effects")]
    public Color underwaterColor = new Color(0f, 0.2f, 0.3f);
    public float fogDensity = 0.02f;
    public bool underwaterFog = true;

    private Camera cam;
    private float currentZoom;
    private float currentRotationY;
    private float currentRotationX;
    private Vector3 freeLookPosition;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        currentZoom = offset.magnitude;
        currentRotationY = 0f;
        currentRotationX = 20f;
        freeLookPosition = transform.position;

        SetupUnderwaterEffect();
    }

    private void SetupUnderwaterEffect()
    {
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = underwaterColor;
        }

        if (underwaterFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = underwaterColor;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = fogDensity;
        }

        RenderSettings.ambientLight = new Color(0.1f, 0.15f, 0.2f);
    }

    private void Update()
    {
        HandleInput();

        if (freeLookEnabled)
        {
            UpdateFreeLook();
        }
        else
        {
            UpdateFollowCamera();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(freeLookKey))
        {
            freeLookEnabled = !freeLookEnabled;
            if (freeLookEnabled && target != null)
            {
                freeLookPosition = transform.position;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);

        if (Input.GetMouseButton(1))
        {
            currentRotationY += Input.GetAxis("Mouse X") * rotationSpeed;
            currentRotationX -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentRotationX = Mathf.Clamp(currentRotationX, -30f, 60f);
        }
    }

    private void UpdateFollowCamera()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
        Vector3 targetPosition = target.position + rotation * (offset.normalized * currentZoom);

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 2f);
    }

    private void UpdateFreeLook()
    {
        float moveX = Input.GetAxis("Horizontal") * freeLookSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * freeLookSpeed * Time.deltaTime;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.Q)) moveY = -freeLookSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) moveY = freeLookSpeed * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        freeLookPosition += rotation * new Vector3(moveX, moveY, moveZ);

        Quaternion lookRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
        transform.position = Vector3.Lerp(transform.position, freeLookPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, followSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        freeLookEnabled = false;
    }

    public void ResetCamera()
    {
        currentRotationY = 0f;
        currentRotationX = 20f;
        currentZoom = offset.magnitude;
    }
}
