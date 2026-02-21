using UnityEngine;

public class CursorLock : MonoBehaviour
{
    [Tooltip("Клавиша для переключения режима курсора (по умолчанию Esc)")]
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;

    [Tooltip("Фиксировать курсор в центре экрана? (для FPS)")]
    [SerializeField] bool lockToCenter = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Переключение режима по нажатию клавиши
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCursorLock();
        }
    }

    /// <summary>
    /// Зафиксировать курсор (скрыт, ограничен в окне)
    /// </summary>
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Освободить курсор (виден, может выходить за пределы окна)
    /// </summary>
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Переключить режим курсора
    /// </summary>
    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void OnDisable()
    {
        // При отключении скрипта — освободить курсор
        UnlockCursor();
    }
}
