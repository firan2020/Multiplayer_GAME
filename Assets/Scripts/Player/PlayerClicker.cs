using PurrNet;
using UnityEngine;

public class PlayerClicker : NetworkBehaviour
{
    [SerializeField] private Inventory _inventory;

    private async void Update()
    {
        if (!isOwner)
            return;

        // Проверяем нажатие левой кнопки мыши
        if (!Input.GetKeyDown(KeyCode.E))  // Только при нажатии (не удерживании)
            return;

        // Создаём луч из позиции курсора в 3D‑пространство
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Делаем Raycast в 3D
        if (!Physics.Raycast(ray, out hit))
            return;

        // Пытаемся получить компонент Item у попавшего объекта
        if (hit.collider.TryGetComponent(out Item item))
        {
            // Пытаемся добавить предмет в инвентарь
            if (await _inventory.TryAddItem(item.preset, 1))
            {
                // Если добавили — уничтожаем объект
                Destroy(item.gameObject);
            }
        }
    }
}
