﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EJournal.ViewModels
{
    public class AdminStudentsTableModel
    {
        public List<AdminTableColumnModel> columns { get; set; }
        public List<AdminTableStudentRowModel> rows { get; set; }
        public List<DropdownIntModel> Specialities { get; set; }
        public List<DropdownIntModel> Groups { get; set; }

    }
}