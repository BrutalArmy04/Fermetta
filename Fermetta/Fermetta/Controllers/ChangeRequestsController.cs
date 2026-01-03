using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Models.ChangeRequests;
using Fermetta.Models.ViewModels.ChangeRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fermetta.Controllers
{
    [Authorize]
    public class ChangeRequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangeRequestsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // -------------------------
        // EDITOR: CREATE (GET)
        // -------------------------
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Create()
        {
            var vm = new ChangeRequestInfo
            {
                Type = ChangeRequestType.Category,
                RequestAction = ChangeAction.Create,
                Categories = await _db.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name))
                    .ToListAsync(),
                Products = await _db.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name))
                    .ToListAsync()
            };

            return View(vm);
        }

        // -------------------------
        // EDITOR: LOAD (prefill pentru Update, fără AJAX)
        // -------------------------
        [Authorize(Roles = "Editor")]
        [HttpGet]
        public async Task<IActionResult> Load(ChangeRequestInfo vm)
        {
            // repopulate dropdowns
            vm.Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name))
                .ToListAsync();

            vm.Products = await _db.Products
                .OrderBy(p => p.Name)
                .Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name))
                .ToListAsync();

            // Prefill are sens doar pentru Update
            if (vm.RequestAction != ChangeAction.Update)
                return View("Create", vm);

            if (vm.Type == ChangeRequestType.Category)
            {
                if (vm.TargetCategoryId.HasValue)
                {
                    var cat = await _db.Categories
                        .FirstOrDefaultAsync(c => c.Category_Id == vm.TargetCategoryId.Value);

                    if (cat != null)
                    {
                        vm.CategoryName = cat.Name;
                        vm.CategoryDescription = cat.Description;
                        vm.CategoryDisponibility = cat.Disponibility;
                    }
                }
            }
            else // Product
            {
                if (vm.TargetProductId.HasValue)
                {
                    var prod = await _db.Products
                        .FirstOrDefaultAsync(p => p.Product_Id == vm.TargetProductId.Value);

                    if (prod != null)
                    {
                        vm.ProductName = prod.Name;
                        vm.Weight = prod.Weight;
                        vm.Valability = prod.Valability.Date;
                        vm.Price = prod.Price;
                        vm.Stock = prod.Stock;
                        vm.Personalised = prod.Personalised;
                        vm.Category_Id = prod.Category_Id;
                    }
                }
            }

            return View("Create", vm);
        }

        // -------------------------
        // EDITOR: CREATE (POST)
        // -------------------------
        [HttpPost]
        [Authorize(Roles = "Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeRequestInfo vm)
        {
            // repopulate dropdowns dacă returnăm view cu erori
            vm.Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name))
                .ToListAsync();

            vm.Products = await _db.Products
                .OrderBy(p => p.Name)
                .Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name))
                .ToListAsync();

            // Validări pe tip
            if (vm.Type == ChangeRequestType.Category)
            {
                if (string.IsNullOrWhiteSpace(vm.CategoryName))
                    ModelState.AddModelError(nameof(vm.CategoryName), "Category name is mandatory.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(vm.ProductName))
                    ModelState.AddModelError(nameof(vm.ProductName), "Product name is mandatory.");
                if (vm.Weight is null) ModelState.AddModelError(nameof(vm.Weight), "Weight is mandatory.");
                if (vm.Valability is null) ModelState.AddModelError(nameof(vm.Valability), "Valability is mandatory.");
                if (vm.Price is null) ModelState.AddModelError(nameof(vm.Price), "Price is mandatory.");
                if (vm.Stock is null) ModelState.AddModelError(nameof(vm.Stock), "Stock is mandatory.");
                if (vm.Category_Id is null || vm.Category_Id < 1)
                    ModelState.AddModelError(nameof(vm.Category_Id), "Select a category.");
            }

            // Validări pentru UPDATE
            if (vm.RequestAction == ChangeAction.Update)
            {
                if (vm.Type == ChangeRequestType.Category && (vm.TargetCategoryId is null || vm.TargetCategoryId < 1))
                    ModelState.AddModelError(nameof(vm.TargetCategoryId), "Select a category to modify.");

                if (vm.Type == ChangeRequestType.Product && (vm.TargetProductId is null || vm.TargetProductId < 1))
                    ModelState.AddModelError(nameof(vm.TargetProductId), "Select a product to modify.");
            }

            if (!ModelState.IsValid) return View(vm);

            string proposedJson;

            if (vm.Type == ChangeRequestType.Category)
            {
                var dto = new CategoryProposal
                {
                    Name = vm.CategoryName!,
                    Description = vm.CategoryDescription,
                    Disponibility = vm.CategoryDisponibility
                };
                proposedJson = JsonSerializer.Serialize(dto);
            }
            else
            {
                var dto = new ProductProposal
                {
                    Name = vm.ProductName!,
                    Weight = vm.Weight!.Value,
                    Valability = vm.Valability!.Value.Date,
                    Price = vm.Price!.Value,
                    Stock = vm.Stock!.Value,
                    Personalised = vm.Personalised,
                    Category_Id = vm.Category_Id!.Value
                };
                proposedJson = JsonSerializer.Serialize(dto);
            }

            var req = new ChangeRequest
            {
                Type = vm.Type,
                Action = vm.RequestAction,
                Status = ChangeRequestStatus.Pending,
                CreatedByUserId = _userManager.GetUserId(User)!,
                ContributorNote = vm.ContributorNote,
                TargetCategoryId = vm.TargetCategoryId,
                TargetProductId = vm.TargetProductId,
                ProposedJson = proposedJson
            };

            _db.ChangeRequests.Add(req);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Your request has been sent to the Admin.";
            return RedirectToAction(nameof(Create));
        }

        // -------------------------
        // ADMIN: INBOX + REVIEW
        // -------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inbox()
        {
            var pending = await _db.ChangeRequests
                .Where(r => r.Status == ChangeRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(pending);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review(int id)
        {
            var req = await _db.ChangeRequests.FirstOrDefaultAsync(r => r.ChangeRequest_Id == id);
            if (req == null) return NotFound();

            var vm = await BuildReviewVm(req);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(ChangeRequestReview vm)
        {
            var req = await _db.ChangeRequests.FirstOrDefaultAsync(r => r.ChangeRequest_Id == vm.Id);
            if (req == null) return NotFound();
            if (req.Status != ChangeRequestStatus.Pending) return Forbid();

            string draftJson;

            if (req.Type == ChangeRequestType.Category)
            {
                if (string.IsNullOrWhiteSpace(vm.DraftCategoryName))
                {
                    TempData["Error"] = "Category name is mandatory.";
                    return RedirectToAction(nameof(Review), new { id = vm.Id });
                }

                var dto = new CategoryProposal
                {
                    Name = vm.DraftCategoryName!,
                    Description = vm.DraftCategoryDescription,
                    Disponibility = vm.DraftCategoryDisponibility
                };
                draftJson = JsonSerializer.Serialize(dto);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(vm.DraftProductName) ||
                    vm.DraftWeight is null || vm.DraftValability is null ||
                    vm.DraftPrice is null || vm.DraftStock is null ||
                    vm.DraftCategory_Id is null || vm.DraftCategory_Id < 1)
                {
                    TempData["Error"] = "Complete all product fields.";
                    return RedirectToAction(nameof(Review), new { id = vm.Id });
                }

                var dto = new ProductProposal
                {
                    Name = vm.DraftProductName!,
                    Weight = vm.DraftWeight.Value,
                    Valability = vm.DraftValability.Value.Date,
                    Price = vm.DraftPrice.Value,
                    Stock = vm.DraftStock.Value,
                    Personalised = vm.DraftPersonalised,
                    Category_Id = vm.DraftCategory_Id.Value
                };
                draftJson = JsonSerializer.Serialize(dto);
            }

            req.AdminDraftJson = draftJson;
            req.AdminNote = vm.AdminNote;

            await _db.SaveChangesAsync();
            TempData["Message"] = "Saved draft.";
            return RedirectToAction(nameof(Review), new { id = vm.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDraft(int id)
        {
            var req = await _db.ChangeRequests.FirstOrDefaultAsync(r => r.ChangeRequest_Id == id);
            if (req == null) return NotFound();
            if (req.Status != ChangeRequestStatus.Pending) return Forbid();

            req.AdminDraftJson = null;
            req.AdminNote = null;

            await _db.SaveChangesAsync();
            TempData["Message"] = "Admin's changes have been removed.";
            return RedirectToAction(nameof(Review), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var req = await _db.ChangeRequests.FirstOrDefaultAsync(r => r.ChangeRequest_Id == id);
            if (req == null) return NotFound();
            if (req.Status != ChangeRequestStatus.Pending) return Forbid();

            req.Status = ChangeRequestStatus.Declined;
            req.ReviewedByUserId = _userManager.GetUserId(User);
            req.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Inbox));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var req = await _db.ChangeRequests.FirstOrDefaultAsync(r => r.ChangeRequest_Id == id);
            if (req == null) return NotFound();
            if (req.Status != ChangeRequestStatus.Pending) return Forbid();

            var jsonToApply = req.AdminDraftJson ?? req.ProposedJson;

            if (req.Type == ChangeRequestType.Category)
            {
                var dto = JsonSerializer.Deserialize<CategoryProposal>(jsonToApply);
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();

                if (req.Action == ChangeAction.Create)
                {
                    _db.Categories.Add(new Category
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        Disponibility = dto.Disponibility
                    });
                }
                else
                {
                    if (req.TargetCategoryId is null) return BadRequest();
                    var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Category_Id == req.TargetCategoryId.Value);
                    if (cat == null) return NotFound();

                    cat.Name = dto.Name;
                    cat.Description = dto.Description;
                    cat.Disponibility = dto.Disponibility;
                }
            }
            else
            {
                var dto = JsonSerializer.Deserialize<ProductProposal>(jsonToApply);
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();

                if (req.Action == ChangeAction.Create)
                {
                    _db.Products.Add(new Product
                    {
                        Name = dto.Name,
                        Weight = dto.Weight,
                        Valability = dto.Valability.Date,
                        Price = dto.Price,
                        Stock = dto.Stock,
                        Personalised = dto.Personalised,
                        Category_Id = dto.Category_Id
                    });
                }
                else
                {
                    if (req.TargetProductId is null) return BadRequest();
                    var prod = await _db.Products.FirstOrDefaultAsync(p => p.Product_Id == req.TargetProductId.Value);
                    if (prod == null) return NotFound();

                    prod.Name = dto.Name;
                    prod.Weight = dto.Weight;
                    prod.Valability = dto.Valability.Date;
                    prod.Price = dto.Price;
                    prod.Stock = dto.Stock;
                    prod.Personalised = dto.Personalised;
                    prod.Category_Id = dto.Category_Id;
                }
            }

            req.Status = ChangeRequestStatus.Accepted;
            req.ReviewedByUserId = _userManager.GetUserId(User);
            req.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Inbox));
        }

        // helper: construiește VM pt Review
        private async Task<ChangeRequestReview> BuildReviewVm(ChangeRequest req)
        {
            var vm = new ChangeRequestReview
            {
                Id = req.ChangeRequest_Id,
                Type = req.Type,
                RequestAction = req.Action,
                Status = req.Status,
                ContributorNote = req.ContributorNote,
                AdminNote = req.AdminNote,
                Categories = await _db.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name))
                    .ToListAsync()
            };

            if (req.Type == ChangeRequestType.Category)
            {
                var proposed = JsonSerializer.Deserialize<CategoryProposal>(req.ProposedJson)!;
                var draft = JsonSerializer.Deserialize<CategoryProposal>(req.AdminDraftJson ?? req.ProposedJson)!;

                vm.ProposedCategoryName = proposed.Name;
                vm.ProposedCategoryDescription = proposed.Description;
                vm.ProposedCategoryDisponibility = proposed.Disponibility;

                vm.DraftCategoryName = draft.Name;
                vm.DraftCategoryDescription = draft.Description;
                vm.DraftCategoryDisponibility = draft.Disponibility;
            }
            else
            {
                var proposed = JsonSerializer.Deserialize<ProductProposal>(req.ProposedJson)!;
                var draft = JsonSerializer.Deserialize<ProductProposal>(req.AdminDraftJson ?? req.ProposedJson)!;

                vm.ProposedProductName = proposed.Name;
                vm.ProposedWeight = proposed.Weight;
                vm.ProposedValability = proposed.Valability;
                vm.ProposedPrice = proposed.Price;
                vm.ProposedStock = proposed.Stock;
                vm.ProposedPersonalised = proposed.Personalised;
                vm.ProposedCategory_Id = proposed.Category_Id;

                vm.DraftProductName = draft.Name;
                vm.DraftWeight = draft.Weight;
                vm.DraftValability = draft.Valability;
                vm.DraftPrice = draft.Price;
                vm.DraftStock = draft.Stock;
                vm.DraftPersonalised = draft.Personalised;
                vm.DraftCategory_Id = draft.Category_Id;
            }

            return vm;
        }
    }
}
