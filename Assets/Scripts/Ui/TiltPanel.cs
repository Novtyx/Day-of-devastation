using UnityEngine;
using UnityEngine.EventSystems;

public class TiltPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Настройки наклона")]
    [SerializeField] private float maxTiltAngle = 15f; // Макс. угол наклона
    [SerializeField] private float tiltSmoothness = 10f; // Плавность наклона

    private RectTransform rectTransform;
    private Vector2 panelCenter;
    private Quaternion targetRotation;
    private bool isHovered;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        panelCenter = rectTransform.rect.center;
    }

    private void Update()
    {
        if (!isHovered)
        {
            // Плавно возвращаем в исходное положение
            targetRotation = Quaternion.identity;
        }

        // Плавный поворот
        rectTransform.rotation = Quaternion.Lerp(
            rectTransform.rotation,
            targetRotation,
            Time.deltaTime * tiltSmoothness
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isHovered) return;

        // Получаем локальные координаты курсора относительно панели
        Vector2 localCursor;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.enterEventCamera,
            out localCursor
        );

        // Нормализуем позицию курсора (от -1 до 1)
        float normalizedX = (localCursor.x - panelCenter.x) / (rectTransform.rect.width * 0.5f);
        float normalizedY = (localCursor.y - panelCenter.y) / (rectTransform.rect.height * 0.5f);

        // Вычисляем угол наклона
        float tiltX = -normalizedY * maxTiltAngle; // Наклон по вертикали (вперёд/назад)
        float tiltY = normalizedX * maxTiltAngle;  // Наклон по горизонтали (влево/вправо)

        // Создаём целевой поворот
        targetRotation = Quaternion.Euler(tiltX, tiltY, 0f);
    }
}