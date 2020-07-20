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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.Objects.I18n
{
    public static partial class AkaI18N
    {
        private static IEnumerable<XmlNode> _i18NXmlNodes;

        public static string GetResource(string key)
        {
            if (_i18NXmlNodes == null)
            {
                // we load settings here because we need to get the culture field
                if (File.Exists(Path.Combine(AppContext.SettingsFolder, "settings.json")))
                {
                    var s = File.ReadAllText(Path.Combine(AppContext.SettingsFolder, "settings.json")).FromJson<Settings>();
                    InitializeXmlNodes(s.Culture);
                }
                else InitializeXmlNodes(CultureInfo.CurrentCulture.Name);
            }

            return GetValueByKey(key);
        }

        public static void Reload(I18NOption option)
        {
            InitializeXmlNodes(option.Name);
            foreach (var prop in typeof(AkaI18N).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                prop.SetValue(null, GetValueByKey(prop.Name));
            }
        }

        private static void InitializeXmlNodes(string culture)
        {
            var fileName =
                new Uri(
                    $"pack://application:,,,/Pixeval;component/Objects/I18n/Xml/resx_{culture.ToLower()}.xml");
            StreamResourceInfo streamResourceInfo;
            try
            {
                streamResourceInfo = Application.GetResourceStream(fileName);
            }
            catch (IOException)
            {
                streamResourceInfo =
                    Application.GetResourceStream(
                        new Uri("pack://application:,,,/Pixeval;component/Objects/I18n/Xml/resx_zh-cn.xml"));
            }

            var xml = new XmlDocument();
            xml.Load(streamResourceInfo?.Stream);
            _i18NXmlNodes = xml.SelectSingleNode("/localization").ChildNodes.Cast<XmlNode>();
        }

        public static string GetCultureAcceptLanguage()
        {
            return CultureInfo.CurrentCulture.Name.ToLower();
        }

        private static string GetValueByKey(string key)
        {
            return _i18NXmlNodes.First(p => p.Attributes["key"].Value == key).Attributes["value"].Value;
        }
    }
}
