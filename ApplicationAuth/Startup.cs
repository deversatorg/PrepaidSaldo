using Amazon.Util.Internal;
using ApplicationAuth.Common.Configs;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Utilities;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.DAL;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.DAL.Repository;
using ApplicationAuth.DAL.UnitOfWork;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.State;
using ApplicationAuth.Helpers;
using ApplicationAuth.Helpers.SwaggerFilters;
using ApplicationAuth.Models.RequestModels.Telegram;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Interfaces.Telegram;
using ApplicationAuth.Services.Services;
using ApplicationAuth.Services.Services.Telegram;
using ApplicationAuth.Services.StartApp;
using Braintree;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using OperationType = Microsoft.OpenApi.Models.OperationType;
using ApplicationAuth.Services.Interfaces.Driver;
using ApplicationAuth.Services.Services.Driver;

namespace ApplicationAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var botConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            var botToken = botConfig.BotToken ?? string.Empty;

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("Connection"));
                options.EnableSensitiveDataLogging(false);
            });

            services.AddCors();

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#=";
            }).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(o =>
            {
                o.Name = "Default";
                o.TokenLifespan = TimeSpan.FromHours(12);
            });

            #region Register services

            #region Basis services

            services.AddScoped<IDataContext>(provider => provider.GetService<DataContext>());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IHashUtility, HashUtility>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IState<>), typeof(State<>));
            services.AddScoped<IStateFactory, StateFactory>();

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<IJWTService, JWTService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ITelegramCoreService, TelegramCoreService>();
            services.AddScoped<ISaldoService, SaldoService>();

            services.AddHostedService<ConfigureWebhook>();
            services.AddSingleton<IHandleUpdateService, HandleUpdateService>();

            services.AddScoped<IWebDriverManager, WebDriverManager>();

            #endregion

            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfileConfiguration());
            });

            services.AddSingleton(config.CreateMapper());
            services.AddScoped<IWebDriver>(provider =>
            {
                var options = new ChromeOptions();
                options.AddArgument("headless");
                options.AddArgument("no-sandbox");
                options.AddArgument("disable-gpu");
                // ”казываем путь к драйверу
                options.AddArgument($"--webdriver={System.Environment.GetEnvironmentVariable("CHROME_DRIVER_PATH")}");
                IWebDriver driver = new ChromeDriver(options);
                return driver;
            });

            #endregion

            services
                .AddDetection()
                .AddCoreServices()
                .AddRequiredPlatformServices();

            services.AddMiniProfiler(opt =>
            {
                opt.RouteBasePath = "/profiler";
            })
            .AddEntityFramework();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddVersionedApiExplorer(
                 options =>
                 {
                     options.GroupNameFormat = "'v'VVV";

                     // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                     // can also be used to control the format of the API version in route templates
                     options.SubstituteApiVersionInUrl = true;
                 });
            
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
            });
            services.AddControllers().AddNewtonsoftJson();
            services.AddMvc(options =>
            {
                // Allow use optional parameters in actions
                options.AllowEmptyInputInBodyModelBinding = true;
                options.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true)
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            if (!_env.IsProduction())
            {
                services.AddSwaggerGen(options =>
                {
                    options.EnableAnnotations();

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        In = ParameterLocation.Header,
                        Description = "Access token",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });

                    options.OrderActionsBy(x => x.ActionDescriptor.DisplayName);

                    /*
                    // resolve the IApiVersionDescriptionProvider service
                    // note: that we have to build a temporary service provider here because one has not been created yet
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    // add a swagger document for each discovered API version
                    // note: you might choose to skip or document deprecated API versions differently
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                    }

                    // add a custom operation filter which sets default values

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                    */
                    options.IgnoreObsoleteActions();

                    options.OperationFilter<DefaultValues>();
                    options.OperationFilter<SecurityRequirementsOperationFilter>("Bearer");

                    // for deep linking
                    options.CustomOperationIds(e => $"{e.HttpMethod}_{e.RelativePath.Replace('/', '-').ToLower()}");
                });

                // instead of options.DescribeAllEnumsAsStrings()
                services.AddSwaggerGenNewtonsoftSupport();
            }

            services.AddHttpClient("TelegramWebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botToken, httpClient));


            var sp = services.BuildServiceProvider();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(/*JwtBearerDefaults.AuthenticationScheme, */options =>
                  {
                      options.RequireHttpsMetadata = true;
                      options.SaveToken = true;
                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuer = true,
                          ValidIssuer = AuthOptions.ISSUER,
                          ValidateAudience = true,
                          ValidateActor = false,
                          ValidAudience = AuthOptions.AUDIENCE,
                          ValidateLifetime = true,
                          //SignatureValidator = (string token, TokenValidationParameters validationParameters) => {

                          //    var jwt = new JwtSecurityToken(token);

                          //    var signKey = AuthOptions.GetSigningCredentials().Key as SymmetricSecurityKey;

                          //    var encodedData = jwt.EncodedHeader + "." + jwt.EncodedPayload;
                          //    var compiledSignature = Encode(encodedData, signKey.Key);

                          //    //Validate the incoming jwt signature against the header and payload of the token
                          //    if (compiledSignature != jwt.RawSignature)
                          //    {
                          //        throw new Exception("Token signature validation failed.");
                          //    }

                          //    /// TO DO: initialize user claims

                          //    return jwt;
                          //},
                          LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                          {
                              var jwt = securityToken as JwtSecurityToken;

                              if (!notBefore.HasValue || !expires.HasValue || DateTime.Compare(expires.Value, DateTime.UtcNow) <= 0)
                              {
                                  return false;
                              }

                              if (jwt == null)
                                  return false;

                              var isRefresStr = jwt.Claims.FirstOrDefault(t => t.Type == "isRefresh")?.Value;

                              if (isRefresStr == null)
                                  return false;

                              var isRefresh = Convert.ToBoolean(isRefresStr);

                              if (!isRefresh)
                              {
                                  try
                                  {
                                      using (var scope = serviceScopeFactory.CreateScope())
                                      {
                                          var hash = scope.ServiceProvider.GetService<IHashUtility>().GetHash(jwt.RawData);
                                          return scope.ServiceProvider.GetService<IRepository<UserToken>>().Find(t => t.AccessTokenHash == hash && t.IsActive) != null;
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      var logger = sp.GetService<ILogger<Startup>>();
                                      logger.LogError(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + ": Exception occured in token validator. Exception message: " + ex.Message + ". InnerException: " + ex.InnerException?.Message);
                                      return false;
                                  }
                              }

                              return false;
                          },
                          IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                          ValidateIssuerSigningKey = true
                      };
                  });

            services.AddRouting();
            services.AddMemoryCache();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider provider)
        {
            var botConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            var botToken = botConfig.BotToken ?? string.Empty;

            app.UseDefaultFiles();

            var cultures = Configuration.GetSection("SupportedCultures").Get<string[]>();

            var supportedCultures = new List<CultureInfo>();

            foreach (var culture in cultures)
            {
                supportedCultures.Add(new CultureInfo(culture));
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            #region Cookie auth

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            });

            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies[".AspNetCore.Application.Id"];
                if (!string.IsNullOrEmpty(token))
                    context.Request.Headers.Add("Authorization", "Bearer " + token);

                await next();
            });

            #endregion

            app.UseMiniProfiler();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor,

                    // IIS is also tagging a X-Forwarded-For header on, so we need to increase this limit, 
                    // otherwise the X-Forwarded-For we are passing along from the browser will be ignored
                    ForwardLimit = 2
                });
            }

            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5)
            };

            if (!_env.IsProduction())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(options =>
                {
                    options.PreSerializeFilters.Add((swagger, httpReq) =>
                    {
                        //swagger.Host = httpReq.Host.Value;

                        var ampersand = "&amp;";

                        foreach (var path in swagger.Paths)
                        {
                            if (path.Value.Operations.Any(x => x.Key == OperationType.Get && x.Value.Deprecated))
                                path.Value.Operations.First(x => x.Key == OperationType.Get).Value.Description = path.Value.Operations.First(x => x.Key == OperationType.Get).Value.Description.Replace(ampersand, "&");

                            if (path.Value.Operations.Any(x => x.Key == OperationType.Delete && x.Value?.Description != null))
                                path.Value.Operations.First(x => x.Key == OperationType.Delete).Value.Description = path.Value.Operations.First(x => x.Key == OperationType.Delete).Value.Description.Replace(ampersand, "&");
                        }

                        var paths = swagger.Paths.ToDictionary(p => p.Key, p => p.Value);
                        foreach (KeyValuePair<string, OpenApiPathItem> path in paths)
                        {
                            swagger.Paths.Remove(path.Key);
                            swagger.Paths.Add(path.Key.ToLowerInvariant(), path.Value);
                        }
                    });
                });

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(options =>
                {
                    options.IndexStream = () => System.IO.File.OpenRead("Views/Swagger/swagger-ui.html");
                    options.InjectStylesheet("/Swagger/swagger-ui.style.css");

                    /*
                    foreach (var description in provider.ApiVersionDescriptions)
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());

                    options.EnableFilter();

                    // for deep linking
                    options.EnableDeepLinking();
                    options.DisplayOperationId();
                    */
                });

                /*
                app.UseReDoc(c =>
                {
                    c.RoutePrefix = "docs";
                    c.SpecUrl("/swagger/v1/swagger.json");
                    c.ExpandResponses("200");
                    c.RequiredPropsFirst();
                });
                */
            }
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().AllowAnyOrigin();
                if (_env.IsDevelopment())
                    builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();

                if (_env.IsStaging())
                    builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();

                if (_env.IsProduction())
                    builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
            app.UseStaticFiles();
            app.UseRouting();

            #region Error handler

            // Different middleware for api and ui requests  
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
            {
                var localizer = serviceProvider.GetService<IStringLocalizer<ErrorsResource>>();
                var logger = loggerFactory.CreateLogger("GlobalErrorHandling");

                // Exception handler - show exception data in api response
                appBuilder.UseExceptionHandler(new ExceptionHandlerOptions
                {
                    ExceptionHandler = async context =>
                    {
                        var errorModel = new ErrorResponseModel(localizer);
                        var result = new ContentResult();

                        var exception = context.Features.Get<IExceptionHandlerPathFeature>();

                        if (exception.Error is CustomException)
                        {
                            var ex = (CustomException)exception.Error;

                            result = errorModel.Error(ex);
                        }
                        else
                        {
                            var message = exception.Error.InnerException?.Message ?? exception.Error.Message;
                            logger.LogError($"{exception.Path} - {message}");

                            errorModel.AddError("general", message);
                            result = errorModel.InternalServerError(_env.IsDevelopment() ? exception.Error.StackTrace : null);
                        }

                        context.Response.StatusCode = result.StatusCode.Value;
                        context.Response.ContentType = result.ContentType;

                        await context.Response.WriteAsync(result.Content);
                    }
                });

                // Handles responses with status codes (correctly executed requests, without any exceptions)
                appBuilder.UseStatusCodePages(async context =>
                    {
                        var errorResponse = ErrorHelper.GetError(localizer, context.HttpContext.Response.StatusCode);

                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                    });
            });

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
            {
                appBuilder.UseExceptionHandler("/Error");
                appBuilder.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
            });

            #endregion

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }

        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = $"ApplicationAuth API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "The ApplicationAuth application with Swagger and API versioning."
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private string Encode(string input, byte[] key)
        {
            HMACSHA256 myhmacsha = new HMACSHA256(key);
            byte[] byteArray = Encoding.UTF8.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            byte[] hashValue = myhmacsha.ComputeHash(stream);
            return Base64UrlEncoder.Encode(hashValue);
        }
    }

}
