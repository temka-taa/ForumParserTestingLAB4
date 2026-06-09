using DatabaseLibrary;
using DatabaseLibrary.Models;
using Xunit;

namespace DatabaseLibrary.Tests;

public class MessageRepositoryTests : IDisposable
{
    private readonly SqliteMessageRepository _repo;

    public MessageRepositoryTests()
    {
        _repo = new SqliteMessageRepository("Data Source=:memory:");
    }

    public void Dispose() => _repo.Dispose();

    private void Seed()
    {
        _repo.Add(new MessageRecord { Id = 1, Name = "User1", Message = "Hello" });
        _repo.Add(new MessageRecord { Id = 2, Name = "User2", Message = "World" });
    }

    [Fact]
    public void GetByID_Exists_ReturnsRecord()
    {
        Seed(); var r = _repo.GetByID(1); Assert.NotNull(r); Assert.Equal("User1", r!.Name);
    }

    [Fact]
    public void GetByID_NotExists_ReturnsNull()
    {
        Seed(); Assert.Null(_repo.GetByID(999));
    }

    [Fact]
    public void GetByName_Exists_ReturnsList()
    {
        Seed(); _repo.Add(new MessageRecord { Id = 3, Name = "User1" }); Assert.Equal(2, _repo.GetByName("User1").Count);
    }

    [Fact]
    public void GetByName_NotExists_ReturnsEmpty()
    {
        Seed(); Assert.Empty(_repo.GetByName("NoName"));
    }

    [Fact]
    public void Add_Valid_Works()
    {
        Seed(); _repo.Add(new MessageRecord { Id = 5, Name = "A", Message = "B" }); Assert.NotNull(_repo.GetByID(5));
    }

    [Fact]
    public void Add_DuplicateId_Throws()
    {
        Seed(); Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() => _repo.Add(new MessageRecord { Id = 1, Name = "X" }));
    }

    [Fact]
    public void Update_Exists_ChangesText()
    {
        Seed(); _repo.Update(1, "new"); Assert.Equal("new", _repo.GetByID(1)!.Message);
    }

    [Fact]
    public void Update_NotExists_NoException()
    {
        Seed(); _repo.Update(99, "abc"); Assert.Equal("Hello", _repo.GetByID(1)!.Message);
    }

    [Fact]
    public void Delete_Exists_Removes()
    {
        Seed(); _repo.Delete(1); Assert.Null(_repo.GetByID(1));
    }

    [Fact]
    public void Delete_NotExists_NoEffect()
    {
        Seed(); _repo.Delete(88); Assert.NotNull(_repo.GetByID(1));
    }
}