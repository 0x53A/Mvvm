using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.App
{
    public interface IRepository
    {
        Task<T> LoadAsync<T>() where T : class;
        Task SaveAsync<T>(T data) where T : class;
    }
}
