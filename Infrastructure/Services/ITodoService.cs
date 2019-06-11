using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiADO.NetCrudPagination.Entities;
using WebApiADO.NetCrudPagination.Enums;

namespace WebApiADO.NetCrudPagination.Infrastructure.Services
{
    public interface ITodoService
    {
        Task<Tuple<int, List<Todo>>> FetchMany(int page = 1, int pageSize = 5, TodoShow show = TodoShow.All);
        Task CreateTodo(Todo todo);
        Task<Todo> Update(Todo currentTodo, Todo todoFromUser);
        Task Delete(int todoId);
        Task DeleteAll();
        Task<Todo> GetById(int id);
        Task<Todo> GetProxyById(int id);
    }
}