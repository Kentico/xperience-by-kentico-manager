SELECT
	UserID,
	UserName
FROM
	CMS_User
WHERE
	UserEnabled = 1
	AND UserAdministrationAccess = 1