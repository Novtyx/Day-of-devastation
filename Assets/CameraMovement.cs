using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private Vector3 _dragOrigin;

    public static CameraMovement instance;
    public Camera cam;
    public float smooth;
    public float smoothspeed; // Скорость приближения
    public float speed; // Скорость перемещения
    public float zoommin;
    public float zoommax;
    private float ortsize; // Текущий размер камеры
    public GameObject player;
    public GameObject MovementPanel;

    [SerializeField] private GameObject MapGameobject;

    private bool isFollow = false; // Нажата ли кнопка "следовать к персонажу"
    private float _velocity; // Нужна для метода Mathf.SmooothDamp
    private Vector3 _velocityVector;

    private void Start()
    {
        instance = this;
        ortsize = cam.GetComponent<Camera>().orthographicSize;
    }

    void Update()
    {
        if (isFollow) GoToPlayer();

        if (Input.touchCount == 0)
        {
            if (MovementPanel.activeInHierarchy == false)
            {
                MovementPanel.SetActive(true);
                MovementPanel.GetComponent<Button>().enabled = true;
            }
        }
        HandleZoom();
        // Перемещение с помощью клавиш WASD
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (x != 0 || y != 0) isFollow = false;
        cam.transform.position += new Vector3(x, y, 0) * speed * cam.GetComponent<Camera>().orthographicSize * Time.deltaTime;
    }
    // Следовать к персонажу
    void GoToPlayer()
    {
        Vector3 pos = player.transform.position;
        pos.z = -10f;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position,
            pos, ref _velocityVector, smooth);
        if (Vector2.Distance(cam.transform.position, new Vector3(player.transform.position.x,
            player.transform.position.y, -10f)) < 0.1f) isFollow = false;
    }
    private void HandleZoom()
    {
        // Масштабирование жестом "пинч" (для тач-устройств)
        if (Input.touchCount == 2)
        {
            smoothspeed = 1f;
            smooth = 0f;
            EventSystem.current.SetSelectedGameObject(null);
            MovementPanel.SetActive(false);
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ortsize -= difference * smoothspeed * 0.01f;
            ortsize = Mathf.Clamp(ortsize, zoommin, zoommax);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, ortsize, Time.deltaTime * 5f);
        }
        // Масштабирование колесиком мыши (для редактора)
        else
        {
            smoothspeed = 5f;
            smooth = 0.3f;
            var inputDelta = Input.GetAxis("Mouse ScrollWheel") * smoothspeed;
            ortsize = Mathf.Clamp(ortsize - inputDelta, zoommin, zoommax);
            var newOrtSize = Mathf.SmoothDamp(cam.orthographicSize, ortsize, ref _velocity, smooth);
            cam.orthographicSize = newOrtSize;
        }
    }
    public void SetOrigin()
    {
        if (Input.touchCount != 2)
        {
            _dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isFollow = false;
        }
    }
    public void MoveOrigin()
    {
        if (Input.touchCount != 2)
        {
            Vector3 difference = _dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference;
        }
    }

    // Функция приближения камеры на 3 единицы (уменьшение orthographicSize)
    public void ZoomIn()
    {
        ortsize = Mathf.Clamp(ortsize - 100f, zoommin, zoommax);
    }

    // Функция отдаления камеры на 3 единицы (увеличение orthographicSize)
    public void ZoomOut()
    {
        ortsize = Mathf.Clamp(ortsize + 100f, zoommin, zoommax);
        if (ortsize == zoommax)
        {
            Animations.instance.SetUiAlpha(MapGameobject, 1f);
            MapGameobject.SetActive(true);
        }
    }
    public void SetMapAlpha(float a)
    {
        Animations.instance.SetUiAlpha(MapGameobject, a);
        Invoke("HideMap", 0.5f);
    }
    void HideMap()
    {
        MapGameobject.SetActive(false);
    }
    public void ZoomToPlayer()
    {
        isFollow = true;
    }
}

