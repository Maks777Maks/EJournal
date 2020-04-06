﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EJournal.Data.EfContext;
using EJournal.Data.Entities;
using EJournal.Data.Entities.AppUeser;
using EJournal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EJournal.Controllers.AdminControllers
{
    [Authorize(Roles = "Director")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<DbUser> _userManager;
        private readonly EfDbContext _context;
        public AdminController(UserManager<DbUser> userManager, EfDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("adduser")]
        public async Task<ActionResult<string>> AddUser([FromBody] AddUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Введіть коректні дані");
            }
            try
            {
                DbUser user = new DbUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };
                BaseProfile prof = new BaseProfile
                {
                    Name = model.Name,
                    LastName = model.LastName,
                    Surname = model.Surname,
                    Adress = model.Adress,
                    DateOfBirth = Convert.ToDateTime(model.DateOfBirth)
                };

                switch (model.Rolename)
                {
                    case "Student":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "Student");
                        break;
                    case "Teacher":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "Teacher");
                        break;
                    case "Director":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "Director");
                        break;
                    case "Curator":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "Curator");
                        break;
                    case "Director deputy":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "DDeputy");
                        break;
                    case "Department head":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "DepartmentHead");
                        break;
                    case "Cycle commision head":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "CycleCommisionHead");
                        break;
                    case "Study room head":
                        await _userManager.CreateAsync(user, model.Password);
                        await _userManager.AddToRoleAsync(user, "StudyRoomHead");
                        break;
                    default:
                        return "Введіть правильну роль";
                }
                prof.Id = user.Id;
                await _context.BaseProfiles.AddAsync(prof);
                await _context.SaveChangesAsync();
                if (model.Rolename == "Student")
                {
                    await _context.StudentProfiles.AddAsync(new StudentProfile { Id = prof.Id });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await _context.TeacherProfiles.AddAsync(new TeacherProfile { Id = prof.Id, Degree = model.Degree });
                    await _context.SaveChangesAsync();
                }
                return Ok("Користувач успішно доданий");
            }
            catch (Exception ex)
            {
                return "Помилка: " + ex.Message;
            }
        }
        [HttpPost]
        [Route("get/students")]
        public IActionResult GetStudents([FromBody]StudentsFiltersModel model)
        {
            try
            {
                AdminStudentsTableModel table = new AdminStudentsTableModel();
                var query = _context.StudentProfiles.AsQueryable();
                List<AdminTableStudentRowModel> tableList = new List<AdminTableStudentRowModel>();
                if (model != null)
                {
                    //var groups = _context.Groups.Where(t => t.Speciality.Name == model.Speciality);
                    //var grToStud = _context.GroupsToStudents.Where(t => groups.Contains(t.Group));
                    if (model.SpecialityId != 0)
                    {
                        table.Groups = _context.Groups.Where(t => t.SpecialityId == model.SpecialityId).Select(t => new DropdownIntModel
                        {
                            Label = t.Name,
                            Value = t.Id
                        }).ToList();
                    }
                    if (model.GroupId != 0)
                    {
                        table.Groups = _context.Groups.Where(t => t.SpecialityId == model.SpecialityId).Select(t => new DropdownIntModel
                        {
                            Label = t.Name,
                            Value = t.Id
                        }).ToList();
                        var grToStud = _context.GroupsToStudents.Where(t => t.GroupId == model.GroupId);
                        query = _context.StudentProfiles.Where(t => grToStud.Any(g => g.StudentId == t.Id)).AsQueryable();
                    }
                }
                tableList = query.Select(t => new AdminTableStudentRowModel
                {
                    Name = t.BaseProfile.Name + " " + t.BaseProfile.LastName + " " + t.BaseProfile.Surname,
                    Address = t.BaseProfile.Adress,
                    DateOfBirth = t.BaseProfile.DateOfBirth.ToString("dd.MM.yyyy"),
                    Email = t.BaseProfile.DbUser.Email,
                    Phone = t.BaseProfile.DbUser.PhoneNumber
                }).ToList();
                List<AdminTableColumnModel> cols = new List<AdminTableColumnModel>
                {
                    new AdminTableColumnModel{label="Name",field="name",sort="asc",width=300},
                    new AdminTableColumnModel{label="Phone",field="phone",sort="asc",width=150},
                    new AdminTableColumnModel{label="Birthday",field="dateOfBirth",sort="asc",width=150},
                    new AdminTableColumnModel{label="Email",field="email",sort="asc",width=200},
                    new AdminTableColumnModel{label="Address",field="address",sort="asc",width=170}
                };
                List<DropdownIntModel> specs = _context.Specialities.Select(t => new DropdownIntModel
                {
                    Label = t.Name,
                    Value = t.Id
                }).ToList();

                table.rows = tableList;
                table.columns = cols;
                table.Specialities = specs;
                return Ok(table);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message + " I: " + ex.InnerException);
            }
        }
        [HttpPost]
        [Route("get/teachers")]
        public IActionResult GetTeachers([FromBody]TeacherFiltersModel model)
        {
            try
            {
                var query = _context.TeacherProfiles.AsQueryable();
                List<AdminTableTeacherRowModel> tableList = new List<AdminTableTeacherRowModel>();
                if (model != null)
                {
                    if (!String.IsNullOrEmpty(model.Rolename))
                    {
                        List<TeacherProfile> temp = new List<TeacherProfile>();
                        foreach (var item in _context.TeacherProfiles)
                        {
                            DbUser user = _context.Users.FirstOrDefault(t => t.Id == item.Id);
                            if (_userManager.GetRolesAsync(user).Result.Contains(model.Rolename))
                                temp.Add(item);
                        }
                        query = _context.TeacherProfiles.Where(t => temp.Contains(t)).AsQueryable();
                    }
                }
                tableList = query.Select(t => new AdminTableTeacherRowModel
                {//eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjdmYjZjYTczLWU0ZGYtNDQ0Zi1hOWI1LTI1ZWY0OWFjZTI3OSIsIm5hbWUiOiJkaXJlY3RvciIsInJvbGVzIjoiRGlyZWN0b3IiLCJleHAiOjE1ODUyNjE2MTV9.pmdGudiz0rqOldnAZqUpN8jd--d7n7HFBJJ3748Ec3M
                    Name = t.BaseProfile.Name + " " + t.BaseProfile.LastName + " " + t.BaseProfile.Surname,
                    Address = t.BaseProfile.Adress,
                    DateOfBirth = t.BaseProfile.DateOfBirth.ToString("dd.MM.yyyy"),
                    Email = t.BaseProfile.DbUser.Email,
                    Phone = t.BaseProfile.DbUser.PhoneNumber,
                    Degree = t.Degree
                }).ToList();
                List<AdminTableColumnModel> cols = new List<AdminTableColumnModel>
                {
                    new AdminTableColumnModel{label="Name",field="name",sort="asc",width=250},
                    new AdminTableColumnModel{label="Phone",field="phone",sort="asc",width=150},
                    new AdminTableColumnModel{label="Birthday",field="dateOfBirth",sort="asc",width=150},
                    new AdminTableColumnModel{label="Email",field="email",sort="asc",width=150},
                    new AdminTableColumnModel{label="Address",field="address",sort="asc",width=150},
                    new AdminTableColumnModel{label="Degree",field="degree",sort="asc",width=120}
                };
                List<DropdownStrModel> roles = _context.Roles.Where(t => t.Name != "Student").Select(t => new DropdownStrModel
                {
                    Label = t.Name,
                    Value = t.Name
                }).ToList();
                AdminTeachersTableModel table = new AdminTeachersTableModel();
                table.rows = tableList;
                table.columns = cols;
                table.Roles = roles;

                return Ok(table);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message + " asd: " + ex.StackTrace);
            }
        }
        [HttpPost]
        [Route("get/marks")]
        public IActionResult GetMarks([FromBody]GetMarksFiltersModel model)
        {
            if (model != null)
            {
                AdminMarksTableModel table = new AdminMarksTableModel();
                if (model.SpecialityId != 0)
                {
                    table.Groups = _context.Groups.Where(t => t.SpecialityId == model.SpecialityId).Select(t => new DropdownIntModel
                    {
                        Label = t.Name,
                        Value = t.Id
                    }).ToList();
                }
                if (model.GroupId != 0)
                {
                    table.Groups = _context.Groups.Where(t => t.SpecialityId == model.SpecialityId).Select(t => new DropdownIntModel
                    {
                        Label = t.Name,
                        Value = t.Id
                    }).ToList();
                    table.Subjects = _context.GroupToSubjects.Where(t => t.GroupId == model.GroupId).Select(t => new DropdownIntModel 
                    {
                        Label= t.Subject.Name,
                        Value=t.SubjectId
                    }).ToList();
                }
                if (model.SubjectId != 0)
                {
                    List<AdminTableMarksRowModel> tableList = new List<AdminTableMarksRowModel>();
                    int jourId = _context.Journals.FirstOrDefault(t => t.GroupId == model.GroupId).Id;
                    var jourCols = _context.JournalColumns.Where(t => t.JournalId == jourId && t.Lesson.SubjectId == model.SubjectId);
                    var lessonDates = jourCols.Select(t => t.Lesson.LessonDate).ToList();
                    lessonDates.OrderByDescending(d => d);

                    var students = _context.GroupsToStudents.Where(t => t.GroupId == model.GroupId).Select(t => t.Student);
                    foreach (var item in students)
                    {
                        var studMarks = _context.Marks.Where(t => jourCols.Contains(t.JournalColumn) && t.StudentId == item.Id);
                        var marksFormatted = new List<string>();
                        foreach (var date in lessonDates)
                        {
                            var cell = studMarks.FirstOrDefault(m => m.JournalColumn.Lesson.LessonDate == date);
                            if (cell != null)
                                marksFormatted.Add(cell.Value);
                            else 
                                marksFormatted.Add("-");
                        }
                        var baseP = _context.BaseProfiles.FirstOrDefault(t => t.Id == item.Id);
                        string name = baseP.Name + " " + baseP.LastName + " " + baseP.Surname;
                        AdminTableMarksRowModel rowModel = new AdminTableMarksRowModel
                        {
                            Name = name,
                            Marks=marksFormatted
                        };
                        tableList.Add(rowModel);
                    }
                    List<string> cols = new List<string>();
                    cols.Add("#");
                    cols.Add("ПІБ");
                    int lenght = lessonDates.Count;
                    //Set the count of cols
                    if (lenght > 7) lenght = 7;
                    for (int i = 0; i < lenght; i++)
                    {
                        cols.Add(lessonDates[i].ToString("dd.MM.yyyy"));
                    }

                    table.rows = tableList;
                    table.columns = cols;
                    table.Groups = _context.Groups.Where(t => t.SpecialityId == model.SpecialityId).Select(t => new DropdownIntModel
                    {
                        Label = t.Name,
                        Value = t.Id
                    }).ToList();
                }
                List<DropdownIntModel> specs = _context.Specialities.Select(t => new DropdownIntModel
                {
                    Label = t.Name,
                    Value = t.Id
                }).ToList();
                table.Specialities = specs;

                return Ok(table);
            }
            return BadRequest("Введені не всі дані");
        }
        //[HttpDelete("delete/{email}")]
        //public async Task<ContentResult> DeleteUserAsync(string email)
        //{
        //    try
        //    {
        //        DbUser user = _context.Users.FirstOrDefault(t => t.Email == email);
        //        if (_userManager.GetRolesAsync(user).Result.Contains("Student"))
        //        {
        //            _context.StudentProfiles.Remove(_context.StudentProfiles.FirstOrDefault(t => t.Id == user.Id));
        //            _context.SaveChanges();
        //        }
        //        else
        //        {
        //            _context.TeacherProfiles.Remove(_context.TeacherProfiles.FirstOrDefault(t => t.Id == user.Id));
        //            _context.SaveChanges();
        //        }
        //        _context.BaseProfiles.Remove(_context.BaseProfiles.FirstOrDefault(t => t.Id == user.Id));
        //        _context.SaveChanges();

        //        await _userManager.DeleteAsync(user);
        //        return Content("User is deleted");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("Error" + ex.Message);
        //    }
        //}
    }

}