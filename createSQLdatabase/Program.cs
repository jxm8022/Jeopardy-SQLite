using System.Data.SQLite;

public class Program
{
    public static void Main()
    {
        createDatabase();
        createTables();
        readFiles();
        printData();
    }

    /// <summary>
    /// Creates the sqlite database file
    /// </summary>
    public static void createDatabase()
    {
        SQLiteConnection.CreateFile("jeopardyDB.sqlite");
    }

    /// <summary>
    /// Creates tables in the sqlite database file
    /// </summary>
    public static void createTables()
    {
        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "CREATE TABLE type (type_id INTEGER PRIMARY KEY, category TEXT NOT NULL)";

        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = "CREATE TABLE question (question_id INTEGER PRIMARY KEY, entry TEXT NOT NULL, type_id INTEGER, FOREIGN KEY(type_id) REFERENCES type(type_id))";

        command = new SQLiteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = "CREATE TABLE answer (answer_id INTEGER PRIMARY KEY, entry TEXT NOT NULL, question_id INTEGER, FOREIGN KEY(question_id) REFERENCES question(question_id))";

        command = new SQLiteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = "CREATE TABLE team (team_id INTEGER PRIMARY KEY, team_name TEXT NOT NULL, score INTEGER DEFAULT 0 NOT NULL)";

        command = new SQLiteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = "CREATE TABLE player (player_id INTEGER PRIMARY KEY, player_name TEXT NOT NULL, team_id INTEGER, FOREIGN KEY(team_id) REFERENCES team(team_id))";

        command = new SQLiteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        dbConnection.Close();
    }

    /// <summary>
    /// Generate new record in table type
    /// </summary>
    /// <param name="type">Name of category</param>
    public static void insertType(string type)
    {
        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "INSERT INTO type(category) VALUES(@category)";
        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        command.Parameters.AddWithValue("@category", type);
        command.ExecuteNonQuery();

        dbConnection.Close();
    }

    /// <summary>
    /// Reads from the type table and returns the id of the type that matches the parameter
    /// </summary>
    /// <param name="category">Name of category</param>
    /// <returns>id of the given category</returns>
    public static int returnTypeId(string category)
    {
        int type = -1;

        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "SELECT * FROM type WHERE category = @category";
        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        command.Parameters.AddWithValue("@category", category);
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();
        type = reader.GetInt32(0);
        reader.Close();

        dbConnection.Close();

        return type;
    }

    /// <summary>
    /// Reads from the question table and returns the id of the question that matches the parameter
    /// </summary>
    /// <param name="question"The question string</param>
    /// <returns>id of the given question string</returns>
    public static int returnQuestionId(string question)
    {
        int id = -1;

        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "SELECT * FROM question WHERE entry = @entry";
        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        command.Parameters.AddWithValue("@entry", question);
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();
        id = reader.GetInt32(0);
        reader.Close();

        dbConnection.Close();

        return id;
    }

    /// <summary>
    /// Adds the question to the database and adds the answer to the database
    /// </summary>
    /// <param name="category">Category of the question</param>
    /// <param name="question">Question string to be inserted</param>
    /// <param name="answer">Answer string to be inserted</param>
    public static void insertRecord(string category, string question, string answer)
    {
        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "INSERT INTO question(entry, type_id) VALUES(@entry, @type_id)";
        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        command.Parameters.AddWithValue("@entry", question);
        command.Parameters.AddWithValue("@type_id", returnTypeId(category));
        command.ExecuteNonQuery();

        sql = "INSERT INTO answer(entry, question_id) VALUES(@entry, @question_id)";
        command = new SQLiteCommand(sql, dbConnection);
        command.Parameters.AddWithValue("@entry", answer);
        command.Parameters.AddWithValue("@question_id", returnQuestionId(question));
        command.ExecuteNonQuery();

        dbConnection.Close();
    }

    /// <summary>
    /// Printing database information to ensure equal questions and answers. Also, checking the number of questions in each category.
    /// </summary>
    public static void printData()
    {
        SQLiteConnection dbConnection = new SQLiteConnection("Data Source=jeopardyDB.sqlite;Version=3;");
        dbConnection.Open();

        string sql = "SELECT COUNT(*) FROM question";
        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();
        Console.WriteLine($"Total questions: {reader.GetInt32(0)}\n");
        reader.Close();

        sql = "SELECT COUNT(*) FROM answer";
        command = new SQLiteCommand(sql, dbConnection);
        reader = command.ExecuteReader();
        reader.Read();
        Console.WriteLine($"Total answers: {reader.GetInt32(0)}\n");
        reader.Close();

        sql = "SELECT category, COUNT(*) FROM question INNER JOIN type ON question.type_id = type.type_id GROUP BY category";
        command = new SQLiteCommand(sql, dbConnection);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader.GetString(0)} {reader.GetInt32(1)}\n");
        }
        reader.Close();

        dbConnection.Close();
    }

    /// <summary>
    /// Reads the questions from files provided from directory/folder
    /// </summary>
    public static void readFiles()
    {
        string[] contents;
        string question;
        string answer;
        string type;
        foreach (string file in Directory.EnumerateFiles("../categories"))
        {
            type = file.Substring(14).Replace(".txt", "");
            insertType(type);
            contents = File.ReadAllLines(file);
            foreach (string line in contents)
            {
                question = line.Split('\t')[0];
                answer = line.Split('\t')[1];
                insertRecord(type, question, answer);
            }
        }
    }
}