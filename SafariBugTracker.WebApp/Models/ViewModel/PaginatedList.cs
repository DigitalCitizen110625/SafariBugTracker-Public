using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafariBugTracker.WebApp.Models.ViewModels
{

    /// <summary>
    /// Allows for the creation of paginated lists/collections, where the data is arranged into groups of smaller-sets (aka pages)
    /// </summary>
    /// <remarks>
    /// SOURCE: https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-3.1
    /// </remarks>
    /// <typeparam name="T">Type of the paginated list</typeparam>
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// Defines the current page number in the paginated list (i.e. page #1, #2, #3 etc.)
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Defines the total number of pages, or sub-sets in the list
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Checks if the page number is at the first index position
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Checks if the page number is at the last index position
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;


        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        /// <summary>
        /// Creates the paginated list according to the number of elements per page, and at the specified page index
        /// </summary>
        /// <param name="source">Queryable collection that can be split into a series of sub-sets (pages)</param>
        /// <param name="pageIndex">Current page number</param>
        /// <param name="pageSize">Number of elements in each sub-set (page)</param>
        /// <returns>Collection containing the elements in the selected page index </returns>
        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }


        /// <summary>
        /// Creates the paginated list according to the number of elements per page, and at the specified page index
        /// Note: This method is used instead of a constructor because constructors can't run asynchronous code
        /// </summary>
        /// <param name="source">Queryable collection that can be split into a series of sub-sets (pages)</param>
        /// <param name="pageIndex">Current page number</param>
        /// <param name="pageSize">Number of elements in each sub-set (page)</param>
        /// <returns>Task of the create list operation </returns>
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }


    }//class
}//namespace