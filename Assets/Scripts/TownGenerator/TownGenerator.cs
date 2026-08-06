using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Linq;

public class TownGenerator
{
    private List<RuleTile> wallTile = new List<RuleTile>();
    private List<RuleTile> treeTile = new List<RuleTile>();
    private TileBase roadTile;
    private TileBase groundTile;

    private Tilemap ground;
    private Tilemap floor;

    private int minRoadsCount;
    private int size;

    public TownGenerator(List<RuleTile> wallTile, List<RuleTile> treeTile, TileBase roadTile, TileBase groundTile, Tilemap floor, Tilemap ground, int minRoadsCount, int size)
    {
        this.wallTile = wallTile;
        this.treeTile = treeTile;
        this.roadTile = roadTile;
        this.groundTile = groundTile;
        this.floor = floor;
        this.ground = ground;
        this.minRoadsCount = minRoadsCount;
        this.size = size;
    }

    public void GenerateCity()
    {
        floor.ClearAllTiles();
        ground.ClearAllTiles();

        List<Vector2Int> startPoints = new List<Vector2Int>();
        List<Vector2Int> endPoints = new List<Vector2Int>();

        // Добавляем точки для дорог
        while (startPoints.Count < minRoadsCount)
        {
            for (int x = -size+5; x < size-5; x += 4)
            {
                for (int y = -size+5; y < size-5; y += 4)
                {
                    if (Random.value < 0.05f) // Шанс 5%
                    {
                        startPoints.Add(new Vector2Int(x, y));
                        endPoints.Add(new Vector2Int(
                            Mathf.Clamp(x + Random.Range(-5, 5), -size + 1, size - 1),
                            Mathf.Clamp(y + Random.Range(-5, 5), -size + 1, size - 1)
                        ));
                    }
                }
            }
        }

        for (int i = 0; i < startPoints.Count; i++)
        {
            DrawRoad(startPoints[i], endPoints[i]);
        }

        // Заполняем землей
        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                if (isNeighbourForGround(new Vector3Int(x, y)))
                {
                    ground.SetTile(new Vector3Int(x, y), groundTile);
                }
            }
        }

        GenerateStructures();
        GenerateTrees();

        Debug.Log("City genearte");
    }
    private void DrawRoad(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        bool horizontalFirst = Random.value > 0.5f;

        if (horizontalFirst)
        {
            MoveAlongX(ref current, end.x);
            MoveAlongY(ref current, end.y);
        }
        else
        {
            MoveAlongY(ref current, end.y);
            MoveAlongX(ref current, end.x);
        }
    }

    private void MoveAlongY(ref Vector2Int current, int targetY)
    {
        int step = (current.y < targetY) ? 1 : -1;
        while (current.y != targetY)
        {
            SetRoadTile(current);
            current.y += step;
        }
        SetRoadTile(current);
    }

    private void MoveAlongX(ref Vector2Int current, int targetX)
    {
        int step = (current.x < targetX) ? 1 : -1;
        while (current.x != targetX)
        {
            SetRoadTile(current);
            current.x += step;
        }
        SetRoadTile(current);
    }

    private void SetRoadTile(Vector2Int current)
    {
        floor.SetTile(new Vector3Int(current.x, current.y, 0), roadTile);
    }

    void GenerateStructures()
    {
        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                if (ground.GetTile(new Vector3Int(x, y)) == groundTile && floor.GetTile(new Vector3Int(x, y)) == null)
                {
                    int rand = Random.Range(0, 3);
                    int randindex = Random.Range(0, wallTile.Count);
                    if (isNeighbour(new Vector3Int(x, y)) && rand == 1) floor.SetTile(new Vector3Int(x, y), wallTile[randindex]);
                }
            }
        }
    }
    void GenerateTrees()
    {
        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                if (ground.GetTile(new Vector3Int(x, y)) == groundTile && floor.GetTile(new Vector3Int(x, y)) == null)
                {
                    int rand = Random.Range(0, 2);
                    int randindex = Random.Range(0, treeTile.Count);
                    if (isNeighbour(new Vector3Int(x, y)) && rand == 1) floor.SetTile(new Vector3Int(x, y), treeTile[randindex]);
                }
            }
        }
    }
    bool isNeighbour(Vector3Int pos)
    {
        if (floor.GetTile(new Vector3Int(pos.x + 1, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x - 1, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y + 1)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y - 1)) == roadTile) return true;
        return false;
    }
    bool isNeighbourForGround(Vector3Int pos)
    {
        if (floor.GetTile(new Vector3Int(pos.x + 1, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x - 1, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y + 1)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y - 1)) == roadTile || 

    floor.GetTile(new Vector3Int(pos.x + 2, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x - 2, pos.y)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y + 2)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x, pos.y - 2)) == roadTile ||

    floor.GetTile(new Vector3Int(pos.x + 1, pos.y + 1)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x - 1, pos.y - 1)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x + 1, pos.y - 1)) == roadTile ||
    floor.GetTile(new Vector3Int(pos.x - 1, pos.y + 1)) == roadTile
    ) return true;
        return false;
    }
}
