using UnityEngine;

public class TestSphere : MonoBehaviour
{
    void Update()
    {
        transform.position = Gaze.Position;
    }
}
