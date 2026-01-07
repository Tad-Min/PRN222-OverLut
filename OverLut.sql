USE master
GO

DROP DATABASE IF EXISTS OverLut
GO
DROP DATABASE IF EXISTS OverLut_Storage
GO

CREATE DATABASE OverLut
GO
CREATE DATABASE OverLut_Storage
GO

------------------- DATABASE: OverLut (Chat Logic) -------------------
USE OverLut
GO

CREATE TABLE VerifyEmail(
	[Key] UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID(),
    Email VARCHAR(255) NOT NULL UNIQUE,
    [Time] BIGINT NOT NULL,

	CONSTRAINT PK_VerifyEmail PRIMARY KEY ([Key])
);

CREATE TABLE Roles(
    RoleID INT NOT NULL,
    RoleName NVARCHAR(255) NOT NULL UNIQUE,

	CONSTRAINT PK_Roles PRIMARY KEY (RoleID)
);

CREATE TABLE Users(
    UserID UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID(),

    UserName VARCHAR(255) NOT NULL UNIQUE,
    [Password] VARCHAR(255) NOT NULL,
    Name NVARCHAR(255) NOT NULL DEFAULT N'User',
    Email VARCHAR(255) UNIQUE,
    RoleID INT,

	CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Channels(
    ChannelID UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID(),

    ChannelType INT DEFAULT 0,
    ChannelName NVARCHAR(255),
	-- Bitwise 0000
	-- 1: Read, 2: Write, 8: Add orther, 16: Kick

    DefaultPermissions INT DEFAULT 3,
    CreateAt DATETIME2 DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Channels PRIMARY KEY (ChannelID)
);

CREATE TABLE ChannelMembers(
    ChannelID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,

    Nickname NVARCHAR(100),
    MemberRole INT DEFAULT 0,

	-- Bitwise 0000
	-- 1: Read, 2: Write, 8: Add orther, 16: Kick
    [Permissions] INT DEFAULT 3,

    CONSTRAINT PK_ChannelMembers PRIMARY KEY (ChannelID, UserID),
    CONSTRAINT FK_ChannelMembers_Channels FOREIGN KEY (ChannelID) REFERENCES Channels(ChannelID),
    CONSTRAINT FK_ChannelMembers_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE [Messages](
    MessageID UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID(),
    ChannelID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
	-- 0: Text, 1: Image, 2: File, 3: Voice, 4: Video
	[MessageType] INT DEFAULT 0,

    Content NVARCHAR(MAX) NOT NULL,
    CreateAt DATETIME2 DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Messages PRIMARY KEY(ChannelID, UserID, MessageID),
    CONSTRAINT FK_Messages_ChannelMembers FOREIGN KEY (ChannelID, UserID) REFERENCES ChannelMembers(ChannelID, UserID) ON DELETE NO ACTION,
);

CREATE TABLE ReadReceipts (
    ChannelID UNIQUEIDENTIFIER NOT NULL, 
    UserID UNIQUEIDENTIFIER NOT NULL,
    
    LastReadMessageID UNIQUEIDENTIFIER,
    LastReadTime DATETIME2 DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_ReadReceipts PRIMARY KEY (ChannelID,UserID),
    CONSTRAINT FK_ReadReceipts_Messages FOREIGN KEY (ChannelID, UserID, LastReadMessageID) REFERENCES [Messages](ChannelID, UserID, MessageID) ON DELETE CASCADE,
);

CREATE TABLE Attachments (
    AttachmentID UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID(),
    MessageID UNIQUEIDENTIFIER NOT NULL,
	ChannelID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,

    [FileName] NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    Width INT NULL,
    Height INT NULL,
    Duration INT NULL,
    FileSize BIGINT NOT NULL,
    FileBlobID UNIQUEIDENTIFIER NOT NULL,

	CONSTRAINT PK_AttachmentID PRIMARY KEY (AttachmentID),
    CONSTRAINT FK_Attachments_Messages FOREIGN KEY (ChannelID, UserID, MessageID) REFERENCES [Messages](ChannelID, UserID, MessageID) ON DELETE CASCADE,
);

CREATE INDEX IX_Messages_ChannelID_CreateAt ON [Messages] (ChannelID ASC, CreateAt DESC);
CREATE INDEX IX_ChannelMembers_UserID ON ChannelMembers(UserID);
CREATE INDEX IX_ReadReceipts_UserID_ChannelID ON ReadReceipts(UserID, ChannelID);
CREATE INDEX IX_Attachments_ChannelID_UserID_MessageID ON Attachments (UserID,ChannelID,MessageID)

GO

------------------- DATABASE: OverLut_Storage (Big Files) -------------------
USE OverLut_Storage
GO

CREATE TABLE FileBlobs (
    FileBlobID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    IsComplete BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);


CREATE TABLE FileChunks (
    ChunkID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    FileBlobID UNIQUEIDENTIFIER NOT NULL,
    SequenceNumber INT NOT NULL,
    [Data] VARBINARY(MAX) NOT NULL,
    CONSTRAINT FK_Chunks_Blobs FOREIGN KEY (FileBlobID) REFERENCES FileBlobs(FileBlobID) ON DELETE CASCADE
);


CREATE INDEX IX_Chunks_Blob_Order ON FileChunks(FileBlobID, SequenceNumber);
GO