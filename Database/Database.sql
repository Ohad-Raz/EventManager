/* =========================================================
   EVENT MANAGER - DATABASE SCRIPT FOR FINAL SUBMISSION

   Seeded login users:
   - admin / 12345678
   - user1 / 12345678
   - organizer1 / 12345678
   - john / 12345678
   - maya / 12345678
   - lea / 12345678
   ========================================================= */

/* =========================================================
   CREATE DATABASE IF NEEDED
   ========================================================= */

IF DB_ID(N'EventManager') IS NULL
BEGIN
    CREATE DATABASE [EventManager];
END
GO

USE [EventManager];
GO

/* =========================================================
   DROP TABLES FOR RE-RUN
   ========================================================= */

IF OBJECT_ID('dbo.EventPerformer', 'U') IS NOT NULL DROP TABLE dbo.EventPerformer;
IF OBJECT_ID('dbo.Registration', 'U') IS NOT NULL DROP TABLE dbo.Registration;
IF OBJECT_ID('dbo.[Log]', 'U') IS NOT NULL DROP TABLE dbo.[Log];
IF OBJECT_ID('dbo.Event', 'U') IS NOT NULL DROP TABLE dbo.Event;
IF OBJECT_ID('dbo.Performer', 'U') IS NOT NULL DROP TABLE dbo.Performer;
IF OBJECT_ID('dbo.Image', 'U') IS NOT NULL DROP TABLE dbo.Image;
IF OBJECT_ID('dbo.EventType', 'U') IS NOT NULL DROP TABLE dbo.EventType;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
IF OBJECT_ID('dbo.[Role]', 'U') IS NOT NULL DROP TABLE dbo.[Role];
GO

/* =========================================================
   ROLE
   ========================================================= */

CREATE TABLE dbo.[Role]
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Role PRIMARY KEY (Id),
    CONSTRAINT UQ_Role_Name UNIQUE (Name)
);
GO

/* =========================================================
   USER
   ========================================================= */

CREATE TABLE dbo.[User]
(
    Id INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PwdHash NVARCHAR(256) NOT NULL,
    PwdSalt NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(256) NOT NULL,
    LastName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(256) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_User_CreatedAt DEFAULT GETDATE(),
    RoleId INT NOT NULL CONSTRAINT DF_User_RoleId DEFAULT 2,

    CONSTRAINT PK_User PRIMARY KEY (Id),
    CONSTRAINT UQ_User_Username UNIQUE (Username),
    CONSTRAINT UQ_User_Email UNIQUE (Email),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES dbo.[Role](Id)
);
GO

/* =========================================================
   EVENT TYPE
   1-to-N entity for Event
   ========================================================= */

CREATE TABLE dbo.EventType
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,

    CONSTRAINT PK_EventType PRIMARY KEY (Id),
    CONSTRAINT UQ_EventType_Name UNIQUE (Name)
);
GO

/* =========================================================
   IMAGE
   Optional image entity for Event
   ========================================================= */

CREATE TABLE dbo.Image
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Image_CreatedAt DEFAULT GETDATE(),

    CONSTRAINT PK_Image PRIMARY KEY (Id)
);
GO

/* =========================================================
   PERFORMER
   M-to-N entity for Event through EventPerformer
   ========================================================= */

CREATE TABLE dbo.Performer
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Bio NVARCHAR(1000) NULL,

    CONSTRAINT PK_Performer PRIMARY KEY (Id)
);
GO

/* =========================================================
   EVENT
   Primary entity
   ========================================================= */

CREATE TABLE dbo.Event
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL,
    EventTypeId INT NOT NULL,
    CreatedById INT NOT NULL,
    ImageId INT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Event_CreatedAt DEFAULT GETDATE(),
    DeletedAt DATETIME2 NULL,

    CONSTRAINT PK_Event PRIMARY KEY (Id),
    CONSTRAINT FK_Event_EventType FOREIGN KEY (EventTypeId) REFERENCES dbo.EventType(Id),
    CONSTRAINT FK_Event_User FOREIGN KEY (CreatedById) REFERENCES dbo.[User](Id),
    CONSTRAINT FK_Event_Image FOREIGN KEY (ImageId) REFERENCES dbo.Image(Id),
    CONSTRAINT CK_Event_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_Event_Capacity CHECK (Capacity > 0)
);
GO

/* =========================================================
   LOG
   Used by API logs endpoints
   ========================================================= */

CREATE TABLE dbo.[Log]
(
    Id INT IDENTITY(1,1) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL CONSTRAINT DF_Log_Timestamp DEFAULT GETDATE(),
    [Level] INT NOT NULL,
    [Message] NVARCHAR(1000) NOT NULL,
    ErrorText NVARCHAR(MAX) NULL,

    CONSTRAINT PK_Log PRIMARY KEY (Id)
);
GO

/* =========================================================
   REGISTRATION
   User desired-action entity
   ========================================================= */

CREATE TABLE dbo.Registration
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    UserId INT NOT NULL,
    EventId INT NOT NULL,
    RegisteredAt DATETIME2 NOT NULL CONSTRAINT DF_Registration_RegisteredAt DEFAULT GETDATE(),
    IsActive BIT NOT NULL CONSTRAINT DF_Registration_IsActive DEFAULT 1,

    CONSTRAINT PK_Registration PRIMARY KEY (Id),
    CONSTRAINT FK_Registration_User FOREIGN KEY (UserId) REFERENCES dbo.[User](Id),
    CONSTRAINT FK_Registration_Event FOREIGN KEY (EventId) REFERENCES dbo.Event(Id),
    CONSTRAINT UQ_Registration_User_Event UNIQUE (UserId, EventId)
);
GO

/* =========================================================
   EVENT PERFORMER
   M-to-N bridge between Event and Performer
   ========================================================= */

CREATE TABLE dbo.EventPerformer
(
    Id INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    PerformerId INT NOT NULL,

    CONSTRAINT PK_EventPerformer PRIMARY KEY (Id),
    CONSTRAINT FK_EventPerformer_Event FOREIGN KEY (EventId) REFERENCES dbo.Event(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EventPerformer_Performer FOREIGN KEY (PerformerId) REFERENCES dbo.Performer(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_EventPerformer_Event_Performer UNIQUE (EventId, PerformerId)
);
GO

/* =========================================================
   SEED DATA
   ========================================================= */

SET IDENTITY_INSERT dbo.[Role] ON;
GO
INSERT INTO dbo.[Role] (Id, Name) VALUES
(1, N'Admin'),
(2, N'User'),
(3, N'Organizer');
GO
SET IDENTITY_INSERT dbo.[Role] OFF;
GO

INSERT INTO dbo.EventType (Name) VALUES
(N'Concert'),
(N'Lecture'),
(N'Workshop'),
(N'Gym Class'),
(N'Networking');
GO

INSERT INTO dbo.Image (Name, FileName, FilePath, ContentType) VALUES
(N'Workshop Banner', N'workshop-banner.jpg', N'/images/events/workshop-banner.jpg', N'image/jpeg'),
(N'Concert Poster', N'concert-poster.jpg', N'/images/events/concert-poster.jpg', N'image/jpeg'),
(N'Lecture Cover', N'lecture-cover.jpg', N'/images/events/lecture-cover.jpg', N'image/jpeg'),
(N'Gym Class Cover', N'gym-class-cover.jpg', N'/images/events/gym-class-cover.jpg', N'image/jpeg'),
(N'Networking Cover', N'networking-cover.jpg', N'/images/events/networking-cover.jpg', N'image/jpeg');
GO

INSERT INTO dbo.Performer (Name, Bio) VALUES
(N'Ana Novak', N'Professional singer and live performer.'),
(N'Marko Horvat', N'Fitness coach and workshop leader.'),
(N'Ivana Kralj', N'Guest lecturer in communication and leadership.'),
(N'Luka Babić', N'Local acoustic performer and event guest.'),
(N'Maja Petrović', N'Project management coach and business mentor.'),
(N'David Cohen', N'Technology speaker focused on software teams.'),
(N'Sara Levi', N'Yoga instructor and wellness coach.'),
(N'Tin Marić', N'Networking host and startup community organizer.');
GO

/* Password for all seeded users: 12345678 */
INSERT INTO dbo.[User]
(
    Username,
    PwdHash,
    PwdSalt,
    FirstName,
    LastName,
    Email,
    Phone,
    RoleId
)
VALUES
(N'admin',      N'7RXALyPLzM0XXj7qhqGmQrVm0EBxB9lyPmFV+24qTUU=', N'ifvbGMwQSx+RghdBcNFFYQ==', N'System', N'Admin',     N'admin@eventmanager.com',      N'0911111111', 1),
(N'user1',      N'Kgiha/RXL8F9tP+qSpThvo206+MSjDllmnqm0xv3bOM=', N'Opp0zp7mgPtXc2O1JOSzPQ==', N'Ohad',   N'Raz',       N'user1@eventmanager.com',      N'0922222222', 2),
(N'organizer1', N'wjF1H4qKcC1zFnWQO8vW+bQ0uaIdNA54ZnajLyB8hmE=', N'1gf8/rM7JLu9PdAmSQVqpw==', N'Nina',   N'Kovač',     N'organizer1@eventmanager.com', N'0933333333', 3),
(N'john',       N'wptgHBxLsPfo5BJOPRLv5xBi0ZDU4PSZLikGPRCh3I4=', N'pDe6w1FzXyjkhicOCtrHSg==', N'John',   N'Miller',    N'john.miller@example.com',     N'0944444444', 2),
(N'maya',       N'1RwLtv4jNB93rwkQPmK/K2flyCehwJ13e2zM3Pd6TPM=', N'GYs+XfI2OXc2ip8HCJeYzw==', N'Maya',   N'Brooks',    N'maya.brooks@example.com',     N'0955555555', 2),
(N'lea',        N'+R9nM8Ehg1FYLdSO+Es1neC4KYC0k+Rdc+Xfuss65bk=', N'H6V+QmAmSl3AOzBjtsad4g==', N'Lea',    N'Novak',     N'lea.novak@example.com',       N'0966666666', 2);
GO

INSERT INTO dbo.Event
(
    Name,
    Description,
    StartTime,
    EndTime,
    Location,
    Capacity,
    EventTypeId,
    CreatedById,
    ImageId
)
VALUES
(N'Public Speaking Workshop', N'A practical workshop for improving confidence and presentation skills.', '2026-05-10 10:00:00', '2026-05-10 13:00:00', N'Zagreb Hall A', 30, 3, 3, 1),
(N'Spring Music Night', N'An evening concert with local live performers.', '2026-05-15 20:00:00', '2026-05-15 22:30:00', N'City Stage', 150, 1, 1, 2),
(N'Leadership in Practice', N'A lecture focused on communication, teamwork, and leadership skills.', '2026-05-20 18:00:00', '2026-05-20 20:00:00', N'Conference Room B', 80, 2, 3, 3),
(N'Morning Functional Training', N'A guided gym class focused on strength, mobility, and endurance.', '2026-05-22 08:00:00', '2026-05-22 09:30:00', N'Fitness Studio 1', 25, 4, 3, 4),
(N'Software Teams Meetup', N'A networking event for students and developers interested in modern web applications.', '2026-05-25 17:00:00', '2026-05-25 19:30:00', N'Innovation Hub Zagreb', 120, 5, 1, 5),
(N'Acoustic Friday Session', N'A relaxed live music evening with acoustic performances.', '2026-05-29 20:00:00', '2026-05-29 22:00:00', N'Old Town Cafe', 60, 1, 3, 2),
(N'Career Branding Lecture', N'A lecture about personal branding, career development, and professional communication.', '2026-06-03 18:00:00', '2026-06-03 20:00:00', N'Lecture Room 204', 90, 2, 3, 3),
(N'Agile Planning Workshop', N'A hands-on workshop about planning tasks, estimating work, and managing project risk.', '2026-06-07 11:00:00', '2026-06-07 15:00:00', N'Zagreb Hall B', 40, 3, 1, 1),
(N'Evening Yoga Class', N'A calming wellness class for flexibility, balance, and stress relief.', '2026-06-11 19:00:00', '2026-06-11 20:15:00', N'Wellness Studio', 35, 4, 3, 4),
(N'Startup Networking Night', N'A social networking event for students, founders, and young professionals.', '2026-06-18 18:30:00', '2026-06-18 21:00:00', N'Business Lounge Zagreb', 200, 5, 1, 5);
GO

INSERT INTO dbo.EventPerformer (EventId, PerformerId) VALUES
(1, 3),
(1, 5),
(2, 1),
(2, 4),
(3, 3),
(3, 5),
(4, 2),
(5, 6),
(5, 8),
(6, 4),
(7, 3),
(7, 5),
(8, 5),
(8, 6),
(9, 7),
(10, 6),
(10, 8);
GO

INSERT INTO dbo.Registration (Name, UserId, EventId, IsActive) VALUES
(N'Registration for Public Speaking Workshop', 2, 1, 1),
(N'Registration for Leadership in Practice', 2, 3, 1),
(N'Registration for Software Teams Meetup', 2, 5, 1),
(N'Registration for Evening Yoga Class', 2, 9, 0),
(N'Registration for Spring Music Night', 4, 2, 1),
(N'Registration for Acoustic Friday Session', 4, 6, 0),
(N'Registration for Morning Functional Training', 5, 4, 1),
(N'Registration for Career Branding Lecture', 5, 7, 0),
(N'Registration for Startup Networking Night', 6, 10, 1);
GO

INSERT INTO dbo.[Log] ([Timestamp], [Level], [Message]) VALUES
(GETDATE(), 1, N'Database seed completed.'),
(GETDATE(), 1, N'Seeded 10 sample events.'),
(GETDATE(), 1, N'Seeded sample users, performers, registrations and event types.');
GO
