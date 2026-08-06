using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CampSlot : MonoBehaviour, IPointerClickHandler
{
    Button slotButton;
    public TMP_Text countText;  // Поле для отображения количества предметов
    private Item _item;
    public int count;

    public Item Item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                Image.enabled = false;
                count = 0;
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
        this.count = count;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item != null)
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                OnDoubleClick();
            }
            else
            {
                DropMenuShow();
                lastClickTime = Time.time;
            }
        }
    }

    public float doubleClickTime = 0.3f;
    private float lastClickTime = 0f;

    private void OnDoubleClick()
    {
        for (int i = 0; i < Inventory.CampGive.items.Count; i++)
        {
            if (Inventory.CampGive.items[i].Item.name == Item.name)
            {
                List<int> strength = new();
                if (_item.isstrength) strength = Inventory.CampGive.items[i].Strength.GetRange(0, count);
                Inventory.instance.AddItem(Item, count, strength);
                Inventory.CampGive.RemoveItem(Item, count);
            }
        }
        Inventory.instance.Raise();
        if (Inventory.CampGive.items.Count == 0)
        {
            Inventory.instance.itemsInCamps.Remove(Inventory.CampGive.index.ToString());
            Inventory.instance.campIds.Remove(Inventory.CampGive.index);
            Inventory.instance.id.Remove(Inventory.CampGive.index);
            Inventory.instance.x.Remove(Inventory.CampGive.transform.position.x);
            Inventory.instance.y.Remove(Inventory.CampGive.transform.position.y);
            Destroy(Inventory.CampGive.gameObject);
            Inventory.instance.SaveInventory();
        }
    }


    private void DropMenuShow()
    {
        Debug.Log("fff");
        Inventory.instance.OnSlotClick(this);
    }
}