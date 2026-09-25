using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Gaze : MonoBehaviour
{
    [SerializeField] private LayerMask layersToInclude = ~0;
    [SerializeField] private Transform leftEyeTransform;
    [SerializeField] private Transform rightEyeTransform;
    [SerializeField] private float maxDistance = 100000f;

    public static Vector3 Direction { get; private set; } = Vector3.forward;
    public static Vector3 Position { get; private set; } = Vector3.zero;
    public static Vector3 Origin { get; private set; } = Vector3.zero;

    public static bool HasHit { get; private set; }
    public static RaycastHit HitInfo { get; private set; }

    private void LateUpdate()
    {


        Origin = (leftEyeTransform.position + rightEyeTransform.position) * 0.5f;
        Direction = (leftEyeTransform.forward + rightEyeTransform.forward).normalized;

        HasHit = Physics.Raycast(Origin, Direction, out RaycastHit hitInfo, maxDistance, layersToInclude);
        HitInfo = hitInfo;

        // Visual raycast debug line in the Scene view (Green = Hit, Red = Miss)
        Debug.DrawRay(Origin, Direction * 20f, HasHit ? Color.green : Color.red);

        if (HasHit)
        {
            Position = hitInfo.point;

            if (hitInfo.collider.TryGetComponent(out GazeInteractable gazeInteractable))
            {
                gazeInteractable.IsHovered = true;
                gazeInteractable.gazePosition = Position;
            }
        }
        else
        {
            Position = Origin + Direction * maxDistance;
        }
    }
}