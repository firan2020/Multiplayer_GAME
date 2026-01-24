using PurrNet;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Inventory : NetworkBehaviour
{
    [SerializeField] protected SyncArray<InventoryItem> _inventoryitems = new();

    //[SerializeField] private ItemPreset testItem;

    //[ContextMenu("Add test item")]
    //private void AddTestIem()
    //{
    //    TryAddItem(testItem, 1);
    //}

    public virtual async Task<bool> TryAddItem(ItemPreset preset, int quantity)
    {
        if (TryStack(preset, quantity))
            return true;

        return TryAddNewItem(preset, quantity);
    }

    private bool TryStack(ItemPreset preset, int quantity)
    {
        for (int i = 0; i < _inventoryitems.Length; i++)
        {
            var invItem = _inventoryitems[i];
            if(invItem.preset != preset)
                continue;

            invItem.quantity += quantity;
            _inventoryitems[i] = invItem;
            return true;
        }
        return false;
    }

    private bool TryAddNewItem(ItemPreset preset, int quantity)
    {
        for (int i = 0; i < _inventoryitems.Length; i++)
        {
            var invItem = _inventoryitems[i];
            if (invItem.preset)
                continue;

            invItem.preset = preset;
            invItem.quantity = quantity;
            _inventoryitems[i] = invItem;

            return true;
        }
        return false;
    }

    public virtual void Interact(int index)
    {

    }

    public virtual void dropItem(int index, int amount)
    {
        if (index < 0 || index >= _inventoryitems.Count)
        {
            Debug.LogError($"Invalid index {index} for inventory items!",this);
            return;
        }

        var invItem = _inventoryitems[index];
        if (invItem.preset == null)
            return;

        amount = Mathf.Min(amount, invItem.quantity);
        for (int i = 0; i < amount; i++)
            Spawnitem(invItem.preset.prefab);
        
        RemoveItem(index, amount);
    }

    private void Spawnitem(Item item)
    {
        // Получаем позицию игрока (предполагается, что Inventory прикреплён к игроку)
        Vector3 playerPosition = transform.position;

        // Случайное смещение в горизонтальной плоскости (XZ)
        Vector2 randomOffset = Random.insideUnitCircle * 3f;

        // Формируем итоговую позицию: 
        // - X и Z — случайное смещение от игрока
        // - Y — высота игрока (сохраняем уровень)
        Vector3 spawnPosition = new Vector3(
            playerPosition.x + randomOffset.x,
            playerPosition.y + 1,                    // ← высота игрока
            playerPosition.z + randomOffset.y
        );

        Instantiate(item, spawnPosition, Quaternion.identity);
    }


    protected virtual void RemoveItem(int index,int amount)
    {
        var invItem = _inventoryitems[index];
        invItem.quantity -= amount;

        if (invItem.quantity <= 0)
            invItem.preset = null;

        _inventoryitems[index] = invItem;
    }
}


public struct InventoryItem
{
    public ItemPreset preset;
    public int quantity;
}