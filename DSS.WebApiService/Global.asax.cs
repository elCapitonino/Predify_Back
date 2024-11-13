using DSS.Model.Contexts;
using DSS.Model.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using Serilog;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

namespace DSS.WebApiService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // 
            AreaRegistration.RegisterAllAreas();

#if !DEBUG
            var configuration = new DSS.Model.Migrations.Configuration();
            //configuration.CommandTimeout = int.MaxValue;
            var migrator = new DbMigrator(configuration);
            migrator.Update();
#endif

            Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("C:\\Logs\\pre-prod-api\\Log-.log",
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

            Serilog.Log.Information("System is starting...");

            // Mongo config
            BsonSerializer.RegisterSerializer(
                typeof(decimal),
                new DecimalSerializer(MongoDB.Bson.BsonType.Double,
                    new RepresentationConverter(
                        true, // allow overflow, return decimal.MinValue or decimal.MaxValue instead
                        true //allow truncation
                    ))
                );

        }
    }
}