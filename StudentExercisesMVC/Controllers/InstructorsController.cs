using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly IConfiguration _configuration;

        public InstructorsController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Instructors
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id AS InstructorId,
                                               i.FirstName, i.LastName, 
                                               i.Slack, i.CohortId,
                                               c.Name AS CohortName
                                          FROM Instructor i LEFT JOIN Cohort c on i.cohortid = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    
                    while(reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Slack = reader.GetString(reader.GetOrdinal("Slack")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();
                    return View(instructors);
                }
            }
        }

        // GET: Instructors/Details/5
        public ActionResult Details(int id)
        {
            Instructor instructor = GetInstructorById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            InstructorDetailViewModel viewModel = new InstructorDetailViewModel
            {
                Id = instructor.Id,
                FirstName = instructor.FirstName,
                LastName = instructor.LastName,
                Slack = instructor.Slack,
                CohortId = instructor.CohortId,
                Cohort =
                {
                    Name = instructor.Cohort.Name,
                    Id = instructor.CohortId
                }
            };

            return View(viewModel);
        }

        // GET: Instructors/Create
        public ActionResult Create()
        {
            InstructorCreateViewModel viewModel = 
                new InstructorCreateViewModel(_configuration.GetConnectionString("DefaultConnection"));
            return View(viewModel);
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO instructor (firstname, lastname, slack, cohortid)
                                             VALUES (@firstname, @lastname, @slack, @cohortid)";
                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slack", viewModel.Instructor.Slack));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", viewModel.Instructor.CohortId));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Instructors/Edit/5
        public ActionResult Edit(int id)
        {
            Instructor instructor = GetInstructorById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            InstructorEditViewModel viewModel = new InstructorEditViewModel
            {
                Cohorts = GetAllCohorts(),
                Instructor = instructor
            };

            return View(viewModel);
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, InstructorEditViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE instructor 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slack = @slack, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slack", viewModel.Instructor.Slack));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", viewModel.Instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Instructors/Delete/5
        public ActionResult Delete(int id)
        {
            Instructor instructor = GetInstructorById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            InstructorDeleteViewModel viewModel = new InstructorDeleteViewModel
            {
                Id = instructor.Id,
                FirstName = instructor.FirstName,
                LastName = instructor.LastName,
                Slack = instructor.Slack,
                CohortId = instructor.CohortId,
                Cohort =
                {
                    Name = instructor.Cohort.Name,
                    Id = instructor.CohortId
                }
            };

            return View(viewModel);
        }

        // POST: Instructors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM instructor WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private Instructor GetInstructorById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id AS InstructorId,
                                               i.FirstName, i.LastName, 
                                               i.Slack, i.CohortId,
                                               c.Name AS CohortName
                                          FROM Instructor i LEFT JOIN Cohort c on i.cohortid = c.id
                                         WHERE  i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;
                    
                    if(reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Slack = reader.GetString(reader.GetOrdinal("Slack")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };
                    }

                    reader.Close();

                    return instructor;
                }
            }
 
        }

        private List<Cohort> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, name from Cohort;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name"))
                        });
                    }
                    reader.Close();

                    return cohorts;
                }
            }
 
        }
    }
}