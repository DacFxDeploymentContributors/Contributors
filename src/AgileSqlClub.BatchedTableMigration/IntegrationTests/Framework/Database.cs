using System.Data.SqlClient;

namespace IntegrationTests.Framework
{
    internal static class Database
    {
        private const string connection_string = "SERVER=.;Integrated Security=SSPI;";
        public const string db_name = "BatchTableMigrationsTests";
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

            RunSql(@"CREATE TABLE [dbo].[ForcedTableMigration]
(
	[count] int not null,
)", db_name);
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