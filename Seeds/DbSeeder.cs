using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Configuration;
using Bogus;
using WebApiADO.NetCrudPagination.Entities;

namespace WebApiADO.NetCrudPagination.Seeds
{
    public class DbSeeder
    {
        private static string _connectionString;

        public static async void Seed()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["MsSql"].ConnectionString;
            await SeedTodos();
            // SeedEntity2();
            // SeedEntity3();
            // ....
        }


        public static async Task SeedTodos()
        {
            var todosCount = await GetTodoCount();
            var todosToSeed = 32;
            todosToSeed -= todosCount;
            if (todosToSeed > 0)
            {
                Console.WriteLine($"[+] Seeding {todosToSeed} Todos");
                var faker = new Faker<Todo>()
                    .RuleFor(a => a.Title, f => string.Join(" ", f.Lorem.Words(f.Random.Int(2, 5))))
                    .RuleFor(a => a.Description, f => f.Lorem.Sentences(f.Random.Int(1, 10)))
                    .RuleFor(t => t.Completed, f => f.Random.Bool(0.4f))
                    .RuleFor(a => a.CreatedAt,
                        f => f.Date.Between(DateTime.Now.AddYears(-5), DateTime.Now.AddDays(-1)))
                    .FinishWith(async (f, todoInstance) =>
                    {
                        todoInstance.UpdatedAt =
                            f.Date.Between(todoInstance.CreatedAt, DateTime.Now);
                    });

                var todos = faker.Generate(todosToSeed);
                using (var connection = new SqlConnection(_connectionString))
                {
                    var sql = "Insert Into Todo (Title, Description, Completed, CreatedAt, UpdatedAt) Values " +
                              "(@Title, @Description, @Completed, @CreatedAt, @UpdatedAt)";

                    connection.Open();

                    foreach (var todo in todos)
                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.CommandType = CommandType.Text;

                            command.Parameters.AddWithValue("Title", todo.Title);
                            command.Parameters.AddWithValue("Description", todo.Description);
                            command.Parameters.AddWithValue("Completed", todo.Completed);
                            command.Parameters.AddWithValue("CreatedAt", todo.CreatedAt);
                            command.Parameters.AddWithValue("UpdatedAt", todo.UpdatedAt);
                            command.ExecuteNonQuery();
                        }

                    connection.Close();
                }
            }
        }

        private static async Task<int> GetTodoCount()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "Select COUNT(*) from Todo";
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    var rowcount = (int) await command.ExecuteScalarAsync();
                    return rowcount;
                }
            }
        }
    }
}