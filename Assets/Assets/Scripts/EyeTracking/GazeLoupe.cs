using UnityEngine;

public class GazeLoupe : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Lerping")]
    [SerializeField] private float lerpSpeed = 25f;

    private Material material;
    private Vector2 currentUV;

    private static readonly int CirclePositionID = Shader.PropertyToID("_CirclePosition");
    private static readonly int BulgeSizeID = Shader.PropertyToID("_BulgeSize");
    private static readonly int BulgeID = Shader.PropertyToID("_Bulge");
    private static readonly int FlatCenterID = Shader.PropertyToID("_FlatCenter");
    private static readonly int FalloffID = Shader.PropertyToID("_Falloff");

    private void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("Renderer is not assigned!");
            return;
        }

        material = targetRenderer.material;
        currentUV = new Vector2(0.5f, 0.5f);
        material.SetVector(CirclePositionID, new Vector4(currentUV.x, currentUV.y, 0f, 0f));
    }

    private void LateUpdate()
    {
        if (material == null || !Gaze.HasHit) return;

        // Note: textureCoord requires a MeshCollider on the target object
        Vector2 rawUV = Gaze.HitInfo.textureCoord;
        Vector2 targetUV = UnwarpUV(rawUV);

        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        currentUV = Vector2.Lerp(currentUV, targetUV, t);

        material.SetVector(CirclePositionID, new Vector4(currentUV.x, currentUV.y, 0f, 0f));
    }

    private Vector2 UnwarpUV(Vector2 rawUV)
    {
        Vector2 circlePos = currentUV;

        Vector4 bulgeSizeV4 = material.GetVector(BulgeSizeID);
        Vector2 bulgeSize = new Vector2(
            Mathf.Approximately(bulgeSizeV4.x, 0f) ? 0.0001f : bulgeSizeV4.x,
            Mathf.Approximately(bulgeSizeV4.y, 0f) ? 0.0001f : bulgeSizeV4.y
        );

        float bulge = material.GetFloat(BulgeID);
        float flatCenter = material.GetFloat(FlatCenterID);
        float falloff = material.GetFloat(FalloffID);

        Vector2 offset = rawUV - circlePos;
        Vector2 normalizedOffset = new Vector2(offset.x / bulgeSize.x, offset.y / bulgeSize.y);

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