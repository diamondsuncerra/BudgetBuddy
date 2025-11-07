using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class TransactionsRepository : IRepository<Transaction, string>
    {
        private readonly Dictionary<string, Transaction> _data = new();
        public bool TryAdd(Transaction entity)
        {
            if (_data.ContainsKey(entity.Id)) return false;
            _data.Add(entity.Id, entity);
            return true;
        }
        public bool Contains(string id)
        {
            return _data.ContainsKey(id);

        }
        public bool Remove(string id)
        {
            return _data.Remove(id);
             
        }
        public bool TryGet(string id, out Transaction? entity)
        {
            return _data.TryGetValue(id, out entity);
        }
        public IEnumerable<Transaction> GetAll()
        {
            return _data.Values;
        }

    }
}