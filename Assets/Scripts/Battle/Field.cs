using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field 
{
    
    public Weather Weather { get; set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(WeatherID weatherId)
    {
        Weather = WeatherDB.Weathers[weatherId];
        Weather.Id = weatherId;
    }
}
