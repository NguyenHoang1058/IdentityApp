using LoginAPI.DbContext;
using LoginAPI.Models;
using LoginAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//be able to inject JWTService class inside our Controllers
builder.Services.AddScoped<JWTService>();

//defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    //password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    //for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>() // be able to add role
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make user of RoleManager
    .AddEntityFrameworkStores<Context>() //providing our context
    .AddSignInManager<SignInManager<User>>() // make use of Signin Manager
    .AddUserManager<UserManager<User>>() //make use of UserManager to create user
    .AddDefaultTokenProviders(); //be able to create tokens for email confirmation

//be able to authenticate user using JWTBearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //validate the token based on the key we have provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            //the issuer signing key based on JWT:Key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            //the issuer which in here is the apu project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            //validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            //don't validate audience (angular side)
            ValidateAudience = false
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };
        return new BadRequestObjectResult(toReturn);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//CORS
app.UseCors(option =>
{
    option.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});
app.UseHttpsRedirection();

//adding UseAuthentication into our pipeline and this should come before UserAuthorization
//Authentication verifies the identity of a user or service and authorization determines their access rights
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
