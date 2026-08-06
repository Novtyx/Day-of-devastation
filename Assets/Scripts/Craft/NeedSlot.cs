using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeedSlot : MonoBehaviour
{
    Button slotButton;
    private Item _item;
    public TMP_Text countText;  // Поле для отображения количества предметов

    public List<Item> needsitems;
    public List<int> needsitemscounts;

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
    public void SetCount(int count, int maxCount)
    {
        // Если количество больше 1, отображаем его, иначе скрываем текст
        if (count > 0)
        {
            countText.text = maxCount.ToString() + "/" + count.ToString();
            countText.gameObject.SetActive(true); // Включаем отображение текста
            Image.enabled = true;
        }
        else
        {
            countText.gameObject.SetActive(false); // Скрываем текст, если количество 1 или меньше
            Image.enabled = false;
        }
    }
    void OnSlotClick()
    {
        if (Item != null)
        {
            Craft.instance.ShowNeedItemInfo(this);
        }
    }
}