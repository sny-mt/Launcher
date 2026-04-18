using System;

namespace DesktopLauncher.Services
{
    internal static class UrlHelper
    {
        public static bool IsUrl(string path)
        {
            return TryNormalizeHttpUrl(path, out _);
        }

        public static bool TryNormalizeHttpUrl(string? input, out string normalizedUrl)
        {
            if (TryParseHttpUrl(input, out var uri))
            {
                normalizedUrl = uri!.AbsoluteUri;
                return true;
            }

            normalizedUrl = string.Empty;
            return false;
        }

        public static bool TryParseHttpUrl(string? input, out Uri? result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var trimmed = input!.Trim();
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = $"https://{trimmed}";
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                return false;
            }

            result = uri;
            return true;
        }
    }
}
