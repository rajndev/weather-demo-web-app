﻿using AutoMapper;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WeatherApp.BLL.HelperClasses;
using WeatherApp.BLL.Interfaces;
using WeatherApp.BLL.Models;

namespace WeatherApp.BLL.Services
{
    public class WeatherService : IWeatherService
    {

        private readonly IMapper _mapper;
        private WeatherAPIProcessor APIProcessorSingleton = WeatherAPIProcessor.GetInstance();
        public WeatherService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<object> GetCurrentWeather(string cityName, string apiKey)
        {
            APIProcessorSingleton.BaseAPIUrl = BaseAPIUrls.GET_CURRENT_WEATHER;
            var query = $"q={cityName}&appid={apiKey}&units=imperial";
            var currentWeather = await APIProcessorSingleton.GetCurrentWeather(query);

            if (currentWeather != null)
            {
                if (currentWeather.GetType() == typeof(string))
                {
                    return "invalid city name";
                }
                else
                {
                    var currentWeatherCast = (WeatherInfoRoot)currentWeather;

                    var currentWeatherDTO = _mapper.Map<WeatherInfoDTO>(currentWeatherCast);

                    var currentDateTime = GetDateTimeFromEpoch(
                        currentWeatherCast.Sys.Sunrise,
                        currentWeatherCast.Sys.Sunset,
                        currentWeatherCast.Dt,
                        currentWeatherCast.Timezone);

                    currentWeatherDTO.CityDate = currentDateTime.Item1;
                    currentWeatherDTO.CityTime = currentDateTime.Item2;
                    currentWeatherDTO.IsDayTime = currentDateTime.Item3;

                    //capitalize each word in the city name

                    cityName = CapitalizeText(cityName);

                    currentWeatherDTO.CityName = cityName;

                    return currentWeatherDTO;
                }
            }

            return "service unavailable";
        }

        private string CapitalizeText(string cityName)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            cityName = textInfo.ToTitleCase(cityName);
            return cityName;
        }

        private Tuple<string, string, bool> GetDateTimeFromEpoch(long sunrise, long sunset, long currentTime, long timezone)
        {
            //DateTime dtime = new DateTime(1970, 1, 1, 0, 0, 0);

            //var dateTimeOffset = dtime.AddSeconds(currentTime + timezone);

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(currentTime + timezone);

            bool isDaytime = currentTime > sunrise && currentTime < sunset;

            var humanReadabledate = dateTimeOffset.DateTime.ToString("D");
            var HumanReadabletime = dateTimeOffset.DateTime.ToString("t");

            return Tuple.Create(humanReadabledate, HumanReadabletime, isDaytime);
        }
    }
}
