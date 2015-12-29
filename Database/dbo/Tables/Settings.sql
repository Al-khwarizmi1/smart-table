CREATE TABLE [dbo].[Settings]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[IntervalLength] INT NOT NULL,
	[ArduinoComPort] NVARCHAR(20) NULL
)
