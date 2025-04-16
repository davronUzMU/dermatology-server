//using Dermatologiya.Server.Data;
//using Dermatologiya.Server.RepositoriesAll.BlockRep;
//using Dermatologiya.Server.RepositoriesAll.CustomerRep;
//using Dermatologiya.Server.RepositoriesAll.DoctorRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalBlockRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalDepartmentRep;
//using Dermatologiya.Server.RepositoriesAll.ImageRep;
//using Dermatologiya.Server.RepositoriesAll.NewsRep;
//using Dermatologiya.Server.RepositoriesAll.PriseForServicesRep;
//using Dermatologiya.Server.RepositoriesAll.QueueRep;
//using Dermatologiya.Server.RepositoriesAll.VideoRep;
//using Dermatologiya.Server.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using Microsoft.IdentityModel.Tokens;
//using Minio;
//using System.Runtime;
//using System.Text;
//using DotNetEnv;
//using Minio.DataModel.Args;

//namespace Dermatologiya.Server
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            Env.Load();

//            var builder = WebApplication.CreateBuilder(args);

//            string? connectionString = Env.GetString("CONNECTIONSTRINGS__WEBAPIDATABASE");
//            builder.Services.AddDbContext<AppDbContext>(options =>
//                options.UseNpgsql(connectionString));

//            var userName = Env.GetString("ADMIN__USERNAME");
//            var password = Env.GetString("ADMIN__PASSWORD");
//            var secretKey = Env.GetString("JWTSETTINGS__SECRETKEY");
//            var issuer = Env.GetString("JWTSETTINGS__ISSUER");
//            var audience = Env.GetString("JWTSETTINGS__AUDIENCE");
//            var tokenLifetime = Env.GetInt("JWTSETTINGS__TOKENLIFETIME");

//            var endpoint = Env.GetString("MINIO_ENDPOINT");
//            var accessKey = Env.GetString("MINIO_ACCESS_KEY");
//            var secretKeyMinio = Env.GetString("MINIO_SECRET_KEY");
//            var secure = bool.Parse(Env.GetString("MINIO_SECURE") ?? "true");


//            //var bucketName = Env.GetString("MINIO_BUCKET");
//            //Console.WriteLine($"Bucket nomi: {bucketName}");




//            var env_port = Env.GetString("ENVIRONMENT_PORT");

//            //builder.Services.AddMinio(accessKey, secretKeyMinio);

//            builder.Services.AddMinio(configureClient => configureClient
//                                      .WithEndpoint(endpoint)
//                                      .WithCredentials(accessKey, secretKeyMinio)
//                                      .WithSSL(secure)
//                                      .Build());




//            const string CorsPolicy = "CorsPolicy";

//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy(CorsPolicy, policy =>
//                {
//                    policy.WithOrigins("https://localhost:" + env_port)  // front-end porti va   localhost:  ham o'zgaradi
//                              .AllowAnyMethod()
//                              .AllowAnyHeader()
//                              .AllowCredentials();

//                });
//            });

//            var key = Encoding.UTF8.GetBytes(secretKey);

//            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.RequireHttpsMetadata = false;
//                    options.SaveToken = true;
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuerSigningKey = true,
//                        IssuerSigningKey = new SymmetricSecurityKey(key),
//                        ValidateIssuer = true,
//                        ValidIssuer = issuer,
//                        ValidateAudience = true,
//                        ValidAudience = audience,
//                        ValidateLifetime = true
//                    };
//                });

//            builder.Services.Configure<FormOptions>(options =>
//            {
//                options.MultipartBodyLengthLimit = 200_000_000; // 200MB
//            });

//            // Repository va Service larni DI ga qo‘shish
//            builder.Services.AddScoped<IVideoRepository, VideoRepository>();
//            builder.Services.AddScoped<IImageRepository, ImageRepository>();
//            builder.Services.AddScoped<IBlockRootRepository, BlockRootRepository>();
//            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
//            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
//            builder.Services.AddScoped<IBlockRepository, BlockRepository>();
//            builder.Services.AddScoped<IHospitalDepartmentRepository, HospitalDepartmentRepository>();
//            builder.Services.AddScoped<INewsRepository, NewsRepository>();
//            builder.Services.AddScoped<IPriceForServiceRepository, PriceForServiceRepository>();
//            builder.Services.AddScoped<IQueueRepository, QueueRepository>();

//            builder.Services.AddScoped<IVideoService, VideoService>();
//            builder.Services.AddScoped<IImageService, ImageService>();
//            builder.Services.AddScoped<BlockRootService>();
//            builder.Services.AddScoped<CustomerService>();
//            builder.Services.AddScoped<DoctorService>();
//            builder.Services.AddScoped<HospitalBlockService>();
//            builder.Services.AddScoped<HospitalDepartmentService>();
//            builder.Services.AddScoped<NewsService>();
//            builder.Services.AddScoped<PriceService>();
//            builder.Services.AddScoped<QueueService>();

//            builder.Services.AddScoped<JwtService>(JwtService =>
//            {
//                return new JwtService(userName, password, secretKey, issuer, audience, tokenLifetime);
//            });
//            //builder.Services.AddScoped<JwtService>();

//            builder.Services.AddControllers();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            var app = builder.Build();

//            // Static fayllarni xizmat qilish
//            app.UseWhen(context => context.Request.Path.StartsWithSegments("/VideoMP4/hls"), appBuilder =>
//            {
//                appBuilder.UseCors("CorsPolicy");

//                var provider = new FileExtensionContentTypeProvider();
//                provider.Mappings[".m3u8"] = "application/x-mpegURL";
//                provider.Mappings[".ts"] = "video/MP2T";

//                appBuilder.UseStaticFiles(new StaticFileOptions
//                {
//                    ContentTypeProvider = provider,
//                    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "VideoMP4/hls")),
//                    RequestPath = "/VideoMP4/hls"
//                });
//            });

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseRouting();

//            app.UseCors(CorsPolicy);
//            app.Use(async (context, next) =>
//            {
//                if (context.Request.Method == HttpMethods.Options)
//                {
//                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
//                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
//                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true"); // Bu muhim
//                    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

//                    context.Response.StatusCode = 200;
//                    await context.Response.CompleteAsync();
//                }
//                else
//                {
//                    await next();
//                }
//            });


//            app.UseHttpsRedirection();

//            app.UseAuthentication();
//            app.UseAuthorization();


//            app.MapControllers();

//            app.MapFallbackToFile("/index.html");

//            app.Run();
//        }
//    }
//}



//using Dermatologiya.Server.Data;
//using Dermatologiya.Server.RepositoriesAll.BlockRep;
//using Dermatologiya.Server.RepositoriesAll.CustomerRep;
//using Dermatologiya.Server.RepositoriesAll.DoctorRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalBlockRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalDepartmentRep;
//using Dermatologiya.Server.RepositoriesAll.ImageRep;
//using Dermatologiya.Server.RepositoriesAll.NewsRep;
//using Dermatologiya.Server.RepositoriesAll.PriseForServicesRep;
//using Dermatologiya.Server.RepositoriesAll.QueueRep;
//using Dermatologiya.Server.RepositoriesAll.VideoRep;
//using Dermatologiya.Server.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using Microsoft.IdentityModel.Tokens;
//using Minio;
//using System.Runtime;
//using System.Text;
//using DotNetEnv;

//namespace Dermatologiya.Server
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            Env.Load();

//            var builder = WebApplication.CreateBuilder(args);

//            string? connectionString = Env.GetString("CONNECTIONSTRINGS__WEBAPIDATABASE");
//            builder.Services.AddDbContext<AppDbContext>(options =>
//                options.UseNpgsql(connectionString));

//            var userName = Env.GetString("ADMIN__USERNAME");
//            var password = Env.GetString("ADMIN__PASSWORD");
//            var secretKey = Env.GetString("JWTSETTINGS__SECRETKEY");
//            var issuer = Env.GetString("JWTSETTINGS__ISSUER");
//            var audience = Env.GetString("JWTSETTINGS__AUDIENCE");
//            var tokenLifetime = Env.GetInt("JWTSETTINGS__TOKENLIFETIME");

//            var env_port = Env.GetString("ENVIRONMENT__PORT");


//            const string CorsPolicy = "CorsPolicy";

//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy(CorsPolicy, policy =>
//                {
//                    policy
//                        .WithOrigins(
//                            "http://localhost:3000",
//                            "http://127.0.0.1:3000",
//                            "http://127.0.0.1:10006",
//                            "http://localhost:10006",
//                            "http://localhost:5044",
//                            "http://127.0.0.1:5044",
//                            "https://dermatology-navoiy.uz",
//                            "https://*.dermatology-navoiy.uz",
//                            "https://api.dermatology-navoiy.uz"
//                        )
//                        .AllowAnyMethod()
//                        .AllowAnyHeader()
//                        .AllowCredentials();
//                });

//            });

//            var key = Encoding.UTF8.GetBytes(secretKey);

//            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.RequireHttpsMetadata = false;
//                    options.SaveToken = true;
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuerSigningKey = true,
//                        IssuerSigningKey = new SymmetricSecurityKey(key),
//                        ValidateIssuer = true,
//                        ValidIssuer = issuer,
//                        ValidateAudience = true,
//                        ValidAudience = audience,
//                        ValidateLifetime = true
//                    };
//                });

//            builder.Services.Configure<FormOptions>(options =>
//            {
//                options.MultipartBodyLengthLimit = 200_000_000; // 200MB
//            });
//            // Repository va Service larni DI ga qo‘shish
//            builder.Services.AddScoped<IVideoRepository, VideoRepository>();
//            builder.Services.AddScoped<IImageRepository, ImageRepository>();
//            builder.Services.AddScoped<IBlockRootRepository, BlockRootRepository>();
//            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
//            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
//            builder.Services.AddScoped<IBlockRepository, BlockRepository>();
//            builder.Services.AddScoped<IHospitalDepartmentRepository, HospitalDepartmentRepository>();
//            builder.Services.AddScoped<INewsRepository, NewsRepository>();
//            builder.Services.AddScoped<IPriceForServiceRepository, PriceForServiceRepository>();
//            builder.Services.AddScoped<IQueueRepository, QueueRepository>();

//            builder.Services.AddScoped<IVideoService, VideoService>();
//            builder.Services.AddScoped<IImageService, ImageService>();
//            builder.Services.AddScoped<BlockRootService>();
//            builder.Services.AddScoped<CustomerService>();
//            builder.Services.AddScoped<DoctorService>();
//            builder.Services.AddScoped<HospitalBlockService>();
//            builder.Services.AddScoped<HospitalDepartmentService>();
//            builder.Services.AddScoped<NewsService>();
//            builder.Services.AddScoped<PriceService>();
//            builder.Services.AddScoped<QueueService>();
//            builder.Services.AddScoped<JwtService>(JwtService =>
//            {
//                return new JwtService(userName, password, secretKey, issuer, audience, tokenLifetime);
//            });
//            //builder.Services.AddScoped<JwtService>();

//            builder.Services.AddControllers();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            var app = builder.Build();

//            // Static fayllarni xizmat qilish
//            app.UseWhen(context => context.Request.Path.StartsWithSegments("/VideoMP4/hls"), appBuilder =>
//            {
//                appBuilder.UseCors("CorsPolicy");

//                var provider = new FileExtensionContentTypeProvider();
//                provider.Mappings[".m3u8"] = "application/x-mpegURL";
//                provider.Mappings[".ts"] = "video/MP2T";

//                appBuilder.UseStaticFiles(new StaticFileOptions
//                {
//                    ContentTypeProvider = provider,
//                    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "VideoMP4/hls")),
//                    RequestPath = "/VideoMP4/hls"
//                });
//            });

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseRouting();

//            // Apply CORS policy before any other middleware that might handle the request
//            app.UseCors(CorsPolicy);

//            app.Use(async (context, next) =>
//            {
//                if (context.Request.Method == HttpMethods.Options)
//                {
//                    // Add CORS headers for preflight requests
//                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
//                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
//                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
//                    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

//                    context.Response.StatusCode = 200;
//                    await context.Response.CompleteAsync();
//                }
//                else
//                {
//                    await next();
//                }
//            });

//            app.UseHttpsRedirection();

//            app.UseAuthentication();
//            app.UseAuthorization();

//            app.MapControllers();

//            app.MapFallbackToFile("/index.html");

//            app.Run();
//        }
//    }
//}
















//using Dermatologiya.Server.Data;
//using Dermatologiya.Server.RepositoriesAll.BlockRep;
//using Dermatologiya.Server.RepositoriesAll.CustomerRep;
//using Dermatologiya.Server.RepositoriesAll.DoctorRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalBlockRep;
//using Dermatologiya.Server.RepositoriesAll.HospitalDepartmentRep;
//using Dermatologiya.Server.RepositoriesAll.ImageRep;
//using Dermatologiya.Server.RepositoriesAll.NewsRep;
//using Dermatologiya.Server.RepositoriesAll.PriseForServicesRep;
//using Dermatologiya.Server.RepositoriesAll.QueueRep;
//using Dermatologiya.Server.RepositoriesAll.VideoRep;
//using Dermatologiya.Server.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using Microsoft.IdentityModel.Tokens;
//using Minio;
//using System.Runtime;
//using System.Text;
//using DotNetEnv;

//namespace Dermatologiya.Server
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            Env.Load();

//            var builder = WebApplication.CreateBuilder(args);

//            string? connectionString = Env.GetString("CONNECTIONSTRINGS__WEBAPIDATABASE");
//            builder.Services.AddDbContext<AppDbContext>(options =>
//                options.UseNpgsql(connectionString));

//            var userName = Env.GetString("ADMIN__USERNAME");
//            var password = Env.GetString("ADMIN__PASSWORD");
//            var secretKey = Env.GetString("JWTSETTINGS__SECRETKEY");
//            var issuer = Env.GetString("JWTSETTINGS__ISSUER");
//            var audience = Env.GetString("JWTSETTINGS__AUDIENCE");
//            var tokenLifetime = Env.GetInt("JWTSETTINGS__TOKENLIFETIME");

//            var endpoint = Env.GetString("MINIO_ENDPOINT");
//            var accessKey = Env.GetString("MINIO_ACCESS_KEY");
//            var secretKeyMinio = Env.GetString("MINIO_SECRET_KEY");
//            var secure = bool.Parse(Env.GetString("MINIO_SECURE") ?? "true");

//            var env_port = Env.GetString("ENVIRONMENT__PORT");


//            builder.Services.AddMinio(configureClient => configureClient
//                                      .WithEndpoint(endpoint)
//                                      .WithCredentials(accessKey, secretKeyMinio)
//                                      .WithSSL(secure)
//                                      .Build());


//            const string CorsPolicy = "CorsPolicy";

//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy(CorsPolicy, policy =>
//                {
//                    policy
//                        .WithOrigins(
//                            "https://localhost:49315/"
//                        )
//                        .AllowAnyMethod()
//                        .AllowAnyHeader()
//                        .AllowCredentials();
//                });

//            });

//            var key = Encoding.UTF8.GetBytes(secretKey);

//            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.RequireHttpsMetadata = false;
//                    options.SaveToken = true;
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuerSigningKey = true,
//                        IssuerSigningKey = new SymmetricSecurityKey(key),
//                        ValidateIssuer = true,
//                        ValidIssuer = issuer,
//                        ValidateAudience = true,
//                        ValidAudience = audience,
//                        ValidateLifetime = true
//                    };
//                });

//            builder.Services.Configure<FormOptions>(options =>
//            {
//                options.MultipartBodyLengthLimit = 200_000_000; // 200MB
//            });
//            // Repository va Service larni DI ga qo‘shish
//            builder.Services.AddScoped<IVideoRepository, VideoRepository>();
//            builder.Services.AddScoped<IImageRepository, ImageRepository>();
//            builder.Services.AddScoped<IBlockRootRepository, BlockRootRepository>();
//            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
//            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
//            builder.Services.AddScoped<IBlockRepository, BlockRepository>();
//            builder.Services.AddScoped<IHospitalDepartmentRepository, HospitalDepartmentRepository>();
//            builder.Services.AddScoped<INewsRepository, NewsRepository>();
//            builder.Services.AddScoped<IPriceForServiceRepository, PriceForServiceRepository>();
//            builder.Services.AddScoped<IQueueRepository, QueueRepository>();

//            builder.Services.AddScoped<IVideoService, VideoService>();
//            builder.Services.AddScoped<IImageService, ImageService>();
//            builder.Services.AddScoped<BlockRootService>();
//            builder.Services.AddScoped<CustomerService>();
//            builder.Services.AddScoped<DoctorService>();
//            builder.Services.AddScoped<HospitalBlockService>();
//            builder.Services.AddScoped<HospitalDepartmentService>();
//            builder.Services.AddScoped<NewsService>();
//            builder.Services.AddScoped<PriceService>();
//            builder.Services.AddScoped<QueueService>();
//            builder.Services.AddScoped<JwtService>(JwtService =>
//            {
//                return new JwtService(userName, password, secretKey, issuer, audience, tokenLifetime);
//            });
//            //builder.Services.AddScoped<JwtService>();

//            builder.Services.AddControllers();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            var app = builder.Build();

//            // Static fayllarni xizmat qilish
//            app.UseWhen(context => context.Request.Path.StartsWithSegments("/VideoMP4/hls"), appBuilder =>
//            {
//                appBuilder.UseCors("CorsPolicy");

//                var provider = new FileExtensionContentTypeProvider();
//                provider.Mappings[".m3u8"] = "application/x-mpegURL";
//                provider.Mappings[".ts"] = "video/MP2T";

//                appBuilder.UseStaticFiles(new StaticFileOptions
//                {
//                    ContentTypeProvider = provider,
//                    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "VideoMP4/hls")),
//                    RequestPath = "/VideoMP4/hls"
//                });
//            });

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseRouting();

//            // Apply CORS policy before any other middleware that might handle the request
//            app.UseCors(CorsPolicy);

//            // Handle OPTIONS requests for CORS preflight
//            app.Use(async (context, next) =>
//            {
//                if (context.Request.Method == HttpMethods.Options)
//                {
//                    // Add CORS headers for preflight requests
//                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
//                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
//                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
//                    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

//                    context.Response.StatusCode = 200;
//                    await context.Response.CompleteAsync();
//                }
//                else
//                {
//                    await next();
//                }
//            });

//            app.UseHttpsRedirection();

//            app.UseAuthentication();
//            app.UseAuthorization();

//            app.MapControllers();

//            app.MapFallbackToFile("/index.html");

//            app.Run();
//        }
//    }
//}





using Dermatologiya.Server.Data;
using Dermatologiya.Server.RepositoriesAll.BlockRep;
using Dermatologiya.Server.RepositoriesAll.CustomerRep;
using Dermatologiya.Server.RepositoriesAll.DoctorRep;
using Dermatologiya.Server.RepositoriesAll.HospitalBlockRep;
using Dermatologiya.Server.RepositoriesAll.HospitalDepartmentRep;
using Dermatologiya.Server.RepositoriesAll.ImageRep;
using Dermatologiya.Server.RepositoriesAll.NewsRep;
using Dermatologiya.Server.RepositoriesAll.PriseForServicesRep;
using Dermatologiya.Server.RepositoriesAll.QueueRep;
using Dermatologiya.Server.RepositoriesAll.VideoRep;
using Dermatologiya.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Minio;
using System.Runtime;
using System.Text;
using DotNetEnv;

namespace Dermatologiya.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            string? connectionString = Env.GetString("CONNECTIONSTRINGS__WEBAPIDATABASE");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            var userName = Env.GetString("ADMIN__USERNAME");
            var password = Env.GetString("ADMIN__PASSWORD");
            var secretKey = Env.GetString("JWTSETTINGS__SECRETKEY");
            var issuer = Env.GetString("JWTSETTINGS__ISSUER");
            var audience = Env.GetString("JWTSETTINGS__AUDIENCE");
            var tokenLifetime = Env.GetInt("JWTSETTINGS__TOKENLIFETIME");

            var endpoint = Env.GetString("MINIO_ENDPOINT");
            var accessKey = Env.GetString("MINIO_ACCESS_KEY");
            var secretKeyMinio = Env.GetString("MINIO_SECRET_KEY");
            var secure = bool.Parse(Env.GetString("MINIO_SECURE") ?? "true");

            var env_port = Env.GetString("ENVIRONMENT__PORT");


            //builder.Services.AddMinio(configureClient => configureClient
            //                          .WithEndpoint(endpoint)
            //                          .WithCredentials(accessKey, secretKeyMinio)
            //                          .WithSSL(secure)
            //                          .Build());

            builder.Services.AddSingleton<MinioClient>(_ =>
                                   (MinioClient)new MinioClient()
                                    .WithEndpoint(endpoint)
                                    .WithCredentials(accessKey, secretKeyMinio)
                                    .WithSSL(secure)
                                   .Build());



            const string CorsPolicy = "CorsPolicy";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy
                        .WithOrigins(
                            "https://localhost:49315"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

            });

            var key = Encoding.UTF8.GetBytes(secretKey);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true
                    };
                });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 200_000_000; // 200MB
            });
            // Repository va Service larni DI ga qo‘shish
            builder.Services.AddScoped<IVideoRepository, VideoRepository>();
            builder.Services.AddScoped<IImageRepository, ImageRepository>();
            builder.Services.AddScoped<IBlockRootRepository, BlockRootRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IBlockRepository, BlockRepository>();
            builder.Services.AddScoped<IHospitalDepartmentRepository, HospitalDepartmentRepository>();
            builder.Services.AddScoped<INewsRepository, NewsRepository>();
            builder.Services.AddScoped<IPriceForServiceRepository, PriceForServiceRepository>();
            builder.Services.AddScoped<IQueueRepository, QueueRepository>();

            builder.Services.AddScoped<IVideoService, VideoService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<BlockRootService>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<DoctorService>();
            builder.Services.AddScoped<HospitalBlockService>();
            builder.Services.AddScoped<HospitalDepartmentService>();
            builder.Services.AddScoped<NewsService>();
            builder.Services.AddScoped<PriceService>();
            builder.Services.AddScoped<QueueService>();
            builder.Services.AddScoped<JwtService>(JwtService =>
            {
                return new JwtService(userName, password, secretKey, issuer, audience, tokenLifetime);
            });
            //builder.Services.AddScoped<JwtService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Static fayllarni xizmat qilish
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/VideoMP4/hls"), appBuilder =>
            {
                appBuilder.UseCors("CorsPolicy");

                var provider = new FileExtensionContentTypeProvider();
                provider.Mappings[".m3u8"] = "application/x-mpegURL";
                provider.Mappings[".ts"] = "video/MP2T";

                appBuilder.UseStaticFiles(new StaticFileOptions
                {
                    ContentTypeProvider = provider,
                    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "VideoMP4/hls")),
                    RequestPath = "/VideoMP4/hls"
                });
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            // Apply CORS policy before any other middleware that might handle the request
            app.UseCors(CorsPolicy);

            // Handle OPTIONS requests for CORS preflight
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == HttpMethods.Options)
                {
                    // Add CORS headers for preflight requests
                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

                    context.Response.StatusCode = 200;
                    await context.Response.CompleteAsync();
                }
                else
                {
                    await next();
                }
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

