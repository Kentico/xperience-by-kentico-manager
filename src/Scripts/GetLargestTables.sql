SELECT TOP 10
    t.name AS 'Table',
    SUM(ps.row_count) AS 'Row count',
    CAST(ROUND(((SUM(ps.reserved_page_count) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS 'Total space (MB)',
    CAST(ROUND(((SUM(ps.used_page_count) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS 'Used space (MB)'
FROM
    sys.tables t
INNER JOIN
    sys.dm_db_partition_stats ps ON t.object_id = ps.object_id
WHERE
    t.name NOT LIKE 'dt%'
    AND t.is_ms_shipped = 0
    AND ps.index_id IN (0, 1)
GROUP BY
    t.name
ORDER BY
    'Total space (MB)' DESC,
    t.name;