using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPrefab : MonoBehaviour
{
    public TMP_Text CountText;
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
                Destroy(this.gameObject);
            }
            else
            {
                Image.sprite = _item.icon;
                Image.enabled = true;
            }
        }
    }

    public SpriteRenderer Image;

    private void OnValidate()
    {
        if (Image == null)
        {
            Image = GetComponent<SpriteRenderer>();
        }
    }

    // Этот метод вызывается, чтобы обновить количество предметов в слоте
    public void SetCount(int count)
    {
        this.count += count;
        CountText.text = "x" + this.count.ToString();
    }
}
