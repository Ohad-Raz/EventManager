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
using System.Security.Claims;

namespace EventManager.WebApp.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventTypeRepository _eventTypeRepository;
        private readonly IEventPerformerRepository _eventPerformerRepository;
        private readonly IPerformerRepository _performerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IMapper _mapper;

        public EventController(
            IEventRepository eventRepository,
            IEventTypeRepository eventTypeRepository,
            IEventPerformerRepository eventPerformerRepository,
            IPerformerRepository performerRepository,
            IUserRepository userRepository,
            IRegistrationRepository registrationRepository,
            IMapper mapper)
        {
            _eventRepository = eventRepository;
            _eventTypeRepository = eventTypeRepository;
            _eventPerformerRepository = eventPerformerRepository;
            _performerRepository = performerRepository;
            _userRepository = userRepository;
            _registrationRepository = registrationRepository;
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

            // 4. for logged-in users, show whether they are already registered
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("User"))
            {
                string? username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    User? currentUser = _registrationRepository.GetUserByUsername(username);
                    if (currentUser != null)
                    {
                        ViewBag.IsAlreadyRegistered = _registrationRepository.UserIsActivelyRegistered(
                            currentUser.Id, id.Value);
                    }
                }
            }

            // 5. for admins, load performers available to assign
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                List<Performer> unassignedPerformers = _eventPerformerRepository.GetUnassignedPerformersForEvent(id.Value);
                ViewBag.AllPerformersAssigned = unassignedPerformers.Count == 0;
                ViewBag.AvailablePerformerItems = unassignedPerformers
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                    .ToList();
            }

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

            // 2. set CreatedById from logged-in Admin id in MVC cookie
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int createdById))
            {
                ModelState.AddModelError(string.Empty, "Cannot create event because the login session is missing the user id. Please log out and log in again.");
                LoadEventDropdowns(selectedEventTypeId: model.EventTypeId, selectedImageId: model.ImageId);
                return View(model);
            }

            // 3. map form model to Event entity
            Event newEvent = _mapper.Map<Event>(model);
            newEvent.CreatedById = createdById;

            // 4. save new event
            _eventRepository.AddEvent(newEvent);
            _eventRepository.SaveChanges();

            TempData["newEventName"] = newEvent.Name;
            return RedirectToAction(nameof(Search));
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
                PrepareSearchViewModel(searchVm);
                return View(searchVm);
            }
            catch
            {
                throw;
            }
        }
        public ActionResult SearchPartial(EventSearchVM searchVm)
        {
            try
            {
                PrepareSearchViewModel(searchVm);
                return PartialView("_SearchPartial", searchVm);
            }
            catch
            {
                throw;
            }
        }
        private void PrepareSearchViewModel(EventSearchVM searchVm)
        {
            IQueryable<Event> events = _eventRepository
                .GetAllEventsWithDetails()
                .AsQueryable();

            if (string.IsNullOrEmpty(searchVm.Q) && string.IsNullOrEmpty(searchVm.Submit))
            {
                searchVm.Q = Request.Cookies["eventQuery"];
            }

            var option = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(15)
            };

            Response.Cookies.Append("eventQuery", searchVm.Q ?? "", option);

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

            if (!string.IsNullOrEmpty(searchVm.Submit))
            {
                searchVm.Page = 1;
            }

            if (searchVm.Size < 1)
            {
                searchVm.Size = 10;
            }

            int totalCount = events.Count();
            int lastPage = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)searchVm.Size);

            if (searchVm.Page < 1)
            {
                searchVm.Page = 1;
            }

            if (searchVm.Page > lastPage)
            {
                searchVm.Page = lastPage;
            }

            searchVm.LastPage = lastPage;
            searchVm.HasPreviousPage = searchVm.Page > 1;
            searchVm.HasNextPage = searchVm.Page < lastPage;

            searchVm.Events = events
                .Skip((searchVm.Page - 1) * searchVm.Size)
                .Take(searchVm.Size)
                .Select(x => _mapper.Map<EventVM>(x))
                .ToList();

            searchVm.EventTypeItems = GetEventTypeListItems();
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
                eventTypeListItems = _eventTypeRepository.GetAllEventTypes()
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

            return RedirectToAction(nameof(Search));
        }

        // POST: Event/Register/5
        [Authorize(Roles = "User")] // Only users can register for events.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(int id)
        {
            // Event id comes from the route; must exist and not be soft-deleted.
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent == null)
            {
                TempData["RegistrationError"] = "This event is not available for registration.";
                return RedirectToAction(nameof(Index));
            }

            // User id is resolved on the server from the signed-in cookie, never from the form.
            string? username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action(nameof(Details), new { id }) });
            }

            User? currentUser = _registrationRepository.GetUserByUsername(username);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action(nameof(Details), new { id }) });
            }

            Registration? existingRegistration =
                _registrationRepository.GetRegistrationByUserAndEvent(currentUser.Id, id);

            // Prevent duplicate active registration.
            if (existingRegistration != null && existingRegistration.IsActive)
            {
                TempData["RegistrationDuplicate"] = "You are already registered for this event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Reactivate a previous registration instead of creating a duplicate row.
            if (existingRegistration != null && !existingRegistration.IsActive)
            {
                existingRegistration.IsActive = true;
                existingRegistration.Name = $"Registration for {existingEvent.Name}";
                _registrationRepository.SaveChanges();

                TempData["RegistrationSuccess"] = "You have successfully registered for this event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            Registration newRegistration = new Registration
            {
                Name = $"Registration for {existingEvent.Name}",
                UserId = currentUser.Id,
                EventId = id,
                RegisteredAt = DateTime.Now,
                IsActive = true
            };

            _registrationRepository.AddRegistration(newRegistration);
            _registrationRepository.SaveChanges();

            TempData["RegistrationSuccess"] = "You have successfully registered for this event.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Event/CancelRegistration/5
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelRegistration(int id)
        {
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent == null)
            {
                TempData["RegistrationError"] = "This event is not available.";
                return RedirectToAction(nameof(Index));
            }

            string? username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action(nameof(Details), new { id }) });
            }

            User? currentUser = _registrationRepository.GetUserByUsername(username);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action(nameof(Details), new { id }) });
            }

            Registration? activeRegistration =
                _registrationRepository.GetActiveRegistrationByUserAndEvent(currentUser.Id, id);
            if (activeRegistration == null)
            {
                TempData["RegistrationError"] = "You are not registered for this event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            activeRegistration.IsActive = false;
            _registrationRepository.SaveChanges();

            TempData["RegistrationSuccess"] = "Your registration for this event has been cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Event/AssignPerformer/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignPerformer(int id, int performerId)
        {
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent == null)
            {
                TempData["PerformerError"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            Performer? performer = _performerRepository.GetPerformerById(performerId);
            if (performer == null)
            {
                TempData["PerformerError"] = "Performer not found.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (_eventPerformerRepository.EventPerformerRelationExists(id, performerId))
            {
                TempData["PerformerError"] = "This performer is already assigned to the event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            EventPerformer eventPerformer = new EventPerformer
            {
                EventId = id,
                PerformerId = performerId
            };

            _eventPerformerRepository.AddEventPerformer(eventPerformer);
            _eventPerformerRepository.SaveChanges();

            TempData["PerformerSuccess"] = $"{performer.Name} was assigned to the event.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Event/RemovePerformer/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePerformer(int id, int performerId)
        {
            Event? existingEvent = _eventRepository.GetEventById(id);
            if (existingEvent == null)
            {
                TempData["PerformerError"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            Performer? performer = _performerRepository.GetPerformerById(performerId);
            if (performer == null)
            {
                TempData["PerformerError"] = "Performer not found.";
                return RedirectToAction(nameof(Details), new { id });
            }

            EventPerformer? existingRelation = _eventPerformerRepository.GetEventPerformerRelation(id, performerId);
            if (existingRelation == null)
            {
                TempData["PerformerError"] = "This performer is not assigned to the event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _eventPerformerRepository.RemoveEventPerformer(existingRelation);
            _eventPerformerRepository.SaveChanges();

            TempData["PerformerSuccess"] = $"{performer.Name} was removed from the event.";
            return RedirectToAction(nameof(Details), new { id });
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
            return RedirectToAction(nameof(Search));
        }

        // Returns true when event exists and is not soft-deleted.
        private bool EventExists(int id)
        {
            return _eventRepository.EventExists(id);
        }

        // Loads dropdown data needed by Create/Edit event forms.
        private void LoadEventDropdowns(int? selectedCreatedById = null, int? selectedEventTypeId = null, int? selectedImageId = null)
        {
            // 1. load dropdown source data from repository
            List<User> users = _userRepository.GetAllUsers();
            List<EventType> eventTypes = _eventTypeRepository.GetAllEventTypes();
            List<Image> images = _eventRepository.GetAllImages();

            // 2. set SelectList values for scaffolded form dropdowns
            ViewData["CreatedById"] = new SelectList(users, "Id", "Email", selectedCreatedById);
            ViewData["EventTypeId"] = new SelectList(eventTypes, "Id", "Name", selectedEventTypeId);
            ViewData["ImageId"] = new SelectList(images, "Id", "FileName", selectedImageId);
        }
    }
}
