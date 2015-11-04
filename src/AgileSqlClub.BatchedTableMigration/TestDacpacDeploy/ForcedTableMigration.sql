CREATE TABLE [dbo].[ForcedTableMigration]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Name] varchar(25) not null,
	[count] int not null,
	[total] as [Id] * [count]
)
