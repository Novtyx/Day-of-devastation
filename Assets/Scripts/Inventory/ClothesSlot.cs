using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClothesSlot : MonoBehaviour
{
    Button slotButton;
    public Text countText;  // Поле для отображения количества предметов
    private Item _item;

    public Item Item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                Image.enabled = false;
            }
            else
            {
                Image.sprite = _item.icon;
                Image.enabled = true;
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
            Inventory.instance.showclothesmenu(this);
        }
    }
}