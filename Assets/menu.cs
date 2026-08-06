using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class CustomToggle
{
    public string Title;
    public string PlayerPrefsPath;
    public bool StartState = true;
    public bool IsDangerous;
}
[System.Serializable]
public class CustomSlider
{
    public string Title;
    public string PlayerPrefsPath;
    public int CurrentValue = 1;
    public int MinValue = 1;
    public int MaxValue = 2;
    public bool IsDangerous;
}
public class menu : MonoBehaviour
{
    public List<CustomToggle> toggles = new List<CustomToggle>();
    private string _currentSavePath;

    public Vector3 pos;
    public GameObject SettngsPanel;
    public GameObject MenuPanel;
    public TMP_Dropdown quality;
    public Toggle toggleColors;
    public Toggle toggleClouds;
    public Toggle toggleParticles;
    public Toggle toggleZerno;
    public Toggle toggleEmbient;
    public Toggle toggleLight;
    public Toggle togglePost;
    public TMP_Text chuckSize;
    public Slider slider;

    [Header("Кастомизация персонажа")]
    [SerializeField] private TMP_Text deathtext;
    [SerializeField] private TMP_Text energytext;
    [SerializeField] private TMP_Text foodtext;
    [SerializeField] private TMP_Text watertext;
    [SerializeField] private TMP_Text radiationtext;
    [SerializeField] private TMP_Text depaturetext;
    [SerializeField] private TMP_Text bloodtext;
    [SerializeField] private TMP_Text strengthtext;
    private int death;
    private int energy;
    private int food;
    private int water;
    private int radiation;
    private int depature;
    private int blood;
    private int strength;

    public void CastomizeCharacter()
    {
        death = Random.Range(80, 120);
        energy = Random.Range(80, 120);
        food = Random.Range(80, 120);
        water = Random.Range(80, 120);
        radiation = Random.Range(80, 120);
        depature = Random.Range(80, 120);
        blood = Random.Range(80, 120);
        strength = Random.Range(2, 7);
        deathtext.text = "максимальное истощение: " + death.ToString();
        energytext.text = "максимальная усталость: " + energy.ToString();
        foodtext.text = "максимальный голод: " + food.ToString();
        watertext.text = "максимальная жажда: " + water.ToString();
        radiationtext.text = "максимальная радиация: " + radiation.ToString();
        depaturetext.text = "максимальное отравление: " + depature.ToString();
        bloodtext.text = "максимальное кровотечение: " + blood.ToString();
        strengthtext.text = "базовое значение силы: " + strength.ToString();
    }
    void Start()
    {
        chuckSize.text = PlayerPrefs.GetInt("chunksize", 5).ToString();
        slider.value = PlayerPrefs.GetInt("chunksize", 5);
        slider.wholeNumbers = true;
        string togle = PlayerPrefs.GetString("fps", "60");
        Application.targetFrameRate = int.Parse(togle);
        Debug.Log(Application.targetFrameRate);
        if (PlayerPrefs.HasKey("quality"))
        {
            quality.value = PlayerPrefs.GetInt("quality");
        }
        else
        {
            quality.value = 3;
        }
        if (PlayerPrefs.GetInt("generatecolors", 1) == 0)
        {
            toggleColors.isOn = false;
        }
        if (PlayerPrefs.GetInt("generateclouds", 0) == 0)
        {
            toggleClouds.isOn = false;
        }
        if (PlayerPrefs.GetInt("generateparticles", 0) == 0)
        {
            toggleParticles.isOn = false;
        }
        if (PlayerPrefs.GetInt("zerno", 0) == 0)
        {
            toggleZerno.isOn = false;
        }
        if (PlayerPrefs.GetInt("embient", 1) == 0)
        {
            toggleEmbient.isOn = false;
        }
        if (PlayerPrefs.GetInt("light", 0) == 0)
        {
            toggleLight.isOn = false;
        }
        if (PlayerPrefs.GetInt("postprocessing", 0) == 0)
        {
            togglePost.isOn = false;
        }
        QualitySettings.SetQualityLevel(quality.value);

        HapticFeedback.onClick.AddListener(PlayHaptic);
        simple.onClick.AddListener(PlaySimple);
        twoticks.onClick.AddListener(PlayDinkDink);
        heart.onClick.AddListener(PlayHeartbeat);
        twoticksprimitive.onClick.AddListener(PlayDinkDinkPrim);
        heartprimitive.onClick.AddListener(PlayReload);
        toggleClouds.onValueChanged.AddListener(SetEmbient);
        foreach (CustomToggle toggle in toggles)
        {

        }
    }
    public void PlayDinkDink()
    {
        // 0мс ждем, 15мс вибрируем, 100мс пауза, 15мс вибрируем
        long[] timings = { 0, 15, 100, 15 };
        // 0 - мотор выключен, 255 - максимальная сила удара
        int[] amplitudes = { 0, 255, 0, 255 };
        AndroidHapticFeedback.PlayCustomPattern(timings, amplitudes);
    }
    // Мягкий нарастающий "тук" (например, для сердцебиения в survival-игре)
    public void PlayHeartbeat()
    {
        long[] timings = { 0, 30, 150, 40 };
        int[] amplitudes = { 0, 100, 0, 255 }; // Первый удар слабее, второй сильнее
        AndroidHapticFeedback.PlayCustomPattern(timings, amplitudes);
    }

    public void PlayDinkDinkPrim()
    {
        int[] primitives = { 1, 1 };
        float[] scales = { 1.0f, 1.0f }; // Сила от 0.0 до 1.0
        int[] delays = { 0, 100 }; // Пауза ПЕРЕД эффектом в миллисекундах
        AndroidHapticFeedback.PlayPrimitivePattern(primitives, scales, delays);
    }
    // Перезарядка оружия (щелчок затвора -> досылание)
    public void PlayReload()
    {
        int[] primitives = { 7, 2 };
        float[] scales = { 0.5f, 1.0f };
        int[] delays = { 0, 150 };
        AndroidHapticFeedback.PlayPrimitivePattern(primitives, scales, delays);
    }
    public void PlaySimple()
    {
        AndroidHapticFeedback.Vibrate(50);
    }
    public void PlayHaptic()
    {
        AndroidHapticFeedback.TriggerHapticFeedback(AndroidHapticFeedback.CONTEXT_CLICK);
    }

    public void Show(GameObject obj)
    {
        obj.SetActive(true);
        Color i = obj.GetComponent<Image>().color;
        i.a = 121 / 255f;

        Sequence sequence = DOTween.Sequence();
        sequence
        .Join(MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, -800, pos.z), 1f).SetEase(Ease.OutCubic))
        .Join(obj.transform.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutCubic))
        .Join(obj.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.OutCubic))
            ;
    }
    public void Hide(GameObject obj)
    {
        Color i = obj.GetComponent<Image>().color;
        i.a = 0 / 255f;
        Sequence sequence = DOTween.Sequence();
        sequence
        .Join(MenuPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(pos.x, pos.y, pos.z), 1f).SetEase(Ease.OutCubic))
        .Join(obj.transform.DOScale(new Vector3(40, 40, 40), 1f).SetEase(Ease.InCubic))
        .Join(obj.GetComponent<Image>().DOColor(i, 1f).SetEase(Ease.OutCubic))
        .OnComplete(() => obj.SetActive(false))
        ;
    }
    public void SetChunkSize(float size)
    {
        PlayerPrefs.SetInt("chunksize", (int)size);
        chuckSize.text = ((int)size).ToString();
    }
    public void SetColor(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("generatecolors", 1);
        else PlayerPrefs.SetInt("generatecolors", 0);
    }
    public void SetCloud(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("generateclouds", 1);
        else PlayerPrefs.SetInt("generateclouds", 0);
    }
    public void SetPostProcessing(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("postprocessing", 1);
        else PlayerPrefs.SetInt("postprocessing", 0);
    }
    public void SetParticles(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("generateparticles", 1);
        else PlayerPrefs.SetInt("generateparticles", 0);
    }
    public void SetZerno(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("zerno", 1);
        else PlayerPrefs.SetInt("zerno", 0);
    }
    public void SetEmbient(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("embient", 1);
        else PlayerPrefs.SetInt("embient", 0);
    }
    public void SetLight(bool bol)
    {
        if (bol) PlayerPrefs.SetInt("light", 1);
        else PlayerPrefs.SetInt("light", 0);
    }
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("quality", index);
    }
    public void RemovCharacters()
    {
        PlayerPrefs.SetInt("Year", 1980);
        PlayerPrefs.SetInt("Month", 8);
        PlayerPrefs.SetInt("Day", 1);
        PlayerPrefs.SetInt("Hour", 7);
        PlayerPrefs.SetInt("Minute", 40);

        File.Delete(Path.Combine(Application.persistentDataPath, "inventory.json"));
        File.Delete(Path.Combine(Application.persistentDataPath, "diseas.json"));
        File.Delete(Path.Combine(Application.persistentDataPath, "builds.json"));

        PlayerPrefs.SetFloat("x", 1860f);
        PlayerPrefs.SetFloat("y", 3137f);

        PlayerPrefs.SetInt("death", 0);
        PlayerPrefs.SetInt("energy", 0);
        PlayerPrefs.SetInt("food", 0);
        PlayerPrefs.SetInt("water", 0);
        PlayerPrefs.SetFloat("radiation", 0);
        PlayerPrefs.SetInt("depature", 0);
        PlayerPrefs.SetInt("blood", 0);

        PlayerPrefs.SetInt("idCamp", 1);
        PlayerPrefs.SetInt("idHouse", 1);
    }
    public Button HapticFeedback;
    public Button simple;
    public Button twoticks;
    public Button heart;
    public Button twoticksprimitive;
    public Button heartprimitive;
}
