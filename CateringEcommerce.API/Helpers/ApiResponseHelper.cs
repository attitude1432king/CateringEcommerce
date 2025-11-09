using Microsoft.AspNetCore.Mvc;
namespace CateringEcommerce.API.Helpers
{
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Returns a standardized success (200 OK) response.
        /// </summary>
        public static IActionResult Success(object? data = null, string? message = null, string? type = null)
        {
            return new OkObjectResult(new
            {
                result = true,
                message,
                type = type ?? "success",
                data
            });
        }

        /// <summary>
        /// Returns a standardized failure (400 BadRequest) response.
        /// </summary>
        public static IActionResult Failure(string message, string? type = null, object? data = null)
        {
            var response = new
            {
                result = false,
                message,
                type = type ?? "error",
                data
            };

            // If it's a true error, return BadRequest; otherwise (like "warning", "info"), return Ok
            if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase) || type == null)
            {
                return new BadRequestObjectResult(response);
            }
            else
            {
                return new OkObjectResult(response);
            }
        }

    }
}
