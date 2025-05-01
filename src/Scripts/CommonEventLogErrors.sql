SELECT TOP 10
	Count(EventDescription) AS Count,
	EventCode,
	Source, 
	EventDescription,
	MIN(EventTime) AS 'First Occurrence', 
	MAX(EventTime) AS 'Last Occurrence' 
FROM
	CMS_EventLog 
WHERE
	EventType = 'E' 
GROUP BY
	Source, EventCode, EventDescription 
ORDER BY
	Count DESC