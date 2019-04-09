using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class InstructorDetailViewModel
    {
        public int Id { get; set; }
        
        public string FirstName { get; set; }
       
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public string Slack { get; set; }

        public int CohortId { get; set; }
        public Cohort Cohort { get; set; } = new Cohort();
    }
}
