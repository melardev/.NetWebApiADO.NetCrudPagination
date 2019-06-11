using WebApiADO.NetCrudPagination.Models;

namespace WebApiADO.NetCrudPagination.Dtos.Responses.Shared
{
    public abstract class PagedDto : SuccessResponse
    {
        public PageMeta PageMeta { get; set; }
    }
}