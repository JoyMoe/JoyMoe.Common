using System;
using Microsoft.AspNetCore.Mvc;

namespace JoyMoe.Common.Json
{
    public static class JsonOptionsExtensions
    {
        /// <summary>
        /// Configure <see cref="JsonOptions"/> to use <see cref="SnakeCaseNamingPolicy"/>
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonOptions UseSnakeCaseNamingPolicy(this JsonOptions option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            option.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();

            return option;
        }

        /// <summary>
        /// Configure <see cref="JsonOptions"/> to use <see cref="IsoDateTimeConverter"/>
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonOptions UseIsoDateTimeConverter(this JsonOptions option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            option.JsonSerializerOptions.Converters.Add(new IsoDateTimeConverter());

            return option;
        }
    }
}
