using System;
using JoyMoe.Common.Json;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonOptionsExtensions
    {
        /// <summary>
        /// Configure <see cref="JsonOptions" /> to use <see cref="SnakeCaseNamingPolicy" />
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonOptions UseSnakeCaseNamingPolicy(this JsonOptions option)
        {
            if (option?.JsonSerializerOptions == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            option.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();

            return option;
        }

        /// <summary>
        /// Configure <see cref="JsonOptions" /> to use <see cref="Int64StringConverter" />
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonOptions UseInt64StringConverter(this JsonOptions option)
        {
            if (option?.JsonSerializerOptions == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            option.JsonSerializerOptions.Converters.Add(new Int64StringConverter());

            return option;
        }

        /// <summary>
        /// Configure <see cref="JsonOptions" /> to use <see cref="IsoDateTimeConverter" />
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonOptions UseIsoDateTimeConverter(this JsonOptions option)
        {
            if (option?.JsonSerializerOptions == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            option.JsonSerializerOptions.Converters.Add(new IsoDateTimeConverter());

            return option;
        }
    }
}
