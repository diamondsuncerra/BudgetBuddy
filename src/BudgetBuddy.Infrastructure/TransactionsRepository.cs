using System.Collections.Concurrent;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure
{
    public class TransactionsRepository : IRepository<Transaction, string>
    {
        private readonly ConcurrentDictionary<string, Transaction> _data = new();
        public bool TryAdd(Transaction entity)
        {
            return _data.TryAdd(entity.Id, entity);   
        }
        public bool Contains(string id)
        {
            return _data.ContainsKey(id);

        }
        public bool Remove(string id)
        {
            return _data.TryRemove(id, out _);
             
        }
        public bool TryGet(string id, out Transaction? entity)
        {
            return _data.TryGetValue(id, out entity);
        }
        public IEnumerable<Transaction> GetAll()
        {
            return _data.Values.OrderBy(t => int.Parse(t.Id));
        }
    }
}