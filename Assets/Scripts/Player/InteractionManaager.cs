using PurrNet;
using System.Diagnostics.Tracing;
using UnityEngine;

public class InteractionManaager : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interacTdisTance = 4f;

    private Camera _camera;
    private AInteractable _currentHoveredInteractable;
}

public abstract class AInteractable : NetworkBehaviour
{
    public abstract void Interact();

    public virtual void OnHover() { }

    public virtual void OnStopHover() { }

    public virtual bool CanInteract()
    {
        return true;
    }

}
