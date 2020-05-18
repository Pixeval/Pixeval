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
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using PropertyChanged;

namespace Pixeval.Objects.I18n
{
    public static partial class AkaI18N
    {
        [DoNotNotify]
        private static IEnumerable<XmlNode> _i18NXmlNodes;

        public static string GetResource(string key)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            var fileName =
                new Uri(
                    $"pack://application:,,,/Pixeval;component/Objects/I18n/Xml/resx_{cultureInfo.Name.ToLower()}.xml");
            if (_i18NXmlNodes == null)
            {
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

            return GetValueByKey(key);
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