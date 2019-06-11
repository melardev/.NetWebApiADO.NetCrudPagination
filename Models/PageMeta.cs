using System;

namespace WebApiADO.NetCrudPagination.Models
{
    public class PageMeta
    {
        public PageMeta()
        {
        }

        public PageMeta(int currentItemsCount, string basePath, int currentPageNumber, int pageSize, int totalItemCount)
        {
            CurrentPageNumber = currentPageNumber;
            CurrentItemsCount = currentItemsCount;
            TotalItemsCount = totalItemCount;
            BasePath = basePath;
            RequestedPageSize = pageSize;

            Offset = (currentPageNumber - 1) * pageSize;

            PrevPageNumber = currentPageNumber;
            NextPageNumber = currentPageNumber;

            var skip = (CurrentPageNumber - 1) * RequestedPageSize;
            var traversedSoFar = skip + CurrentItemsCount;
            var remaining = TotalItemsCount - traversedSoFar;
            HasNextPage = remaining > pageSize;
            HasPrevPage = currentPageNumber > 1;
            if (pageSize == 0) // avoid the 0/0 Division
                NumberOfPages = 0;
            else
                NumberOfPages = (int) Math.Ceiling((decimal) (totalItemCount / pageSize));


            if (HasNextPage)
                NextPageNumber = CurrentPageNumber + 1;

            NextPageUrl = $"{basePath}/?page={NextPageNumber}&pageSize={RequestedPageSize}";

            if (HasPrevPage)
                PrevPageNumber = CurrentPageNumber - 1;


            PrevPageUrl = $"{basePath}/?page={PrevPageNumber}&pageSize={RequestedPageSize}";
        }

        public int CurrentPageNumber { get; set; }

        public int TotalItemsCount { get; set; }
        public int CurrentItemsCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPrevPage { get; set; }
        public string PrevPageUrl { get; set; }
        public string NextPageUrl { get; set; }

        public int PrevPageNumber { get; set; }
        public int NextPageNumber { get; set; }
        public string BasePath { get; set; }
        public int NumberOfPages { get; set; }
        public int Offset { get; set; }
        public int RequestedPageSize { get; set; }
    }
}