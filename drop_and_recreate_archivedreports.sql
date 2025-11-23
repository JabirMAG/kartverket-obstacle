-- Drop the ArchivedReports table to allow migrations to recreate it properly
-- WARNING: This will delete all archived reports data!

-- First, drop the ArchivedRapports table if it exists (foreign key dependency)
DROP TABLE IF EXISTS `ArchivedRapports`;

-- Then drop the ArchivedReports table
DROP TABLE IF EXISTS `ArchivedReports`;

-- After running this, run: dotnet ef database update --context ApplicationDBContext
-- This will recreate the table with all the correct columns including RapportComments




