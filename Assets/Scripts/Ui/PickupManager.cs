using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PickupManager : MonoBehaviour
{
    public static PickupManager instance;
    [Header("Pickup")]
    public RectTransform pickupPanel;
    public Transform target;
    public Canvas Canvas;

    public RectTransform locationPanel;
    public Transform locationtarget;

    public RectTransform TileName;
    public RectTransform CharactersPanel;
    public TMP_Text TileText;
    public bool isActive = true;
    private string _currentTile;

    public RectTransform VoidPanel;
    public RectTransform PlayerGlobalMarker;

    public bool isCamp;
    public bool isLocation;

    [SerializeField] float offset;
    void Start()
    {
        instance = this;
    }
    public void Closepickup()
    {
        Inventory.instance.PickupIsTrue = false;
    }

    void Update()
    {
        if (isCamp)
        {
            Vector3 ScreenPoint = RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                target.position);
            Vector2 vector2;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Canvas.transform as RectTransform,
                ScreenPoint,
                Canvas.worldCamera,
                out vector2);
            pickupPanel.anchoredPosition = new Vector3(vector2.x, vector2.y + 200f);
        }
        if (isLocation)
        {
            Vector3 ScreenPoint = RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                locationtarget.position);
            Vector2 vector2;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Canvas.transform as RectTransform,
                ScreenPoint,
                Canvas.worldCamera,
                out vector2);
            locationPanel.anchoredPosition = new Vector3(vector2.x, vector2.y + 200f);
        }
        /*
        Vector2 vector3;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Canvas.transform as RectTransform,
            Input.mousePosition,
            Canvas.worldCamera,
            out vector3);
        TileName.anchoredPosition = new Vector3(vector3.x, vector3.y + 30f);
        Vector2 _mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        try
        {
            string name = Movement.instance.tilemap.GetTile(new Vector3Int((int)_mousepos.x, (int)_mousepos.y, 0)).name;
            if (name != _currentTile)
            {
                for (int i = 0; i < Movement.instance.tiledata.Count; i++)
                {
                    if (name == Movement.instance.tiledata[i].name)
                    {
                        _currentTile = name;
                        TileText.text = Movement.instance.tiledata[i].NameTile;
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            TileText.text = "";
            _currentTile = "";
        }*/
        /*
        Vector3 playerPoint = RectTransformUtility.WorldToScreenPoint(
    Camera.main,
    Movement.instance.gameObject.transform.position);
        Vector2 vector4;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Canvas.transform as RectTransform,
            playerPoint,
            Canvas.worldCamera,
            out vector4);
        CharactersPanel.anchoredPosition = new Vector3(vector4.x, vector4.y + 200f);
        */
        // Перемещение VoidPanel
        Vector3 voidPoint = RectTransformUtility.WorldToScreenPoint(
        Camera.main,
        Movement.instance.target.transform.position);
        Vector2 vectorVoid;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Canvas.transform as RectTransform,
            voidPoint,
            Canvas.worldCamera,
            out vectorVoid);
        VoidPanel.anchoredPosition = new Vector3(vectorVoid.x, vectorVoid.y + offset);
        // Перемещение маркера игрока
        Vector3 playerPoint = RectTransformUtility.WorldToScreenPoint(
Camera.main,
Movement.instance.gameObject.transform.position);
        Vector2 vectorPlayer;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Canvas.transform as RectTransform,
            playerPoint,
            Canvas.worldCamera,
            out vectorPlayer);
        PlayerGlobalMarker.anchoredPosition = new Vector3(vectorPlayer.x, vectorPlayer.y);
    }
}