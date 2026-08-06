using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DiseaseSlot : MonoBehaviour
{
    Button slotButton;
    public TMP_Text nameText;  // Поле для предметов
    private Disease _item;
    public List<Item> needsitems;
    public List<int> needsitemscounts;

    public Disease Item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                nameText.text = Item.Name;
            }
        }
    }


    private void Start()
    {
        slotButton = GetComponent<Button>();
        slotButton.onClick.AddListener(OnSlotClick);
    }

    void OnSlotClick()
    {
        if (Item != null)
        {
            HealthManager.instance.ShowItemMenu(this);
        }
    }
}