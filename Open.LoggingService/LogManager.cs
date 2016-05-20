using System;
using Open.LoggingService.Utils;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Open.LoggingService
{
    class LogManager
    {
        private readonly ILog _log = log4net.LogManager.GetLogger(typeof(Service));
        private readonly BlockingQueue<LoggingEventData> _queue = new BlockingQueue<LoggingEventData>(); 
        private readonly WorkScheduler _scheduler = new WorkScheduler();

        internal ILog Logger
        {
            get { return _log; }
        }

        public LogManager()
        {
            XmlConfigurator.Configure();
            _scheduler.QueueForever(DumpLogEntries, TimeSpan.FromMilliseconds(100));
            _scheduler.Start();
        }

        private void DumpLogEntries()
        {
            var entryCount = _queue.Count;
            for (var i = 0; i < entryCount; i++)
            {
                var entry = _queue.Take();
                _log.Logger.Log(new LoggingEvent(entry));
            }
        }

        internal void Enqueue(LoggingEventData trace)
        {
            _queue.Add(trace);
        }

        ~LogManager()
        {
            _scheduler.Stop();
            DumpLogEntries();
        }
    }
}
