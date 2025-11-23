-- Fix ArchivedReports table by dropping and recreating
-- This will delete all archived reports data!

-- Step 1: Drop the ArchivedRapports table first (has foreign key to ArchivedReports)
DROP TABLE IF EXISTS `ArchivedRapports`;

-- Step 2: Drop the ArchivedReports table
DROP TABLE IF EXISTS `ArchivedReports`;

-- Step 3: Remove migration history entries so they can be re-applied
DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251112105135_InitialCreate';
DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251113114246_CombineArchivedTables';

-- After running this script, run:
-- dotnet ef database update --context ApplicationDBContext
-- This will recreate the tables with all correct columns including RapportComments




