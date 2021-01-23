using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models
{
    public class PagingInfo
    {
        public int TotalItem { get; set; }
        public int ItemPerPage { get; set; } 
        public int Currentpage { get; set; }
        public string urlParam { get; set; }
        public int TotalPage => (int)Math.Ceiling((decimal)TotalItem / ItemPerPage);
    }
}
