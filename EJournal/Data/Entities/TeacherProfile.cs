﻿using EJournal.Data.Entities.AppUeser;
using System.Collections.Generic;

namespace EJournal.Data.Entities
{
    public class TeacherProfile
    {
        public string Id { get; set; }
        public string Degree { get; set; }

        public BaseProfile BaseProfile { get; set; }

        public ICollection<TeacherToSubject> TeacherToSubjects { get; set; }
        public ICollection<Group> Groups { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}