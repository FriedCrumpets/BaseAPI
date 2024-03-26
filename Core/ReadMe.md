To correctly use this core you will require some nuget packages

- EntityFrameworkCore
- apspnetcore.JsonPatch
- NewtonsoftJson
- serilog

# API
Dependency injection for services, controllers, repositories and configurations should be done through extension
methods in groups. This keeps it all clean and tidy for features to be added quickly. Keyyed services.

-> think this should be middleware? 
Inject APIController and use the ExecuteAsync<T> method for Controller Calls
    - This keeps controllers only for the endpoint names and settings 
    - this way services are the only things that contain code and talk to other components

ServiceResponse, ServiceResponse<T> should be used when responding from a service to the controller 
    - this will be used to determine if it was successful/failed and why 
    - the result data can be held within this response
    
Response<T> will be returned from APIController with the status, time elapsed and more
    - this creates a standard for comms and makes all requests simple to extend if necessary 

appsettings.json can be passed down into services and repositories as IConfiguration you can then 
get anything you put in here through a key string such as "ApiConfig:CurrentAPIVersion" 

Execution environments, such as Development, Staging, and Production, are available in ASP.NET Core. 
Specify the environment an app is running in by setting the ASPNETCORE_ENVIRONMENT environment variable. 
ASP.NET Core reads that environment variable at app startup and stores the value in an IWebHostEnvironment implementation.
```csharp
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
```

## Logging
inject ILogger<[IServiceName]> logger into a classes that require logging... duh. 
The below are the built-in provider aliases used to configure logging in appsettings.json
- Console
- Debug
- EventSource
- EventLog
- AzureAppServicesFile
- AzureAppServicesBlob
- ApplicationInsights
ILogger<T> is equivalent to calling [ILoggerFactory].CreateLogger with the fully qualified type name of T.
Below is an example of error logging
```csharp 
try { /* Some code */ } 
catch (Exception ex)
{
    _logger.LogWarning(MyLogEvents.GetItemNotFound, ex, "TestExp({Id})", id);
    return NotFound();\\
}
```
HTTP logging, logs request/ response information. Use it carefully not to expose personal information.
Perhaps most useful in Middleware to log certain information relating to the request. 

## patch doc
as long as this line is in program.cs in some way the build pipeline for patch will enable the following code snippet
```csharp
builder.Services.AddControllers(options => {
    options.InputFormatters.Insert(0, JsonPatchConfig.GetJsonPatchInputFormatter()); // eh?
});
```

```csharp
[HttpPatch]
public IActionResult UpdateThing([FromBody] JsonPatchDocument<Thing> patch){
    var thing = CreateThing();

    patchDoc.ApplyTo(thing, ModelState); // where are we getting 'ModelState' from?
} 
```

## Content negotiation
Content negotiation takes place when an Accept header appears in the request. When a request contains an accept header, ASP.NET Core:

If no formatter is found that can satisfy the client's request, ASP.NET Core:

Returns 406 Not Acceptable if MvcOptions.ReturnHttpNotAcceptable is set to true

By default ASP.NET Core supports the following media types
- application/json
- text/json
- text/plain

## Middleware 
Recommended order for built-in middleware 
```csharp
var app = builder.Build();
 
 if (app.Environment.IsDevelopment())
 {
     app.UseMigrationsEndPoint();
 }
 else
 {
     app.UseExceptionHandler("/Error");
     app.UseHsts();
 }
 
 app.UseHttpsRedirection();
 app.UseStaticFiles();
 // app.UseCookiePolicy();
 
 app.UseRouting();
 // app.UseRateLimiter();
 // app.UseRequestLocalization();
 // app.UseCors();
 
 app.UseAuthentication();
 app.UseAuthorization();
 // app.UseSession();
 // app.UseResponseCompression();
 // app.UseResponseCaching();
 ```

Also it should be noted when using middleware this is how a standard middleware class should look. Why I'm not sure 
```csharp
public class MyMiddleWare(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context) {
        // regarding the request
        await next(context);
        // regarding the response
    }
}
```

## Entity Framework Core 
[Key] for the key of the entity
[Required] if the key is required property
[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 🤷‍♂️
[ForeignKey("Prop name")] example : 
```csharp
[ForeignKey("User")] int UserID  { get; set; }
User User { get; set; }
```

never delete a migration it could break something. If a Migration goes wrong just update what you need 
to and add-migration [MigrationName] the previous migration should update with all of the new data

replace [text] with the relevant text and remove the []
add-migration [MigrationName]
update-database

Models are what are used with the database. Once they're set in the DBContext EFCore can be used to create 
the tables as required.

## Identity
For default identity usage
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
    
Create a custom class ApplicationUser : IdentityUser
modify ApplicationDbContext to inherit from IdentityDbContext<ApplicationUser>
Add DbSet<ApplicationUser> to the ApplicationDbContext
in ApplicationDbContext; OnModelCreating(ModelBuilder builder) { [needs to call base if implemented] }
Pass UserManager<ApplicationUser> into the User Repository  

Oh boy...

in program.cs make sure to add
```csharp
builder.Services.AddDbContext<[IdentityDbContext]>(options => {
    options.[UseMiddleware]());   
    options.[UseServer]([ConnectionString]));   
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<UserDbContext>();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();  // authentication must be added before authorization
app.UseAuthorization();
```

## Roles
Roles typically define who the user is in a given context
A single user can have multiple roles to grant access certain things. 
Treat this like a tag system. Tagging a user with roles providing access.

## Claims
piece of information about that user. Usually data driven and should not be specific to user
Think of them as attributes of a user.

## Notes
It appears that in most cases Identity was designed for Single Page Applications and web apps; not APIs. ( Is this a lost cause? )
it can however be used to generate JWT tokens on return. Meaning that login logic can be completely
dealt with via Identity. Not sure of the security risks, but this should simplify and speed up development. 
Are JWT tokens 100% necessary? Can I use a different implementation?    
https://www.c-sharpcorner.com/article/jwt-authentication-and-authorization-in-net-6-0-with-identity-framework/

### Roles
below is a snippet of code used when assigning a role on registration 
    - create the role if it doesn't exist
    - add the user to that role

I'm not entirely sure of the security behind this, but it's good to know how to do it simply
and this covers most of the simple basis. You would want to secure an endpoint to create a new role  
behind an Admin user.  
'
    if(false == await roleManager.RoleExistsAsync(userRegistrationDTO.Role){
        await roleManager.CreateAsync(new IdentityRole(userRegistrationDTO.Role)); 
    }
    
    await userManager.AddToRoleAsync(user, userRegistrationDTO.Role); 
'

## Caching
builder.Services.AddResponseCaching()

add the following [] to a controller method returning IActionResult to enable caching.
[ResponseCache(Duration = [time], Location = [Location], NoStore = [bool])] adds caching to a request.    
    - if [time] = 30; 50 requests in one minute will be 2 calculated requests.
    - [Location] = location of store. None means it will not be cached 
    - [NoStore] = if true it won't be stored anywhere (mainly used for error pages) 
    
[ResponseCache(CacheProfileName = [config name])]
    - [config name] is the cache profile defined in controllers configuration 
        (AddControllers(options => { [define here]}))

## DTOs
DTOs should be different objects for each operation where different data is passed 
even if said data is to modify one type, pass only the necessary data. Even if data is the same. Create a different DTO.
The aim here is to prevent difficulties later down the line, but it will create a lot of scripts/project bloat. <- not an issue

## Versioning
nuget asp.net.versioning [get the MVC package... for some bizarre reason]

builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.UnsupportedApiVersionStatusCode = (int)StatusCode.NotFound;
    options.ReportApiVersions = true;
}).AddMvc().AddApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

[ApiVersionNeutral] for controllers that don't have different versions (user/auth controllers are good examples of this)
[Route("{version:apiVersion}/[controllerName]"] route versioning. Typically the controller will sit in a version directory
[ApiVersion("[version]", Deprecated = [state])] 
    - controller versioning (one controller can have multiple versions <- this is bad practice though)
    - Deprecated flag sets state in Header to deprecated. Consumers should listen for this flag
[MapToApiVersion("[version]")] Controller endpoint versioning. One controller can contain multiple versioned endpoints

# Needs further research
## File Uploading

The data model stored in the Db will need to include the filepath and the url to that filePath. 
This way the data can be provided to the relevant DTOs and passed back towards the user

There's a interface called IFormFile, this is one way of passing files to a backend using a form. 
    - this IFormFile would need to be included in the uploadDTO, Update DTO. Anything that updates the file.

## Blob storage
No idea about this one yet. Very curious.

## Signal R
Could be a spicy thing to learn.
  
 ### app.UseStaticFiles()
Should only be included when adding static files to a project in program.cs. 
All static files must be stored in a wwwroot folder. <- what do we define as a static file? 

