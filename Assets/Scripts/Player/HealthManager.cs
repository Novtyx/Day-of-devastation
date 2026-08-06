using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;

    public int eatingpower;
    public int wateringpower;
    public int energyingpower;
    public int deathingpower;
    public int radiationpower;
    public int depaturepower;
    public int bloodpower;
    public List<Disease> diseases;
    public List<string> diseasesInt;
    public List<int> diseasesHours;
    private string filePath;
    public Transform diseasParent;
    public DiseaseSlot diseaseSlot;

    public GameObject panel;
    public TMP_Text title;
    [SerializeField] Transform needParent;
    [SerializeField] NeedSlot[] needSlots;

    public TMP_Text Description;
    public Slider TimeSlider;
    public Button removeButton;

    private DiseaseSlot currentSlot;
    private string currentSlotName;

    public class DiseaseData
    {
        public List<string> diseases = new List<string>();
        public List<int> diseasesHours = new List<int>();
    }
    public List<Disease> MaybeDeseases = new List<Disease>();
    private List<Item> needsitems;
    private List<int> needsitemscounts;
    public void ShowItemMenu(DiseaseSlot itemSlot)
    {
        int g = 0;
        for (; g < needSlots.Length; g++)
        {
            needSlots[g].Item = null;
            needSlots[g].SetCount(0, 0); // Обновляем количество предметов в слотах
        }
        title.text = itemSlot.Item.Name;
        needsitems = itemSlot.Item.items;
        needsitemscounts = itemSlot.Item.itemsCounts;
        for (int i = 0; i < itemSlot.Item.items.Count; i++)
        {
            needSlots[i].Item = itemSlot.Item.items[i];
            for (int x = 0; x < Inventory.instance.items.Count; x++)
            {
                if (Inventory.instance.items[x].Item.name == needSlots[i].Item.name)
                {
                    needSlots[i].SetCount(itemSlot.Item.itemsCounts[i], Inventory.instance.items[x].Count); // Обновляем количество предметов в слотах
                    break;
                }
                needSlots[i].SetCount(itemSlot.Item.itemsCounts[i], 0);
            }
        }
        List<string> features = new List<string>();
        features.Add("Воздействие:");
        if (itemSlot.Item.iseating)
        {
            features.Add("Голод: " + itemSlot.Item.eatingpower);
        }
        if (itemSlot.Item.iswatering)
        {
            features.Add("Жажда: " + itemSlot.Item.wateringpower);
        }
        if (itemSlot.Item.isenergy)
        {
            features.Add("Усталость: " + itemSlot.Item.energyingpower);
        }
        if (itemSlot.Item.isradiation)
        {
            features.Add("Радиация: " + itemSlot.Item.radiationpower);
        }
        if (itemSlot.Item.isdepature)
        {
            features.Add("Отравление: " + itemSlot.Item.depaturepower);
        }
        if (itemSlot.Item.isblood)
        {
            features.Add("Кровотечение: " + itemSlot.Item.bloodpower);
        }
        if (itemSlot.Item.isdeath)
        {
            features.Add("Истощение: " + itemSlot.Item.deathingpower);
        }
        string feature = string.Join("\n", features);
        Description.text = feature;
        currentSlot = itemSlot;
        for (int i = 0; i < diseasesInt.Count; i++)
        {
            if (diseasesInt[i] == itemSlot.Item.Name)
            {
                TimeSlider.maxValue = itemSlot.Item.timeHours;
                TimeSlider.value = diseasesHours[i];
            }
        }
        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(RemoveDisease);
        panel.SetActive(true);
    }
    public void CloseInfoPanel()
    {
        panel.SetActive(false);
    }
    private void Start()
    {
        instance = this;
        needSlots = needParent.GetComponentsInChildren<NeedSlot>();
        filePath = Path.Combine(Application.persistentDataPath, "diseas.json");
        EnsureFileExists();
        LoadData();
        for (int i = 0; i < diseases.Count; i++)
        {
            eatingpower += diseases[i].eatingpower;
            wateringpower += diseases[i].wateringpower;
            energyingpower += diseases[i].energyingpower;
            deathingpower += diseases[i].deathingpower;
            radiationpower += diseases[i].radiationpower;
            depaturepower += diseases[i].depaturepower;
            bloodpower += diseases[i].bloodpower;
            GameObject obj = Instantiate(diseaseSlot.gameObject, diseasParent);
            obj.GetComponent<DiseaseSlot>().Item = diseases[i];
        }
    }

    public void AddDisease(Disease disease)
    {
        for (int i = 0; i < diseases.Count; i++)
        {
            if (diseases[i].name == disease.name) return;
        }
        diseases.Add(disease);
        diseasesInt.Add(disease.Name);
        diseasesHours.Add(0);
        eatingpower += disease.eatingpower;
        wateringpower += disease.wateringpower;
        energyingpower += disease.energyingpower;
        deathingpower += disease.deathingpower;
        radiationpower += disease.radiationpower;
        depaturepower += disease.depaturepower;
        bloodpower += disease.bloodpower;
        SaveData();
        GameObject obj = Instantiate(diseaseSlot.gameObject, diseasParent);
        obj.GetComponent<DiseaseSlot>().Item = disease;
        NotifyManager.instance.SetPermanentNotify($"получена болезнь: {disease.Name}");
    }
    public void RemoveDisease()
    {
        int bol = 0;
        for (int i = 0; i < Inventory.instance.items.Count; i++)
        {
            for (int g = 0; g < needsitems.Count; g++)
            {
                if (needsitems[g].ID == Inventory.instance.items[i].Item.ID && Inventory.instance.items[i].Count >= needsitemscounts[g])
                {
                    Debug.Log("test");
                    bol++;
                }
            }
        }
        if (bol >= needsitems.Count)
        {
            Disease disease = currentSlot.Item;
            for (int i = 0; i < diseasesInt.Count; i++)
            {
                if (diseasesInt[i] == disease.Name)
                {
                    diseasesHours.RemoveAt(i);
                }
            }
            diseasesInt.Remove(disease.Name);
            diseases.Remove(disease);
            eatingpower -= disease.eatingpower;
            wateringpower -= disease.wateringpower;
            energyingpower -= disease.energyingpower;
            deathingpower -= disease.deathingpower;
            radiationpower -= disease.radiationpower;
            depaturepower -= disease.depaturepower;
            bloodpower -= disease.bloodpower;
            NotifyManager.instance.SetPermanentNotify($"прошла болезнь: {disease.Name}");
            SaveData();
            currentSlot.Item = null;
            for (int g = 0; g < needsitems.Count; g++)
            {
                Inventory.instance.RemoveItem(needsitems[g], needsitemscounts[g]);
            }
            panel.SetActive(false);
        }
        else NotifyManager.instance.SetNotify("Недостаточно ресурсов");
        bol = 0;

    }

    private void EnsureFileExists()
    {
        if (!File.Exists(filePath))
        {
            DiseaseData data = new DiseaseData();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(filePath, json);
        }
    }

    public void SaveData()
    {
        DiseaseData data = new DiseaseData();

        data.diseases = this.diseasesInt;
        data.diseasesHours = this.diseasesHours;

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(filePath, json);
    }

    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<DiseaseData>(json, settings);

            this.diseasesInt = data.diseases;
            this.diseasesHours = data.diseasesHours;
            for (int i = 0; i < data.diseases.Count; i++)
            {
                foreach (Disease it in MaybeDeseases)
                {
                    if (data.diseases[i] == it.Name)
                    {
                        diseases.Add(it);
                    }
                }
            }
        }
    }
}
