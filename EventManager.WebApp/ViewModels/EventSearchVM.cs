using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class EventSearchVM
    {
        //exercise11 persisting search query
        public string? Submit { get; set; }

        [Display(Name = "Search")]
        public string? Q { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [Display(Name = "Sort by")]
        public string? OrderBy { get; set; }

        public int Page { get; set; } = 1;

        [Display(Name = "Page size")]
        public int Size { get; set; } = 5;

        public int LastPage { get; set; } = 1;

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }

        public IEnumerable<EventVM> Events { get; set; } = new List<EventVM>();

        public IEnumerable<SelectListItem> EventTypeItems { get; set; } = new List<SelectListItem>();
    }
}