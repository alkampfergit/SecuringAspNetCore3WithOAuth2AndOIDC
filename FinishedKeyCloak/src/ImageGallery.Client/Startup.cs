using IdentityModel;
using ImageGallery.Client.HttpHandlers;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddAuthorization(authorizationOptions =>
                {
                    authorizationOptions.AddPolicy(
                        "CanOrderFrame",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("country", "be");
                            policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");
                        });
                });

            //services.AddTransient<IClaimsTransformation, CustomClaimTransform>();

            services.Configure<KeyCloakConfiguration>(options => Configuration.GetSection("KeyCloak").Bind(options));
            var keyCloakConfig = new KeyCloakConfiguration();
            Configuration.Bind("KeyCloak", keyCloakConfig);

            services.AddHttpContextAccessor();

            services.AddTransient<BearerTokenHandler>();

            // create an HttpClient used for accessing the API
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://api.oauth2demo.local/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>();
            // create an HttpClient used for accessing the IDP
            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri(keyCloakConfig.Authority);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = keyCloakConfig.Authority;
                options.ClientId = keyCloakConfig.ClientId;
                options.ClientSecret = keyCloakConfig.ClientSecret;
                options.RequireHttpsMetadata = false;
                options.ResponseType = "code";
                options.Scope.Add("address");
                options.Scope.Add("roles");
                //options.Scope.Add("imagegalleryapi");
                //options.Scope.Add("subscriptionlevel");
                //options.Scope.Add("country");

                //Remember keycloak always return a refresh token, but only with offline_access we will have
                //a refresh token with longer timespan.
                //options.Scope.Add("offline_access");
                options.ClaimActions.DeleteClaim("sid");
                options.ClaimActions.DeleteClaim("idp");
                options.ClaimActions.DeleteClaim("s_hash");
                options.ClaimActions.DeleteClaim("auth_time");
                //options.ClaimActions.MapUniqueJsonKey("role", "role");
                //options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                //options.ClaimActions.MapUniqueJsonKey("country", "country");
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };

                options.Events.OnTokenValidated = OnTokenValidated;
            });
        }

        private Task OnTokenValidated(TokenValidatedContext ctx)
        {
            string sub = ctx.Principal.Claims.Single(c => c.Type == "sub").Value;

            //Get context usually to access database or service that contains your user information
            //var db = ctx.HttpContext.RequestServices.GetRequiredService<AuthorizationDbContext>();

            string internalUserId = "User_23";

            //Add every claims you need
            var claims = new List<Claim>
            {
                new Claim("internalId", internalUserId)
            };
            var appIdentity = new ClaimsIdentity(claims);

            ctx.Principal.AddIdentity(appIdentity);
            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
