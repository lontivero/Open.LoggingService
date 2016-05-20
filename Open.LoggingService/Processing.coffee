# Global variables
# These are long-life variables and their values persist between invocations.
exceptions = {}

getTime = -> new Date().getTime() 
servers =
	dmz14: 'NOL'
	dmz15: 'NOL'
	dmz16: 'NOL'
	tnvm110: 'NOL Test'
	tnvm111: 'NOL Test'
	localhost: 'Logging Service'

# Invoked when a new trace package arrives
onTraceArrives = (trace, ip)->
	
	#try
		trace.Properties.Application = servers[trace.Properties['log4net:HostName']] 

		exceptionTypes = trace.Message.match(/\w+\.*Exception\b/)
		return if exceptionTypes is null
		exceptionType = exceptionTypes[0]

		trace.Properties['ExceptionType'] = exceptionType
		#print "exceptionType = #{exceptionType}"
		record = exceptions[exceptionType] ?= count: 0, time: getTime()
		#print record

		if record.count++ > 10 and (getTime() - record.time) < 10000
			fatal "Too many #{exceptionType}"
			exceptions[exceptionType] = null
		
	#catch e
	#	event e.toString()
