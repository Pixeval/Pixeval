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
using System.Diagnostics.Eventing.Reader;
using System.Xml;

namespace Pixeval.Objects.Exceptions.Logger
{
    public class ApplicationLogStack
    {
        private readonly string id;
        private readonly string provider;

        public ApplicationLogStack(string id, string provider)
        {
            this.id = id;
            this.provider = provider;
        }

        public ApplicationLog GetFirst()
        {
            var doc = GetFirstMatchRecord();
            if (doc == null) return null;
            var xml = new XmlDocument();
            xml.LoadXml(doc);
            var xmlNsManager = new XmlNamespaceManager(xml.NameTable);
            xmlNsManager.AddNamespace("event", "http://schemas.microsoft.com/win/2004/08/events/event");
            return new ApplicationLog
            {
                Data = xml.SelectSingleNode("event:Event/event:EventData/event:Data", xmlNsManager).FirstChild.Value,
                Creation = DateTime.Parse(xml.SelectSingleNode("//@SystemTime").Value)
            };
        }

        private string GetFirstMatchRecord()
        {
            var query = new EventLogQuery("Application", PathType.LogName, $"*[System/Level=2] and *[System/Provider/@Name=\"{provider}\"]")
            {
                ReverseDirection = true
            };
            var reader = new EventLogReader(query);
            EventRecord record;
            while ((record = reader.ReadEvent()) != null)
            {
                var doc = record.ToXml();
                if (doc.Contains(id, StringComparison.OrdinalIgnoreCase)) return doc;
            }

            return null;
        }
    }
}
