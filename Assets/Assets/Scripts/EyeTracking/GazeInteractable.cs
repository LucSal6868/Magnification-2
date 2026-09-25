using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class GazeInteractable : MonoBehaviour
{
    public bool IsHovered { get; set;}    
    public Vector3 gazePosition { get; set;}    

    void Start()
    {
        
    }

    void Update()
    {
        if (IsHovered)
        {
            OnGaze();
            if(IsHovered == false)
                OnGazeEnter();
            IsHovered = true;
        }
        else
        {
            OnGazeExit();
            IsHovered = false;
        }
    }

    public virtual void OnGazeEnter()
    {
    }

    public virtual void OnGazeExit()
    {
    }

    public virtual void OnGaze()
    {
        
    }
    
}
