using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New weather", menuName = "Inventory/Weather")]
public class Weather : ScriptableObject
{
    public string Name;
    public string Description;
    // Цвета для разных времени суток
    public Color dayColor = Color.white;
    public Color eveningColor = new Color(1f, 0.8f, 0.6f); // Теплый оранжевый
    public Color nightColor = new Color(0.3f, 0.3f, 0.5f); // Синеватый ночной
    public Color morningColor = new Color(1f, 0.9f, 0.7f); // Светлый утренний

    public float morningIntensity = 1f;
    public float dayIntensity = 1f;
    public float eveningIntensity = 1f;
    public float nightIntensity = 1f;
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
