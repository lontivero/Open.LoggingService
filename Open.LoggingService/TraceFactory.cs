using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using log4net.Core;
using log4net.Util;

namespace Open.LoggingService
{
    internal class TraceFactory
    {
        private static readonly XmlParserContext Ctx;

        static TraceFactory()
        {
            var mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("log4net", "urn:ignore");

            Ctx = new XmlParserContext(null, mgr, null, XmlSpace.None);
        }

        public static LoggingEventDataWrapper Parse(string traceStr)
        {
            var evntData = new LoggingEventDataWrapper();
            var levels = new Dictionary<string, Level> {
                                                           {"FATAL", Level.Fatal},
                                                           {"ERROR", Level.Error},
                                                           {"DEBUG", Level.Debug},
                                                           {"INFO", Level.Info},
                                                           {"CRITICAL", Level.Critical},
                                                           {"WARN", Level.Warn}
                                                       };

            using (var reader = XmlReader.Create(new StringReader(traceStr), null, Ctx))
            {
                var doc = XDocument.Load(reader);
                var root = doc.Root;
                evntData.LoggerName = root.Attribute("logger").GetValueOrNull();
                evntData.TimeStamp = DateTime.Parse(root.Attribute("timestamp").GetValueOrNull());
                evntData.Level = levels[root.Attribute("level").GetValueOrNull()];
                evntData.ThreadName = root.Attribute("thread").GetValueOrNull();
                evntData.Domain = root.Attribute("domain").GetValueOrNull();
                evntData.UserName = root.Attribute("username").GetValueOrNull();
                evntData.Identity = root.Attribute("identity").GetValueOrNull();
                evntData.Message = root.Element(XName.Get("message", "urn:ignore")).GetValueOrNull();
                evntData.ExceptionString = root.Element(XName.Get("exception", "urn:ignore")).GetValueOrNull(); 

                var properties = root.Element(XName.Get("properties", "urn:ignore"));
                if(properties != null)
                {
                    evntData.Properties = new PropertiesDictionary();
                    foreach (var property in properties.Elements(XName.Get("data", "urn:ignore")))
                    {
                        var key = property.Attribute("name").Value;
                        var value = property.Attribute("value").Value;
                        evntData.Properties[key] = value;
                    }
                }
            }
            return evntData;
        }
    }

    internal class LoggingEventDataWrapper
    {
        private LoggingEventData _data;

        public LoggingEventData Data
        {
            get { return _data; }
        }

        public string Domain
        {
            get { return _data.Domain; }
            set { _data.Domain = value; }
        }
        public string ExceptionString
        {
            get { return _data.ExceptionString; }
            set { _data.ExceptionString = value; }
        }

        public string Identity
        {
            get { return _data.Identity; }
            set { _data.Identity = value; }
        }
        public Level Level
        {
            get { return _data.Level; }
            set { _data.Level = value; }
        }
        public LocationInfo LocationInfo
        {
            get { return _data.LocationInfo; }
            set { _data.LocationInfo = value; }
        }
        public string LoggerName
        {
            get { return _data.LoggerName; }
            set { _data.LoggerName = value; }
        }
        public string Message
        {
            get { return _data.Message; }
            set
            {
                _data.Message = value;
            }
        }
        public PropertiesDictionary Properties
        {
            get { return _data.Properties; }
            set { _data.Properties = value; }
        }
        public string ThreadName
        {
            get { return _data.ThreadName; }
            set { _data.ThreadName = value; }
        }
        public DateTime TimeStamp
        {
            get { return _data.TimeStamp; }
            set { _data.TimeStamp = value; }
        }
        public string UserName
        {
            get { return _data.UserName; }
            set { _data.UserName = value; }
        }
    }
}