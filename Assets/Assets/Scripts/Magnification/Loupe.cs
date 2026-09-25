using UnityEngine;

[RequireComponent(typeof(Magnifier))]
public class Loupe : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lerpSpeed = 16f;

    [Header("Loupe Shader Parameters")]
    [SerializeField] private Vector2 bulgeSize = new Vector2(0.2f, 0.2f);
    [SerializeField] private float bulge = 0.5f;
    [SerializeField] private float flatCenter = 0.1f;
    [SerializeField] private float falloff = 2f;

    private Magnifier magnifier;
    [SerializeField] private Renderer targetRenderer;

    private Material material;
    private Vector2 currentUV;

    private static readonly int CirclePositionID = Shader.PropertyToID("_CirclePosition");
    private static readonly int BulgeSizeID = Shader.PropertyToID("_BulgeSize");
    private static readonly int BulgeID = Shader.PropertyToID("_Bulge");
    private static readonly int FlatCenterID = Shader.PropertyToID("_FlatCenter");
    private static readonly int FalloffID = Shader.PropertyToID("_Falloff");

    #region Public C# Properties
    public Vector2 BulgeSize
    {
        get => bulgeSize;
        set
        {
            bulgeSize = value;
            if (material != null) material.SetVector(BulgeSizeID, new Vector4(bulgeSize.x, bulgeSize.y, 0f, 0f));
        }
    }

    public float Bulge
    {
        get => bulge;
        set
        {
            bulge = value;
            if (material != null) material.SetFloat(BulgeID, bulge);
        }
    }

    public float FlatCenter
    {
        get => flatCenter;
        set
        {
            flatCenter = value;
            if (material != null) material.SetFloat(FlatCenterID, flatCenter);
        }
    }

    public float Falloff
    {
        get => falloff;
        set
        {
            falloff = value;
            if (material != null) material.SetFloat(FalloffID, falloff);
        }
    }
    #endregion

    private void Awake()
    {
        magnifier = GetComponent<Magnifier>();

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            material = targetRenderer.material;
            currentUV = new Vector2(0.5f, 0.5f);

            // Initialize material values from code defaults/inspector
            SyncMaterialProperties();
        }
    }

    private void OnValidate()
    {
        // Allows real-time material updates directly in the Editor Inspector during Play mode
        if (Application.isPlaying && material != null)
        {
            SyncMaterialProperties();
        }
    }

    private void SyncMaterialProperties()
    {
        material.SetVector(CirclePositionID, new Vector4(currentUV.x, currentUV.y, 0f, 0f));
        material.SetVector(BulgeSizeID, new Vector4(bulgeSize.x, bulgeSize.y, 0f, 0f));
        material.SetFloat(BulgeID, bulge);
        material.SetFloat(FlatCenterID, flatCenter);
        material.SetFloat(FalloffID, falloff);
    }

    private void LateUpdate()
    {
        if (material == null || magnifier == null)
            return;

        Vector2 rawUV = magnifier.gazeUV;
        Vector2 targetUV = UnwarpUV(rawUV);

        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        currentUV = Vector2.Lerp(currentUV, targetUV, t);

        material.SetVector(CirclePositionID, new Vector4(currentUV.x, currentUV.y, 0f, 0f));
    }

    private Vector2 UnwarpUV(Vector2 rawUV)
    {
        Vector2 circlePos = currentUV;

        Vector2 safeBulgeSize = new Vector2(
            Mathf.Approximately(bulgeSize.x, 0f) ? 0.0001f : bulgeSize.x,
            Mathf.Approximately(bulgeSize.y, 0f) ? 0.0001f : bulgeSize.y
        );

        Vector2 offset = rawUV - circlePos;
        Vector2 normalizedOffset = new Vector2(offset.x / safeBulgeSize.x, offset.y / safeBulgeSize.y);

        float distanceFromCenter = normalizedOffset.magnitude;
        float radius = distanceFromCenter;

        float denom = Mathf.Max(1f - flatCenter, 0.0001f);
        float transition = Mathf.Clamp01((radius - flatCenter) / denom);

        float influence = 1f - Smoothstep(0f, 1f, transition);
        influence = Mathf.Pow(influence, falloff);

        float distortion = 1f - (bulge * influence);

        return circlePos + offset * distortion;
    }

    private static float Smoothstep(float edge0, float edge1, float x)
    {
        float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }
}