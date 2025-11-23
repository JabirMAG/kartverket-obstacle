-- Add RapportComments column to ArchivedReports table
-- Run this SQL script manually if the migration fails

ALTER TABLE `ArchivedReports` 
ADD COLUMN `RapportComments` longtext CHARACTER SET utf8mb4 NOT NULL DEFAULT ('[]');

-- If you have existing data in ArchivedRapports table, migrate it:
-- UPDATE ArchivedReports ar
-- SET RapportComments = (
--     SELECT CONCAT('[', GROUP_CONCAT(
--         CONCAT('"', REPLACE(RapportComment, '"', '\\"'), '"')
--         SEPARATOR ','
--     ), ']')
--     FROM ArchivedRapports arr
--     WHERE arr.ArchivedReportId = ar.ArchivedReportId
-- )
-- WHERE EXISTS (
--     SELECT 1 FROM ArchivedRapports arr 
--     WHERE arr.ArchivedReportId = ar.ArchivedReportId
-- );

-- Then drop the ArchivedRapports table:
-- DROP TABLE IF EXISTS `ArchivedRapports`;

