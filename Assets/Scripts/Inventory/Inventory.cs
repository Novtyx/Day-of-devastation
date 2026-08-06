using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

[System.Serializable]
public class ItemInstance
{
    public Item Item;
    public int Count;
    public List<int> Strength;
}
public struct ItemInstanceID
{
    public int ItemId;
    public int Count;
    public List<int> Strength;
}
public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public LuaModsManager LuaModsManager;

    public List<ItemInstance> items = new();                // Список предметов

    public List<int> id;
    public List<float> x;
    public List<float> y;
    public List<int> campIds;
    public Dictionary<string, List<ItemInstanceID>> itemsInCamps = new();

    public List<Item> maymeitems;                // Список предметов
    public Dictionary<string, ItemInstanceID> clothesitems = new();
    [System.Serializable]
    public class InventoryData
    {
        public List<ItemInstanceID> items = new();
        public List<int> id = new();
        public List<float> x = new();
        public List<float> y = new();
        public List<int> campIds = new();
        public Dictionary<string, List<ItemInstanceID>> itemsInCamps = new();

        public Dictionary<string, ItemInstanceID> clothesitems = new();
    }

    [SerializeField] Transform itemsParent;
    [SerializeField] ItemSlot[] itemSlots;
    public static Camps CampGive;
    public static TopDownGenerator TownGive;
    [SerializeField] LootBuild LootGive;
    private string filePath;

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

    public GameObject camp;

    private ItemSlot currentItemSlot;
    private ClothesSlot currentClothesSlot;

    public static bool isCamp = false;
    public static bool isLocation = false; // Находится ли на какой-нибудь локации
    public static bool isLoot = false;
    public static bool isTown = false;
    public static bool isSave = false;
    public static bool isRaise = false;
    public static bool isLoad = false;

    private int idCamp = 1;

    [SerializeField] Transform clothesParent;
    [SerializeField] ClothesSlot[] clothesSlots;
    public Button snatbutton;
    public Button closeButtonclothes;
    public GameObject clothesMenu;
    public Image clothicon;
    public Text clothitemName;
    public Text clothitemDescription;
    private bool isStart = false;
    [Space]
    public Image TownImage;
    public Sprite TownSprite;
    [Header("Pickup")]
    public RectTransform pickupPanel;
    public bool PickupIsTrue = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStart)
        {
            if (collision.CompareTag("Camp"))
            {
                if (PickupIsTrue)
                {
                    pickupPanel.gameObject.SetActive(true);
                    PickupManager.instance.isCamp = true;
                    PickupManager.instance.target = collision.transform;
                }
            }
            if (collision.CompareTag("Enemy"))
            {
                collision.GetComponent<EnemyEntity>().StartAttack();
            }
            if (collision.CompareTag("Location"))
            {
                PickupManager.instance.isLocation = true;
                PickupManager.instance.locationtarget = collision.transform;
                EventManager.instance.StartEvent1(collision.GetComponent<DynamicEvent>().Event, collision.gameObject);
                PickupManager.instance.locationPanel.gameObject.SetActive(true);
            }
            if (collision.CompareTag("Town"))
            {
                if (isLoad == false) { isLoad = true; }
                isTown = true;
                TownImage.sprite = TownSprite;
                TownGive = collision.GetComponent<TopDownGenerator>();
                if (!TownGive.isGenerated)
                {
                    TownGive.GenerateCity();
                    TownGive.isGenerated = true;
                }
                TownGive.fog.SetActive(false);
                TownGive.floor.gameObject.SetActive(true);
                TownGive.isActive = true;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Camp"))
        {
            CampGive = collision.GetComponent<Camps>();
            CampGive.yesCamp();
            isCamp = true;
        }
        if (collision.CompareTag("Loot"))
        {
            LootGive = collision.GetComponent<LootBuild>();
            isLoot = true;
            LootGive.yesLoot();
        }
        if (collision.CompareTag("Fire"))
        {
            Characters.isFire = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Camp"))
        {
            isCamp = false;
            CampGive = collision.GetComponent<Camps>();
            CampGive.noCamp();
            pickupPanel.gameObject.SetActive(false);
            PickupManager.instance.isCamp = false;
        }
        if (collision.CompareTag("Loot"))
        {
            LootGive = collision.GetComponent<LootBuild>();
            isLoot = false;
            LootGive.noLoot();
        }
        if (collision.CompareTag("Town"))
        {
            TownGive = collision.GetComponent<TopDownGenerator>();
            isTown = false;
            TownGive.fog.SetActive(true);
            TownGive.floor.gameObject.SetActive(false);
            TownGive.isActive = false;
            Movement.instance.tile = null;
        }
        if (collision.CompareTag("Location"))
        {
            PickupManager.instance.isLocation = false;
            Animations.instance.HideSecretLocationfromMap(EventManager.instance.locationbutton.gameObject);
        }
        if (collision.CompareTag("Fire"))
        {
            Characters.isFire = false;
        }
    }
    private void Update()
    {
        if (isCamp)
        {

        }
    }
    public void OnSlotClick(CampSlot campSlot)
    {
        CampGive.ShowItemMenu(campSlot);
    }
    public void OnSlotClick1(LootSlot lootSlot)
    {
        if (LootGive)
        {
            LootGive.ShowItemMenu(lootSlot);
        }
        else
        {
            Movement.instance.ShowItemMenu(lootSlot);
        }
    }
    public void OnSlotClick2(CraftSlot campSlot)
    {
        Craft.instance.ShowItemMenu(campSlot);
    }
    public void OnSlotClick2(AttackSlot campSlot)
    {
        AttackManager.instance.ShowItemMenu(campSlot);
    }

    //Обновление айтемов
    int category = 0;
    void RefreshUI()
    {
        int totalweight = 0;
        for (int n = 0; n < items.Count; n++)
        {
            totalweight += items[n].Count * items[n].Item.height;
        }
        Characters.instance.currentheight = totalweight;
        if (category == 1)
        {
            List<Item> items1 = new List<Item>();
            List<int> counts = new List<int>();
            for (int y = 0; y < items.Count && y < itemSlots.Length; y++)
            {
                if (items[y].Item is FoodItem foodItem && foodItem.eatingpower != 0)
                {
                    items1.Add(foodItem);
                    counts.Add(items[y].Count);
                }
            }
            int x = 0;
            for (; x < items1.Count && x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = items1[x];
                if (items1[x].isstrength)
                {
                    itemSlots[x].SetCount(Mathf.CeilToInt(counts[x] / items1[x].strengtcount) + 1); // Обновляем количество предметов в слотах
                }
                else
                {
                    itemSlots[x].SetCount(counts[x]); // Обновляем количество предметов в слотах
                }
            }
            for (; x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = null;
                itemSlots[x].SetCount(0); // Устанавливаем ноль в пустых слотах
            }
            return;
        }
        if (category == 2)
        {
            List<Item> items1 = new List<Item>();
            List<int> counts = new List<int>();
            for (int y = 0; y < items.Count && y < itemSlots.Length; y++)
            {
                if (items[y].Item is FoodItem foodItem && foodItem.wateringpower != 0)
                {
                    items1.Add(foodItem);
                    counts.Add(items[y].Count);
                }
            }
            int x = 0;
            for (; x < items1.Count && x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = items1[x];
                itemSlots[x].SetCount(counts[x]); // Обновляем количество предметов в слотах
            }
            for (; x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = null;
                itemSlots[x].SetCount(0); // Устанавливаем ноль в пустых слотах
            }
            return;
        }
        if (category == 3)
        {
            List<Item> items1 = new List<Item>();
            List<int> counts = new List<int>();
            for (int y = 0; y < items.Count && y < itemSlots.Length; y++)
            {
                if (items[y].Item is ClothItem clothItem)
                {
                    items1.Add(clothItem);
                    counts.Add(items[y].Count);
                }
            }
            int x = 0;
            for (; x < items1.Count && x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = items1[x];
                itemSlots[x].SetCount(counts[x]); // Обновляем количество предметов в слотах
            }
            for (; x < itemSlots.Length; x++)
            {
                itemSlots[x].Item = null;
                itemSlots[x].SetCount(0); // Устанавливаем ноль в пустых слотах
            }
            return;
        }
        int i = 0;
        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = items[i].Item;
            itemSlots[i].SetCount(items[i].Count); // Обновляем количество предметов в слотах
            Characters.instance.height_text.text = $"Вес: {Characters.instance.height} / {Characters.instance.maxheight / 1000} кг";
        }
        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].Item = null;
            itemSlots[i].SetCount(0); // Устанавливаем ноль в пустых слотах
        }
        if (items.Count < 1) Characters.instance.height_text.text = $"Вес: 0 / {Characters.instance.maxheight / 1000} кг";
        SaveInventory();
        Debug.Log(Characters.instance.currentheight);
    }

    public void SetCategory(int index)
    {
        category = index;
        RefreshUI();
    }

    public void additem(Item newItem)
    {
        AddItem(newItem, 1, null);
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
                Debug.Log($"{item.Count} / {item.Strength.Count}");
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
                itemInstance.Strength.Add(10);
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
                return true;
            }
        }
        return false;
    }

    public bool IsFull()
    {
        return items.Count >= itemSlots.Length;
    }

    public int GetTotalStrength(Item item) // получить прочности всех нужных предметов
    {
        int total = 0;
        foreach (var needitem in items)
        {
            if (needitem.Item.ID == item.ID)
            {
                foreach (int s in needitem.Strength)
                {
                    total += s;
                }
            }
        }
        return total;
    }

    public bool HasEnoughResources(Item item, int amount)
    {
        if (item.isstrength)
        {
            return GetTotalStrength(item) >= amount;
        }
        int count = 0;
        foreach (var needitem in items)
        {
            if (needitem.Item.ID == item.ID)
            {
                count += needitem.Count;
            }
        }
        return count >= amount;
    }

    public void RemoveResourcesForCraft(Item item, int amount)
    {
        int remaing = amount;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (remaing <= 0) break;
            if (items[i].Item.ID != item.ID) continue;

            if (item.isstrength)
            {
                for (int j = items[i].Strength.Count - 1; j >= 0; j--)
                {
                    if (remaing <= 0) break;
                    if (items[i].Strength[j] > remaing) 
                    {
                        items[i].Strength[j] -= remaing;
                        remaing = 0;
                    }
                    else
                    {
                        remaing -= items[i].Strength[j];
                        items[i].Strength.RemoveAt(j);
                        items[i].Count--;
                    }
                }
            }
            else
            {
                int take = Mathf.Min(items[i].Count, remaing);
                items[i].Count -= take;
                remaing -= take;
                if (items[i].Count < items[i].Strength.Count)
                {
                    items[i].Strength.RemoveRange(0, items[i].Strength.Count - items[i].Count);
                }
            }
            if (items[i].Count <= 0) items.RemoveAt(i);
        }
        RefreshUI();
        SaveInventory();
    }

    private void Start()
    {
        if (itemsParent != null)
        {
            itemSlots = itemsParent.GetComponentsInChildren<ItemSlot>();
        }
        if (clothesParent != null)
        {
            clothesSlots = clothesParent.GetComponentsInChildren<ClothesSlot>();
        }
        Debug.Log(LuaModsManager.isLoaded);
        // Устанавливаем слоты для одежды
        ItemInstanceID item = new ItemInstanceID();
        clothesitems.Add("0", item);
        clothesitems.Add("1", item);
        clothesitems.Add("2", item);
        clothesitems.Add("3", item);
        clothesitems.Add("4", item);
        clothesitems.Add("5", item);
        clothesitems.Add("6", item);
        idCamp = PlayerPrefs.GetInt("idCamp");
        filePath = Path.Combine(Application.persistentDataPath, "inventory.json");
        EnsureFileExists();
        Debug.Log(maymeitems.Count);
        LoadInventory();
        isStart = true;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    [SerializeField] private Transform ActionsPanel;

    [SerializeField] private Button ActionButton;
    public void InitializeCustomIvents(List<CustomAction> actions)
    {
        Button[] tradingObjects = ActionsPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < tradingObjects.Length; i++)
        {
            Destroy(tradingObjects[i].gameObject);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            Button obj = Instantiate(ActionButton, ActionsPanel);
            obj.gameObject.GetComponentInChildren<TMP_Text>().text = actions[i].ActionName;
            obj.onClick.RemoveAllListeners();
            CustomAction action = actions[i];
            obj.onClick.AddListener(() => action.Use());
        }
    }

    public void ShowItemMenu(ItemSlot itemSlot)
    {
        currentItemSlot = itemSlot;
        icon.sprite = itemSlot.Item.icon;
        itemName.text = itemSlot.Item.Name;
        itemDescription.text = itemSlot.Item.Description;
        InitializeCustomIvents(itemSlot.Item.CustomActions);
        List<string> features = new List<string>();
        features.Add($"количество: {itemSlot.count}");
        features.Add($"общий вес: {itemSlot.Item.height * itemSlot.count / 1000}кг {(itemSlot.Item.height * itemSlot.count) % 1000} г");
        features.Add("Характеристика:");
        if (itemSlot.Item is FoodItem food)
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
        currentItemSlot.Item.Use();
    }

    void DropMenuShow() // Меню со слайдером для выбрасывания предметов
    {
        SliderPanel.SetActive(true);
        SliderPanel.GetComponentInChildren<TMP_Text>().text = "Выбросить предметы";
        Slider.maxValue = currentItemSlot.count;

        Slider.minValue = 1f;
        Slider.wholeNumbers = true;
        DropButton.onClick.RemoveAllListeners();
        DropButton.onClick.AddListener(OnDropButtonClicked);
    }
    void OnDropButtonClicked()
    {
        int count = (int) Slider.value;
        if (isCamp == false)
        {
            Instantiate(camp, gameObject.transform.position, Quaternion.identity).SetActive(true);
            isSave = true;
        }
        else
        {
            List<int> strength = new();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Item.ID == currentItemSlot.Item.ID && items[i].Item.isstrength) strength = items[i].Strength.GetRange(0, count);
            }
            CampGive.AddItem(currentItemSlot.Item, count, strength);
            RemoveItem(currentItemSlot.Item, count);
            if (isSave == true)
            {
                SaveCamp();
            }
            Raise();
            SliderPanel.SetActive(false);
        }
    }
    public void SaveCamp()
    {
        this.id.Add(idCamp);
        this.x.Add(CampGive.gameObject.transform.position.x);
        this.y.Add(CampGive.gameObject.transform.position.y);
        this.campIds.Add(idCamp);
        itemsInCamps[idCamp.ToString()] = new();
        isSave = false;
        CampGive.SetIndex(idCamp);
        idCamp++;
        PlayerPrefs.SetInt("idCamp", idCamp);
    }
    public void ChangeSlider(float v)
    {
        int value = (int)v;
        Slidertext.text = value.ToString();
    }
    public void Raise() // Обновляем предметы в лагере
    {
        List<ItemInstanceID> tt = new();
        foreach (var y in CampGive.items)
        {
            ItemInstanceID item = new ItemInstanceID();
            item.ItemId = y.Item.ID;
            item.Count = y.Count;
            item.Strength = y.Strength;
            tt.Add(item);
        }
        itemsInCamps[CampGive.index.ToString()] = tt;

        SaveInventory();
    }
    public void RaiseBuilding()
    {
        List<ItemInstanceID> tt = new();
        foreach (var y in LootGive.items)
        {
            ItemInstanceID item = new ItemInstanceID();
            item.ItemId = y.Item.ID;
            item.Count = y.Count;
            tt.Add(item);
        }
        SaveInventory();
    }
    public void LoadCamp()
    {
        if (isCamp == false)
        {
            Instantiate(camp, gameObject.transform.position, Quaternion.identity).SetActive(true);
            isSave = true;
        }
    }
    public void Looting(Item item, int count, List<int> strength = null) // Добавить предмет из биома или здания в лагерь
    {
        CampGive.AddItem(item, count, strength);
        if (isSave == true)
        {
            this.id.Add(idCamp);
            this.x.Add(CampGive.gameObject.transform.position.x);
            this.y.Add(CampGive.gameObject.transform.position.y);
            this.campIds.Add(idCamp);
            itemsInCamps[idCamp.ToString()] = new();
            isSave = false;
            CampGive.SetIndex(idCamp);
            idCamp++;
            PlayerPrefs.SetInt("idCamp", idCamp);
        }
    }

    public void OnCloseButtonClicked()
    {
        currentItemSlot = null;
        itemMenu.SetActive(false);
    }

    private void OnDisable()
    {
        useButton.onClick.RemoveListener(OnUseButtonClicked);
        dropButton.onClick.RemoveListener(OnDropButtonClicked);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        currentItemSlot = null;
    }
    private void EnsureFileExists()
    {
        if (!File.Exists(filePath))
        {
            InventoryData data = new InventoryData();
            ItemInstanceID item = new ItemInstanceID();
            data.clothesitems.Add("0", item);
            data.clothesitems.Add("1", item);
            data.clothesitems.Add("2", item);
            data.clothesitems.Add("3", item);
            data.clothesitems.Add("4", item);
            data.clothesitems.Add("5", item);
            data.clothesitems.Add("6", item);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(filePath, json);
        }
    }

    public void SaveInventory()
    {
        InventoryData data = new InventoryData();
        int i = 0;
        for (; i < this.items.Count; i++)
        {
            ItemInstanceID item = new ItemInstanceID();
            item.ItemId = items[i].Item.ID;
            item.Count = items[i].Count;
            item.Strength = items[i].Strength;
            data.items.Add(item);
        }

        data.id = this.id;
        data.x = this.x;
        data.y = this.y;
        data.campIds = this.campIds;
        data.itemsInCamps = this.itemsInCamps;
        data.clothesitems = this.clothesitems;
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(filePath, json);
    }
    public void LoadInventory()
    {
        if (File.Exists(filePath))
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<InventoryData>(json, settings);

            if (data != null)
            {
                StartCoroutine(LoadItems(data));
                Debug.Log("hhh");
                this.id = data.id;
                this.x = data.x;
                this.y = data.y;
                this.campIds = data.campIds;
                this.itemsInCamps = data.itemsInCamps;

                // Восстановление объектов лагерей
                StartCoroutine(LoadCampItems(data));
                Debug.Log(data.campIds.Count);
            }
        }
    }
    IEnumerator LoadCampItems(InventoryData data)
    {
        yield return new WaitUntil(() => LuaModsManager.isLoaded);

        this.clothesitems = data.clothesitems;
        RefreshClothes();

        for (int n = 0; n < data.campIds.Count; n++)
        {
            GameObject camps = Instantiate(camp, new Vector2(data.x[n], data.y[n]), Quaternion.identity);

            camps.GetComponent<Camps>().index = data.campIds[n];
            int ttu = camps.GetComponent<Camps>().index;
            List<ItemInstanceID> itemsin = data.itemsInCamps[ttu.ToString()];
            for (int g = 0; g < itemsin.Count; g++)
            {
                foreach (Item it1 in maymeitems)
                {
                    if (itemsin[g].ItemId == it1.ID)
                    {
                        ItemInstance item = new ItemInstance();
                        item.Item = it1;
                        item.Count = itemsin[g].Count;
                        item.Strength = itemsin[g].Strength;
                        camps.GetComponent<Camps>().items.Add(item);
                    }
                }
            }

            camps.GetComponent<SpriteRenderer>().sortingOrder = 110;
            camps.SetActive(true);
            RefreshUI();
        }
    }
    IEnumerator LoadItems(InventoryData data)
    {
        yield return new WaitUntil(() => LuaModsManager.isLoaded);
        for (int i = 0; i < data.items.Count; i++)
        {
            foreach (Item it in maymeitems)
            {
                if (data.items[i].ItemId == it.ID)
                {
                    AddItem(it, data.items[i].Count, data.items[i].Strength);
                }
            }
        }
    }
    public void ClothesManager(string position, int count, int strength, Item item)
    {
        foreach (Item it in maymeitems)
        {
            if (clothesitems[position].ItemId == it.ID)
            {
                List<int> strength1 = new();
                strength1.Add(strength);
                AddItem(it, count, strength1);
            }
        }
        ItemInstanceID itemID = new ItemInstanceID();
        itemID.ItemId = item.ID;
        itemID.Count = count;
        List<int> strength2 = new();
        strength2.Add(strength);
        itemID.Strength = strength2;
        clothesitems[position] = itemID;
        RefreshClothes();
    }
    public void RefreshClothes()
    {
        // Cловарь для быстрого доступа к предметам по ID
        Dictionary<string, Item> itemDictionary = new Dictionary<string, Item>();
        foreach (Item item in maymeitems)
        {
            itemDictionary[item.ID.ToString()] = item;
        }

        // Обновляем слоты
        for (int i = 0; i < clothesSlots.Length; i++)
        {
            if (clothesitems.ContainsKey(i.ToString()))
            {
                string itemId = clothesitems[i.ToString()].ItemId.ToString();
                if (itemDictionary.ContainsKey(itemId))
                {
                    clothesSlots[i].Item = itemDictionary[itemId];
                    if (clothesSlots[i].Item is ClothItem item)
                    {
                        Characters.instance.radiation_protect += item.protection_radiation;
                        Characters.instance.protection += item.protection;
                        Characters.instance.body_covering += item.body_covering;
                        Movement.speed += item.speed;
                    }
                }
                else
                {
                    clothesSlots[i].Item = null; // Одежда не найдена
                    Debug.LogWarning($"Item with ID {itemId} not found!");

                }
            }
            else
            {
                clothesSlots[i].Item = null;
            }
        }
        Characters.instance.Refresh();
        SaveInventory();
    }
    public void showclothesmenu(ClothesSlot itemSlot)
    {
        currentClothesSlot = itemSlot;
        clothicon.sprite = itemSlot.Item.icon;
        clothitemName.text = itemSlot.Item.Name;
        clothitemDescription.text = itemSlot.Item.Description;

        clothesMenu.SetActive(true);

        snatbutton.onClick.RemoveAllListeners();
        closeButtonclothes.onClick.RemoveAllListeners();

        snatbutton.onClick.AddListener(onSnatButtonClicked);
        closeButtonclothes.onClick.AddListener(OnCloseButtonClickedClothes);
    }
    public void OnCloseButtonClickedClothes()
    {
        currentItemSlot = null;
        clothesMenu.SetActive(false);
    }
    public void onSnatButtonClicked()
    {
        if (currentClothesSlot.Item is ClothItem item)
        {
            Characters.instance.radiation_protect -= item.protection_radiation;
            Characters.instance.protection -= item.protection;
            Characters.instance.body_covering -= item.body_covering;

            Movement.speed -= item.speed;

            AddItem(currentClothesSlot.Item, clothesitems[item.GetSlotId(item.clothesname).ToString()].Count, clothesitems[item.GetSlotId(item.clothesname).ToString()].Strength);
            ItemInstanceID item1 = new ItemInstanceID();
            clothesitems[item.GetSlotId(item.clothesname).ToString()] = item1;
        }
        RefreshUI();
        RefreshClothes();
        clothesMenu.SetActive(false);
    }
}