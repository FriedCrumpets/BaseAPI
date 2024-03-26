namespace RESTful_web_API_Course.Core.Middleware; 

public static class ProgramMiddleware {
    // todo: Not a good way to do this. Middleware should be declared at a feature level not as a whole.
    public static void UseMiddleware(this WebApplication app) {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(/*options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
            }*/);
            // app.UseMigrationsEndPoint();
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
    }
}