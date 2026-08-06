using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class TopDownGenerator : MonoBehaviour
{
    public List<RuleTile> wallTile = new List<RuleTile>();
    public List<RuleTile> treeTile = new List<RuleTile>();
    public TileBase roadTile;
    [SerializeField] private TileBase groundTile;
    public Tilemap floor;
    public Tilemap ground;

    public GameObject fog;
    public TextMeshPro nameoftown;

    public int MinRoadsCount = 1;
    public int TownSize = 15;
    public bool isActive;
    public bool isGenerated = false;

    [ContextMenu("—ќздать")]
    public void GenerateCity()
    {
        TownGenerator townGenerator = new TownGenerator(wallTile, treeTile, roadTile, groundTile, floor, ground, MinRoadsCount, TownSize);
        townGenerator.GenerateCity();
    }
}
