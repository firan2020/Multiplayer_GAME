using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    
    [SerializeField] private string _viewName;
    [SerializeField] private InventoryTile[] _inventoryTiles;

    public static Dictionary<string, InventoryView> instances = new();

    private Inventory _inventory;

    private void Awake()
    {
        

        instances[_viewName] = this;

        for (int i = 0; i < _inventoryTiles.Length; i++)
        {
            _inventoryTiles[i].Init(this, i);
        }
    }


    

    private void OnDestroy()
    {
        instances.Remove(_viewName);
    }

    public void Init(Inventory inventory)
    {
        _inventory = inventory;
    }

    public void RedrawEverything(InventoryItem[] inventoryItems)
    {
        for (int i = 0; i < _inventoryTiles.Length; i++)
        {
            var invItem = inventoryItems[i];
            if (_inventoryTiles.Length <= i)
            {
                Debug.LogError($"Sent more items than tiles! {i} > {_inventoryTiles.Length}", this);
                return;
            }

            _inventoryTiles[i].SetItem(invItem);
        }
    }

    public void dRopItem(int index)
    {
        _inventory.dropItem(index,1);
    }

    public void Interact(int index)
    {
        _inventory.Interact(index);
    }
}
