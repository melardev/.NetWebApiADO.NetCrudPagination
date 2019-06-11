using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApiADO.NetCrudPagination.Dtos.Responses.Todos;
using WebApiADO.NetCrudPagination.Entities;
using WebApiADO.NetCrudPagination.Enums;
using WebApiADO.NetCrudPagination.Infrastructure.Services;
using WebApiADO.NetCrudPagination.Models;

namespace WebApiADO.NetCrudPagination.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/todos")]
    public class TodosController : ApiController
    {
        private readonly TodoService _todosService;


        public TodosController()
        {
            _todosService = new TodoService();
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetTodos([FromUri] int page = 1, [FromUri] int pageSize = 5)
        {
            var result = await _todosService.FetchMany(page, pageSize);
            return StatusCodeAndDtoWrapper.BuildSuccess(TodoListResponse.Build(result.Item2,
                Request.RequestUri.LocalPath, page,
                pageSize, result.Item1));
        }


        [HttpGet]
        [Route("pending")]
        public async Task<HttpResponseMessage> GetPending([FromUri] int page = 1, [FromUri] int pageSize = 5)
        {
            var result = await _todosService.FetchMany(page, pageSize, TodoShow.Pending);
            return StatusCodeAndDtoWrapper.BuildSuccess(TodoListResponse.Build(result.Item2,
                Request.RequestUri.LocalPath, page,
                pageSize, result.Item1));
        }

        [HttpGet]
        [Route("completed")]
        public async Task<HttpResponseMessage> GetCompleted([FromUri] int page = 1, [FromUri] int pageSize = 5)
        {
            var result = await _todosService.FetchMany(page, pageSize, TodoShow.Completed);
            return StatusCodeAndDtoWrapper.BuildSuccess(TodoListResponse.Build(result.Item2,
                Request.RequestUri.LocalPath, page,
                pageSize, result.Item1));
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<HttpResponseMessage> GetTodoDetails(int id)
        {
            var todo = await _todosService.GetById(id);
            if (todo != null)
                return StatusCodeAndDtoWrapper.BuildSuccess(TodoDetailsDto.Build(todo));
            return StatusCodeAndDtoWrapper.BuildNotFound("Requested todo not found");
        }


        [HttpPost]
        public async Task<HttpResponseMessage> CreateTodo([FromBody] Todo todo)
        {
            await _todosService.CreateTodo(todo);
            return StatusCodeAndDtoWrapper.BuildSuccess(TodoDetailsDto.Build(todo), "Todo Created Successfully");
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<HttpResponseMessage> UpdateTodo(int id, [FromBody] Todo todo)
        {
            var todoFromDb = await _todosService.GetProxyById(id);

            if (todoFromDb != null)
                return StatusCodeAndDtoWrapper.BuildSuccess(
                    TodoDetailsDto.Build(await _todosService.Update(todoFromDb, todo)),
                    "Todo Updated Successfully");
            return StatusCodeAndDtoWrapper.BuildNotFound("Todo Was not found");
        }


        [HttpDelete]
        [Route("{id:int}")]
        public async Task<HttpResponseMessage> DeleteTodo(int id)
        {
            await _todosService.Delete(id);
            return StatusCodeAndDtoWrapper.BuildSuccess("Todo Deleted Successfully");
        }


        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAll()
        {
            await _todosService.DeleteAll();
            return StatusCodeAndDtoWrapper.BuildSuccess("Todos Deleted Successfully");
        }
    }
}