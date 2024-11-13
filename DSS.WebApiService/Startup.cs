using System;
using System.Net;
using System.Web.Http;
using DSS.Business.Configs;
using DSS.Model.Contexts;
using DSS.WebApiService.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(DSS.WebApiService.Startup))]
namespace DSS.WebApiService
{

    public class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public static GoogleOAuth2AuthenticationOptions GoogleAuthOptions { get; private set; }
        public static FacebookAuthenticationOptions FacebookAuthOptions { get; private set; }

       
        public void Configuration(IAppBuilder app)
        {

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(DssContext.Create);
            app.CreatePerOwinContext<AppUsuarioManager>(AppUsuarioManager.Create);
            app.CreatePerOwinContext<AppSignInManager>(AppSignInManager.Create);
            app.CreatePerOwinContext<AppPerfilManager>(AppPerfilManager.Create);

            HttpConfiguration config = new HttpConfiguration();

            ConfigureOAuth(app);

            WebApiConfig.Register(config);
            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);



        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            //use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);

            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {

                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(6),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            ////Configure Google External Login
            //GoogleAuthOptions = new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "897814971207-ffbckjbhjkjric6k5233gau0cd4i5n41.apps.googleusercontent.com",
            //    ClientSecret = "tDebxchE6BRyvZaQnjIUNPJ3",
            //    Provider = new GoogleAuthProvider()
            //};
            //GoogleAuthOptions.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
            //GoogleAuthOptions.Scope.Add("https://www.googleapis.com/auth/plus.me");
            //GoogleAuthOptions.Scope.Add("https://www.googleapis.com/auth/plus.login");
            //GoogleAuthOptions.Scope.Add("https://www.googleapis.com/auth/userinfo.email");

            //app.UseGoogleAuthentication(GoogleAuthOptions);
            
            ////Configure Facebook External Login
            //FacebookAuthOptions = new FacebookAuthenticationOptions()
            //{
            //    // MinDSS | Decision Support System
            //    AppId = "1652926861636711",
            //    AppSecret = "a4c49152f0030f2865eb6bd5ba79f635",
            //    Provider = new FacebookAuthProvider()
            //};

            //FacebookAuthOptions.Scope.Add("email");
            //FacebookAuthOptions.Scope.Add("public_profile");

            //app.UseFacebookAuthentication(FacebookAuthOptions);
        }
    }
}