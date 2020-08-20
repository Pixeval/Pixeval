#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Pixeval.Wpf.Objects.Primitive;
using Pixeval.Wpf.Persisting;
using Pixeval.Wpf.Properties;
using Pixeval.Wpf.ViewModel;

namespace Pixeval.Wpf.Objects.I18n
{
    public static partial class AkaI18N
    {
        private static List<XmlNode> _i18NXmlNodes;

        public static string GetResource(string key)
        {
            if (_i18NXmlNodes == null)
            {
                // we load settings here because we need to get the culture field
                if (File.Exists(Path.Combine(AppContext.SettingsFolder, "settings.json")))
                {
                    var s = File.ReadAllText(Path.Combine(AppContext.SettingsFolder, "settings.json")).FromJson<Settings>();
                    InitializeXmlNodes(CultureInfo.GetCultureInfo(s.Culture));
                }
                else InitializeXmlNodes(CultureInfo.CurrentCulture);
            }

            return GetValueByKey(key);
        }

        public static void Reload(I18NOption option)
        {
            InitializeXmlNodes(CultureInfo.GetCultureInfo(option.Name));
            foreach (var prop in typeof(AkaI18N).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                prop.SetValue(null, GetValueByKey(prop.Name));
            }
        }

        private static void InitializeXmlNodes(CultureInfo culture)
        {
            var resName = $"resx_{culture.Name.ToLower()}";
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(Resources.ResourceManager.GetString(resName, culture));
            }
            catch (IOException)
            {
                xml.LoadXml(Resources.ResourceManager.GetString("resx_zh-cn", culture));
            }
            _i18NXmlNodes = xml.SelectSingleNode("/localization").ChildNodes.Cast<XmlNode>().ToList();
        }

        public static string GetCultureAcceptLanguage()
        {
            return CultureInfo.CurrentCulture.Name.ToLower();
        }

        private static string GetValueByKey(string key)
        {
            return _i18NXmlNodes.Find(p => p.Attributes["key"].Value == key)?.Attributes["value"].Value;
        }
    }
}
