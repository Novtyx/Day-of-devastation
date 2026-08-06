using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodItem", menuName = "Items/Food")]
public class FoodItem : Item
{
    [Header("Показатели")]
    public int eatingpower;
    public int wateringpower;
    public int energyingpower;
    public int deathingpower;
    public int radiationpower;
    public int depaturepower;
    public int bloodpower;

    public override void Use()
    {
        if (IsUsed())
        {
            Characters.instance.food = Mathf.Clamp(eatingpower + Characters.instance.food, 0, 500);
            Characters.instance.water = Mathf.Clamp(wateringpower + Characters.instance.water, 0, 500);
            Characters.instance.energy = Mathf.Clamp(energyingpower + Characters.instance.energy, 0, 500);
            Characters.instance.radiation = Mathf.Clamp(radiationpower + Characters.instance.radiation, 0, 500);
            Characters.instance.depature = Mathf.Clamp(depaturepower + Characters.instance.depature, 0, 500);
            Characters.instance.blood = Mathf.Clamp(bloodpower + Characters.instance.blood, 0, 500);
            Characters.instance.death = Mathf.Clamp(deathingpower + Characters.instance.death, 0, 500);
            Characters.instance.SaveCharacters();
            Inventory.instance.RemoveItem(this, 1);
        }


    }
    public override void UseByCamp(Camps camp)
    {
        if (IsUsed())
        {
            Characters.instance.food = Mathf.Clamp(eatingpower + Characters.instance.food, 0, 500);
            Characters.instance.water = Mathf.Clamp(wateringpower + Characters.instance.water, 0, 500);
            Characters.instance.energy = Mathf.Clamp(energyingpower + Characters.instance.energy, 0, 500);
            Characters.instance.radiation = Mathf.Clamp(radiationpower + Characters.instance.radiation, 0, 500);
            Characters.instance.depature = Mathf.Clamp(depaturepower + Characters.instance.depature, 0, 500);
            Characters.instance.blood = Mathf.Clamp(bloodpower + Characters.instance.blood, 0, 500);
            Characters.instance.death = Mathf.Clamp(deathingpower + Characters.instance.death, 0, 500);
            Characters.instance.SaveCharacters();
            camp.RemoveItem(this, 1);
        }
    }
    bool IsUsed()
    {
        bool usage = false;
        if (eatingpower != 0)
        {
            if (eatingpower < 0)
            {
                if (Characters.instance.food >= Mathf.Abs(eatingpower))
                {
                    usage = true;
                }
            }
        }
        if (wateringpower != 0)
        {
            if (wateringpower < 0)
            {
                if (Characters.instance.water >= Mathf.Abs(wateringpower))
                {
                    usage = true;
                }
            }
            else
            {
                usage = true;
            }
        }
        if (energyingpower != 0)
        {
            if (energyingpower < 0)
            {
                if (Characters.instance.energy >= Mathf.Abs(energyingpower))
                {
                    usage = true;
                }
            }
            else
            {
                usage = true;
            }
        }
        if (radiationpower != 0)
        {
            if (radiationpower < 0)
            {
                if (Characters.instance.radiation >= Mathf.Abs(radiationpower))
                {
                    usage = true;
                }
            }
        }
        if (depaturepower != 0)
        {
            if (depaturepower < 0)
            {
                if (Characters.instance.depature >= Mathf.Abs(depaturepower))
                {
                    usage = true;
                }
            }
        }
        if (bloodpower != 0)
        {
            if (bloodpower < 0)
            {
                if (Characters.instance.blood >= Mathf.Abs(bloodpower))
                {
                    usage = true;
                }
            }
        }
        if (deathingpower != 0)
        {
            if (deathingpower < 0)
            {
                if (Characters.instance.death >= Mathf.Abs(deathingpower))
                {
                    usage = true;
                }
            }
        }
        return usage;
    }
}
