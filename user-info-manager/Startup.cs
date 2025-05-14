using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserInfoManager.Data;
using UserInfoManager.Service;

namespace UserInfoManager;

public class Startup
{
    /// <summary>
    /// 读取配置信息
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        // 获取连接字符串
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        // 打印连接字符串
        Console.WriteLine(connectionString);
    }

    public IConfiguration Configuration { get; }

    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]);

        // 配置 JWT 和 Cookie 身份验证
        services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        services.AddAuthorization();
        services.AddControllers();

        // 配置跨域策略
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
        });

        services.AddControllersWithViews();//启用MVC

        // 添加AdoNetHelper
        services.AddSingleton(provider =>
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            return new AdoNetHelper(connectionString);
        });
        // 添加数据访问类
        services.AddScoped<IMembersDataAccess, MembersDataAccess>();
        services.AddScoped<IUserContactDataAccess, UserContactDataAccess>();
        services.AddScoped<IUserAddressDataAccess, UserAddressDataAccess>();
        services.AddScoped<IRelationshipDataAccess, RelationshipDataAccess>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<LookupsDataAccess>();

        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        services.AddHostedService<DeleteUnverifiedUsersService>();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserInfoManager API", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        //异常处理
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowAllOrigins");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserInfoManager API V1");
        });

        //路由
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=AccountView}/{action=Login}/{id?}");
        });
    }
}