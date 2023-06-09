﻿using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Repository;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository m_raceRepository;
        private readonly IPhotoService m_photoService;
        private readonly IHttpContextAccessor m_httpContextAccessor;
        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            m_raceRepository = raceRepository;
            m_photoService = photoService;
            m_httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            return View(await m_raceRepository.GetAll());
        }

        public async Task<IActionResult> Detail(int id)
        {
            Models.Race race = await m_raceRepository.GetByIdAsync(id);
            return View(race);
        }

        public async Task<IActionResult> Create()
        {
            var currUserId = m_httpContextAccessor.HttpContext.User.GetUserId();
            var createVM = new CreateRaceViewModel
            {
                AppUserId = currUserId
            };
            return View(createVM);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {
            if (ModelState.IsValid)
            {
                var uploadedImage = await m_photoService.AddPhotoAsync(raceVM.Image);

                Race RaceToAdd = new Race
                {
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    RaceCategory = raceVM.RaceCategory,
                    Image = uploadedImage.Url.ToString(),
                    AppUserId = raceVM.AppUserId,
                    Address = new Address
                    {
                        Street = raceVM.Address.Street,
                        City = raceVM.Address.City,
                        State = raceVM.Address.State
                    }
                };
                m_raceRepository.Add(RaceToAdd);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ClubToEdit = await m_raceRepository.GetByIdAsync(id);
            if (ClubToEdit == null) return View("Error");

            var raceVM = new EditRaceViewModel
            {
                Title = ClubToEdit.Title,
                Description = ClubToEdit.Description,
                AddressId = ClubToEdit.Address.id,
                Address = ClubToEdit.Address,
                RaceCategory = ClubToEdit.RaceCategory,
                URL = ClubToEdit.Image
            };

            return View(raceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVMToEdit)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit race");
                return View("Edit", raceVMToEdit);
            }
            var userClub = await m_raceRepository.GetByIdAsync(id);

            if (userClub != null)
            {
                try
                {
                    await m_photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Couldnt delete previously uploaded photo!");
                    return View(raceVMToEdit);
                }

                var newImage = await m_photoService.AddPhotoAsync(raceVMToEdit.Image);


                userClub.RaceCategory = raceVMToEdit.RaceCategory;
                userClub.Address = raceVMToEdit.Address;
                userClub.AddressId = raceVMToEdit.AddressId;
                userClub.Image = newImage.Url.ToString();
                userClub.Title = raceVMToEdit.Title;
                userClub.Description = raceVMToEdit.Description;
                userClub.Id = raceVMToEdit.Id;

                m_raceRepository.Update(userClub);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var raceDetails = await m_raceRepository.GetByIdAsync(id);
            if (raceDetails == null) return View("Error");

            return View(raceDetails);
        }

        [HttpPost, ActionName("DeleteRace")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var raceToDelete = await m_raceRepository.GetByIdAsync(id);
            if (raceToDelete == null) return View("Error");
            m_raceRepository.Delete(raceToDelete);

            return RedirectToAction("Index");
        }
    }
}
