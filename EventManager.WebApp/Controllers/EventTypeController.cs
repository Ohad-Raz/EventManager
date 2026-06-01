using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebApp.Controllers
{
    public class EventTypeController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventTypeController(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        // GET: EventType
        public IActionResult Index()
        {
            List<EventType> eventTypes = _eventRepository.GetAllEventTypes();
            List<EventTypeVM> model = _mapper.Map<List<EventTypeVM>>(eventTypes);
            return View(model);
        }

        // GET: EventType/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EventType? existingEventType = _eventRepository.GetEventTypeById(id.Value);
            if (existingEventType == null)
            {
                return NotFound();
            }

            EventTypeVM model = _mapper.Map<EventTypeVM>(existingEventType);
            return View(model);
        }

        // GET: EventType/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new EventTypeVM());
        }

        // POST: EventType/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string trimmedName = model.Name.Trim();
            if (_eventRepository.EventTypeNameExists(trimmedName))
            {
                ModelState.AddModelError(nameof(model.Name), $"Event type name '{trimmedName}' already exists.");
                return View(model);
            }

            EventType newEventType = _mapper.Map<EventType>(model);
            newEventType.Name = trimmedName;

            _eventRepository.AddEventType(newEventType);
            _eventRepository.SaveChanges();

            TempData["newEventTypeName"] = newEventType.Name;
            return RedirectToAction(nameof(Index));
        }

        // GET: EventType/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EventType? existingEventType = _eventRepository.GetEventTypeById(id.Value);
            if (existingEventType == null)
            {
                return NotFound();
            }

            EventTypeVM model = _mapper.Map<EventTypeVM>(existingEventType);
            return View(model);
        }

        // POST: EventType/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EventTypeVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            EventType? existingEventType = _eventRepository.GetEventTypeById(id);
            if (existingEventType == null)
            {
                return NotFound();
            }

            string trimmedName = model.Name.Trim();
            if (_eventRepository.EventTypeNameExists(trimmedName, id))
            {
                ModelState.AddModelError(nameof(model.Name), $"Event type name '{trimmedName}' already exists.");
                return View(model);
            }

            existingEventType.Name = trimmedName;
            _eventRepository.UpdateEventType(existingEventType);
            _eventRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: EventType/Delete/5
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EventType? existingEventType = _eventRepository.GetEventTypeById(id.Value);
            if (existingEventType == null)
            {
                return NotFound();
            }

            EventTypeVM model = _mapper.Map<EventTypeVM>(existingEventType);
            if (_eventRepository.EventTypeHasEvents(id.Value))
            {
                ViewBag.DeleteBlocked = true;
            }

            return View(model);
        }

        // POST: EventType/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            EventType? existingEventType = _eventRepository.GetEventTypeById(id);
            if (existingEventType == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (_eventRepository.EventTypeHasEvents(id))
            {
                ModelState.AddModelError(string.Empty, "Cannot delete this event type because one or more events use it.");
                EventTypeVM model = _mapper.Map<EventTypeVM>(existingEventType);
                ViewBag.DeleteBlocked = true;
                return View(model);
            }

            _eventRepository.RemoveEventType(existingEventType);
            _eventRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
