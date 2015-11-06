using System.Data.SqlClient;

namespace IntegrationTests.Framework
{
    internal static class Database
    {
        private const string connection_string = "SERVER=.;Integrated Security=SSPI;";
        public const string db_name = "StopDeploymentsOnBreakingProcedureChangesTests";
        public const string server_name = ".";

        public static void CleanOrCreateDatabase()
        {
            RunSql(string.Format(@"
                        if exists(select * from sys.databases where name = '{0}')" +
                                 @"    begin

                            exec sp_executesql N'alter database {0} set single_user with rollback immediate';
                            exec sp_executesql N'drop database {0}' ;
                         end
                        
                            create database {0};
", db_name));

            RunSql(@"create procedure TestoAddParameterNoDefault(@a int) 
	as select 100;
", db_name);

            RunSql(@"create procedure TestoAddParameterWithDefault(@a int, @cc int =1480)
as
	select 199;
", db_name);

            RunSql(@"create procedure TestoRemoveParameter(@a int, @jj int)
as
	select 1980;

", db_name);

            RunSql(@"create procedure TestoRemoveDefaultFromProcedure(@a int, @mm int = 190)
as
	select 87;
", db_name);
            
        }

        private static void RunSql(string sql)
        {
            using (var con = new SqlConnection(connection_string))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void RunSql(string sql, string dbName)
        {
            using (var con = new SqlConnection(connection_string))
            {
                con.Open();
                con.ChangeDatabase(dbName);
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}