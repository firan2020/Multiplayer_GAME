using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VignetteController : MonoBehaviour
{
    [SerializeField] private Image vignetteImage; // Ссылка на Image с виньеткой
    [SerializeField] private float fadeInDuration = 0.3f; // Время появления (сек)
    [SerializeField] private float holdDuration = 0.5f;   // Время удержания (сек)
    [SerializeField] private float fadeOutDuration = 0.3f; // Время исчезновения (сек)

    private CanvasGroup canvasGroup; // Компонент Canvas Group для управления прозрачностью
    private bool isShowing = false;  // Флаг: виньетка показывается?

    void Start()
    {
        // Получаем Canvas Group (если его нет — добавляем)
        canvasGroup = vignetteImage.GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            canvasGroup = vignetteImage.gameObject.AddComponent<CanvasGroup>();
        }

        // Начальная прозрачность (невидимая)
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Показывает виньетку при получении урона или по команде
    /// </summary>
    public void ShowVignette()
    {
        if (isShowing) return; // Если уже показывается — выходим

        isShowing = true;
        StartCoroutine(FadeInAndOut());
    }

    /// <summary>
    /// Корутина для плавного появления и исчезновения виньетки
    /// </summary>
    private IEnumerator FadeInAndOut()
    {
        // 1. Плавное появление (fade in)
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeInDuration;
            yield return null;
        }

        // 2. Удерживаем виньетку видимой
        yield return new WaitForSeconds(holdDuration);

        // 3. Плавное исчезновение (fade out)
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1f - (t / fadeOutDuration);
            yield return null;
        }

        // 4. Сбрасываем флаг и прозрачность
        isShowing = false;
        canvasGroup.alpha = 0f;
    }
}
