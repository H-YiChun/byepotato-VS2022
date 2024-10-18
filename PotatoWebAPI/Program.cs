using PotatoWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using MailKit;
using PotatoWebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<GoodbyepotatoContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("goodbyepotato")));
builder.Services.AddScoped<SendEmail>();

//�]�w�}�����
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("http://localhost:5173").WithHeaders("*").WithMethods("*"));
});

//�֨�
builder.Services.AddControllers(); // �[�J MVC ����A��
builder.Services.AddMemoryCache(); // �[�J���s�֨��A��

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("AllowAll");  //���\�����Ū��

app.UseStaticFiles();//�ǰe�R�A�Ϥ�

app.UseAuthorization();

app.MapControllers();

app.Run();
