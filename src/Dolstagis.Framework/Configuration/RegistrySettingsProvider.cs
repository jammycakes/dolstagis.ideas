﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Dolstagis.Framework.Configuration
{
    public class RegistrySettingsProvider : ISettingsProvider
    {
        private RegistryHive hKey;
        private RegistryView view;
        private string baseKey;

        public RegistrySettingsProvider
            (RegistryHive hKey, RegistryView view = RegistryView.Default, string baseKey = null)
        {
            this.hKey = hKey;
            this.view = view;
            this.baseKey = baseKey ?? ProductInfo.Company + "\\" + ProductInfo.Product;
        }

        public bool TryGetSetting(string ns, string key, out object value)
        {
            using (var root = RegistryKey.OpenBaseKey(hKey, view))
            using (var objKey = root.OpenSubKey(this.baseKey + "\\" + ns)) {
                if (objKey == null) {
                    value = null;
                    return false;
                }
                else {
                    value = objKey.GetValue(key);
                    return true;
                }
            }
        }

        private static string ToString(object obj)
        {
            if (obj == null) return null;
            return (obj as string) ?? obj.ToString();
        }

        public ConnectionStringSettings GetConnectionString(string connectionStringName)
        {
            using (var root = RegistryKey.OpenBaseKey(hKey, view))
            using (var objKey = root.OpenSubKey("SOFTWARE\\" + this.baseKey + "\\ConnectionStrings\\" + connectionStringName)) {
                if (objKey == null) {
                    return null;
                }
                else {
                    var connectionString = ToString(objKey.GetValue(null));
                    var providerName = ToString(objKey.GetValue("Provider"));
                    return new ConnectionStringSettings(connectionStringName, connectionString, providerName);
                }
            }
        }
    }
}
