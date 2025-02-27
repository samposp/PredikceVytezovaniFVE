using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel : PageModel
    {
        private readonly FVEDbContext _db;

        public List<int> labels = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public List<int> values = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public FVEModel(FVEDbContext database)
        {
            _db = database;
        }
        public void OnGet()
        {

        }
    }
}
