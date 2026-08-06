using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPocketCraft", menuName = "Actions/ItemPocketCraft")]
public class ItemPocketCraft : CustomAction
{
    public Item item;
    public List<Item> needsitems;
    public List<int> needsitemscounts;
    public override void Use()
    {
        Craft(1);
    }
    public override void Use(Item item)
    {
        Craft(1);
    }
    public override void UseByCamp(Camps camp)
    {
        throw new System.NotImplementedException();
    }
    void Craft(int count = 1)
    {
        if (item.needFire)
        {
            if (!Characters.isFire)
            {
                NotifyManager.instance.SetNotify("необходим огонь");
                return;
            };
        }
        if (CanCraftMultiplied(count))
        {
            TimeLineManager.instance.StartTime(item.craftingTime, () =>
            {
                AllItems(count);
            });
        }
        else NotifyManager.instance.SetNotify("Недостаточно ресурсов");
    }
    public bool CanCraftMultiplied(int count)
    {
        if (item == null) return false;
        for (int i = 0; i < needsitems.Count; i++)
        {
            int totalRequired = needsitemscounts[i] * count;
            Debug.Log(totalRequired);
            if (!Inventory.instance.HasEnoughResources(needsitems[i], totalRequired)) return false;
        }
        return true;
    }
   void AllItems(int count)
    {
        for (int g = 0; g < needsitems.Count; g++)
        {
            Inventory.instance.RemoveResourcesForCraft(needsitems[g], needsitemscounts[g] * count);
        }

        int countitems = item.craftingCount;

        List<int> strength = new List<int>(countitems);
        for (int i = 0; i < countitems; i++)
        {
            strength.Add(item.strengtcount);
        }
        Inventory.instance.AddItem(item, countitems, strength);

        Characters.instance.StopTime();
        NotifyManager.instance.SetMinNotify("крафт закончен. создано: " + count);
        Debug.Log("Crafting complete!");
    }
}
