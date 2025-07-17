SELECT
	ClassName AS 'Class missing table',
	ClassTableName AS 'Expected table'
FROM
	CMS_Class
WHERE
	ClassContentTypeType IS NULL
	AND ClassTableName NOT IN (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES)
ORDER BY
	ClassDisplayName,
	ClassName,
	ClassTableName