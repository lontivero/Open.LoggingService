using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Open.LoggingService.Network;
using log4net;
using log4net.Core;

namespace Open.LoggingService
{
    public class Service : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Service));
        private readonly UdpListener _listener;
        private EventLog _eventLog;

        static void Main(string[] args)
        {
            var serviceToRun = new Service();

            if (Environment.UserInteractive)
            {
                serviceToRun.OnStart(args);
                Console.WriteLine("Press any key to stop program");
                Console.Read();
                serviceToRun.OnStop();
            }
            else
            {
                Run(new ServiceBase[] { serviceToRun });
            }
        }

        public Service()
        {
            InitializeEventLogger();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var portStr = ConfigurationManager.AppSettings["loggingService.Listener.Port"] ?? "8111";
            int port;
            if(int.TryParse(portStr, out port) && port > 0 && port < short.MaxValue)
            {
                _listener = new UdpListener(8911);
                _listener.UdpPacketReceived += OnTraceArrives;
            }
            else
            {
                
            }
        }

        private void InitializeEventLogger()
        {
            AutoLog = false;
            if (!EventLog.SourceExists("Logging Service"))
            {
                EventLog.CreateEventSource("Logging Service", "Logging Service");
            }
            _eventLog = new EventLog { Source = "Logging Service", Log = "Logging Service" };
        }

        private void OnTraceArrives(object sender, UdpPacketReceivedEventArgs e)
        {
            var traceStr = Encoding.UTF8.GetString(e.Data);
            var trace = TraceFactory.Parse(traceStr);
            Log.Logger.Log(new LoggingEvent(trace));
        }

        protected override void OnStart(string[] args)
        {
            _listener.Start();
            _eventLog.WriteEntry("Service Started", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            _listener.Stop();
            _eventLog.WriteEntry("Service Stopped", EventLogEntryType.Information);
        }

        protected override void OnPause()
        {
            _eventLog.WriteEntry("Service Paused", EventLogEntryType.Information);
        }

        protected override void OnContinue()
        {
            _eventLog.WriteEntry("Service Continue", EventLogEntryType.Information);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _eventLog.WriteEntry(e.ExceptionObject.ToString(), EventLogEntryType.Error);
        }
    }

    internal class TraceFactory
    {
        private static readonly XmlParserContext Ctx;

        static TraceFactory()
        {
            var mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("log4net", "urn:ignore");

            Ctx = new XmlParserContext(null, mgr, null, XmlSpace.None);
        }

        public static LoggingEventData Parse(string traceStr)
        {
            var evntData = new LoggingEventData();

            using (var reader = XmlReader.Create(new StringReader(traceStr), null, Ctx))
            {
                var doc = XDocument.Load(reader);
                var root = doc.Root;
                evntData.LoggerName = root.Attribute("logger").GetValueOrNull();
                evntData.TimeStamp = DateTime.Parse(root.Attribute("timestamp").GetValueOrNull());
                evntData.Level = (Level)Enum.Parse(typeof(Level), root.Attribute("level").GetValueOrNull());
                evntData.ThreadName = root.Attribute("thread").GetValueOrNull();
                evntData.Domain = root.Attribute("domain").GetValueOrNull();
                evntData.UserName = root.Attribute("username").GetValueOrNull();
                evntData.Identity = root.Attribute("identity").GetValueOrNull();
                evntData.Message = root.Element(XName.Get("message", "urn:ignore")).GetValueOrNull();
                evntData.ExceptionString = root.Element(XName.Get("exception", "urn:ignore")).GetValueOrNull(); 

                var properties = root.Element(XName.Get("properties", "urn:ignore"));
                if(properties != null)
                {
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

    static class XExtensions
    {
        public static string GetValueOrNull(this XAttribute node)
        {
            return node != null ? node.Value : null;
        }
        public static string GetValueOrNull(this XElement node)
        {
            return node != null ? node.Value : null;
        }

    }
}
