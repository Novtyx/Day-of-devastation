using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CaseItem", menuName = "Items/Case")]
public class CaseItem : Item
{
    public List<ItemInstance> maybeitems;                // Список предметов
    public List<ItemInstance> items;                // Список предметов
    [SerializeField] List<int> itemchances;            // Список для хранения количества предметов
    public override void Use()
    {
        FillItems();

        for (int i = 0; i < items.Count; i++)
        {
            Inventory.instance.AddItem(items[i].Item, items[i].Count, null);
        }
        Inventory.instance.RemoveItem(this, 1);
    }
    public override void UseByCamp(Camps camp)
    {
        throw new System.NotImplementedException();
    }
    public void FillItems()
    {
        items.Clear();
        for (int i = 0; i < maybeitems.Count; i++)
        {
            // Если шанс не задан, устанавливаем его на 0.5 (50%)
            if (i >= itemchances.Count || itemchances[i] == 0)
            {
                itemchances.Add(50);
            }
        }
        for (int i = 0; i < maybeitems.Count; i++)
        {
            if (Random.Range(0, 100) <= itemchances[i])
            {
                ItemInstance item = new ItemInstance();
                item.Item = maybeitems[i].Item;
                item.Count = Random.Range(1, maybeitems[i].Count);
                items.Add(item);
            }
        }
    }
}