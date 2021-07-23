﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Configuration
{
    public interface IConfigProvider
    {
        /// <summary>
        /// Get Connection String with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of connection string name</param>
        /// <returns>Connection string</returns>
        string GetConnectionString(string name, bool required = true);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the AppSettings section.
        /// </summary>
        /// <param name="name">Name of config property</param>
        /// <returns>App setting value</returns>
        string GetAppSetting(string name, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the AppSettings section.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="name">Name of config property</param>
        /// <returns>App setting value converted to <typeparamref name="T"/></returns>
        T GetAppSetting<T>(string name, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the <paramref name="section"/> section.
        /// </summary>
        /// <param name="name">Name of config property</param>
        /// <param name="section">Name of config section</param>
        /// <returns>Setting value</returns>
        string GetSectionSetting(string name, string section, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the <paramref name="sections"/> sections.
        /// </summary>
        /// <param name="name">Name of config property</param>
        /// <param name="sections">Name of the config sections</param>
        /// <returns>Setting value</returns>
        string GetSectionSetting(string name, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null, params string[] sections);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the <paramref name="section"/> section.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="name">Name of config property</param>
        /// <param name="section">Name of config section</param>
        /// <returns>Setting value converted to <typeparamref name="T"/></returns>
        T GetSectionSetting<T>(string name, string section, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null);
        /// <summary>
        /// Get setting with <paramref name="name"/> from the <paramref name="sections"/> sections.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="name">Name of config property</param>
        /// <param name="sections">Name of the config sections</param>
        /// <returns>Setting value converted to <typeparamref name="T"/></returns>
        T GetSectionSetting<T>(string name, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null, params string[] sections);
        /// <summary>
        /// Get all the properties in <paramref name="section"/> as key value pairs in a dictionary.
        /// </summary>
        /// <param name="section">Name of config section</param>
        /// <returns>Key value pairs</returns>
        Dictionary<string, object> GetSectionAs(string section, bool required = true, Predicate<Dictionary<string, object>> validator = null, Func<Dictionary<string, object>, string> misconfigurationReason = null);
        /// <summary>
        /// Get all the properties in the <paramref name="sections"/> as key value pairs in a dictionary.
        /// </summary>
        /// <param name="sections">Name of config section</param>
        /// <returns>Key value pairs</returns>
        Dictionary<string, object> GetSectionAs(bool required = true, Predicate<Dictionary<string, object>> validator = null, Func<Dictionary<string, object>, string> misconfigurationReason = null, params string[] sections);
        /// <summary>
        /// Get the <paramref name="section"/> from config and convert it to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="section">Name of config section</param>
        /// <returns>Section converted to <typeparamref name="T"/></returns>
        T GetSectionAs<T>(string section, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null);
        /// <summary>
        /// Get the last section of <paramref name="sections"/> from config and convert it to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="sections">Name of config section</param>
        /// <returns>Section converted to <typeparamref name="T"/></returns>
        T GetSectionAs<T>(bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null, params string[] sections);
    }
}
