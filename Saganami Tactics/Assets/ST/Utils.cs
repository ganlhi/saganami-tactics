using System;

namespace ST
{
    public class Utils
    {
        public static string GenerateId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "")
                .Replace("+", "-")
                .Replace("/", "_");
        }
    }
}