﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAcademy.CoreWebApi.DataTransferObject
{
    public class LikeModel
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public string PostUserId { get; set; }  
    }
}
