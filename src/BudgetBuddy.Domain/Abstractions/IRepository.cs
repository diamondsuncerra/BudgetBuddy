using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain
{
    public interface IRepository<T, TKey>
    {
        int Count();
        bool TryAdd(T entity);
        bool Contains(TKey id);
        bool Remove(TKey id);
        bool TryGet(TKey id, out T? entity);
        IEnumerable<T> GetAll();
    }
}