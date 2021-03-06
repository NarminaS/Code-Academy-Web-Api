﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeAcademy.CoreWebApi.BusinessLogicLayer.Abstract;
using CodeAcademy.CoreWebApi.DataAccessLayer.AppIdentity;
using CodeAcademy.CoreWebApi.DataAccessLayer.Entities;
using CodeAcademy.CoreWebApi.DataTransferObject;
using CodeAcademy.CoreWebApi.DataTransferObject.FromView;
using CodeAcademy.CoreWebApi.Helpers.Extensions;
using CodeAcademy.CoreWebApi.Helpers.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CodeAcademy.CoreWebApi.Controllers.Editor
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private Logger _logger;
        private IAppRepository _context;
        private IAuthRepository _auth;

        public StudentsController(IAppRepository context, IAuthRepository auth, Logger logger)
        {
            _logger = logger;
            _context = context;
            _auth = auth;
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                if (this.ValidRoleForAction(_context, _auth, new string[] { "Editor" }))
                {
                    List<AppIdentityUser> students = await _auth.GetUsersByRole("Student");
                    return Ok(students);
                }
                return Forbid();
            }
            catch (Exception ex)
            {
                var arguments = this.GetBaseData(_context, _auth);
                _logger.LogException(ex, arguments.Email, arguments.Path);
                return BadRequest($"{ex.GetType().Name} was thrown.");
            }
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddStudent([FromForm] StudentModel model)
        {
            try
            {
                if (this.ValidRoleForAction(_context, _auth, new string[] { "Editor" }))
                {
                    if (ModelState.IsValid)
                    {
                        if (await _auth.FindUserByEmail(model.Email) == null)
                        {
                            Student student = new Student
                            {
                                BirthDate = model.BirthDate,
                                Email = model.Email,
                                FacultyId = model.FacultyId,
                                GenderId = (byte)model.GenderId,
                                Group = await _context.GetByIdAsync<Group>(x => x.Id == model.GroupId),
                                LessonStatusId = 1,
                                Name = model.Name,
                                Surname = model.Surname,
                                PhoneNumber = model.PhoneNumber,
                                PhotoId = 1,
                                UserName = model.Email
                            };

                            await _auth.Register(student, model.Password);

                            bool saved = await _auth.AddUserToRole(student, "Student") != null;
                            if (saved == true)
                            {
                                var urlHelper = HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
                                await this.SendConfirmaitionMail(student, _auth, urlHelper, _logger);
                                return Ok(student);
                            }
                            return BadRequest("Error creating student");
                        }
                        return BadRequest($"Stident with {model.Email} exists in database");
                    }
                    return BadRequest("Model is not valid");
                }
                return Forbid();
            }
            catch (Exception ex)
            {
                var arguments = this.GetBaseData(_context, _auth);
                _logger.LogException(ex, arguments.Email, arguments.Path);
                return BadRequest($"{ex.GetType().Name} was thrown.");
            }
        }
    }
}