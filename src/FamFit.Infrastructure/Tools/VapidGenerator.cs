using FamFit.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebPush;

namespace FamFit.Infrastructure.Tools;

/// <summary>
/// Утилита для генерации VAPID ключей и записи их в appsettings.json.
/// Запустить: dotnet run --project FamFit.Infrastructure -- generate-vapid
/// </summary>
public static class VapidGenerator
{
    public static void Generate(string? appsettingsPath = null)
    {
        appsettingsPath ??= "appsettings.json";

        // Генерируем VAPID ключи
        var vapidConfig = VapidConfig.GenerateVapidConfig();

        Console.WriteLine("=== FamFit VAPID Key Generator ===");
        Console.WriteLine();
        Console.WriteLine("Generated VAPID Keys:");
        Console.WriteLine($"  Public Key:    {vapidConfig.PublicKey}");
        Console.WriteLine($"  Private Key:   {vapidConfig.PrivateKey}");
        Console.WriteLine();
        Console.WriteLine("Copy these values into your appsettings.json:");
        Console.WriteLine();
        Console.WriteLine("  \"WebPush\": {");
        Console.WriteLine($"    \"PublicKey\": \"{vapidConfig.PublicKey}\",");
        Console.WriteLine($"    \"PrivateKey\": \"{vapidConfig.PrivateKey}\",");
        Console.WriteLine("    \"ContactEmail\": \"admin@famfit.local\"");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("IMPORTANT: Store these keys securely and do NOT commit them to git!");
    }
}
