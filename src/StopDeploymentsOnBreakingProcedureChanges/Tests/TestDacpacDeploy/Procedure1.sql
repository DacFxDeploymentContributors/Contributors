create procedure TestoAddParameterNoDefault(@a int, @b int) 
	as select 100;
go
create procedure new_procedure
as
select 88
go
create procedure TestoAddParameterWithDefault(@a int, @cc int =1480)
as
	select 199;
go
create procedure TestoRemoveParameter(@a int)
as
	select 1980;
go
create procedure TestoRemoveDefaultFromProcedure(@a int, @mm int)
as
	select 87;