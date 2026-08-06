using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LootBuildManager : MonoBehaviour
{
    public static LootBuildManager instance;
    [SerializeField] private Transform itemsParent;
    public LootSlot[] itemSlots;
    [SerializeField] private Transform enemysParent;
    public EnemySlot[] enemysSlots;
    [SerializeField] private Button useButton;

    [Header("Панель описания предмета локации")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDescription;

    [Header("Панель локации")]
    [SerializeField] private Image icon_locate;
    [SerializeField] private TMP_Text itemName_locate;
    [SerializeField] private TMP_Text itemDescription_locate;


    private void Start()
    {
        itemSlots = itemsParent.GetComponentsInChildren<LootSlot>();
        enemysSlots = enemysParent.GetComponentsInChildren<EnemySlot>();
        instance = this;
    }

    public void SetLocationPanel(string title, string description, Sprite sprite)
    {
        icon_locate.sprite = sprite;
        itemName_locate.text = title;
        itemDescription_locate.text = description;
    }

    public void ShowItemInfoPanel(string title, string description, Sprite sprite)
    {
        itemName.text = title;
        itemDescription.text = description;
        icon.sprite = sprite;
        panel.SetActive(true);
    }

    public void SetButtonListener(UnityAction action)
    {
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(action);
    }

    public void RefreshUI(List<Item> items, List<int> itemCounts)
    {
        int i = 0;
        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = items[i];
            itemSlots[i].SetCount(itemCounts[i]); // Обновляем количество предметов в слотах
        }
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
            itemSlots[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
    }
    public void RefreshEnemys(List<Enemy> enemys)
    {
        int i = 0;
        for (; i < enemys.Count && i < itemSlots.Length; i++)
        {
            enemysSlots[i].Item = enemys[i];
        }
        for (; i < itemSlots.Length; i++)
        {
            enemysSlots[i].Item = null;
        }
    }
    public void RefreshUI(List<ItemInstance> items)
    {
        int i = 0;
        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = items[i].Item;
            itemSlots[i].SetCount(items[i].Count); // Обновляем количество предметов в слотах
        }
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
            itemSlots[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
    }
    public void noRefresh()
    {
        int i = 0;
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
            itemSlots[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
    }
}