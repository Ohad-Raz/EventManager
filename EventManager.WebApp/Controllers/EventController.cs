using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebApp.Extensions;
using EventManager.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;

namespace EventManager.WebApp.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventController(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        // GET: Event
        public IActionResult Index()
        {
            // 1. load all events with related entities
            List<Event> events = _eventRepository.GetAllEventsWithDetails();

            // 2. map entities to display view models
            List<EventVM> model = _mapper.Map<List<EventVM>>(events);

            // 3. return list view
            return View(model);
        }

        // GET: Event/Details/5
        public IActionResult Details(int? id)
        {
            // 1. validate route id
            if (id == null)
            {
                return NotFound();
            }

            // 2. load event with related entities
            Event? existingEvent = _eventRepository.GetEventByIdWithDetails(id.Value);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // 3. map entity to display model and return view
            EventVM model = _mapper.Map<EventVM>(existingEvent);
            return View(model);
        }

        // GET: Event/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // 1. load dropdown values needed for create form
            LoadEventDropdowns();

            // 2. return empty create model
            return View(new EventCreateVM());
        }

        // POST: Event/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventCreateVM model)
        {
            // 1. validate form input
            if (!ModelState.IsValid)
            {
                LoadEventDropdowns(selectedEventTypeId: model.EventTypeId, selectedImageId: model.ImageId);
                return View(model);
            }

            // 2. resolve CreatedById using existing fallback
            int? fallbackCreatedById = GetFallbackCreatedById();
            if (!fallbackCreatedById.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Cannot create event because no users exist.");
                LoadEventDropdowns(selectedEventTypeId: model.EventTypeId, selectedImageId: model.ImageId);
                return View(model);
            }

            // 3. map form model to Event entity
            Event newEvent = _mapper.Map<Event>(model);
            newEvent.CreatedById = fallbackCreatedById.Value;

            // 4. save new event
            _eventRepository.AddEvent(newEvent);
            _eventRepository.SaveChanges();

            TempData["newEventName"] = newEvent.Name;
            return RedirectToAction(nameof(Index));
        }

        //exercise 11 example:

        public ActionResult GetEventsByCapacity([FromQuery] int? min, [FromQuery] int? max)
        {
            try
            {
                IQueryable<Event> events = _eventRepository.GetAllEventsWithDetails().AsQueryable();

                if (min.HasValue)
                {
                    events = events.Where(x => x.Capacity >= min.Value);
                }

                if (max.HasValue)
                {
                    events = events.Where(x => x.Capacity <= max.Value);
                }

                //var eventVms = events.Select(x => new EventVM
                //{
                //    Id = x.Id,
                //    Name = x.Name,
                //    Description = x.Description,
                //    StartTime = x.StartTime,
                //    EndTime = x.EndTime,
                //    Location = x.Location,
                //    Capacity = x.Capacity,
                //    EventTypeId = x.EventTypeId,
                //    EventTypeName = x.EventType.Name
                //}).ToList();
                List<EventVM> eventVms = events.Select(x => _mapper.Map<EventVM>(x)).ToList();


                return View("Index", eventVms);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //exercise 11 example:

        //public ActionResult Search([FromQuery] string? q, [FromQuery] string? sortPropery)
        //{
        //    try
        //    {
        //        IQueryable<Event> events = _eventRepository.GetAllEventsWithDetails().AsQueryable();

        //        if (!string.IsNullOrEmpty(SearchVm.Q))
        //        {
        //            events = events.Where(x =>
        //                x.Name.Contains(searchVm.Q) ||
        //                x.Description.Contains(searchVm.Q) ||
        //                x.Location.Contains(searchVm.Q));
        //        }

        //        if (max.HasValue)
        //        {
        //            events = events.Where(x => x.Capacity <= max.Value);
        //        }

        //        List<SearchVm> searchVms = events.Select(x => _mapper.Map<EventVM>(x)).ToList();


        //        return View("View", searchVms);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public ActionResult Search(EventSearchVM searchVm)
        {
            try
            {
                IQueryable<Event> events = _eventRepository
                    .GetAllEventsWithDetails()
                    .AsQueryable();
                //ex11
                if (string.IsNullOrEmpty(searchVm.Q) && string.IsNullOrEmpty(searchVm.Submit))
                {
                    searchVm.Q = Request.Cookies["eventQuery"];
                }
                var option = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(15)
                };

                Response.Cookies.Append("eventQuery", searchVm.Q ?? "", option);
                //end

                if (!string.IsNullOrEmpty(searchVm.Q))
                {
                    events = events.Where(x =>
                        x.Name.Contains(searchVm.Q) ||
                        x.Description.Contains(searchVm.Q) ||
                        x.Location.Contains(searchVm.Q));
                }

                if (searchVm.EventTypeId.HasValue)
                {
                    events = events.Where(x => x.EventTypeId == searchVm.EventTypeId.Value);
                }

                switch ((searchVm.OrderBy ?? "").ToLower())
                {
                    case "name":
                        events = events.OrderBy(x => x.Name);
                        break;
                    case "starttime":
                        events = events.OrderBy(x => x.StartTime);
                        break;
                    case "capacity":
                        events = events.OrderBy(x => x.Capacity);
                        break;
                    case "location":
                        events = events.OrderBy(x => x.Location);
                        break;
                    case "eventtype":
                        events = events.OrderBy(x => x.EventType.Name);
                        break;
                    default:
                        events = events.OrderBy(x => x.Id);
                        break;
                }

                searchVm.Events = events
                    .Skip((searchVm.Page - 1) * searchVm.Size)
                    .Take(searchVm.Size)
                    .Select(x => _mapper.Map<EventVM>(x))
                    .ToList();

                //FillEventTypeItems(searchVm);
                searchVm.EventTypeItems = GetEventTypeListItems();
                return View(searchVm);
            }
            catch
            {
                throw;
            }
        }
    // For each EventType from the database: use the Id as the dropdown value
    //use the Name as the displayed text
//        private void FillEventTypeItems(EventSearchVM searchVm)
//        {
//            searchVm.EventTypeItems = _eventRepository
//                .GetAllEventTypes()
//                .ConvertAll(x => new SelectListItem
//                {
//                    Value = x.Id.ToString(),
//                    Text = x.Name
//                })
//;
//        }
        private List<SelectListItem> GetEventTypeListItems()
        {
            var eventTypeListItemsJson = HttpContext.Session.GetString("EventTypeListItems");

            List<SelectListItem> eventTypeListItems;

            if (eventTypeListItemsJson == null)
            {
                eventTypeListItems = _eventRepository.GetAllEventTypes()
                    .ConvertAll(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
;

                HttpContext.Session.SetString("EventTypeListItems", eventTypeListItems.ToJson());
            }
            else
            {
                //If deserialization gives me null, at least return an empty list instead of crashing later
                eventTypeListItems = eventTypeListItemsJson.FromJson<List<SelectListItem>>()
                    ?? new List<SelectListItem>();
            }

            return eventTypeListItems;
        }
        //end of ex11.7 example!

        // GET: Event/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            // 1. validate route id
            if (id == null)
            {
                return NotFound();
            }

            // 2. load existing event
            Event? existingEvent = _eventRepository.GetEventById(id.Value);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // 3. map entity to edit model
            EventEditVM model = _mapper.Map<EventEditVM>(existingEvent);

            // 4. load dropdown values for edit form
            LoadEventDropdowns(existingEvent.CreatedById, model.EventTypeId, model.ImageId);
            return View(model);
        }

        // POST: Event/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EventEditVM model)
        {
            // 1. protect against id mismatch
            if (id != model.Id)
            {
                return NotFound();
            }

            // 2. validate form input
            if (!ModelState.IsValid)
            {
                LoadEventDropdowns(selectedEventTypeId: model.EventTypeId, selectedImageId: model.ImageId);
                return View(model);
            }

            // 3. load tracked entity for update
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            try
            {
                // 4. apply edited values and persist
                _mapper.Map(model, existingEvent);
                _eventRepository.UpdateEvent(existingEvent);
                _eventRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Event/Delete/5
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id)
        {
            // 1. validate route id
            if (id == null)
            {
                return NotFound();
            }

            // 2. load event with related entities
            Event? existingEvent = _eventRepository.GetEventByIdWithDetails(id.Value);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // 3. map entity to delete confirmation model
            EventVM model = _mapper.Map<EventVM>(existingEvent);
            return View(model);
        }

        // POST: Event/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // 1. load existing event
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent != null)
            {
                // 2. soft-delete event and save changes
                _eventRepository.RemoveEvent(existingEvent);
                _eventRepository.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // Returns true when event exists and is not soft-deleted.
        private bool EventExists(int id)
        {
            return _eventRepository.EventExists(id);
        }

        // Returns fallback CreatedById using first available user.
        private int? GetFallbackCreatedById()
        {
            List<User> users = _eventRepository.GetAllUsers();
            User? fallbackUser = users.FirstOrDefault();
            return fallbackUser?.Id;
        }

        // Loads dropdown data needed by Create/Edit event forms.
        private void LoadEventDropdowns(int? selectedCreatedById = null, int? selectedEventTypeId = null, int? selectedImageId = null)
        {
            // 1. load dropdown source data from repository
            List<User> users = _eventRepository.GetAllUsers();
            List<EventType> eventTypes = _eventRepository.GetAllEventTypes();
            List<Image> images = _eventRepository.GetAllImages();

            // 2. set SelectList values for scaffolded form dropdowns
            ViewData["CreatedById"] = new SelectList(users, "Id", "Email", selectedCreatedById);
            ViewData["EventTypeId"] = new SelectList(eventTypes, "Id", "Name", selectedEventTypeId);
            ViewData["ImageId"] = new SelectList(images, "Id", "FileName", selectedImageId);
        }
    }
}
