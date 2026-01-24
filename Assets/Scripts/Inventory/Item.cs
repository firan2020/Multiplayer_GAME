using PurrNet;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] private ItemPreset _preset;
    public ItemPreset preset => _preset;
}
