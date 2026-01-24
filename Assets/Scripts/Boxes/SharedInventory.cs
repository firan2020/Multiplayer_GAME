using PurrNet;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class SharedInventory : Inventory
{
    [SerializeField] private InventoryView _inventoryView;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        InstanceHandler.UnregisterInstance<SharedInventory>();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _inventoryView.Init(this);
        _inventoryitems.onChanged += OnInventoryChanged;
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();

        _inventoryitems.onChanged -= OnInventoryChanged;
    }

    [ServerRpc(requireOwnership:false)]
    public async override Task<bool> TryAddItem(ItemPreset preset, int quantity)
    {
        return await base.TryAddItem(preset, quantity);
    }

    private void OnInventoryChanged(SyncArrayChange<InventoryItem> change)
    {
        _inventoryView.RedrawEverything(_inventoryitems.ToArray());
    }

    public async override void Interact(int index)
    {
        if (!InstanceHandler.TryGetInstance(out PlayerInventory playerInventory))
        {
            Debug.LogError($"Failed to get  shared inventory", this);
            return;
        }

        if (_inventoryitems[index].preset == null)
            return;

        if (await playerInventory.TryAddItem(_inventoryitems[index].preset, 1))
            RemoveItem(index, 1);

    }


    [ServerRpc(requireOwnership:false)]
    protected override void RemoveItem(int index, int amount)
    {
        base.RemoveItem(index, amount);
    }
}
