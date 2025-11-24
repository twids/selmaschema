-- Initial database schema for Co-Parenting Calendar (PostgreSQL)

-- Create DayAssignments table
CREATE TABLE "DayAssignments" (
    "Id" SERIAL PRIMARY KEY,
    "Date" DATE NOT NULL UNIQUE,
    "Parent" VARCHAR(1) NULL,
    "IsVAB" BOOLEAN NOT NULL DEFAULT FALSE,
    "Comment" VARCHAR(500) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP NULL
);

CREATE INDEX "IX_DayAssignments_Date" ON "DayAssignments"("Date");

-- Create Configurations table
CREATE TABLE "Configurations" (
    "Id" SERIAL PRIMARY KEY,
    "Key" VARCHAR(100) NOT NULL UNIQUE,
    "Value" VARCHAR(500) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP NULL
);

CREATE INDEX "IX_Configurations_Key" ON "Configurations"("Key");

-- Seed default configurations
INSERT INTO "Configurations" ("Key", "Value", "CreatedAt") VALUES
('ParentAName', 'Parent A', CURRENT_TIMESTAMP),
('ParentBName', 'Parent B', CURRENT_TIMESTAMP);

-- Create Users table
CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "GoogleId" VARCHAR(255) NOT NULL UNIQUE,
    "Email" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "IsDemo" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP NULL
);

CREATE INDEX "IX_Users_GoogleId" ON "Users"("GoogleId");
CREATE INDEX "IX_Users_Email" ON "Users"("Email");

-- Create Children table
CREATE TABLE "Children" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "DateOfBirth" DATE NULL,
    "PrimaryParentId" INTEGER NOT NULL,
    "SecondaryParentId" INTEGER NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP NULL,
    FOREIGN KEY ("PrimaryParentId") REFERENCES "Users"("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("SecondaryParentId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Create Invitations table
CREATE TABLE "Invitations" (
    "Id" SERIAL PRIMARY KEY,
    "InviterId" INTEGER NOT NULL,
    "InviteeEmail" VARCHAR(255) NOT NULL,
    "ChildId" INTEGER NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RespondedAt" TIMESTAMP NULL,
    FOREIGN KEY ("InviterId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("ChildId") REFERENCES "Children"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Invitations_ChildId_InviteeEmail" ON "Invitations"("ChildId", "InviteeEmail");

