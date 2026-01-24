using PurrNet;
using PurrNet.Packing;
using UnityEngine;

public static class InventorydataUtils
{
    public static void itemPresetWriter(BitPacker packer, ItemPreset preset)
    {
        if (preset == null)
        {
            Packer<bool>.Write(packer, false);
            return;
        }

        Packer<bool>.Write(packer, true);
        Packer<string>.Write(packer, preset.uid);
    }

    public static void ItemPresetReader(BitPacker packer, ref ItemPreset preset)
    {
        bool hasPreset = false;
        Packer<bool>.Read(packer, ref hasPreset);
        if (!hasPreset)
        {
            preset = null;
            return;
        }

        if (!InstanceHandler.TryGetInstance(out Itemdatabase dataBase))
        {
            Debug.LogError($"Failed to get items database instance for reading item preset!");
            return;
        }

        var uid = default(string);
        Packer<string>.Read(packer, ref uid);

        dataBase.TryGetItemPreset(uid, out preset);
    }
}
