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
