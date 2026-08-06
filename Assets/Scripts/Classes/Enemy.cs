using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy
{
    public string EnemyName;
    public Sprite sprite;
    public int Count = 1;
    public int Power = 1;
    public int index; //смещение шанса от нормы
    public Disease diseaseOnLose;
    public Disease diseaseOnWin;
    public List<Item> itemsOnWin;
    public List<int> itemsOnWinCount;
    public Enemy(Enemy other)
    {
        Count = other.Count;
        Power = other.Power;
        index = other.index;
        sprite = other.sprite;
        this.diseaseOnLose = other.diseaseOnLose;
        this.diseaseOnWin = other.diseaseOnWin;
        this.EnemyName = other.EnemyName;
        this.itemsOnWin = other.itemsOnWin;
        this.itemsOnWinCount = other.itemsOnWinCount;
    }
    public Enemy(int count, int power, int index, Disease diseaseOnLose, Disease diseaseOnWin, string EnemyName, List<Item> itemsOnWin, List<int> itemsOnWinCount)
    {
        Count = count;
        Power = power;
        this.index = index;
        this.diseaseOnLose = diseaseOnLose;
        this.diseaseOnWin = diseaseOnWin;
        this.EnemyName = EnemyName;
        this.itemsOnWin = itemsOnWin;
        this.itemsOnWinCount = itemsOnWinCount;
}
}
