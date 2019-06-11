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
    public class TodoService : ITodoService
    {
        private readonly string _connectionString;


        public TodoService()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["MsSql"].ConnectionString;
        }


        public async Task<Tuple<int, List<Todo>>> FetchMany(int page = 1, int pageSize = 5,
            TodoShow show = TodoShow.All)
        {
            var todos = new List<Todo>();
            int totalCount;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var offset = (page - 1) * pageSize;

                string sql;
                SqlCommand command;

                if (show == TodoShow.All)
                {
                    command = new SqlCommand("Select COUNT(*) From [dbo].[Todo]", connection);
                    totalCount = (int) await command.ExecuteScalarAsync();

                    command = new SqlCommand(
                        "Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo ORDER BY CreatedAt " +
                        $"OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY", connection);
                }
                else
                {
                    command = new SqlCommand("Select COUNT(*) From [dbo].[Todo] Where Completed=@Completed",
                        connection);
                    command.Parameters.AddWithValue("Completed", show == TodoShow.Completed ? true : false);
                    totalCount = (int) await command.ExecuteScalarAsync();

                    command = new SqlCommand(
                        "Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo Where Completed = @Completed ORDER BY CreatedAt " +
                        $"OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                        connection);

                    command.Parameters.Add(new SqlParameter("Completed", show == TodoShow.Completed ? true : false));
                }


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

        public async Task CreateTodo(Todo todo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "Insert Into [dbo].[Todo] (Title, Description, Completed) Values " +
                          "(@Title, @Description, @Completed); Select SCOPE_IDENTITY()";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    command.Parameters.AddWithValue("Title", todo.Title);
                    command.Parameters.AddWithValue("Description", todo.Description);
                    command.Parameters.AddWithValue("Completed", todo.Completed);

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
                var sql =
                    "Update Todo SET Title=@Title, Description=@Description, Completed=@Completed, UpdatedAt= @UpdatedAt " +
                    "Where Id= @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    var now = DateTime.UtcNow;
                    command.Parameters.AddWithValue("Id", currentTodo.Id);
                    command.Parameters.AddWithValue("Title", todoFromUser.Title);
                    command.Parameters.AddWithValue("Description", todoFromUser.Description);
                    command.Parameters.AddWithValue("Completed", todoFromUser.Completed);
                    command.Parameters.AddWithValue("UpdatedAt", now);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    todoFromUser.Id = currentTodo.Id;
                    todoFromUser.UpdatedAt = now;
                }
            }

            return todoFromUser;
        }

        public async Task Delete(int todoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("Delete FROM Todo WHERE Id = @todoId", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("todoId", todoId);

                    var affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("Delete from Todo", connection))
                {
                    command.CommandType = CommandType.Text;
                    var affectedRows = await command.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<Todo> GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("Select * From Todo Where Id= @Id", connection);

                var parameter = command.Parameters.Add("Id", SqlDbType.Int);
                parameter.Value = id;

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

                var command = new SqlCommand("Select id From Todo Where Id= @Id", connection);

                var parameter = command.Parameters.Add("Id", SqlDbType.Int);
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

        public async Task CreateTodoVulnerable(Todo todo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "Insert Into [dbo].[Todo] (Title, Description, Completed) Values " +
                          $"('{todo.Title}', '{todo.Description}','{todo.Completed}'); Select SCOPE_IDENTITY()";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    if (result != null) todo.Id = int.Parse(result.ToString());

                    connection.Close();
                }
            }
        }

        public async Task UpdateVulnerable(int id, Todo todoFromUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql =
                    $"Update Todo SET Title='{todoFromUser.Title}', Description='{todoFromUser.Description}', Completed='{todoFromUser.Completed}' " +
                    $"Where Id='{id}'";
                using (var command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }
        }

        public async Task<Todo> UpdateVulnerable(Todo currentTodo, Todo todoFromUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql =
                    $"Update Todo SET Title='{todoFromUser.Title}', Description='{todoFromUser.Description}', Completed='{todoFromUser.Completed}' " +
                    $"Where Id='{currentTodo.Id}'";
                using (var command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    todoFromUser.Id = currentTodo.Id;
                }
            }

            return todoFromUser;
        }


        /// <summary>
        ///     Deletes a To do
        /// </summary>
        /// <param name="todoId"></param>
        /// <returns></returns>
        public async Task DeleteVulnerable(int todoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = $"Delete From Todo Where Id='{todoId}'";
                using (var command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    try
                    {
                        var affectedRows = command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                    }

                    connection.Close();
                }
            }
        }

        public async Task<Todo> GetByIdVulnerable(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql;

                sql = $"Select * From Todo Where Id={id}";

                var command = new SqlCommand(sql, connection);

                using (var dataReader = command.ExecuteReader())
                {
                    if (await dataReader.ReadAsync())
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
    }
}