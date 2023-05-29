using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Services.Implementation;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICalculationService, CalculationService>();

builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<RepositoryContext>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
