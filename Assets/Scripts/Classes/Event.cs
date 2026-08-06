using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Event
{
    public string title;
    public string description;
    public Sprite sprite;
    public List<Item> items;
    public List<int> itemsCounts;
    public bool IsSearching;
    public bool IsAttacking;
    [Header("Цвета")]
    public Color textColor;
    public Color panelColor;
    public Color buttonColor;

    public Event() { }
    public Event(string title, string description, Sprite sprite, List<Item> items, List<int> itemsCounts, bool isSearching, bool isAttacking, Color textColor, Color panelColor, Color buttonColor)
    {
        this.title = title;
        this.description = description;
        this.sprite = sprite;
        this.items = items;
        this.itemsCounts = itemsCounts;
        IsSearching = isSearching;
        IsAttacking = isAttacking;
        this.textColor = textColor;
        this.panelColor = panelColor;
        this.buttonColor = buttonColor;
    }
}
