﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreWebsite.EntityFramework;
using CoreWebsite.EntityFramework.Dtos.EntityRelationTest;
using CoreWebsite.EntityFramework.Models.EntityRelationTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CoreWebsite.Controllers
{
    public class EntityRelationTestController : Controller
    {
        private readonly WebsiteDbContext _dbContext;
        public EntityRelationTestController(WebsiteDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult GetStudentList()
        {
            //没有启动延迟加载，需要这样写
            //var students = _dbContext.Students
            //    .Include(x=>x.AdmissionRecord)
            //    .Include(x => x.Class)
            //    .Include(x=>x.StudentTeacherRelationships)
            //        //参考资料:https://docs.microsoft.com/zh-cn/ef/core/querying/related-data#eager-loading
            //        .ThenInclude(x=>x.Teacher)
            //    .ToList();
            ////需要map to dto，否则就循环了(eg.Student-StudentTeacherRelationship-Student)
            //List<StudentDto> dto= Mapper.Map<List<StudentDto>>(students);
            //return Json(dto);

            //启动延迟加载
            var students = _dbContext.Students.ToList();
            List<StudentDto> dto = Mapper.Map<List<StudentDto>>(students);
            return Json(dto);
        }
    }
}