using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class Itemdatabase : MonoBehaviour
{
    private Dictionary<string, ItemPreset> _itempreset = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);

        var presets = Resources.LoadAll<ItemPreset>(path: "");
        foreach (var preset in presets)
        {
            if (!_itempreset.TryAdd(preset.uid, preset))
                Debug.LogError($"duplicate item preset uid found ({preset.uid})");
        }
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<Itemdatabase>();
    }

    public bool TryGetItemPreset(string uid, out ItemPreset preset)
    {
        if (string.IsNullOrEmpty(uid))
        {
            preset = null;
            return false;
        }

        return _itempreset.TryGetValue(uid,out preset);
    }
}
