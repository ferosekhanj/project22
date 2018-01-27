﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project22.ViewModel
{
    public class SignupInfo
    {
        public string Name { get; set; }

        public string MobileNumber { get; set; }

        [DataType(DataType.Password)]
        public int Pin { get; set; }
    }
}
