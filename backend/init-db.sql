-- Initial database schema for Co-Parenting Calendar

-- Create DayAssignments table
CREATE TABLE DayAssignments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Date DATE NOT NULL UNIQUE,
    Parent NVARCHAR(1) NULL,
    IsVAB BIT NOT NULL DEFAULT 0,
    Comment NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL
);

CREATE INDEX IX_DayAssignments_Date ON DayAssignments(Date);

-- Create Configurations table
CREATE TABLE Configurations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL UNIQUE,
    Value NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL
);

CREATE INDEX IX_Configurations_Key ON Configurations([Key]);

-- Seed default configurations
INSERT INTO Configurations ([Key], Value, CreatedAt) VALUES
('ParentAName', 'Parent A', GETUTCDATE()),
('ParentBName', 'Parent B', GETUTCDATE());
