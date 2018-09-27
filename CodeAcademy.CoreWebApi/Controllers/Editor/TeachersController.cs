﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeAcademy.CoreWebApi.BusinessLogicLayer.Abstract;
using CodeAcademy.CoreWebApi.DataAccessLayer.AppIdentity;
using CodeAcademy.CoreWebApi.DataAccessLayer.Entities;
using CodeAcademy.CoreWebApi.DataTransferObject;
using CodeAcademy.CoreWebApi.Helpers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CodeAcademy.CoreWebApi.Controllers.Editor
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private IAppRepository _context;
        private IAuthRepository _auth;

        public TeachersController(IAppRepository context, IAuthRepository auth)
        {
            _context = context;
            _auth = auth;
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IActionResult> GetAllTeachers()
        {
            List<AppIdentityUser> teachers = await _auth.GetUsersByRole("Teacher");
            return Ok(teachers);
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddTeacher([FromForm] TeacherModel model)
        {
            if (this.ValidRoleForAction(_context, _auth, new string[] { "Editor" }))
            {
                if (ModelState.IsValid)
                {
                    if (await _auth.FindUserByEmail(model.Email) == null)
                    {
                        Teacher teacher = new Teacher
                        {
                            BirthDate = model.BirthDate,
                            Email = model.Email,
                            FacultyId = model.FacultyId,
                            GenderId = (byte)model.GenderId,
                            Name = model.Name,
                            Surname = model.Surname,
                            PhoneNumber = model.PhoneNumber,
                            PhotoId = 1,
                            UserName = model.Email
                        };

                        await _auth.Register(teacher, model.Password);
                        bool saved = await _auth.AddUserToRole(teacher, "Teacher") != null;
                        if (saved == true)
                        {
                            var urlHelper = HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
                            await this.SendConfirmaitionMail(teacher, _auth, urlHelper);
                            return Ok(teacher);
                        }
                        return BadRequest("Error creating teacher");
                    }
                    return BadRequest($"Teacher with {model.Email} exists in database");
                }
                return BadRequest("Model is not valid");
            }
            return Forbid();
        }


    }
}