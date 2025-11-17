using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PilotController(
            IObstacleRepository obstacleRepository,
            IRegistrarRepository registrarRepository,
            UserManager<ApplicationUser> userManager)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var myObstacles = await _obstacleRepository.GetObstaclesByOwner(user.Id);
            return View(myObstacles);
        }

        [HttpGet]
        public async Task<IActionResult> DetaljerOmRapport(int obstacleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null || obstacle.OwnerUserId != user.Id)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }

            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View(obstacle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacle(int obstacleId, string obstacleName, string obstacleDescription, double obstacleHeight)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null || obstacle.OwnerUserId != user.Id)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }

            if (obstacle.ObstacleStatus != 1)
            {
                TempData["Error"] = "Du kan kun redigere mens hindringen er under behandling.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            // Valider input
            if (string.IsNullOrWhiteSpace(obstacleName) || obstacleName.Length > 100)
            {
                TempData["Error"] = "Navn må være mellom 1 og 100 tegn.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            if (obstacleHeight < 0 || obstacleHeight > 200)
            {
                TempData["Error"] = "Høyde må være mellom 0 og 200 meter.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            if (string.IsNullOrWhiteSpace(obstacleDescription) || obstacleDescription.Length > 1000)
            {
                TempData["Error"] = "Beskrivelse må være mellom 1 og 1000 tegn.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            // Spor endringer for å lage en informativ kommentar
            var changes = new List<string>();
            if (obstacle.ObstacleName != obstacleName)
            {
                changes.Add($"Navn: '{obstacle.ObstacleName}' → '{obstacleName}'");
            }
            if (Math.Abs(obstacle.ObstacleHeight - obstacleHeight) > 0.01)
            {
                changes.Add($"Høyde: {obstacle.ObstacleHeight}m → {obstacleHeight}m");
            }
            if (obstacle.ObstacleDescription != obstacleDescription)
            {
                changes.Add("Beskrivelse er oppdatert");
            }

            // Oppdater hindringen
            obstacle.ObstacleName = obstacleName;
            obstacle.ObstacleDescription = obstacleDescription;
            obstacle.ObstacleHeight = obstacleHeight;

            await _obstacleRepository.UpdateObstacles(obstacle);

            // Legg igjen en kommentar om at piloten oppdaterte (kun hvis det var endringer)
            if (changes.Any())
            {
                var changeDescription = changes.Count == 1 
                    ? changes.First() 
                    : string.Join(", ", changes);
                
                await _registrarRepository.AddRapport(new RapportData
                {
                    ObstacleId = obstacle.ObstacleId,
                    RapportComment = $"Piloten oppdaterte hindringen. Endringer: {changeDescription}."
                });
            }

            TempData["Success"] = "Hindringen er oppdatert.";
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }
    }
}

