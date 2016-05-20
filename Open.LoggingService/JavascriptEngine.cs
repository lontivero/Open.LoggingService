using System;
using System.Configuration;
using System.IO;
using System.Net;
using Jint;
using log4net.Core;

namespace Open.LoggingService
{
    class JavascriptEngine
    {
        private readonly Engine _engine;
        private FileSystemWatcher _fileSystemWatcher;
        private bool _hasErrors;

        public JavascriptEngine()
        {
            _engine = new Engine(cfg => cfg.AllowClr());
            WatchScriptFile();
        }

        public void LoadAndExecuteScriptFile()
        {
            var scriptPath = ConfigurationManager.AppSettings["loggingService.Script.Path"];
            _hasErrors = false;

            if(File.Exists(scriptPath))
            {
                try
                {
                    _engine.Execute(File.ReadAllText(scriptPath));
                    _hasErrors = false;
                }
                catch (Exception e)
                {
                    _hasErrors = true;
                    Service.ServiceLog.WriteEntry("There was a problem parsing the processing script\n" + e);
                }
            }
        }

        private void WatchScriptFile()
        {
            var assemblyDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            _fileSystemWatcher = new FileSystemWatcher() {
                Path = assemblyDirectory,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "processing.js"
            };
            _fileSystemWatcher.Changed += OnChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                LoadAndExecuteScriptFile();
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        public void SetGlobal(string name, object value)
        {
            _engine.SetValue(name, value);
        }

        public void SetGlobal(string name, Action<object> value)
        {
            _engine.SetValue(name, value);
        }

        public bool OnTraceArrives(LoggingEventDataWrapper trace, IPEndPoint endPoint)
        {
            if(_hasErrors) return true;
            var ret = _engine.Invoke("onTraceArrives", trace, endPoint.Address.ToString());
            return ret.IsNull() || ret.IsUndefined() || (ret.IsBoolean() && ret.AsBoolean());
        }
    }

    class Configuration
    {
        
    }
}
