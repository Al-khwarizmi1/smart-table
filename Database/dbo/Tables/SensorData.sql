﻿CREATE TABLE [dbo].[SensorData]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[DateTime] DATETIME2 NOT NULL,
	[Height] INT NOT NULL
)
