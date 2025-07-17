SELECT
    ChannelName,
    ChannelType,
    CASE
        WHEN ChannelType = 'Website' THEN (
			SELECT COUNT(p.WebPageItemID)
			FROM CMS_WebPageItem p
			INNER JOIN CMS_WebsiteChannel w ON p.WebPageItemWebsiteChannelID = w.WebsiteChannelID
			WHERE w.WebsiteChannelChannelID = ChannelID
		)
        WHEN ChannelType = 'Headless' THEN (
			SELECT COUNT(i.HeadlessItemID)
			FROM CMS_HeadlessItem i
			INNER JOIN CMS_HeadlessChannel h ON i.HeadlessItemHeadlessChannelID = h.HeadlessChannelID
			WHERE h.HeadlessChannelChannelID = ChannelID
		)
        WHEN ChannelType = 'Email' THEN (
			SELECT COUNT(e.EmailConfigurationID)
			FROM EmailLibrary_EmailConfiguration e
			WHERE e.EmailConfigurationEmailChannelID = ChannelID
		)
        ELSE 0
    END AS 'Statistic'
FROM
    CMS_Channel
WHERE
    ChannelType IN ('Website', 'Headless', 'Email')
GROUP BY
    ChannelID, ChannelName, ChannelType
ORDER BY
    Statistic