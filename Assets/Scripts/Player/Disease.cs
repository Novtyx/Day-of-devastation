using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New disease", menuName = "Inventory/Disease")]
public class Disease : ScriptableObject
{
    public string Name;
    public List<Item> items = new List<Item>();
    public List<int> itemsCounts = new List<int>();
    public int timeHours = 10;

    [Header("Показатели")]
    public bool iseating;
    public int eatingpower;
    public bool iswatering;
    public int wateringpower;
    public bool isenergy;
    public int energyingpower;
    public bool isdeath;
    public int deathingpower;
    public bool isradiation;
    public int radiationpower;
    public bool isdepature;
    public int depaturepower;
    public bool isblood;
    public int bloodpower;
}
