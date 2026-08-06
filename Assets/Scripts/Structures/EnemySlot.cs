using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemySlot : MonoBehaviour
{
    Button slotButton;
    private Enemy _item;

    public Enemy Item
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
                Image.sprite = _item.sprite;
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
            NotifyManager.instance.SetMinNotify(Item.EnemyName);
        }
    }
}