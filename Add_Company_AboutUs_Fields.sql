-- Script to add About Us fields to Company table
-- Run this script in your SQL Server database

USE [Otmsdb]; -- Replace with your actual database name
GO

-- Add About Us Section Fields
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'AboutUsTitle')
BEGIN
    ALTER TABLE [Company]
    ADD [AboutUsTitle] NVARCHAR(200) NULL;
    PRINT 'Column AboutUsTitle added successfully';
END
ELSE
BEGIN
    PRINT 'Column AboutUsTitle already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'AboutUsDescription1')
BEGIN
    ALTER TABLE [Company]
    ADD [AboutUsDescription1] NVARCHAR(MAX) NULL;
    PRINT 'Column AboutUsDescription1 added successfully';
END
ELSE
BEGIN
    PRINT 'Column AboutUsDescription1 already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'AboutUsDescription2')
BEGIN
    ALTER TABLE [Company]
    ADD [AboutUsDescription2] NVARCHAR(MAX) NULL;
    PRINT 'Column AboutUsDescription2 added successfully';
END
ELSE
BEGIN
    PRINT 'Column AboutUsDescription2 already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'AboutUsImageUrl')
BEGIN
    ALTER TABLE [Company]
    ADD [AboutUsImageUrl] NVARCHAR(300) NULL;
    PRINT 'Column AboutUsImageUrl added successfully';
END
ELSE
BEGIN
    PRINT 'Column AboutUsImageUrl already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'AboutUsImageAlt')
BEGIN
    ALTER TABLE [Company]
    ADD [AboutUsImageAlt] NVARCHAR(100) NULL;
    PRINT 'Column AboutUsImageAlt added successfully';
END
ELSE
BEGIN
    PRINT 'Column AboutUsImageAlt already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'ExperienceNumber')
BEGIN
    ALTER TABLE [Company]
    ADD [ExperienceNumber] NVARCHAR(50) NULL;
    PRINT 'Column ExperienceNumber added successfully';
END
ELSE
BEGIN
    PRINT 'Column ExperienceNumber already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'ExperienceText')
BEGIN
    ALTER TABLE [Company]
    ADD [ExperienceText] NVARCHAR(100) NULL;
    PRINT 'Column ExperienceText added successfully';
END
ELSE
BEGIN
    PRINT 'Column ExperienceText already exists';
END
GO

-- Add Stats Fields
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'HappyTravelersCount')
BEGIN
    ALTER TABLE [Company]
    ADD [HappyTravelersCount] INT NULL;
    PRINT 'Column HappyTravelersCount added successfully';
END
ELSE
BEGIN
    PRINT 'Column HappyTravelersCount already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'CountriesCoveredCount')
BEGIN
    ALTER TABLE [Company]
    ADD [CountriesCoveredCount] INT NULL;
    PRINT 'Column CountriesCoveredCount added successfully';
END
ELSE
BEGIN
    PRINT 'Column CountriesCoveredCount already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Company') AND name = 'YearsExperienceCount')
BEGIN
    ALTER TABLE [Company]
    ADD [YearsExperienceCount] INT NULL;
    PRINT 'Column YearsExperienceCount added successfully';
END
ELSE
BEGIN
    PRINT 'Column YearsExperienceCount already exists';
END
GO

-- Update existing company record with sample data
UPDATE [Company]
SET 
    [AboutUsTitle] = 'Explore the World with Confidence',
    [AboutUsDescription1] = 'Wanderlust Horizons Co., Ltd. Travel is proud to be a leading travel company dedicated to providing safe, comfortable, and memorable journeys for every customer. With a team of professional tour guides and a global network of partners, we are committed to delivering the highest quality service in every trip you take.',
    [AboutUsDescription2] = 'From unique cultural discovery tours to luxurious relaxation experiences, FastRail Travel accompanies you on every journey. We believe that each trip is not only about exploring the world but also about enjoying life, making meaningful connections, and creating unforgettable memories.',
    [AboutUsImageUrl] = '~/assets/img/travel/showcase-8.webp',
    [AboutUsImageAlt] = 'Travel Experience',
    [ExperienceNumber] = '15+',
    [ExperienceText] = 'Years of Excellence',
    [HappyTravelersCount] = 1200,
    [CountriesCoveredCount] = 85,
    [YearsExperienceCount] = 15
WHERE [CompanyID] = 1;
GO

PRINT 'Company table updated successfully!';
GO

