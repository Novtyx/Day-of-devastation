using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    [Header("Базовые характеристики")]
    public string Name = " ";
    [TextArea] public string Description = "Описание предмета";
    public Sprite icon = null;
    public int ID;
    public int height;
    public List<CustomAction> CustomActions;

    [Header("Крафт")]
    public bool isCrafting;
    public float craftingTime;
    public int craftingCount;
    public List<Item> needsitems;
    public List<int> needsitemscounts;
    public bool needFire;

    public abstract void Use();
    public abstract void UseByCamp(Camps camp);


    [Header("Игровые характеристики")]
    public string feature = "Хар-ки";
    public bool isstrength;
    public int strengtcount;


    public Item() { }
}
