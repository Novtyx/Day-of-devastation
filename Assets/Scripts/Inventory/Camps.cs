using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Camps : MonoBehaviour
{
    public List<ItemInstance> items;                // Список предметов

    [SerializeField] Transform itemsParent;
    [SerializeField] CampSlot[] itemSlots;

    [SerializeField] Transform itemsParent1;
    [SerializeField] CampSlot[] itemSlots1;


    [Header("Item menu")]
    public GameObject itemMenu;
    public Image icon;
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public TMP_Text itemFeature;
    public Button useButton;
    public Button dropButton;
    public Button closeButton;
    [Space]

    public Slider Slider;
    public TMP_Text Slidertext;
    public GameObject SliderPanel;
    public Button DropButton;

    private CampSlot currentItemSlot;

    public int index;
    public bool IsIndex()
    {
        if (index == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SetIndex(int i)
    {
        index = i;
    }
    private void Start()
    {
    }
    public void yesCamp()
    {
        if (itemsParent != null)
        {
            itemSlots = itemsParent.GetComponentsInChildren<CampSlot>();
        }
        if (itemsParent1 != null)
        {
            itemSlots1 = itemsParent1.GetComponentsInChildren<CampSlot>();
        }
        RefreshUI();
    }
    public void noCamp()
    {
        noRefresh();
        if (itemsParent != null)
        {
            itemSlots = itemsParent.GetComponentsInChildren<CampSlot>();
        }
        if (itemsParent1 != null)
        {
            itemSlots1 = itemsParent1.GetComponentsInChildren<CampSlot>();
        }
        noRefresh();
        itemSlots = null;
        itemSlots1 = null;
    }

    public void RefreshUI()
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
        RefreshUIPickup();
    }
    public void noRefresh()
    {
        int i = 0;
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
            itemSlots[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
        noRefreshPickup();
    }
    public void RefreshUIPickup()
    {
        int i = 0;
        for (; i < items.Count && i < itemSlots1.Length; i++)
        {
            itemSlots1[i].Item = items[i].Item;
            itemSlots1[i].SetCount(items[i].Count); // Обновляем количество предметов в слотах
        }
        for (; i < itemSlots1.Length; i++)
        {
            itemSlots1[i].Item = null;
            itemSlots1[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
    }
    public void noRefreshPickup()
    {
        int i = 0;
        for (; i < itemSlots1.Length; i++)
        {
            itemSlots1[i].Item = null;
            itemSlots1[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
    }

    public void additem(Item newItem, int count)
    {
        AddItem(newItem, count, null);
    }

    public bool AddItem(Item newItem, int count, List<int> strength)
    {
        var item = items.Find(e => e.Item.ID == newItem.ID);

        // Проверяем, есть ли уже такой предмет в инвентаре
        if (item != null)
        {
            // Если предмет уже есть, увеличиваем его количество
            item.Count += count;
            if (strength != null && item.Item.isstrength)
            { 
                if (strength.Count != 0) item.Strength.AddRange(strength);
            }
            if (item.Count != item.Strength.Count && item.Item.isstrength)
            {
                int max = item.Count - item.Strength.Count;
                if (max > 0)
                {
                    for (int y = 0; y < max; y++)
                    {
                        item.Strength.Add(10);
                    }
                }
                else
                {
                    for (int y = 0; y < Mathf.Abs(max); y++)
                    {
                        item.Strength.RemoveAt(0);
                    }
                }
            }
            RefreshUI();
            return false;
        }

        // Если инвентарь полон, предмет не добавляется
        if (IsFull())
        {
            return false;
        }

        // Добавляем предмет как новый и устанавливаем его количество на 1
        ItemInstance itemInstance = new ItemInstance();
        itemInstance.Item = newItem;
        itemInstance.Count = count;
        itemInstance.Strength = new List<int>();
        if (strength != null && itemInstance.Item.isstrength) itemInstance.Strength = strength;
        if (itemInstance.Count != itemInstance.Strength.Count && itemInstance.Item.isstrength)
        {
            int max = itemInstance.Count - itemInstance.Strength.Count;
            for (int y = 0; y < max; y++)
            {
                itemInstance.Strength.Add(0);
            }
        }
        items.Add(itemInstance);
        RefreshUI();
        return true;
    }

    public bool RemoveItem(Item item, int count)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Item.ID == item.ID)
            {
                items[i].Count -= count; // Уменьшаем количество предмета
                if (items[i].Item.isstrength) items[i].Strength.RemoveRange(0, count);
                int max = items[i].Count - items[i].Strength.Count;
                if (max > 0)
                {
                    for (int y = 0; y < max; y++)
                    {
                        items[i].Strength.Add(10);
                    }
                }
                else
                {
                    for (int y = 0; y < Mathf.Abs(max); y++)
                    {
                        items[i].Strength.RemoveAt(0);
                    }
                }
                if (items[i].Count <= 0)
                {
                    // Если количество предметов равно нулю, удаляем предмет из инвентаря
                    items.RemoveAt(i);
                    itemMenu.SetActive(false);
                }
                RefreshUI();
                Inventory.instance.Raise();
                return true;
            }
        }
        return false;
    }

    public bool IsFull()
    {
        return items.Count >= itemSlots.Length;
    }



    public void ShowItemMenu(CampSlot campSlot)
    {
        currentItemSlot = null;
        currentItemSlot = campSlot;
        icon.sprite = campSlot.Item.icon;
        itemName.text = campSlot.Item.Name;
        itemDescription.text = campSlot.Item.Description;

        List<string> features = new List<string>();
        features.Add($"количество: {campSlot.count}");
        features.Add($"общий вес: {campSlot.Item.height * campSlot.count / 1000}кг {campSlot.Item.height * campSlot.count % 1000} г");
        features.Add("Характеристика:");
        if (campSlot.Item is FoodItem food)
        {
            if (food.eatingpower != 0)
            {
                features.Add("Голод: " + food.eatingpower);
            }
            if (food.wateringpower != 0)
            {
                features.Add("Жажда: " + food.wateringpower);
            }
            if (food.energyingpower != 0)
            {
                features.Add("Усталость: " + food.energyingpower);
            }
            if (food.radiationpower != 0)
            {
                features.Add("Радиация: " + food.radiationpower);
            }
            if (food.depaturepower != 0)
            {
                features.Add("Отравление: " + food.depaturepower);
            }
            if (food.bloodpower != 0)
            {
                features.Add("Кровотечение: " + food.bloodpower);
            }
            if (food.deathingpower != 0)
            {
                features.Add("Истощение: " + food.deathingpower);
            }
        }
        string feature = string.Join("\n", features);
        itemFeature.text = feature;

        itemMenu.SetActive(true);
        // Очищаем предыдущие слушатели событий
        useButton.onClick.RemoveAllListeners();
        dropButton.onClick.RemoveAllListeners();
        DropButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // Добавляем новые слушатели событий
        useButton.onClick.AddListener(OnUseButtonClicked);
        dropButton.onClick.AddListener(DropMenuShow);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    void OnUseButtonClicked()
    {
        currentItemSlot.Item.UseByCamp(this);
    }
    void DropMenuShow() // Меню со слайдером для выбрасывания предметов
    {
        SliderPanel.SetActive(true);
        SliderPanel.GetComponentInChildren<TMP_Text>().text = "Поднять предметы";
        Slider.maxValue = currentItemSlot.count;

        Slider.minValue = 1f;
        Slider.wholeNumbers = true;
        DropButton.onClick.RemoveAllListeners();
        DropButton.onClick.AddListener(OnDropButtonClicked);
    }

    void OnDropButtonClicked()
    {
        int count = (int)Slider.value;
        List<int> strength = new();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Item.ID == currentItemSlot.Item.ID && items[i].Item.isstrength) strength = items[i].Strength.GetRange(0, count);
        }
        Inventory.instance.AddItem(currentItemSlot.Item, count, strength);
        RemoveItem(currentItemSlot.Item, count);

        if (items.Count == 0)
        {
            Inventory.instance.itemsInCamps.Remove(index.ToString());
            Inventory.instance.campIds.Remove(index);
            Inventory.instance.id.Remove(index);
            Inventory.instance.x.Remove(transform.position.x);
            Inventory.instance.y.Remove(transform.position.y);
            Destroy(this.gameObject);
            Inventory.instance.SaveInventory();
        }
        SliderPanel.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        itemMenu.SetActive(false);
    }

    private void OnDisable()
    {
        useButton.onClick.RemoveListener(OnUseButtonClicked);
        dropButton.onClick.RemoveListener(OnDropButtonClicked);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        currentItemSlot = null;
    }
}