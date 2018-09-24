﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademy.CoreWebApi.DataAccessLayer.Entities
{
    public class TeacherGroup
    {
        public int ID { get; set; }
        public DateTime DateAdded { get; set; }

        public TeacherGroup()
        {
            DateAdded = DateTime.Now;
        }

        public string AppIdentityUserId { get; set; }
        public Teacher Teacher { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

    }
}