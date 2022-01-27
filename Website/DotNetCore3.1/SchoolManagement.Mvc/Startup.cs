using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Core.Repositories;
using SchoolManagement.EntityFrameworkCore;
using SchoolManagement.EntityFrameworkCore.DataRepositories;

namespace SchoolManagement.Mvc
{
    public class Startup
    {
        private IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ʹ��SQLServer���ݿ⣬ͨ��IConfiguration����ȥ��ȡ���ݿ�����
            //AddDbContextPool()�����ṩ�����ݿ����ӳأ�DbContextPool��
            //AddDbContextPool()��������������AddDbContext()����
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("SchoolManagementDBConnection")));

            //AddControllersWithViews+AddRazorPages
            //services.AddMvc();
            //services.AddMvcCore();
            services.AddControllersWithViews(option => option.EnableEndpointRouting = false)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                })
                //Э������
                //����ͷ����ΪAccept:application/xml
                .AddXmlSerializerFormatters();
            
            //��ͨ�ִ�
            services.AddScoped<IStudentRepository, MemoryStudentRepository>();
            //���Ͳִ�
            services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.Use(async (context, next) =>
            {
                logger.LogInformation($"��������ʱ��:{DateTime.Now}");
                await next();
                logger.LogInformation($"������Ӧʱ��:{DateTime.Now}");
            });

            if (env.IsDevelopment())
            {
                DeveloperExceptionPageOptions developerExceptionPageOptions = new
                    DeveloperExceptionPageOptions
                {
                    SourceCodeLineCount = 3
                };

                app.UseDeveloperExceptionPage(developerExceptionPageOptions);
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                //app.UseStatusCodePages();
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            //֧��wwwrootĿ¼�µ�Index.htm��Index.html��default.htm��default.html   
            //DefaultFilesMiddleware -> DefaultFilesOptions
            app.UseDefaultFiles();
            //������ʾ�̬�ļ�  eg./images/th.png
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //Ĭ��·��
            //��controller=Home��/��action=Index��/��id?��
            //app.UseMvcWithDefaultRoute();
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

            //�ս��·��
            //����·�ɹ���
            //UseEndpoints��һ�����Դ���粻ͬ�м��ϵͳ����MVC�� Razor Pages�� Blazor��SignalR��gRPC����·��ϵͳ��ͨ���ս��·�ɿ���ʹ�˵��໥Э������ʹϵͳ��û���໥�Ի����ն��м����ȫ�档
            //������UseRouting()�м��֮��
            //dotnet�����Ŷ��Ƽ�ʹ���ս��·��
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
