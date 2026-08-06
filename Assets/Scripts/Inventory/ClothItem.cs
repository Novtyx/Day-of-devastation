using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClothSlotType { Head, Face, Tors, Leggs, Transport, Bag, Stops}

[CreateAssetMenu(fileName = "ClothItem", menuName = "Items/Cloth")]
public class ClothItem : Item
{
    [Header("Одежда")]
    public bool isclothes;
    public ClothSlotType clothesname;
    public int protection;
    public int protection_radiation;
    public int body_covering;
    public int speed;
    public int GetSlotId(ClothSlotType slot)
    {
       switch (slot)
        {
            case (ClothSlotType.Head): return 0;
            case (ClothSlotType.Face): return 1;
            case (ClothSlotType.Tors): return 2;
            case (ClothSlotType.Leggs): return 3;
            case (ClothSlotType.Transport): return 4;
            case (ClothSlotType.Bag): return 5;
            case (ClothSlotType.Stops): return 6;
        }
        return 10;
    }
    public override void Use()
    {
        if (Inventory.instance.clothesitems[GetSlotId(clothesname).ToString()].ItemId != ID)
        {
            int strength = 0;
            for (int i = 0; i < Inventory.instance.items.Count; i++)
            {
                if (ID == Inventory.instance.items[i].Item.ID)
                {
                    strength = Inventory.instance.items[i].Strength[0];
                }
            }
            Debug.Log(strength);
            Inventory.instance.ClothesManager(GetSlotId(clothesname).ToString(), 1, strength, this);
            Inventory.instance.RemoveItem(this, 1);
        }
    }
    public override void UseByCamp(Camps camp)
    {
        if (Inventory.instance.clothesitems[GetSlotId(clothesname).ToString()].ItemId != ID)
        {
            int strength = 0;
            for (int i = 0; i < Inventory.instance.items.Count; i++)
            {
                if (ID == Inventory.instance.items[i].Item.ID)
                {
                    strength = Inventory.instance.items[i].Strength[0];
                }
            }
            Debug.Log(strength);
            Inventory.instance.ClothesManager(GetSlotId(clothesname).ToString(), 1, strength, this);
            camp.RemoveItem(this, 1);
        }
    }
}
