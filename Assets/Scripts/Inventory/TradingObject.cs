using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TradingObject : MonoBehaviour
{
    [SerializeField] private TMP_Text NeedItem;
    [SerializeField] private TMP_Text GivingItem;
    [SerializeField] private Button button;
    [SerializeField] private Color SuccesFull;
    [SerializeField] private Color Reject;
    private ItemInstance needItem;
    private ItemInstance givingItem;

    public void InitializeObject(ItemInstance need, ItemInstance giving)
    {
        needItem = need;
        givingItem = giving;
        NeedItem.text = $"{need.Item.Name} 0/{need.Count}";
        GivingItem.text = $"{giving.Item.Name} x{giving.Count}";
        bool bol = false;
        for (int g = 0; g < Inventory.instance.items.Count; g++)
        {
            if (Inventory.instance.items[g].Item.ID == needItem.Item.ID && Inventory.instance.items[g].Count >= needItem.Count)
            {
                NeedItem.text = $"{need.Item.Name} {Inventory.instance.items[g].Count}/{need.Count}";
                bol = true;
                break;
            }
        }
        if (!bol) gameObject.GetComponent<Image>().color = Reject;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Trade);
    }

    private void Trade()
    {
        var item = Inventory.instance.items.Find(e => e.Item.ID == needItem.Item.ID);
        if (item != null)
        {
            Inventory.instance.AddItem(givingItem.Item, givingItem.Count, givingItem.Strength);
            Inventory.instance.RemoveItem(needItem.Item, needItem.Count);
            gameObject.GetComponent<Image>().color = SuccesFull;
            NotifyManager.instance.SetMinNotify("удачно");
            AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantClick());
            bool bol = false;
            var item1 = Inventory.instance.items.Find(e => e.Item.ID == needItem.Item.ID);
            if (item1 != null)
            {
                if (item1.Count >= needItem.Count)
                {
                    NeedItem.text = $"{needItem.Item.Name} {item1.Count}/{needItem.Count}";
                    bol = true;
                }
            }
            if (!bol)
            {
                NeedItem.text = $"{needItem.Item.Name} 0/{needItem.Count}";
                gameObject.GetComponent<Image>().color = Reject;
            }
            return;
        }
        NotifyManager.instance.SetNotify("недостаточно ресурсов");
        gameObject.GetComponent<Image>().color = Reject;
        AndroidHapticFeedback.TriggerHapticFeedback(
AndroidHapticFeedback.GetConstantConfirm());
    }
}
