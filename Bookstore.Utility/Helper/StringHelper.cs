namespace Bookstore.Utility.Helper
{
    public static class StringHelper
    {
        public static string BuildUrl(string domain, string? area = null, string? controller = null, string? action = null, string? id = null)
        {
            if (string.IsNullOrEmpty(area)) return domain;

            if (string.IsNullOrEmpty(controller))
            {
                return $"{domain}/{area}";
            }

            if (string.IsNullOrEmpty(action))
            {
                return $"{domain}/{area}/{controller}";
            }

            if (string.IsNullOrEmpty(id))
            {
                return $"{domain}/{area}/{controller}/{action}";
            }

            return $"{domain}/{area}/{controller}/{action}?id={id}";
        }
    }
}
