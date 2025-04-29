SELECT
	ChannelName,
	ChannelType,
	COUNT(p.WebPageItemID) AS 'Statistic'
FROM
	CMS_Channel c
INNER JOIN
	CMS_WebsiteChannel w ON c.ChannelID = w.WebsiteChannelChannelID
INNER JOIN
	CMS_WebPageItem p ON p.WebPageItemWebsiteChannelID = w.WebsiteChannelID
WHERE
	ChannelType = 'Website'
GROUP BY
	ChannelName, ChannelType
UNION
SELECT
	ChannelName,
	ChannelType,
	COUNT(i.HeadlessItemID) AS 'Statistic'
FROM
	CMS_Channel c
INNER JOIN
	CMS_HeadlessChannel h ON c.ChannelID = h.HeadlessChannelChannelID
INNER JOIN
	CMS_HeadlessItem i ON i.HeadlessItemHeadlessChannelID = h.HeadlessChannelID
WHERE
	ChannelType = 'Headless'
GROUP BY
	ChannelName, ChannelType
UNION
SELECT
	ChannelName,
	ChannelType,
	COUNT(e.EmailConfigurationID) AS 'Statistic'
FROM
	CMS_Channel c
INNER JOIN
	EmailLibrary_EmailConfiguration e ON c.ChannelID = e.EmailConfigurationEmailChannelID
WHERE
	ChannelType = 'Email'
GROUP BY
	ChannelName, ChannelType
ORDER BY Statistic