using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class TilemapVoid : MonoBehaviour, IBeginDragHandler
{
    public static TilemapVoid instance;
    public Transform VoidPanel;
    public Transform VoidPanelContent;
    public LayerMask ObjectMask;
    public float Radius = 4f;

    public GameObject StopTaskPanel;
    public GameObject button;
    private Collider2D[] results = new Collider2D[20];

    private List<GameObject> buttons = new List<GameObject>();
    bool _vibrate = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
    }
    [SerializeField] Vector2 bas;
    [SerializeField] Vector2 end;
    [SerializeField] Vector2 basc;
    [SerializeField] Vector2 endc;
    [SerializeField] Sprite basSprite;
    [SerializeField] Sprite endSprite;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_vibrate) return;
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i], 0.1f);
        }
        buttons.Clear();
        _vibrate = false;

        Animations.instance.ShowCursor(bas, basSprite, 1f, basc);
    }

    public void VoidShow()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i]);
        }

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int hitCount = Physics2D.OverlapCircleNonAlloc(pos, Radius, results, ObjectMask);
        Dictionary<Type, List<IInteractable>> groupedInteractables = new Dictionary<Type, List<IInteractable>>();
        for (int i = 0; i < hitCount; i++)
        {
            if (results[i].TryGetComponent<IInteractable>(out var interactable))
            {
                Type type = interactable.GetType();
                if (!groupedInteractables.ContainsKey(type))
                {
                    groupedInteractables[type] = new List<IInteractable>();
                }
                groupedInteractables[type].Add(interactable);
            }
        }

        foreach (var group in groupedInteractables)
        {
            var items = group.Value;
            var firstItem = items[0];
            if (items.Count == 1)
            {
                CreateButton(firstItem.ActionName, () => StartTask(firstItem.ExecuteInteraction(Movement.instance)));
            }
            else
            {
                // Массовое действие (Паттерн Команда генерируется на лету)
                CreateButton($"{firstItem.ActionName} все ({items.Count} шт.)", () => StartTask(ExecuteMultiple(items)));
                // Для разнообразия можно оставить и возможность собрать только ближайший
                CreateButton($"{firstItem.ActionName} один", () => StartTask(firstItem.ExecuteInteraction(Movement.instance)));
            }
        }
        CreateButton("идти сюда", () => { Movement.instance.Movement_start(); HideContextMenu(); }, false);
        _vibrate = true;

        Animations.instance.ShowCursor(end, endSprite, 0f, endc);
    }

    private void HideContextMenu()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i], 0.1f);
        }
        buttons.Clear();
        _vibrate = false;
        Animations.instance.ShowCursor(bas, basSprite, 1f, basc);
    }

    private IEnumerator ExecuteMultiple(List<IInteractable> items)
    {
        foreach (var item in items)
        {
            if (item != null && (item as MonoBehaviour) != null)
            {
                yield return Movement.instance.currentTask = Movement.instance.StartCoroutine(item.ExecuteInteraction(Movement.instance));
            }
        }
        StopTaskPanel.SetActive(false);
    }
    public void StopAllTask()
    {
        StopCoroutine(ExecuteMultiple(null));
        Movement.instance.StopTask();
        Movement.instance.go_not = false;
    }


    void StartTask(IEnumerator task)
    {
        Movement.instance.StartTask(task);
        HideContextMenu();
        StartCoroutine(startTask());
    }
    IEnumerator startTask()
    {
        yield return Movement.instance.currentTask;
        StopTaskPanel.SetActive(false);
    }
    private void CreateButton(string label, Action onClick, bool showUi = true)
    {
        GameObject btnObj = Instantiate(button, VoidPanelContent);
        btnObj.GetComponentInChildren<TMP_Text>().text = label;
        btnObj.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        if (showUi)
        {
            btnObj.GetComponent<Button>().onClick.AddListener(() => StopTaskPanel.GetComponentInChildren<TMP_Text>().text = "выполняю: " + label);
            btnObj.GetComponent<Button>().onClick.AddListener(() => StopTaskPanel.SetActive(true));
        }
        buttons.Add(btnObj);
    }

    void ClearButtons()
    {

    }
}
