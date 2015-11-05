CREATE TABLE [dbo].[CreateIndexTable]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Name] varchar(25) not null,	
)
go
create index ix_on_or_offline on CreateIndexTable(Name);