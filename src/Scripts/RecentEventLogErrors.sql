SELECT TOP 10
	EventTime,
	UserID,
	EventCode,
	Source,
	EventDescription
FROM
	CMS_EventLog
WHERE
	EventType = 'E'
ORDER BY
	EventTime DESC