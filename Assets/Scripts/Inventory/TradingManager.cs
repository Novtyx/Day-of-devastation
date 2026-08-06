using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradingManager : MonoBehaviour
{
    [SerializeField] private Transform tradingObjectsParent;
    [SerializeField] private GameObject TradingPanel;

    [SerializeField] private TradingObject tradingObject;
    public List<ItemInstance> needitems;
    public List<ItemInstance> givingitems;

    public void Trade()
    {
        TradingObject[] tradingObjects = tradingObjectsParent.GetComponentsInChildren<TradingObject>();
        for (int i = 0; i < tradingObjects.Length; i++)
        {
            Destroy(tradingObjects[i].gameObject);
        }
        for (int i = 0; i < needitems.Count; i++)
        {
            TradingObject obj = Instantiate(tradingObject, tradingObjectsParent);
            obj.InitializeObject(needitems[i], givingitems[i]);
        }
        TradingPanel.SetActive(true);
    }
}
