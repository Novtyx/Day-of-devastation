using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemFindAction", menuName = "Actions/ItemFind")]
public class ItemFindAction : CustomAction
{
    public List<Item> items = new List<Item>();
    public List<int> itemCounts = new List<int>();
    public override void Use()
    {
        throw new System.NotImplementedException();
    }
    public override void Use(Item item)
    {
        throw new System.NotImplementedException();
    }
    public override void UseByCamp(Camps camp)
    {
        SpawnItemInCamp();
    }
    public void SpawnItemInCamp()
    {
        Debug.Log("TTT");
        Inventory.instance.LoadCamp();
        for (int i = 0; i < items.Count; i++)
        {
            int counti = Random.Range(0, itemCounts[i] + 1);
            if (counti < 1) continue;
            if (Inventory.isCamp == true)
            {
                Inventory.instance.Looting(items[i], counti);
            }
        }
        if (Inventory.isCamp == true)
        {
            Inventory.instance.Raise();
        }
    }
}
