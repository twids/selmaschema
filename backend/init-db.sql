-- Initial database schema for Co-Parenting Calendar (PostgreSQL)
-- Updated to support multiple comments per parent and special statuses

-- Create DayAssignments table
CREATE TABLE "DayAssignments" (
    "Id" SERIAL PRIMARY KEY,
    "Date" DATE NOT NULL UNIQUE,
    "Parent" VARCHAR(1) NULL,  -- 'A' or 'B' or NULL for unassigned
    "IsVAB" BOOLEAN NOT NULL DEFAULT FALSE,
    "SpecialStatus" VARCHAR(50) NULL,  -- NULL, 'PreschoolClosed', 'Holiday', etc.
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP NULL
);

CREATE INDEX "IX_DayAssignments_Date" ON "DayAssignments"("Date");

-- Create Comments table (supports multiple comments per day per parent)
CREATE TABLE "Comments" (
    "Id" SERIAL PRIMARY KEY,
    "DayAssignmentId" INT NOT NULL,
    "Parent" VARCHAR(1) NOT NULL,  -- 'A' or 'B'
    "CommentText" VARCHAR(1000) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP NULL,
    CONSTRAINT "FK_Comments_DayAssignments" FOREIGN KEY ("DayAssignmentId") 
        REFERENCES "DayAssignments"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Comments_DayAssignmentId" ON "Comments"("DayAssignmentId");
CREATE INDEX "IX_Comments_Parent" ON "Comments"("Parent");

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
('ParentAName', 'Tomas', CURRENT_TIMESTAMP),
('ParentBName', 'Carro', CURRENT_TIMESTAMP);
