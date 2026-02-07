-- Migration: Add Authentication and Authorization
-- Date: 2026-01-10
-- Description: Adds Users, Sessions, MagicLinkTokens, and ChangeRequests tables for authentication

-- Users table
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Role VARCHAR(50) NOT NULL CHECK (Role IN ('Admin', 'ParentA', 'ParentB')),
    DisplayName VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    LastLoginAt TIMESTAMP WITH TIME ZONE
);

-- Sessions table for long-lived authentication
CREATE TABLE IF NOT EXISTS Sessions (
    Id SERIAL PRIMARY KEY,
    Token VARCHAR(512) NOT NULL UNIQUE,
    UserId INTEGER NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT FK_Sessions_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Sessions_Token ON Sessions(Token);
CREATE INDEX IF NOT EXISTS IX_Sessions_UserId ON Sessions(UserId);

-- Magic link tokens for passwordless authentication
CREATE TABLE IF NOT EXISTS MagicLinkTokens (
    Id SERIAL PRIMARY KEY,
    Token VARCHAR(512) NOT NULL UNIQUE,
    UserId INTEGER NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,
    UsedAt TIMESTAMP WITH TIME ZONE,
    CONSTRAINT FK_MagicLinkTokens_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_MagicLinkTokens_Token ON MagicLinkTokens(Token);

-- Change requests for parent approval workflow
CREATE TABLE IF NOT EXISTS ChangeRequests (
    Id SERIAL PRIMARY KEY,
    RequestedByUserId INTEGER NOT NULL,
    RequestedForDate DATE NOT NULL,
    CurrentParent VARCHAR(10) NOT NULL,
    RequestedParent VARCHAR(10) NOT NULL,
    Status VARCHAR(50) NOT NULL CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ReviewedAt TIMESTAMP WITH TIME ZONE,
    ReviewedByUserId INTEGER,
    Comment TEXT,
    CONSTRAINT FK_ChangeRequests_RequestedByUserId FOREIGN KEY (RequestedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_ChangeRequests_ReviewedByUserId FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IF NOT EXISTS IX_ChangeRequests_Status ON ChangeRequests(Status);
CREATE INDEX IF NOT EXISTS IX_ChangeRequests_RequestedForDate ON ChangeRequests(RequestedForDate);

-- Insert default admin user (email can be updated later)
INSERT INTO Users (Email, Role, DisplayName) 
VALUES ('admin@coparenting.local', 'Admin', 'Administrator')
ON CONFLICT (Email) DO NOTHING;

-- Create default parent users for backward compatibility
INSERT INTO Users (Email, Role, DisplayName) 
VALUES ('parenta@coparenting.local', 'ParentA', 'Parent A')
ON CONFLICT (Email) DO NOTHING;

INSERT INTO Users (Email, Role, DisplayName) 
VALUES ('parentb@coparenting.local', 'ParentB', 'Parent B')
ON CONFLICT (Email) DO NOTHING;
