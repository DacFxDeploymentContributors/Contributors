namespace AgileSqlClub.BatchedTableMigration
{
    public struct NewStatementContext
    {
        public int StartPos;
        public int Length;

        public string NewText;
    }
}