using System;
using System.Data;
using System.Data.SQLite;

namespace ConsoleApplication1
{
    class SQL
    {
        public SQL(string path, bool autoconnect = true)
        {
            open(path);
        }
        private SQLiteConnection db_handle;
        private SQLiteCommand command;
        private SQLiteDataReader reader;
        private string p;

        public SQLiteDataReader result
        {
            get
            {
                return reader;
            }
        }

        public void open(string path)
        {
            db_handle = new SQLiteConnection("Data Source=" + path + ";Version=3;");
            db_handle.Open();
            Console.WriteLine("SQL: opened database connection");
        }
        public bool query(string text, bool expectsresult = false)
        {
            if (db_handle == null)
                throw new NullReferenceException("SQLite connection handle was null.");
            Console.WriteLine("SQL: executing query '" + text.Substring(0,6) + "'");
            command = new SQLiteCommand(text, db_handle);
            if (text.Contains("SELECT") || expectsresult) reader = command.ExecuteReader();
            else
            {
                try
                {
                    command.ExecuteNonQuery();       
                } 
                catch (SQLiteException e)
                {
                    if(e.ErrorCode == 19)
                        Console.WriteLine("SQLiteException: row already exists (id match)");
                }
            }
            return false;
        }
        public void free()
        {
            reader.Dispose();
            command.Dispose();
        }
        public void close()
        {
            free();
            db_handle.Close();
            db_handle.Dispose();
        }
    }
}
public class SQLException : Exception
{ 
    public SQLException():base() { } 
    public SQLException (string message): base(message) { }
}
