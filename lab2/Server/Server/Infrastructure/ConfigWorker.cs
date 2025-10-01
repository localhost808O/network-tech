using System.Text.Json;
using Server.Core.Dto;

namespace Server.Infrastructure.Io;

public class ConfigWorker
{
    public static T GetConfigFromJson<T>(string path) 
    {
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }
        Console.WriteLine($"Reading config from: {fullPath}");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Config file not found: {path}");            
        }
        
        string json;
        try
        {
            using (var reader = new StreamReader(fullPath))
            {
                json = reader.ReadToEnd();
            }
        }
        catch (JsonException ex)
        {
            throw new FileLoadException($"Failed to load config file: {path}", ex);
        }   
        var ob = JsonSerializer.Deserialize<T>(json);
        return ob;
    }
}