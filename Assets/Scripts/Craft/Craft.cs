using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Craft : MonoBehaviour
{
    public static Craft instance;
    //public LuaModsManager LuaModsManager;

    [SerializeField] Transform itemsParent;
    [SerializeField] CraftSlot[] itemSlots;
    [SerializeField] Transform needParent;
    [SerializeField] NeedSlot[] needSlots;

    public GameObject itemMenu;
    public Image icon;
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public Button useButton;
    public Button useButtonx5;
    public Button useButtonx10;
    public Button useButtonx50;
    public Button closeButton;
    [SerializeField] private Color SuccesFull;
    [SerializeField] private Color Reject;

    public Slider Slider;
    public Text Slidertext;

    private CraftSlot currentItemSlot;
    public List<Item> items;                // Список предметов
    public List<Item> needsitems;
    public List<int> needsitemscounts;

    private void OnValidate()
    {
        RefreshUI();
    }
    private void Start()
    {
        instance = this;
        if (itemsParent != null)
        {
            itemSlots = itemsParent.GetComponentsInChildren<CraftSlot>();
            needSlots = needParent.GetComponentsInChildren<NeedSlot>();
        }
        StartCoroutine(LoadCrafts());
        RefreshUI();
    }
    IEnumerator LoadCrafts()
    {
        yield return null; // new WaitUntil(() => LuaModsManager.isLoaded);

        for (int i = 0; i < Inventory.instance.maymeitems.Count; i++)
        {
            if (Inventory.instance.maymeitems[i].isCrafting)
            {
                AddItem(Inventory.instance.maymeitems[i], Inventory.instance.maymeitems[i].craftingCount);
            }
        }
    }
    public void RefreshUI()
    {
        int i = 0;
        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = items[i];
        }
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
        }
    }
    public void RefreshneedUI()
    {
        for (int i = 0; i < needsitems.Count; i++)
        {
            needSlots[i].Item = needsitems[i];
            for (int x = 0; x < Inventory.instance.items.Count; x++)
            {
                if (Inventory.instance.items[x].Item.name == needSlots[i].Item.name)
                {
                    needSlots[i].SetCount(needsitemscounts[i], Inventory.instance.items[x].Count); // Обновляем количество предметов в слотах
                    break;
                }
                needSlots[i].SetCount(needsitemscounts[i], 0);
            }
            if (Inventory.instance.items == null) needSlots[i].SetCount(needsitemscounts[i], 0);
        }
    }
    public void noRefresh()
    {
        int i = 0;
        for (; i < needSlots.Length; i++)
        {
            needSlots[i].Item = null;
            needSlots[i].SetCount(0, 0); // Обновляем количество предметов в слотах
        }
    }

    public bool AddItem(Item newItem, int count)
    {
        // Проверяем, есть ли уже такой предмет в инвентаре
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Name == newItem.Name)
            {
                // Если предмет уже есть, увеличиваем его количество
                RefreshUI();
                return false;
            }
        }

        // Если инвентарь полон, предмет не добавляется
        if (IsFull())
        {
            return false;
        }

        // Добавляем предмет как новый и устанавливаем его количество на 1
        items.Add(newItem);
        RefreshUI();
        return true;
    }
    public bool AddNeedsItem(Item newItem, int count)
    {
        // Проверяем, есть ли уже такой предмет в инвентаре
        for (int i = 0; i < needsitems.Count; i++)
        {
            if (needsitems[i].Name == newItem.Name)
            {
                // Если предмет уже есть, увеличиваем его количество
                RefreshneedUI();
                return false;
            }
        }

        // Если инвентарь полон, предмет не добавляется
        if (IsFull())
        {
            return false;
        }

        // Добавляем предмет как новый и устанавливаем его количество на 1
        needsitems.Add(newItem);
        RefreshneedUI();
        Debug.Log("CraftUpdateted");
        return true;
    }


    public bool IsFull()
    {
        return items.Count >= itemSlots.Length;
    }



    public void ShowItemMenu(CraftSlot campSlot)
    {
        noRefresh();
        currentItemSlot = null;
        currentItemSlot = campSlot;
        icon.sprite = campSlot.Item.icon;
        itemName.text = campSlot.Item.Name;
        itemDescription.text = campSlot.Item.Description;
        needsitems = null;
        needsitemscounts = null;
        needsitems = campSlot.needsitems;
        needsitemscounts = campSlot.needsitemscounts;
        craftingTime = campSlot.craftTime;

        itemMenu.SetActive(true);
        // Очищаем предыдущие слушатели событий
        useButton.onClick.RemoveAllListeners();
        useButtonx5.onClick.RemoveAllListeners();
        useButtonx10.onClick.RemoveAllListeners();
        useButtonx50.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // Добавляем новые слушатели событий
        useButton.onClick.AddListener(() => OnUseButtonClicked(1));
        useButtonx5.onClick.AddListener(() => OnUseButtonClicked(5));
        useButtonx10.onClick.AddListener(() => OnUseButtonClicked(10));
        useButtonx50.onClick.AddListener(() => OnUseButtonClicked(50));
        SetButtonColor(useButton, 1);
        SetButtonColor(useButtonx5, 5);
        SetButtonColor(useButtonx10, 10);
        SetButtonColor(useButtonx50, 50);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        for (int i = 0; i < needsitems.Count; i++)
        {
            AddNeedsItem(needsitems[i], needsitemscounts[i]);
        }
    }
    private void SetButtonColor(Button button, int count)
    {
        if (CanCraftMultiplied(count)) button.GetComponent<Image>().color = SuccesFull;
        else button.GetComponent<Image>().color = Reject;
    }

    public void ShowNeedItemInfo(NeedSlot needSlot)
    {
        NotifyManager.instance.SetMinNotify(needSlot.Item.Name);
    }

    void OnUseButtonClicked(int count = 1)
    {
        if (currentItemSlot.Item.needFire)
        {
            if (!Characters.isFire) 
            {
                NotifyManager.instance.SetNotify("необходим огонь");
                return; 
            };
        }
        if (CanCraftMultiplied(count))
        {
            StartCraft(count);
        }
        else NotifyManager.instance.SetNotify("Недостаточно ресурсов");
    }
    public GameObject sliderpanel;
    public Slider slider;
    public bool iscreating;
    public float craftingTime;
    public bool CanCraftMultiplied(int count)
    {
        if (currentItemSlot == null) return false;
        for(int i = 0; i < needsitems.Count; i++)
        {
            int totalRequired = needsitemscounts[i] * count;
            Debug.Log(totalRequired);
            if (!Inventory.instance.HasEnoughResources(needsitems[i], totalRequired)) return false;
        }
        return true;
    }
    public void StartCraft(int count)
    {
        float time = craftingTime * count;
        TimeLineManager.instance.StartTime(time, () =>
        {
            AllItems(count);
        });
    }
    void AllItems(int count)
    {

        for (int g = 0; g < needsitems.Count; g++)
        {
            Inventory.instance.RemoveResourcesForCraft(needsitems[g], needsitemscounts[g] * count);
        }

        int countitems = 1;
        for (int i = 0; i < items.Count; i++)
        {
            if (currentItemSlot.Item.ID == items[i].ID)
            {
                countitems = items[i].craftingCount * count;
            }
        }

        List<int> strength = new List<int>(countitems);
        for (int i = 0; i < countitems; i++)
        {
            strength.Add(currentItemSlot.Item.strengtcount);
        }
        Inventory.instance.AddItem(currentItemSlot.Item, countitems, strength);

        RefreshneedUI();
        ShowItemMenu(currentItemSlot);

        NotifyManager.instance.SetMinNotify("крафт закончен. создано: " + count);
        Debug.Log("Crafting complete!");

    }
    public void OnCloseButtonClicked()
    {
        itemMenu.SetActive(false);
    }

    private void OnDisable()
    {
        useButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        currentItemSlot = null;
    }
}