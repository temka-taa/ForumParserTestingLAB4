using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseLibrary.Models;

namespace DatabaseLibrary
{
    public interface IMessageRepository
    {
        MessageRecord? GetByID(int id);
        List<MessageRecord> GetByName(string name);
        void Add(MessageRecord record);
        void Update(int id, string newMessage);
        void Delete(int id);
    }
}
