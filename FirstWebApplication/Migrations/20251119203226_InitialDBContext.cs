using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class InitialDBContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create ArchivedReports table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ArchivedReports` (
                    `ArchivedReportId` int NOT NULL AUTO_INCREMENT,
                    `OriginalObstacleId` int NOT NULL,
                    `ObstacleName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `ObstacleHeight` double NOT NULL,
                    `ObstacleDescription` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
                    `GeometryGeoJson` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `ObstacleStatus` int NOT NULL,
                    `ArchivedDate` datetime(6) NOT NULL,
                    `RapportComments` longtext CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_ArchivedReports` PRIMARY KEY (`ArchivedReportId`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Add RapportComments column if it doesn't exist (for backward compatibility)
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'ArchivedReports' 
                    AND COLUMN_NAME = 'RapportComments'
                );
                SET @sql = IF(@col_exists = 0, 
                    'ALTER TABLE `ArchivedReports` ADD `RapportComments` longtext CHARACTER SET utf8mb4 NOT NULL DEFAULT (''[]'')',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Create AspNetRoles table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetRoles` (
                    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetUsers table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetUsers` (
                    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `DesiredRole` longtext CHARACTER SET utf8mb4 NULL,
                    `IaApproved` tinyint(1) NOT NULL,
                    `FullName` longtext CHARACTER SET utf8mb4 NULL,
                    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `Organization` longtext CHARACTER SET utf8mb4 NULL,
                    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `EmailConfirmed` tinyint(1) NOT NULL,
                    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
                    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
                    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
                    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
                    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
                    `TwoFactorEnabled` tinyint(1) NOT NULL,
                    `LockoutEnd` datetime(6) NULL,
                    `LockoutEnabled` tinyint(1) NOT NULL,
                    `AccessFailedCount` int NOT NULL,
                    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");
            
            // Add Organization column if it doesn't exist (for backward compatibility)
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'AspNetUsers' 
                    AND COLUMN_NAME = 'Organization'
                );
                SET @sql = IF(@col_exists = 0, 
                    'ALTER TABLE `AspNetUsers` ADD `Organization` longtext CHARACTER SET utf8mb4 NULL',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Create Feedback table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Feedback` (
                    `adviceID` int NOT NULL AUTO_INCREMENT,
                    `adviceMessage` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_Feedback` PRIMARY KEY (`adviceID`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create ObstaclesData table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ObstaclesData` (
                    `ObstacleId` int NOT NULL AUTO_INCREMENT,
                    `OwnerUserId` varchar(255) CHARACTER SET utf8mb4 NULL,
                    `ObstacleName` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `ObstacleHeight` double NOT NULL,
                    `ObstacleDescription` varchar(1000) CHARACTER SET utf8mb4 NULL,
                    `GeometryGeoJson` longtext CHARACTER SET utf8mb4 NULL,
                    `ObstacleStatus` int NOT NULL,
                    CONSTRAINT `PK_ObstaclesData` PRIMARY KEY (`ObstacleId`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetRoleClaims table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
                    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetUserClaims table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
                    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetUserLogins table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
                    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
                    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
                    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetUserRoles table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
                    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
                    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create AspNetUserTokens table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
                    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `Value` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
                    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create Rapports table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Rapports` (
                    `RapportID` int NOT NULL AUTO_INCREMENT,
                    `ObstacleId` int NOT NULL,
                    `RapportComment` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_Rapports` PRIMARY KEY (`RapportID`),
                    CONSTRAINT `FK_Rapports_ObstaclesData_ObstacleId` FOREIGN KEY (`ObstacleId`) REFERENCES `ObstaclesData` (`ObstacleId`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Create indexes only if they don't exist
            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetRoleClaims' AND INDEX_NAME = 'IX_AspNetRoleClaims_RoleId');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetRoles' AND INDEX_NAME = 'RoleNameIndex');
                SET @sql = IF(@index_exists = 0, 'CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUserClaims' AND INDEX_NAME = 'IX_AspNetUserClaims_UserId');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUserLogins' AND INDEX_NAME = 'IX_AspNetUserLogins_UserId');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUserRoles' AND INDEX_NAME = 'IX_AspNetUserRoles_RoleId');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers' AND INDEX_NAME = 'EmailIndex');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers' AND INDEX_NAME = 'UserNameIndex');
                SET @sql = IF(@index_exists = 0, 'CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Rapports' AND INDEX_NAME = 'IX_Rapports_ObstacleId');
                SET @sql = IF(@index_exists = 0, 'CREATE INDEX `IX_Rapports_ObstacleId` ON `Rapports` (`ObstacleId`)', 'SELECT 1');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedReports");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Rapports");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ObstaclesData");
        }
    }
}
