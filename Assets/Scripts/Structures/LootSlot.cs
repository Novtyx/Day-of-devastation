using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootSlot : MonoBehaviour
{
    Button slotButton;
    public TMP_Text countText;  // Поле для отображения количества предметов
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

    // Этот метод вызывается, чтобы обновить количество предметов в слоте
    public void SetCount(int count)
    {
        // Если количество больше 1, отображаем его, иначе скрываем текст
        if (count > 1)
        {
            countText.text = "x" + count.ToString();
            countText.gameObject.SetActive(true); // Включаем отображение текста
        }
        else
        {
            countText.gameObject.SetActive(false); // Скрываем текст, если количество 1 или меньше
        }
    }
    void OnSlotClick()
    {
        if (Item != null)
        {
            Inventory.instance.OnSlotClick1(this);
        }
    }
}