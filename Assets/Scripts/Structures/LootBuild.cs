using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootBuild : MonoBehaviour
{
    public List<ItemInstance> items;                // Список предметов
    [SerializeField] List<int> itemchances;            // Список для хранения количества предметов

    public string NameBuild;
    public string DescriptionBuild;
    public Sprite imageBuild;

    private LootSlot currentItemSlot;

    public int index;
    private void Start()
    {
        for (int i = 0; i < townsgenerator.instance.names.Count; i++)
        {
            if (NameBuild == townsgenerator.instance.names[i])
            {
                items = new List<ItemInstance>(townsgenerator.instance.items[i]);
                itemchances = new List<int>(townsgenerator.instance.itemchances[i]);
                for (int x = 0; x < items.Count; x++)
                {
                    int count = items[x].Count;
                    items[x].Count = Random.Range(1, count);
                }
                return;
            }
        }
        FillItems();
    }

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

    public void FillItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            // Если шанс не задан, устанавливаем его на 0.5 (50%)
            if (i >= itemchances.Count || itemchances[i] == 0)
            {
                itemchances.Add(50);
            }
        }
        for (int i = 0; i < items.Count; i++)
        {
            if (Random.Range(0, 100) <= itemchances[i])
            {
                items[i].Count = Random.Range(1, items[i].Count);
            }
            else
            {
                items.RemoveAt(i);
                itemchances.RemoveAt(i);
            }
        }
        for (int i = 0; i < items.Count; i++)
        {
            for (int y = 0; y < items[i].Count; y++)
            {
                if (y >= items[i].Strength.Count)
                {
                    items[i].Strength.Add(0);
                }
            }
        }
    }
    public void yesLoot()
    {
        LootBuildManager.instance.SetLocationPanel(NameBuild, DescriptionBuild, imageBuild);
        LootBuildManager.instance.SetButtonListener(OnUseButtonClicked);
        LootBuildManager.instance.RefreshUI(items);
    }
    public void noLoot()
    {
        LootBuildManager.instance.SetLocationPanel("Городские окрестности", "безлюдные просторы на километры", imageBuild);
        LootBuildManager.instance.noRefresh();
    }


    public bool RemoveItem(Item item, int count)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Item.ID == item.ID)
            {
                items[i].Count -= count; // Уменьшаем количество предмета
                items[i].Strength.RemoveRange(0, count);
                if (items[i].Count <= 0)
                {
                    // Если количество предметов равно нулю, удаляем предмет из инвентаря
                    items.RemoveAt(i);
                }
                LootBuildManager.instance.RefreshUI(items);
                return true;
            }
        }
        return false;
    }

    public bool IsFull()
    {
        return items.Count >= LootBuildManager.instance.itemSlots.Length;
    }



    public void ShowItemMenu(LootSlot campSlot)
    {
        currentItemSlot = null;
        currentItemSlot = campSlot;
        LootBuildManager.instance.ShowItemInfoPanel(campSlot.Item.Name,
            campSlot.Item.Description, campSlot.Item.icon);
    }
    void OnUseButtonClicked()
    {
        if (items.Count < 1)
        {
            NotifyManager.instance.SetNotify("Ничего нельзя найти");
            return;
        }
        Inventory.instance.LoadCamp();
        craftingTime = 5;
        ChangeTime(craftingTime);
    }
    public int craftingTime;

    void ChangeTime(int time)
    {
        TimeLineManager.instance.StartTime(time, () =>
        {
            AllItems();
        });
    }
    public void AllItems()
    {
        Debug.Log("TTT");
        Inventory.instance.LoadCamp();
        for (int i = 0; i < items.Count; i++)
        {
            int counti = Random.Range(1, items[i].Count);
            Debug.Log(counti);
            if (Inventory.isCamp == true)
            {
                List<int> strength = new();
                strength = items[i].Strength.GetRange(0, counti);
                Inventory.instance.Looting(items[i].Item, counti, strength);
                RemoveItem(items[i].Item, counti);
            }
        }
        if (Inventory.isCamp == true)
        {
            Inventory.instance.RaiseBuilding();
            Inventory.instance.Raise();
        }
        Inventory.instance.RaiseBuilding();
    }

    private void OnDisable()
    {
        currentItemSlot = null;
    }
}