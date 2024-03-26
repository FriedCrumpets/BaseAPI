using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTful_web_API_Course.Core;
using RESTful_web_API_Course.Core.Config;
using RESTful_web_API_Course.Core.Middleware;
using RESTful_web_API_Course.Core.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("Log/logfile.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

builder.Services.AddControllers(options => {
    options.InputFormatters.Insert(0, JsonPatchConfig.GetJsonPatchInputFormatter());
    options.CacheProfiles.Add("Default30", new CacheProfile { Duration = 30 }); // default cache profile for 30 seconds
});

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddApiVersioning(options => {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.UnsupportedApiVersionStatusCode = (int)StatusCode.NotFound;
        options.ReportApiVersions = true;
    }).AddMvc()
    .AddApiExplorer(options => {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddSwaggerGen();

var app = builder.Build();

// todo: This is just bad. Middleware should be added feature by feature here. eg. app.UseIdentity(); app.UseSwagger(); and so on
app.UseMiddleware();

app.Run();
