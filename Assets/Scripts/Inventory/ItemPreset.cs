using PurrNet.Utils;
using UnityEngine;

[CreateAssetMenu(fileName ="ItemPreset",menuName = "ScriptableObjects/ItemPreset")]
public class ItemPreset : ScriptableObject
{

    [PurrReadOnly] public string uid;

    public string itemName;
    public Item prefab;
    public Sprite icon;

#if UNITY_EDITOR

    private void OnValidate()
    {
        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

        if (assetPath == null)
        {
            uid = string.Empty;
            return;
        }

        var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
        if(string.IsNullOrEmpty(uid) || uid != assetGuid)
            uid = assetGuid;
    }

#endif

}
