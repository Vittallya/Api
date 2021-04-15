using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.SqlServer;
using Api.DAL;
using Microsoft.AspNetCore.Http;
// ниже для настройки swagger
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;//для использования в классе OpenApiInfo
using System.Text.Json.Serialization;
using Api.BLL;
using Api.Web.Logger;
using Microsoft.EntityFrameworkCore;

namespace Api.Web
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
           



            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DocContext>(options => {
                options.UseSqlServer(connection, b => b.MigrationsAssembly("Api.DAL"));
                });
            //options.UseInMemoryDatabase("DocsList"));


            services.AddScoped<IDocsService, DocsService>();
            //Transient(временный): при каждом обращении к сервису создается новый объект сервиса. 
            //Scoped(областной): для каждого запроса создается свой объект сервиса.То есть если в течение одного запроса есть несколько обращений к одному сервису, 
            //то при всех этих обращениях будет использоваться один и тот же объект сервиса.
            //Singleton: объект сервиса создается при первом обращении к нему, все последующие запросы используют один и тот же ранее созданный объект сервиса

            //конвертирует Enum в Json, благодаря этому на странице в cswagger отображается категория текстом, а не цифрами
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSingleton<ILogStorage, FileLoggerStorage>();//добавляем свой новый сервис файл для хранениия логов на базе ILogStorage

            //подключаем swagger
            //Действие по настройке, передаваемое в метод AddSwaggerGen, можно использовать для добавления таких сведений, как автор, лицензия и описание
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Api1",
                    Description = "It's my first API ",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Olga", 
                    },
                });
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
          

            // для обслуживания созданного документа JSON и пользовательского интерфейса Swagger.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //Чтобы предоставить пользовательский интерфейс Swagger в корневом 
                //каталоге приложения (http://localhost:<port>/)
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<LoggerMiddleware>(); //добавляем свой LogMiddleware

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
