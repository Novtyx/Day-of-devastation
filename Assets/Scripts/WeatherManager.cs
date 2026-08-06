using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager instance;
    public List<Weather> weathers = new List<Weather>();
    public Weather currentWeather;
    public float delay = 60f;
    private void Awake()
    {
        instance = this;
        StartCoroutine(WeatherChanger());
    }
    private IEnumerator WeatherChanger()
    {
        while (true)
        {
            currentWeather = weathers[Random.Range(0, weathers.Count)];
            NotifyManager.instance.SetMinNotify($"погода: {currentWeather.Name}");
            yield return new WaitForSeconds(delay);
        }
    }
}
