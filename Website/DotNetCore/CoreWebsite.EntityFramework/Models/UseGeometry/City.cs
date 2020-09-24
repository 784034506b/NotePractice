﻿using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CoreWebsite.EntityFramework.Models.UseGeometry
{
    [Table("Cities")]
    public class City
    {
        [Key]
        public int CityID { get; set; }

        public string CityName { get; set; }

        public IPoint Location { get; set; }
    }
}
