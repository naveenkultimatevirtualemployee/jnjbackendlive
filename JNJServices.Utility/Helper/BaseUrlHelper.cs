namespace JNJServices.Utility.Helper
{
    public static class BaseUrlHelper
    {
        public static string EnsureUrlHost(string inputUrl, string defaultHost)
        {

            if (string.IsNullOrEmpty(inputUrl))
            {
                return string.Empty;
            }
            // Try to create a Uri object to parse the input URL
            if (Uri.TryCreate(inputUrl, UriKind.Absolute, out Uri? uri))
            {
                // If the input URL already has a host, return it as is
                if (!string.IsNullOrEmpty(uri.Host))
                {
                    return inputUrl;
                }
            }
            else if (Uri.TryCreate(defaultHost, UriKind.Absolute, out Uri? defaultUri))
            {
                // If the input URL is not a valid absolute URL, assume it's a relative path
                // Combine it with the default host
                var combinedUri = new Uri(defaultUri, inputUrl);
                return combinedUri.ToString();
            }

            // If the default host is invalid, or another error occurs, return the original input
            return inputUrl;
        }
    }
}
