// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Solutions.CosmosDB.Security.ManagedIdentity;
using Microsoft.Solutions.CosmosDB.SQL.SDK.TODO.Service;
using Microsoft.Solutions.CosmosDB.SQL.SDK.TODO.Service.Models;
using System.Threading.Tasks;

namespace Microsoft.Solutions.CosmosDB.TODO.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddCosmosHelper(Configuration);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contoso TODO Web API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contoso TODO Web API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCosmosHelper(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IDataRepositoryProvider<ToDo>, TODOService>(x => { return new TODOService(ConnectionStringHelper.GetConnectionString(configuration).Result, "CosmosHandson", "ToDoSample"); });
            
            return services;
        }
    }

    static class ConnectionStringHelper
    {
        private static string _connectionString;
        
        public async static Task<string> GetConnectionString(IConfiguration Configuration)
        {
            if (!string.IsNullOrEmpty(ConnectionStringHelper._connectionString)) return ConnectionStringHelper._connectionString;
            
            var objConnectionStrings =  await ConnectionStringAccessor.Create(Configuration["App:SubscriptionId"], 
                                                                              Configuration["App:ResourceGroupName"], 
                                                                              Configuration["App:DatabaseAccountName"])
                                                                      .GetConnectionStringsAsync(Configuration["App:ManagedIdentityId"]);
            ConnectionStringHelper._connectionString = objConnectionStrings.PrimaryReadWriteKey;

            return ConnectionStringHelper._connectionString;
        }
    }

}
