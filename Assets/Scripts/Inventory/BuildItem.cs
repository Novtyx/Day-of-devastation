using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildItem", menuName = "Items/Build")]
public class BuildItem : Item
{

    [Header("Постройка")]
    public GameObject BuildObject;

    public override void Use()
    {
        BuildManager.instance.AddBuildInPlayer(BuildObject);
        Inventory.instance.RemoveItem(this, 1);
    }
    public override void UseByCamp(Camps camp)
    {
        throw new System.NotImplementedException();
    }
}
