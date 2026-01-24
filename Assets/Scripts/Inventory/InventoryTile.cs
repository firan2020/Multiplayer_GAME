using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTile : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _quantityText;


    private InventoryView _inventoryView;
    private int _index;

    private void Awake()
    {
        ResetTile();
    }

    public void Init(InventoryView inventoryView,int index)
    {
        _inventoryView = inventoryView;
        _index = index;
    }

    public void SetItem(InventoryItem inventoryItem)
    {
        if (!inventoryItem.preset)
        {
            ResetTile();
            return;
        }

        _quantityText.text = inventoryItem.quantity.ToString();
        _icon.sprite = inventoryItem.preset.icon;
        _icon.color = Color.white;
    }

    private void ResetTile()
    {
        _icon.color = Color.clear;
        _icon.sprite = null;
        _quantityText.text = "";

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _inventoryView.Interact(_index);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _inventoryView.dRopItem(_index);
        }
    }
}
