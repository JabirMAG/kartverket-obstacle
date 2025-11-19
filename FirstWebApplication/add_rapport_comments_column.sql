-- SQL script to manually add the RapportComments column to ArchivedReports table
-- Run this script directly against your MySQL database if the migration didn't create the column
-- 
-- Connection details from appsettings.json:
-- Server: localhost
-- Port: 3307
-- Database: KartverketDB
-- User: root
-- Password: Mammaerbest1
--
-- You can run this using:
-- mysql -h localhost -P 3307 -u root -pMammaerbest1 KartverketDB < add_rapport_comments_column.sql
-- OR connect using MySQL Workbench, HeidiSQL, or any MySQL client

USE KartverketDB;

-- Add the RapportComments column
-- Note: This will fail if the column already exists, which is fine - just means it's already there
ALTER TABLE `ArchivedReports` 
ADD COLUMN `RapportComments` LONGTEXT NOT NULL DEFAULT '[]';

