using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebApiADO.NetCrudPagination.Dtos.Responses.Shared;
using WebApiADO.NetCrudPagination.Infrastructure.DataStructures;

namespace WebApiADO.NetCrudPagination.Models

{
    public class StatusCodeAndDtoWrapper : HttpResponseMessage
    {
        public StatusCodeAndDtoWrapper(AppResponse dto, ICollection<string> messages,
            HttpStatusCode statusCode = HttpStatusCode.OK) : base(statusCode)
        {
            Content = new ObjectContent(typeof(AppResponse), dto,
                GlobalConfiguration.Configuration.Formatters.JsonFormatter);

            if (dto.FullMessages == null && messages != null)
                dto.FullMessages = new List<string>(1);

            if (messages != null)
                dto.FullMessages.AddRange(messages);
        }

        public StatusCodeAndDtoWrapper(AppResponse dto, string message, int statusCode = 200) : this(dto,
            message != null ? new SingletonList<string>(message) : null,
            statusCode)
        {
        }


        public StatusCodeAndDtoWrapper(AppResponse dto, ICollection<string> messages, int statusCode = 200) : this(dto,
            messages, (HttpStatusCode) statusCode)
        {
        }


        private StatusCodeAndDtoWrapper(AppResponse dto, int statusCode) : this(dto, (ICollection<string>) null,
            statusCode)
        {
        }

        private StatusCodeAndDtoWrapper(ErrorDtoResponse dto, string message, HttpStatusCode statusCode) : this(dto,
            new SingletonList<string>(message), statusCode)
        {
        }


        public static HttpResponseMessage BuildGenericNotFound()
        {
            return new StatusCodeAndDtoWrapper(new ErrorDtoResponse("Not Found"), 404);
        }

        public static StatusCodeAndDtoWrapper BuildBadRequest(ModelStateDictionary modelStateDictionary)
        {
            var errorRes = new ErrorDtoResponse();

            foreach (var key in modelStateDictionary.Keys)
            foreach (var error in modelStateDictionary[key].Errors)
                errorRes.FullMessages.Add(error.ErrorMessage);

            return new StatusCodeAndDtoWrapper(errorRes, 400);
        }

        public static HttpResponseMessage BuildSuccess(AppResponse dto)
        {
            return new StatusCodeAndDtoWrapper(dto, 200);
        }

        public static HttpResponseMessage BuildSuccess(AppResponse dto, string message)
        {
            return new StatusCodeAndDtoWrapper(dto, message);
        }


        public static HttpResponseMessage BuildSuccess(string message)
        {
            return new StatusCodeAndDtoWrapper(new SuccessResponse(message), 200);
        }

        public static HttpResponseMessage BuildErrorResponse(string message)
        {
            return new StatusCodeAndDtoWrapper(new ErrorDtoResponse(message), 500);
        }

        public static HttpResponseMessage BuildGeneric(AppResponse dto, ICollection<string> messages = null,
            int statusCode = 200)
        {
            return new StatusCodeAndDtoWrapper(dto, messages, statusCode);
        }


        public static HttpResponseMessage BuildUnauthorized(ICollection<string> errors = null)
        {
            var res = new ErrorDtoResponse();
            if (errors != null)
                foreach (var error in errors)
                    res.FullMessages.Add(error);

            return new StatusCodeAndDtoWrapper(res, 401);
        }

        public static HttpResponseMessage BuildUnauthorized(string message = null)
        {
            if (message != null)
            {
                var fullMessages = new List<string>(1);
                fullMessages.Add(message);
                return BuildUnauthorized(fullMessages);
            }

            return BuildUnauthorized((ICollection<string>) null);
        }

        public static HttpResponseMessage BuildNotFound(AppResponse responseDto)
        {
            return new StatusCodeAndDtoWrapper(responseDto, 404);
        }

        public static HttpResponseMessage BuildNotFound(string message)
        {
            return new StatusCodeAndDtoWrapper(new ErrorDtoResponse(), message, HttpStatusCode.NotFound);
        }
    }
}