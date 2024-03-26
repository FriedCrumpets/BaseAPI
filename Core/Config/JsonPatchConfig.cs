using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace RESTful_web_API_Course.Core.Config; 

public static class JsonPatchConfig {
    public static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }
}