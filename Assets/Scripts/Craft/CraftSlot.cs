using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftSlot : MonoBehaviour
{
    Button slotButton;
    private Item _item;
    public List<Item> needsitems;
    public List<int> needsitemscounts;
    public float craftTime = 1;

    public Item Item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                Image.enabled = false;
                needsitems = null;
                needsitemscounts = null;
                craftTime = 1f;
            }
            else
            {
                Image.sprite = _item.icon;
                Image.enabled = true;
                craftTime = _item.craftingTime;
                needsitems = _item.needsitems;
                needsitemscounts = _item.needsitemscounts;
            }
        }
    }

    public Image Image;

    private void Start()
    {
        slotButton = GetComponent<Button>();
        slotButton.onClick.AddListener(OnSlotClick);
    }

    private void OnValidate()
    {
        if (Image == null)
        {
            Image = GetComponent<Image>();
        }
    }
    void OnSlotClick()
    {
        if (Item != null)
        {
            Inventory.instance.OnSlotClick2(this);
        }
    }
}