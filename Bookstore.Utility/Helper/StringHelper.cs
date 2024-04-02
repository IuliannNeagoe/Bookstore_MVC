namespace Bookstore.Utility.Helper
{
    public static class StringHelper
    {
        public static string BuildUrl(string? area = null, string? controller = null, string? action = null, string? id = null)
        {
            if (string.IsNullOrEmpty(area)) return ConstantDefines.DOMAIN;

            if (string.IsNullOrEmpty(controller))
            {
                return $"{ConstantDefines.DOMAIN}/{area}";
            }

            if (string.IsNullOrEmpty(action))
            {
                return $"{ConstantDefines.DOMAIN}/{area}/{controller}";
            }

            if (string.IsNullOrEmpty(id))
            {
                return $"{ConstantDefines.DOMAIN}/{area}/{controller}/{action}";
            }

            return $"{ConstantDefines.DOMAIN}/{area}/{controller}/{action}?id={id}";
        }
    }
}
