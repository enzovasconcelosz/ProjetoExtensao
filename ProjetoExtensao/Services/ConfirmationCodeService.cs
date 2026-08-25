using System;
using System.Collections.Concurrent;

namespace ProjetoExtensao.Services;

public static class ConfirmationCodeService
{
    private static readonly ConcurrentDictionary<string, string> Codes = new();

    public static string GenerateCodeFor(string email)
    {
        var rnd = new Random();
        var code = rnd.Next(100000, 999999).ToString();
        Codes[email] = code;
        return code;
    }

    public static string? GetCodeFor(string email)
    {
        return Codes.TryGetValue(email, out var code) ? code : null;
    }

    public static bool ValidateCode(string email, string code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        if (Codes.TryGetValue(email, out var expected))
        {
            var ok = expected == code;
            if (ok)
                Codes.TryRemove(email, out _);
            return ok;
        }
        return false;
    }
}
