SELECT
	WorkspaceDisplayName,
	COUNT(ContentItemID) AS 'Content items'
FROM
	CMS_Workspace w
INNER JOIN
	CMS_ContentItem i ON w.WorkspaceID = i.ContentItemWorkspaceID
GROUP BY
	WorkspaceDisplayName
ORDER BY 'Content items'