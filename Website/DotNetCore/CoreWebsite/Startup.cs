﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using CoreWebsite.EntityFramework;
using AutoMapper;
using CoreWebsite.EntityFramework.Models.EntityRelationTest;
using CoreWebsite.EntityFramework.Dtos.EntityRelationTest;
using CoreWebsite.EntityFramework.Dtos.TreeTest;
using CoreWebsite.EntityFramework.Models.TreeTest;
using AspNetCore.ResponseCaching.Extensions;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using CoreWebsite.Castle.Windsor.Demo.Interfaces;
using CoreWebsite.Castle.Windsor.Demo.Classes;

namespace CoreWebsite
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //数据库配置
            //参考资料：https://docs.microsoft.com/en-us/ef/core/get-started/aspnetcore/new-db
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<WebsiteDbContext>(
                //启动延迟加载
                options => options.UseLazyLoadingProxies()
                .UseSqlServer(_configuration.GetConnectionString("SqlServer"),
                    //https://docs.microsoft.com/zh-cn/ef/core/modeling/spatial
                    //映射到空间数据的数据库中的类型在模型中使用 NTS 类型
                    x => x.UseNetTopologySuite()));
            services.AddDiskBackedMemoryResponseCaching((x, y) =>
            {
                x.MaximumBodySize = 5 * 1024 * 1024; // Default of 64MB is probably way too big for most scenarios
            });

            services.AddMvc();

            //spa参考资料：
            //http://www.talkingdotnet.com/implement-asp-net-core-spa-template-feature-in-angular6-app/
            //https://docs.microsoft.com/zh-cn/aspnet/core/client-side/spa/angular?view=aspnetcore-2.1&tabs=visual-studio
            services.AddSpaStaticFiles(c =>
            {
                c.RootPath = "CoreNgAlain/dist";
            });
            SetAutoMapper();
            //依赖注入:https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
            services.AddScoped<IMyDependency, MyDependency>();

            //允许跨域
            //services.AddCors();
        }

        public void SetAutoMapper()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Student, StudentDto>();
                cfg.CreateMap<AdmissionRecord, AdmissionRecordDto>();
                cfg.CreateMap<Class, ClassDto>();
                cfg.CreateMap<StudentTeacherRelationship, StudentTeacherRelationshipDto>();
                cfg.CreateMap<Teacher, TeacherDto>();
                cfg.CreateMap<TreeNode, TreeNodeDto>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), @"Views")),
                        RequestPath = new PathString("/Views"),
                        ContentTypeProvider = new FileExtensionContentTypeProvider(
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                { ".js", "application/javascript" },
                                { ".css", "text/css" },
                                { ".html", "text/html" },
                            })
                }
             );
            app.UseSpaStaticFiles();
            app.UseCustomResponseCaching();

            //允许跨域
            //app.UseCors(builder => builder
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //使用Angular SPA from ASP.NET Core
            //但不建议这样操作，可以直接建立Angular站点，将Angular站点与ASP.NET Core站点独立开来，速度更快
            if (env.IsDevelopment())
            {
                app.UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501
                    //这里是angular项目的根目录
                    spa.Options.SourcePath = $"{_hostingEnvironment.ContentRootPath}\\..\\CoreNgAlain";
                    spa.UseAngularCliServer(npmScript: "start");
                });
            }
                
        }
    }
}
