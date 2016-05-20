var exceptions, getTime, onTraceArrives, servers;

exceptions = {};

getTime = function() {
  return new Date().getTime();
};

servers = {
  dmz14: 'NOL',
  dmz15: 'NOL',
  dmz16: 'NOL',
  tnvm110: 'NOL Test',
  tnvm111: 'NOL Test',
  localhost: 'Logging Service'
};

onTraceArrives = function(trace, ip) {
  var exceptionType, exceptionTypes, record;
  trace.Properties.Application = servers[trace.Properties['log4net:HostName']];
  exceptionTypes = trace.Message.match(/\w+\.*Exception\b/);
  if (exceptionTypes === null) {
    return;
  }
  exceptionType = exceptionTypes[0];
  trace.Properties['ExceptionType'] = exceptionType;
  record = exceptions[exceptionType] != null ? exceptions[exceptionType] : exceptions[exceptionType] = {
    count: 0,
    time: getTime()
  };
  if (record.count++ > 10 && (getTime() - record.time) < 10000) {
    fatal("Too many " + exceptionType);
    return exceptions[exceptionType] = null;
  }
};
//@ sourceMappingURL=Processing.js.map