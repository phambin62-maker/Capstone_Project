-- Script to create Feature table
-- Run this script in your SQL Server database

USE [Otmsdb]; -- Replace with your actual database name
GO

-- Create Feature table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Feature]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Feature](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Icon] [nvarchar](100) NOT NULL,
        [Title] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Delay] [int] NOT NULL DEFAULT 200,
        [DisplayOrder] [int] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK__Feature__3214EC27] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Feature table created successfully';
END
ELSE
BEGIN
    PRINT 'Feature table already exists';
END
GO

-- Insert sample features
IF NOT EXISTS (SELECT 1 FROM [Feature])
BEGIN
    INSERT INTO [Feature] ([Icon], [Title], [Description], [Delay], [DisplayOrder], [IsActive])
    VALUES
        ('bi bi-people-fill', 'Local Experts', 'Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium totam.', 200, 1, 1),
        ('bi bi-shield-check', 'Safe & Secure', 'At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum.', 250, 2, 1),
        ('bi bi-cash', 'Best Prices', 'Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet consectetur adipisci velit.', 300, 3, 1),
        ('bi bi-headset', '24/7 Support', 'Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam nisi.', 350, 4, 1),
        ('bi bi-geo-alt-fill', 'Global Destinations', 'Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae.', 400, 5, 1),
        ('bi bi-star-fill', 'Premium Experience', 'Excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim.', 450, 6, 1);
    
    PRINT 'Sample features inserted successfully';
END
ELSE
BEGIN
    PRINT 'Features already exist';
END
GO

