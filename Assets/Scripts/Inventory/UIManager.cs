using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject Inventoryobject;
    public GameObject GlobalInventoryobject;
    public GameObject PickupPanel; // Элемент со списком предметов в лагере
    public GameObject Craftpanel;
    public GameObject PlaceboPanel; // ПАнель для фона
    public GameObject WeatherPanel;
    public TMP_Text WeatherName;
    public TMP_Text WeatherDescription;
    public TMP_Text WeatherFeatures;
    public Camera MainCamera;
    public Color nventoryFonColor;

    public GameObject Wgatischpanel;
    public Text namech;
    public Text descrch;
    public static bool stena = false;
    private RectTransform panel;
    public GameObject player;

    public Vector2 pos;
    private void Start()
    {
        panel = Wgatischpanel.GetComponent<RectTransform>();
        UnityEngine.Debug.Log(Wgatischpanel.transform.position);
        if (PlayerPrefs.HasKey("quality"))
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality"));
        }
        else
        {
            QualitySettings.SetQualityLevel(3);
        }
    }

    public void toplayer()
    {
        MainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
    }
    public void OpenMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void openInventory()
    {
        GlobalInventoryobject.SetActive(false);
        Inventoryobject.SetActive(true);
        Craftpanel.SetActive(false);
        MovePanel(Inventoryobject, new Vector3(0f, 0f, 0f));
        Color color = nventoryFonColor;
        PlaceboPanel.GetComponent<Image>().DOColor(color, 1f);
    }
    public void openCraft()
    {
        GlobalInventoryobject.SetActive(false);
        Inventoryobject.SetActive(false);
        Craftpanel.SetActive(true);
        MovePanel(Craftpanel, new Vector3(0f, 0f, 0f));
        Color color = nventoryFonColor;
        PlaceboPanel.GetComponent<Image>().DOColor(color, 1f);
        Craft.instance.RefreshUI();
        Craft.instance.RefreshneedUI();
    }
    public void closeInventory()
    {
        if (Inventoryobject.activeInHierarchy) MovePanel(Inventoryobject, new Vector3(0f, -1100f, 0f));
        if (GlobalInventoryobject.activeInHierarchy) MovePanel(GlobalInventoryobject, new Vector3(0f, -1100f, 0f));
        if (Craftpanel.activeInHierarchy) MovePanel(Craftpanel, new Vector3(0f, -1100f, 0f));
        Invoke("Hide", 0.4f);
        Color color = PlaceboPanel.GetComponent<Image>().color;
        color.a = 0f;
        PlaceboPanel.GetComponent<Image>().DOColor(color, 1f);
        Inventory.instance.OnCloseButtonClicked();

    }
    void Hide()
    {
        Inventoryobject.SetActive(false);
        GlobalInventoryobject.SetActive(false);
        Craftpanel.SetActive(false);
    }
    public void openGlobalInventory()
    {
        Inventoryobject.SetActive(false);
        Craftpanel.SetActive(false);
        GlobalInventoryobject.SetActive(true);
        PickupPanel.SetActive(false);
        Color color = new Color(0f, 0f, 0f);
        color.a = 180/255f;
        PlaceboPanel.GetComponent<Image>().DOColor(color, 1f);
        MovePanel(GlobalInventoryobject, new Vector3(0f, 0f, 0f));
        Inventory.instance.OnCloseButtonClicked();
    }
    void MovePanel(GameObject transform, Vector3 pos)
    {
        transform.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, pos.y, pos.z), 0.4f).SetEase(Ease.OutCubic);
    }
    public void WhatisCharacter(string name)
    {
        namech.text = name;
    }
    public void WhatisCharacterdesc(string desc)
    {
        descrch.text = desc;
    }
    public void WhatisCharacterpos(GameObject characres) // Информация о характеристиках
    {
        Wgatischpanel.SetActive(true);
    }
    public void ShowPanelWeather()
    {
        Weather weather = WeatherManager.instance.currentWeather;
        WeatherPanel.GetComponent<Image>().color = weather.dayColor;
        WeatherPanel.SetActive(true);
        WeatherName.text = weather.Name;
        WeatherDescription.text = weather.Description;
        List<string> features = new List<string>();
        features.Add("воздействие:");
        if (weather.eatingpower != 0)
        {
            features.Add("Голод: " + weather.eatingpower);
        }
        if (weather.wateringpower != 0)
        {
            features.Add("Жажда: " + weather.wateringpower);
        }
        if (weather.energyingpower != 0)
        {
            features.Add("Усталость: " + weather.energyingpower);
        }
        if (weather.radiationpower != 0)
        {
            features.Add("Радиация: " + weather.radiationpower);
        }
        if (weather.depaturepower != 0)
        {
            features.Add("Отравление: " + weather.depaturepower);
        }
        if (weather.bloodpower != 0)
        {
            features.Add("Кровотечение: " + weather.bloodpower);
        }
        if (weather.deathingpower != 0)
        {
            features.Add("Истощение: " + weather.deathingpower);
        }
        string feature = string.Join("\n", features);
        WeatherFeatures.text = feature;
    }

    public void OpenModFolder()
    {
        Process.Start(Path.Combine(Application.persistentDataPath, "mods"));
    }
}
