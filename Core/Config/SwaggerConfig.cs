using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RESTful_web_API_Course.Core.Config; 

public class SwaggerConfig : IConfigureOptions<SwaggerGenOptions> {
    public void Configure(SwaggerGenOptions options) {
        options.SwaggerDoc("v1", new OpenApiInfo {
            Version = "v1.0",
            Title = "Boilerplate API",
            Description = "Api boilerplate code",
            TermsOfService = new Uri("toc/url"),
            Contact = new OpenApiContact {
                Name = "Jamie",
                Url = new Uri("jamie@jamiemullis.com")
            }
        });
        
        options.SwaggerDoc("v2", new OpenApiInfo {
            Version = "v2.0",
            Title = "Boilerplate API",
            Description = "Api boilerplate code",
            TermsOfService = new Uri("toc/url"),
            Contact = new OpenApiContact {
                Name = "Jamie",
                Url = new Uri("jamie@jamiemullis.com")
            }
        });
    }
}