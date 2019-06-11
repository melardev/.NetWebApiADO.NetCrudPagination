using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Configuration;
using WebApiADO.NetCrudPagination.Entities;
using WebApiADO.NetCrudPagination.Enums;

namespace WebApiADO.NetCrudPagination.Infrastructure.Services
{
    public class TodoServiceStoredProcedures : ITodoService
    {
        private readonly string _connectionString;


        public TodoServiceStoredProcedures()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["MsSql"].ConnectionString;
        }


        public async Task<Tuple<int, List<Todo>>> FetchMany(int page = 1, int pageSize = 5,
            TodoShow show = TodoShow.All)
        {
            var todos = new List<Todo>();
            var totalCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command;

                if (show == TodoShow.All)
                {
                    command = new SqlCommand("GetAllTodosCount", connection);
                    totalCount = (int) await command.ExecuteScalarAsync();

                    command = new SqlCommand("GetAllTodosWithPagination", connection);
                }
                else
                {
                    command = show == TodoShow.Pending
                        ? new SqlCommand("GetAllPendingTodosCount", connection)
                        : new SqlCommand("GetAllCompletedTodosCount", connection);

                    totalCount = (int) await command.ExecuteScalarAsync();

                    command = show == TodoShow.Pending
                        ? new SqlCommand("GetPendingWithPagination", connection)
                        : new SqlCommand("GetCompletedWithPagination", connection);
                }

                command.Parameters.AddWithValue("@Page", page);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                command.CommandType = CommandType.StoredProcedure;

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    while (dataReader.Read())
                    {
                        var todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        todo.Title = Convert.ToString(dataReader["Title"]);
                        todo.Completed = Convert.ToBoolean(dataReader["Completed"]);
                        todo.CreatedAt = Convert.ToDateTime(dataReader["CreatedAt"]);
                        todo.UpdatedAt = Convert.ToDateTime(dataReader["UpdatedAt"]);

                        todos.Add(todo);
                    }
                }

                connection.Close();
            }

            return Tuple.Create(totalCount, todos);
        }

        public async Task<Todo> GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("GetTodoById", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    if (dataReader.Read())
                    {
                        var todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        todo.Title = Convert.ToString(dataReader["Title"]);
                        todo.Description = Convert.ToString(dataReader["Description"]);
                        todo.Completed = Convert.ToBoolean(dataReader["Completed"]);
                        todo.CreatedAt = Convert.ToDateTime(dataReader["CreatedAt"]);
                        todo.UpdatedAt = Convert.ToDateTime(dataReader["UpdatedAt"]);

                        return todo;
                    }
                }

                connection.Close();
            }

            return null;
        }

        public async Task<Todo> GetProxyById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("GetTodoProxyById", connection);

                command.CommandType = CommandType.StoredProcedure;
                var parameter = command.Parameters.Add("@Id", SqlDbType.Int);
                parameter.Value = id;

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    if (dataReader.Read())
                    {
                        var todo = new Todo();
                        todo.Id = Convert.ToInt32(dataReader["Id"]);
                        return todo;
                    }
                }

                connection.Close();
            }

            return null;
        }

        public async Task CreateTodo(Todo todo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("CreateTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Title", todo.Title);
                    command.Parameters.AddWithValue("@Description", todo.Description);
                    command.Parameters.AddWithValue("@Completed", todo.Completed);

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    if (result != null) todo.Id = int.Parse(result.ToString());

                    connection.Close();
                }
            }
        }

        public async Task<Todo> Update(Todo currentTodo, Todo todoFromUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("UpdateTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var now = DateTime.UtcNow;
                    command.Parameters.AddWithValue("@Id", currentTodo.Id);
                    command.Parameters.AddWithValue("@Title", todoFromUser.Title);
                    command.Parameters.AddWithValue("@Description", todoFromUser.Description);
                    command.Parameters.AddWithValue("@Completed", todoFromUser.Completed);
                    command.Parameters.AddWithValue("@UpdatedAt", now);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    todoFromUser.Id = currentTodo.Id;
                    todoFromUser.UpdatedAt = now;
                }
            }

            return todoFromUser;
        }


        /// <summary>
        ///     Deletes a To do
        /// </summary>
        /// <param name="todoId"></param>
        /// <returns></returns>
        public async Task Delete(int todoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("DeleteTodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@Id", todoId);

                    var affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DeleteAllTodos", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    var affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}