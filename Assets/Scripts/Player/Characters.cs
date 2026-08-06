using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class Characters : MonoBehaviour
{
    public static Characters instance;
    public int death;
    public int energy;
    public int food;
    public int water;
    public int radiation;
    public int depature;
    public int blood;
    public int nastroenie;
    public Disease nastroenieDisease;

    public TMP_Text deathtext;
    public TMP_Text energytext;
    public TMP_Text foodtext;
    public TMP_Text watertext;
    public TMP_Text radiationtext;
    public TMP_Text depaturetext;
    public TMP_Text bloodtext;
    public Slider nastroenieSlider;
    public TMP_Text nastroenieText;

    // Переменные для состояния тела


    // Переменные для хранения текущей даты и времени
    private int year;
    private int month;
    private int day;
    private int hour;
    private int minute;

    // Количество дней в каждом месяце, не учитывая високосные годы
    private readonly int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    public TMP_Text data;
    public bool changedata = false;

    // Флаги для отслеживания запущенных корутин
    private bool isTimeCoroutineRunning = false;
    private bool isCharactersCoroutineRunning = false;
    private bool isWaterCoroutineRunning = false;

    // Ссылки на корутины
    private Coroutine timeCoroutine;
    private Coroutine charactersCoroutine;
    private Coroutine waterCoroutine;


    public int death_strength = 0;
    public int energy_strength = 1;
    public int food_strength = 1;
    public int water_strength = 1;
    public int radiation_strength = 0;
    public int depature_strength = 0;
    public int blood_strength = 0;

    public int aec_radiation_strength = 0;


    public int radiation_protect = 0;
    public int protection = 0;
    public int body_covering = 0;
    public TMP_Text radiation_protect_text;
    public TMP_Text protection_text;
    public TMP_Text body_covering_text;

    public int currentheight;
    public string height { get { return $"{currentheight / 1000} кг {currentheight % 1000} г"; } }
    public int maxheight = 70000;
    public TMP_Text height_text;

    public GameObject DeathPanel;

    [Header("Sleep Panel")]
    [SerializeField] GameObject SleepPanel;

    [Header("Sleep Panel")]
    public static bool isFire = false; 


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // Считываем данные из PlayerPrefs или устанавливаем значения по умолчанию
        year = PlayerPrefs.GetInt("Year", 1980);
        month = PlayerPrefs.GetInt("Month", 8);
        day = PlayerPrefs.GetInt("Day", 1);
        hour = PlayerPrefs.GetInt("Hour", 7);
        minute = PlayerPrefs.GetInt("Minute", 40);
        LoadCharacters();
        data.text = $"{hour}:{minute}, {day}.{month}.{year}";
    }
    public void StartTime()
    {
        if (changedata) return;
        changedata = true;

        // Запускаем корутины только если они не запущены
        if (!isTimeCoroutineRunning)
        {
            timeCoroutine = StartCoroutine(ChangeTime());
        }

        if (!isCharactersCoroutineRunning)
        {
            charactersCoroutine = StartCoroutine(ChangeCharacters());
        }

        if (!isWaterCoroutineRunning)
        {
            waterCoroutine = StartCoroutine(ChangeWater());
        }
    }

    public void StartSleep()
    {
        StartCoroutine(Sleep());
    }

    public void StopTime()
    {
        changedata = false;

        // Останавливаем корутины и сбрасываем флаги
        if (isTimeCoroutineRunning && timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
            isTimeCoroutineRunning = false;
        }

        if (isCharactersCoroutineRunning && charactersCoroutine != null)
        {
            StopCoroutine(charactersCoroutine);
            isCharactersCoroutineRunning = false;
        }

        if (isWaterCoroutineRunning && waterCoroutine != null)
        {
            StopCoroutine(waterCoroutine);
            isWaterCoroutineRunning = false;
        }
    }
    public void LoadCharacters()
    {
        death = PlayerPrefs.GetInt("death", 0);
        energy = PlayerPrefs.GetInt("energy", 0);
        food = PlayerPrefs.GetInt("food", 0);
        water = PlayerPrefs.GetInt("water", 0);
        radiation = PlayerPrefs.GetInt("radiation", 0);
        depature = PlayerPrefs.GetInt("depature", 0);
        blood = PlayerPrefs.GetInt("blood", 0);
        nastroenie = PlayerPrefs.GetInt("nastroenie", 100);
        deathtext.text = death.ToString();
        if (death > 70) deathtext.text = $"<color=red>{death}</color>";
        energytext.text = energy.ToString();
        foodtext.text = food.ToString();
        watertext.text = water.ToString();
        radiationtext.text = radiation.ToString();
        depaturetext.text = depature.ToString();
        bloodtext.text = blood.ToString();
        nastroenieText.text = nastroenie.ToString();
        nastroenieSlider.value = nastroenie;
        radiation_protect_text.text = "Защита от радиации: " + radiation_protect.ToString();
        protection_text.text = "Защита: " + protection.ToString();
        body_covering_text.text = "Покрытие тела: " + body_covering.ToString();
        Debug.Log(PlayerPrefs.GetInt("food"));
    }
    public void SaveCharacters()
    {
        PlayerPrefs.SetInt("death", death);
        PlayerPrefs.SetInt("energy", energy);
        PlayerPrefs.SetInt("food", food);
        PlayerPrefs.SetInt("water", water);
        PlayerPrefs.SetInt("radiation", radiation);
        PlayerPrefs.SetInt("depature", depature);
        PlayerPrefs.SetInt("blood", blood);
        PlayerPrefs.SetInt("nastroenie", nastroenie);
        Refresh();
    }
    public void Refresh()
    {
        deathtext.text = death.ToString();
        if (death > 70) deathtext.text = $"<color=red>{death}</color>";
        energytext.text = energy.ToString();
        foodtext.text = food.ToString();
        watertext.text = water.ToString();
        radiationtext.text = radiation.ToString();
        depaturetext.text = depature.ToString();
        bloodtext.text = blood.ToString();
        nastroenieText.text = nastroenie.ToString();
        nastroenieSlider.value = nastroenie;
        radiation_protect_text.text = "Защита от радиации: " + radiation_protect.ToString();
        protection_text.text = "Защита: " + protection.ToString();
        body_covering_text.text = "Покрытие тела: " + body_covering.ToString();
    }
    void SaveTime()
    {
        PlayerPrefs.SetInt("Year", year);
        PlayerPrefs.SetInt("Month", month);
        PlayerPrefs.SetInt("Day", day);
        PlayerPrefs.SetInt("Hour", hour);
        PlayerPrefs.SetInt("Minute", minute);
        PlayerPrefs.Save();
    }

    private bool isNight = false;
    public Light2D light1;

    IEnumerator ChangeTime()
    {
        isTimeCoroutineRunning = true;
        while (changedata)
        {
            // Каждую секунду будем изменять время
            yield return new WaitForSeconds(0.05f);

            // Аналогично добавляем минуту
            minute++;

            // Проверяем и корректируем время и дату
            if (minute >= 60)
            {
                if (PlayerPrefs.GetInt("light", 0) == 1)
                {
                    // Обновляем освещение при каждом изменении часа
                    UpdateLighting();
                }
                minute = 0;
                hour++;
                for (int i = 0; i < HealthManager.instance.diseasesHours.Count; i++)
                {
                    HealthManager.instance.diseasesHours[i]++;
                    HealthManager.instance.SaveData();
                    if (HealthManager.instance.diseasesHours[i] >= HealthManager.instance.diseases[i].timeHours)
                    {
                        Disease disease = HealthManager.instance.diseases[i];
                        HealthManager.instance.diseasesInt.RemoveAt(i);
                        HealthManager.instance.diseasesHours.RemoveAt(i);
                        HealthManager.instance.diseases.RemoveAt(i);
                        HealthManager.instance.eatingpower -= disease.eatingpower;
                        HealthManager.instance.wateringpower -= disease.wateringpower;
                        HealthManager.instance.energyingpower -= disease.energyingpower;
                        HealthManager.instance.deathingpower -= disease.deathingpower;
                        HealthManager.instance.radiationpower -= disease.radiationpower;
                        HealthManager.instance.depaturepower -= disease.depaturepower;
                        HealthManager.instance.bloodpower -= disease.bloodpower;
                        HealthManager.instance.SaveData();
                        var allDiseases = HealthManager.instance.diseasParent.GetComponentsInChildren<DiseaseSlot>();
                        allDiseases[i].Item = null;
                        Debug.Log("болезнь прошла");
                    }
                }

                if (hour >= 24)
                {
                    hour = 0;
                    day++;
                    if (day > daysInMonth[month - 1])
                    {
                        day = 1;
                        month++;
                        if (month > 12)
                        {
                            month = 1;
                            year++;
                        }
                    }
                }
            }

            SaveTime();
            data.text = $"{hour}:{minute}, {day}.{month}.{year}";
        }
        isTimeCoroutineRunning = false;
    }

    private void UpdateLighting()
    {
        // Определяем текущий период суток и получаем целевые настройки
        var targetSettings = GetTimeOfDaySettings();
        bool shouldBeNight = targetSettings.isNight;

        // Если состояние изменилось, применяем новые настройки
        if (shouldBeNight != isNight || !isNight)
        {
            // Плавно меняем интенсивность и цвет
            light1.DOIntensity(targetSettings.intensity, 5f);
            light1.DOColor(targetSettings.color, 5f);

            Debug.Log($"{targetSettings.timeName}! Intensity: {targetSettings.intensity}");
            isNight = shouldBeNight;
        }
    }

    // Структура для хранения настроек времени суток
    private struct TimeOfDaySettings
    {
        public string timeName;
        public float intensity;
        public Color color;
        public bool isNight;
    }

    // Метод для получения настроек в зависимости от времени
    private TimeOfDaySettings GetTimeOfDaySettings()
    {
        if (hour >= 20 || hour < 4)
        {
            // Ночь (20:00 - 4:00)
            return new TimeOfDaySettings
            {
                timeName = "Night",
                intensity = 0.4f,
                color = WeatherManager.instance.currentWeather.nightColor,
                isNight = true
            };
        }
        else if (hour >= 4 && hour < 7)
        {
            // Утро (4:00 - 7:00)
            return new TimeOfDaySettings
            {
                timeName = "Morning",
                intensity = 0.6f,
                color = WeatherManager.instance.currentWeather.morningColor,
                isNight = false
            };
        }
        else if (hour >= 7 && hour < 17)
        {
            // День (7:00 - 17:00)
            return new TimeOfDaySettings
            {
                timeName = "Day",
                intensity = 1f,
                color = WeatherManager.instance.currentWeather.dayColor,
                isNight = false
            };
        }
        else
        {
            // Вечер (17:00 - 20:00)
            return new TimeOfDaySettings
            {
                timeName = "Evening",
                intensity = 0.6f,
                color = WeatherManager.instance.currentWeather.eveningColor,
                isNight = false
            };
        }
    }
    IEnumerator ChangeCharacters()
    {
        isCharactersCoroutineRunning = true;
        while (changedata)
        {
            yield return new WaitForSeconds(2f);
            food += food_strength + HealthManager.instance.eatingpower;
            energy += energy_strength + HealthManager.instance.energyingpower;
            if (radiation_strength + aec_radiation_strength - radiation_protect > 0 && (radiation_strength > 0 || aec_radiation_strength > 0)) { radiation += (radiation_strength + aec_radiation_strength - radiation_protect); }
            else { 
                radiation += radiation_strength + HealthManager.instance.radiationpower;
            }
            if (food >= 60 | energy >= 60 | water >= 60)
            {
                if (nastroenie > 0) nastroenie -= 1;
                if (nastroenie < 20)
                {
                    HealthManager.instance.AddDisease(nastroenieDisease);
                }
            }
            else
            {
                if (nastroenie < 100) nastroenie += 1;
            }
            if (food >= 100 | energy >= 100 | radiation >= 30 | water >= 100)
            {
                death += death_strength + HealthManager.instance.deathingpower;
                if (death > 100) DeathPanel.SetActive(true);
            }
            else
            {
                if (death > 0) death -= 1;
            }
            SaveCharacters();
        }
        isCharactersCoroutineRunning = false;
    }
    IEnumerator Sleep()
    {
        StartTime();
        //SleepPanel.SetActive(true);
        Animations.instance.ShowSleep();
        int time = energy / 10;
        if (time < 1) time = 1;
        else if (time > 5) time = 5;
        yield return new WaitForSeconds(time);
        food += Mathf.RoundToInt(energy / 5) * food_strength + HealthManager.instance.eatingpower;
        water += Mathf.RoundToInt(energy / 10) * water_strength + HealthManager.instance.wateringpower;
        energy = 0;
        //SleepPanel.SetActive(false);
        Animations.instance.HideSleep();
        SaveCharacters();
        StopTime();
        StopCoroutine(Sleep());
    }
    IEnumerator ChangeWater()
    {
        isWaterCoroutineRunning = true;
        while (changedata)
        {
            yield return new WaitForSeconds(1f);
            water += water_strength + HealthManager.instance.wateringpower;
            SaveCharacters();
            SaveCharacters();
        }
        isWaterCoroutineRunning = false;
    }
}
