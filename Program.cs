using Dapper;
using FluentValidation;
using Median.Authentication.Simple;
using Median.Authentication.Simple.Common;
using Median.Authentication.Simple.Facade;
using Median.Authentication.Simple.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Profakt.Intranet.Common;
using Serilog;
using System.ComponentModel.Design;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// CONFIGURATION
// -----------------------------------------------------------

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// -----------------------------------------------------------
// LOGGING
// -----------------------------------------------------------

string sequrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Median.Intranet")
    .WriteTo.Seq(sequrl)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// -----------------------------------------------------------
// DATABASE
// -----------------------------------------------------------

var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
);
var dataSource = dataSourceBuilder.Build();

// -----------------------------------------------------------
// SERVICES
// -----------------------------------------------------------

builder.Services.AddRequestValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// nice validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            Message = "Validation failed",
            Errors = errors
        });
    };
});

// -----------------------------------------------------------
// CORS – ALLOW ALL ORIGINS (SAFE WITH JWT)
// -----------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)   // allow ANY origin
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// -----------------------------------------------------------
// Dependency Injection
// -----------------------------------------------------------

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddControllers();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// -----------------------------------------------------------
// BUILD
// -----------------------------------------------------------

var app = builder.Build();

// -----------------------------------------------------------
// SWAGGER
// -----------------------------------------------------------

if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -----------------------------------------------------------
// PIPELINE
// -----------------------------------------------------------

app.UseHttpsRedirection();

app.UseRouting();

// ? CORS must be here
app.UseCors("Frontend");

app.UseAuthentication();

//debug
app.Use(async (context, next) =>
{
    Log.Information("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    Log.Information("Authorization header: {Auth}", context.Request.Headers["Authorization"].ToString());

    await next();

    Log.Information("Response status: {Status}", context.Response.StatusCode);
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        Log.Information("User authenticated: {Email}", context.User.FindFirstValue(ClaimTypes.Email));
    }
    else
    {
        Log.Warning("User NOT authenticated");
    }
});
//enddebug


app.UseAuthorization();

app.MapControllers();

// OPTIONS fallback
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .AllowAnonymous();

// -----------------------------------------------------------
// MINIMAL API ENDPOINTS
// -----------------------------------------------------------

// Basic health check
app.MapGet("/", () => "Median RAG Server")
   .RequireCors("Frontend");

app.MapPost("/login", [AllowAnonymous] async (IAuthFacade auth, LoginRequest req) =>
{
    var result = await auth.LoginAsync(req.Email, req.Password);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
})
.RequireCors("Frontend");

app.MapPost("/register", [Authorize(Roles = UserRoles.Admin)] async (IAuthFacade auth, RegisterRequest req) =>
{
    var result = await auth.RegisterAsync(req.Email, req.Password);
    return result ? Results.Unauthorized() : Results.Ok(result);
})
.RequireCors("Frontend");

app.MapPost("/refresh", [AllowAnonymous] async (IAuthFacade auth, RefreshRequest req) =>
{
    var result = await auth.RefreshAsync(req.RefreshToken);
    return result is null ? Results.Unauthorized() : Results.Ok(result);
})
.RequireCors("Frontend");

app.MapPost("/logout", [Authorize] async (IAuthFacade auth, RefreshRequest req) =>
{
    var result = await auth.LogoutAsync(req.RefreshToken);
    return result
        ? Results.Ok(new { Message = "Logout successful" })
        : Results.BadRequest(new { Message = "Invalid or expired token" });
})
.RequireCors("Frontend");

app.MapPost("/changerole", [Authorize(Roles = UserRoles.Admin)] async (ClaimsPrincipal user, ChangeRoleRequest request, IAuthFacade authFacade) =>
{
    if (request.NewRole != UserRoles.Normal && request.NewRole != UserRoles.Admin)
    {
        return Results.BadRequest(new { status = "failed", reason = $"Invalid role: {request.NewRole}" });
    }

    var email = user.FindFirstValue(ClaimTypes.Email);
    var result = await authFacade.ChangeRole(request.UserId, request.NewRole);
    return Results.Ok(result);
})
.RequireCors("Frontend");

app.MapGet("/me", [Authorize] (ClaimsPrincipal user) =>
{
    var id = user.FindFirstValue("userId");
    var email = user.FindFirstValue(ClaimTypes.Email);
    var role = user.FindFirstValue(ClaimTypes.Role);

    return Results.Ok(new
    {
        Message = "Du er logget ind",
        UserId = id,
        Email = email,
        Role = role,
        IssuedAt = DateTime.UtcNow
    });
})
.RequireCors("Frontend");

// -----------------------------------------------------------
// RUN
// -----------------------------------------------------------

app.Run();
