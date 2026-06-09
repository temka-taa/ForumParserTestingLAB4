using Microsoft.Data.Sqlite;
using DatabaseLibrary.Models;

namespace DatabaseLibrary;

public class SqliteMessageRepository : IMessageRepository, IDisposable
{
    private readonly SqliteConnection _conn;

    public SqliteMessageRepository(string connectionString)
    {
        _conn = new SqliteConnection(connectionString);
        _conn.Open();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL CHECK(length(Name) <= 256),
                Message TEXT CHECK(length(Message) <= 8096)
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public MessageRecord? GetByID(int id)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Message FROM Messages WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return new MessageRecord
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Message = reader.IsDBNull(2) ? "" : reader.GetString(2)
            };
        return null;
    }

    public List<MessageRecord> GetByName(string name)
    {
        var list = new List<MessageRecord>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Message FROM Messages WHERE Name = @name";
        cmd.Parameters.AddWithValue("@name", name);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new MessageRecord
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Message = reader.IsDBNull(2) ? "" : reader.GetString(2)
            });
        return list;
    }

    public void Add(MessageRecord record)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Messages (Id, Name, Message) VALUES (@id, @name, @message)";
        cmd.Parameters.AddWithValue("@id", record.Id);
        cmd.Parameters.AddWithValue("@name", record.Name);
        cmd.Parameters.AddWithValue("@message", record.Message);
        cmd.ExecuteNonQuery();
    }

    public void Update(int id, string newMessage)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "UPDATE Messages SET Message = @message WHERE Id = @id";
        cmd.Parameters.AddWithValue("@message", newMessage);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Messages WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Dispose() => _conn?.Dispose();
}