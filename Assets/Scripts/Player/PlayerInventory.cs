using PurrNet;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PlayerInventory : Inventory
{
    
    [SerializeField] private string _viewName;
    private InventoryView _inventoryView;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _inventoryView = InventoryView.instances[_viewName];

        if(!isOwner)
            return;

        InstanceHandler.RegisterInstance(this);
        _inventoryView.Init(this);
        _inventoryitems.onChanged += OnInventoryChanged;
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();

        if(!isOwner)
            return;

        _inventoryitems.onChanged -= OnInventoryChanged;
        InstanceHandler.UnregisterInstance<PlayerInventory>();
    }

    private void OnInventoryChanged(SyncArrayChange<InventoryItem> change)
    {
        if(_inventoryView != null)
            _inventoryView.RedrawEverything(_inventoryitems.ToArray());
    }

    public async  override void Interact(int index)
    {
        if (_inventoryitems[index].preset == null)
            return;

        if (!InstanceHandler.TryGetInstance(out SharedInventory sharedInventory))
        {
            Debug.LogError($"Failed to get shared inventory", this);

            return;
        }

        if (await sharedInventory.TryAddItem(_inventoryitems[index].preset, 1))
            RemoveItem(index, 1);
    }
}
