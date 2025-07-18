﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetworkWebApp.Data.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(int id);

        Task Create(T item);

        Task Update(T item);

        Task Delete(T item);
    }
}
