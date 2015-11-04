CREATE TABLE [dbo].[ForcedTableMigration]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[count] int not null,
	[Name] varchar(25) not null,	
	[total] as [Id] * [count],
	[new] int not null,
	constraint fk_one foreign key (new) references r(id)
)
go 
create table r
(
 id int not null primary key
 )