using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;




namespace padvMVC.Models
{
    public class Exist_UsersDto
    {

        public long user_id { get; set; }
        public bool? user_admin { get; set; }
        public string validation { get; set; }

      
        [RequiredPerson]
        [StringLengthPerson(100)]
        public string user_name { get; set; }

        [RequiredPerson]
        [StringLengthPerson(20)]
        public  string user_password { get; set; }

        [RequiredPerson]
        [StringLengthPerson(20)]
        public string user_password_confirm { get; set; }

        [RequiredPerson]
        [StringLengthPerson(100)]
        public string user_company { get; set; }
        public CountriesDto countries { get; set; }
        public long user_country { get; set; }
       
        [RequiredPerson]
        [Validations(WhatVal = Validations.WhatValidation.Email)]
        public string user_email { get; set; }
        public Exist_LanguagesDto lang_id { get; set; }
        public long? table_id { get; set; }

    
        [StringLengthPerson(15)]
        public string user_phone { get; set; }
        public DateTime? user_date_expire { get; set; }
        public bool? user_blocked { get; set; }
        public string user_account { get; set; }
        public string user_head_representative { get; set; }
        public int _action { get; set; }
        public long measure_config_id { get; set; }
        public int? user_accesslevel { get; set; }
        public long? system_id { get; set; }
        public long en_system_id { get; set; }
        public DateTime? date_answer { get; set; }
        //public int action { get; set; }
        public string comments { get; set; }
        public DateTime? user_date_register { get; set; }
        public string last_name { get; set; }
        public string treatment { get; set; }
        public long base_calc { get; set; }
        public bool e_local { get; set; }
        public long specie_id { get; set; }
        public long currency_id { get; set; }
        public long dose { get; set; }
        public bool chbx { get; set; }
    }
    public class Exist_UsersDtoCollection : List<Exist_UsersDto>
    {

    }
}