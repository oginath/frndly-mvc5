using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class SearchViewModel
    {
        public List<ApplicationUser> searchResult { get; set; }

        public List<string> imageDataURLs { get; set; }

    }
}