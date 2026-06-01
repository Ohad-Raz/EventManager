using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PerformerController : Controller
    {
        private readonly IPerformerRepository _performerRepository;
        private readonly IMapper _mapper;

        public PerformerController(IPerformerRepository performerRepository, IMapper mapper)
        {
            _performerRepository = performerRepository;
            _mapper = mapper;
        }

        // GET: Performer
        public IActionResult Index()
        {
            List<Performer> performers = _performerRepository.GetAllPerformers();
            List<PerformerVM> model = _mapper.Map<List<PerformerVM>>(performers);
            return View(model);
        }

        // GET: Performer/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Performer? existingPerformer = _performerRepository.GetPerformerById(id.Value);
            if (existingPerformer == null)
            {
                return NotFound();
            }

            PerformerVM model = _mapper.Map<PerformerVM>(existingPerformer);
            return View(model);
        }

        // GET: Performer/Create
        public IActionResult Create()
        {
            return View(new PerformerVM());
        }

        // POST: Performer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PerformerVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string trimmedName = model.Name.Trim();
            // Prevent duplicate performer names.
            if (_performerRepository.PerformerNameExists(trimmedName))
            {
                ModelState.AddModelError(nameof(model.Name), $"Performer name '{trimmedName}' already exists.");
                return View(model);
            }

            Performer newPerformer = _mapper.Map<Performer>(model);
            newPerformer.Name = trimmedName;

            _performerRepository.AddPerformer(newPerformer);
            _performerRepository.SaveChanges();

            TempData["newPerformerName"] = newPerformer.Name;
            return RedirectToAction(nameof(Index));
        }

        // GET: Performer/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Performer? existingPerformer = _performerRepository.GetPerformerById(id.Value);
            if (existingPerformer == null)
            {
                return NotFound();
            }

            PerformerVM model = _mapper.Map<PerformerVM>(existingPerformer);
            return View(model);
        }

        // POST: Performer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PerformerVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Performer? existingPerformer = _performerRepository.GetPerformerById(id);
            if (existingPerformer == null)
            {
                return NotFound();
            }

            string trimmedName = model.Name.Trim();
            // Prevent duplicate performer names on update.
            if (_performerRepository.PerformerNameExists(trimmedName, id))
            {
                ModelState.AddModelError(nameof(model.Name), $"Performer name '{trimmedName}' already exists.");
                return View(model);
            }

            existingPerformer.Name = trimmedName;
            existingPerformer.Bio = model.Bio;

            _performerRepository.UpdatePerformer(existingPerformer);
            _performerRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: Performer/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Performer? existingPerformer = _performerRepository.GetPerformerById(id.Value);
            if (existingPerformer == null)
            {
                return NotFound();
            }

            PerformerVM model = _mapper.Map<PerformerVM>(existingPerformer);
            // Block delete when performer is still assigned to events.
            if (_performerRepository.PerformerHasEventAssignments(id.Value))
            {
                ViewBag.DeleteBlocked = true;
            }

            return View(model);
        }

        // POST: Performer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Performer? existingPerformer = _performerRepository.GetPerformerById(id);
            if (existingPerformer == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (_performerRepository.PerformerHasEventAssignments(id))
            {
                ModelState.AddModelError(string.Empty, "Cannot delete this performer because one or more events use them.");
                PerformerVM model = _mapper.Map<PerformerVM>(existingPerformer);
                ViewBag.DeleteBlocked = true;
                return View(model);
            }

            _performerRepository.RemovePerformer(existingPerformer);
            _performerRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
