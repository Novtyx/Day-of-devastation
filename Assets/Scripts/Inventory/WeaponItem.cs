using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Items/Weapon")]
public class WeaponItem : Item
{

    [Header("Оружие")]
    public bool isWeapon;
    public int power;

    public override void Use()
    {
        NotifyManager.instance.SetMinNotify("нельзя использовать напрямую");
    }
    public override void UseByCamp(Camps camp)
    {
        NotifyManager.instance.SetMinNotify("нельзя использовать напрямую");
    }
}
