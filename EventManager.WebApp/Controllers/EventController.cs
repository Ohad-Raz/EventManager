using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Create()
        {
            // 1. load dropdown values needed for create form
            LoadEventDropdowns();

            // 2. return empty create model
            return View(new EventCreateVM());
        }

        // POST: Event/Create
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

            return RedirectToAction(nameof(Index));
        }

        // GET: Event/Edit/5
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
