using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New biome", menuName = "Biomes/Biome")]
public class Biome : ScriptableObject
{
    public string NameTile;
    public string DescriptionTile;

    public Sprite Sprite;
    public RuleTile Tile;
    public List<Item> items;
    public List<int> itemCounts;
    public int radiation;
    public int MusicIndex;
    public float SpeedCoefficient;
    public Color FirstColor;
    public Color SecondColor;

    public List<Enemy> enemys = new List<Enemy>();
    public List<Event> events = new List<Event>();
    public List<Event> structures = new List<Event>();
    public List<CustomAction> CustomActions = new List<CustomAction>();
}
