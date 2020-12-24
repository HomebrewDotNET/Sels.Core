using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Configuration
{
    public interface IConfigProvider
    {
        string GetConnectionString(string key);
        string GetAppSetting(string key);
        T GetAppSetting<T>(string key);
        string GetSectionSetting(string key, string section);
        string GetSectionSetting(string key, params string[] sections);
        T GetSectionSetting<T>(string key, string section);
        T GetSectionSetting<T>(string key, params string[] sections);
        Dictionary<string, object> GetSectionAs(string section);
        Dictionary<string, object> GetSectionAs(params string[] sections);
        T GetSectionAs<T>(string section);
        T GetSectionAs<T>(params string[] sections);
    }
}
