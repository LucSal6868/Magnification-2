using UnityEngine;

[RequireComponent(typeof(Magnifier))]
public class Zoom : MonoBehaviour
{
    private Magnifier magnifier;

    private void Awake()
    {
        magnifier = GetComponent<Magnifier>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 uv = magnifier.gazeUV;

        
    }
}
