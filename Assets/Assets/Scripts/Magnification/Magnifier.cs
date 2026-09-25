using UnityEngine;


public class Magnifier : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;

    [Header("Gaze Output Data")]
    [System.NonSerialized] public Vector2 gazeUV;
    [System.NonSerialized] public Vector3 gazeWorldPos;
    [System.NonSerialized]public Vector3 gazeLocalPos;


    private void Reset()
    {
        targetCollider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        if (!Gaze.HasHit || Gaze.HitInfo.collider != targetCollider)
            return;
        gazeUV = Gaze.HitInfo.textureCoord;
        gazeWorldPos = Gaze.HitInfo.point;
        gazeLocalPos = transform.InverseTransformPoint(gazeWorldPos);
    }
}