using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Extensions
{
    public class PaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; set; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }
        public PaginatedList()
        {
        }
        public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
        {
            // check range of pageNumber, pageSize
            if (pageNumber < 1)
            {
                throw new ValidationException("Page number must be greater than 0");
            }

            if (pageSize < 1)
            {
                throw new ValidationException("Page size must be greater than 0");
            }

            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            ValidatePageNumberAndPageSize(pageNumber, pageSize);
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
        public static PaginatedList<T> Create(List<T> source, int pageNumber, int pageSize)
        {
            ValidatePageNumberAndPageSize(pageNumber, pageSize);
            var totalCount = source.Count;
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }

        private static void ValidatePageNumberAndPageSize(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ValidationException("Page number must be greater than 0");
            }

            if (pageSize < 1)
            {
                throw new ValidationException("Page size must be greater than 0");
            }
        }
    }
}
