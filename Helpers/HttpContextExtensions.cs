using Microsoft.EntityFrameworkCore;

namespace CineMatrix_API.Helpers
{
    public static class HttpContextExtensions
    {
        public async static Task InsertPaginationParametersInResponse<T>(this HttpContext httpContext,
            IQueryable<T> queryable, int recordsPerPage)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }
            // Count the total number of records in the IQueryable
            double count = await queryable.CountAsync();
            // Calculate the total number of pages needed for pagination
            double totalAmountPages = Math.Ceiling(count / recordsPerPage);
            // Add the total number of pages to the response headers
            httpContext.Response.Headers.Add("totalAmountPages", totalAmountPages.ToString());
        }
    }
}
