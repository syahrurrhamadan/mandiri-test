using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public static class JsonHelper
{
    // public static string SerializeWithIgnoreCycles<T>(T obj)
    // {
    //     var options = new JsonSerializerOptions
    //     {
    //         ReferenceHandler = ReferenceHandler.IgnoreCycles
    //     };

    //     return JsonSerializer.Serialize(obj, options);
    // }

    public static string SerializeWithIgnoreCycles<T>(T obj)
    {
        var options = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        return JsonConvert.SerializeObject(obj, options);
    }

    public static T ParseJson<T>(string jsonString)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<T>(jsonString);
            return response;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return default;
        }
    }

    public static ContentResult Content<T>(T obj)
    {
        return new ContentResult
        {
            Content = SerializeWithIgnoreCycles(obj),
            ContentType = "application/json",
            StatusCode = 200 // You can customize the status code if needed
        };
    }
}
