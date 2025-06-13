using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace LoginRegister.Helpers
{
    public static class JwtHelper
    {
        public static string GetClaim(string token, string claimType)
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return null;

            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(claimType, out var claim))
            {
                return claim.GetString();
            }

            return null;
        }
    }
}
