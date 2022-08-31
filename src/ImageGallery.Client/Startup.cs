using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace ImageGallery.Client
{ 
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // claim coming from the IDP is mapped to some claim in the webClient
            // we have a mapping Dictionary, that will be looked at wjen  mapping the claim types

            // if we run in run and debug mode and u comment and Uncommnet this line of code u
            // u will see the value of this line of code
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            // create an HttpClient used for accessing the API the image gallery api
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            // we need to add another Http client to talk with our IDP Api
            // this is not compulsory. we only do ut here because we want to manaully call 
            // our UserEndpoint TO get some userInfo


            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44318/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            // ADD the authentication Middleware
            services.AddAuthentication(options =>
            {
                // we need to set two scheme here 
                //this refers to a string values of Cookies, we can logout, signin and do some good stuff just by referring to cookies
                //the authenticationscheme is just a string costant that point to "Cookies" we can specify ohter name we wish to use
                // but since we are not doing some complex stuff and we dont have matching names we can just refer to the one given by asp.net
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //the authenticationscheme just point to a string constant named OpenIdConnects
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                // this addCookie configures our application to use cookie-based authentication
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                // configures some OIDC FLOw, this configuration specified allows our Application to use OIDC
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // if any parrt of our application requires authentication, OIDC WILL BE triggered as default
                    // from above we also set our default challenge scheme to the same value
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    // the authority is our IDP
                    // THE client uses the base url and add .well-known/openid-configuration to check for the metadata required for our client
                    options.Authority = "https://localhost:44318/";
                    // our clientID should match the Id specify at our IDP Level
                    options.ClientId = "imagegalleryclient";
                    // response type is grant so we use code
                    options.ResponseType ="code";
                    // pkce is requires
                    //  options.UsePkce = false;
                    options.UsePkce = true;
                    //the redirecturl we set at the level of the IDp
                    // options.CallbackPath = new PathString("....")
                    // scope we want to request, you dont need to add this scope cause by default there are requested
                    //options.Scope.Add("openid");
                    //options.Scope.Add("profile");
                    options.Scope.Add("address");
                    // this allows middleware to save tokens
                    options.SaveTokens = true;
                    // the secret we pass must match the secret specify at our IDP LEVel
                    options.ClientSecret = "secret";
                    // this endpoint is hit so that we can get certain claims from the user Endpoint
                    options.GetClaimsFromUserInfoEndpoint = true;

                    // we can also remove, delete somw claims
                  //  options.ClaimActions.Remove("nbf");
                    options.ClaimActions.DeleteClaims("sid");
                    options.ClaimActions.DeleteClaims("idp");
                    options.ClaimActions.DeleteClaims("s_hash");
                    options.ClaimActions.DeleteClaims("auth_time");
                    options.ClaimActions.DeleteClaims("address");
                    // we can also map a claim type to another claim type 



                });

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


// to allow Oidc we need to do two things here, first we need somwthing to take care of the Client side part of the OIDC flow
// and we need a way to store the users IdenTITY




// if  the Logout page is hit, we need the prevent the user from getting stuck in a IdP LOGoutpage
//we  need to provide a page the user will be redirected to the default whiich us .../signout-callback-oidc