using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AiChangeWeather", menuName = "AiActions/AiChangeWeather")]
public class AiChangeWeather : AiEvent
{
    public Enemy enemy;
    public override void Activate()
    {
        base.Activate();
        WeatherManager.instance.currentWeather = WeatherManager.instance.weathers[Random.Range(0, WeatherManager.instance.weathers.Count)];
        NotifyManager.instance.SetPermanentNotify(eventName);
    }
}
