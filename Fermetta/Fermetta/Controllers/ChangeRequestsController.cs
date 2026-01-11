using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Models.ChangeRequests;
using Fermetta.Models.ViewModels.ChangeRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChangeRequestsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Contribuitor")]
        public async Task<IActionResult> Create()
        {
            var vm = new ChangeRequestInfo
            {
                Type = ChangeRequestType.Category,
                RequestAction = ChangeAction.Create,
                Categories = await _db.Categories.OrderBy(c => c.Name).Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name)).ToListAsync(),
                Products = await _db.Products.OrderBy(p => p.Name).Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name)).ToListAsync()
            };
            return View(vm);
        }

        [Authorize(Roles = "Contribuitor")]
        [HttpGet]
        public async Task<IActionResult> Load(ChangeRequestInfo vm)
        {
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name)).ToListAsync();
            vm.Products = await _db.Products.OrderBy(p => p.Name).Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name)).ToListAsync();

            if (vm.RequestAction != ChangeAction.Update) return View("Create", vm);

            if (vm.Type == ChangeRequestType.Category)
            {
                if (vm.TargetCategoryId.HasValue)
                {
                    var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Category_Id == vm.TargetCategoryId.Value);
                    if (cat != null)
                    {
                        vm.CategoryName = cat.Name;
                        vm.CategoryDescription = cat.Description;
                        vm.CategoryAvailability = cat.Disponibility;
                    }
                }
            }
            else // Product
            {
                if (vm.TargetProductId.HasValue)
                {
                    var prod = await _db.Products.FirstOrDefaultAsync(p => p.Product_Id == vm.TargetProductId.Value);
                    if (prod != null)
                    {
                        vm.ProductName = prod.Name;
                        vm.ProductDescription = prod.Description; 
                        vm.Weight = prod.Weight;
                        vm.Validity = prod.Validity.Date;
                        vm.Price = prod.Price;
                        vm.Stock = prod.Stock;
                        vm.Personalised = prod.Personalised;
                        vm.Category_Id = prod.Category_Id;
                        vm.ExistingImagePath = prod.ImagePath;
                    }
                }
            }
            return View("Create", vm);
        }

        [HttpPost]
        [Authorize(Roles = "Contribuitor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeRequestInfo vm)
        {
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name)).ToListAsync();
            vm.Products = await _db.Products.OrderBy(p => p.Name).Select(p => new ValueTuple<int, string>(p.Product_Id, p.Name)).ToListAsync();

            if (vm.Type == ChangeRequestType.Category)
            {
                if (string.IsNullOrWhiteSpace(vm.CategoryName)) ModelState.AddModelError(nameof(vm.CategoryName), "Category name is mandatory.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(vm.ProductName)) ModelState.AddModelError(nameof(vm.ProductName), "Product name is mandatory.");
                if (string.IsNullOrWhiteSpace(vm.ProductDescription)) ModelState.AddModelError(nameof(vm.ProductDescription), "Description is mandatory."); // <--- VALIDARE
                if (vm.Weight is null) ModelState.AddModelError(nameof(vm.Weight), "Weight is mandatory.");
                if (vm.Validity is null) ModelState.AddModelError(nameof(vm.Validity), "Validity is mandatory.");
                if (vm.Price is null) ModelState.AddModelError(nameof(vm.Price), "Price is mandatory.");
                if (vm.Stock is null) ModelState.AddModelError(nameof(vm.Stock), "Stock is mandatory.");
                if (vm.Category_Id is null || vm.Category_Id < 1) ModelState.AddModelError(nameof(vm.Category_Id), "Select a category.");
            }

            if (vm.RequestAction == ChangeAction.Update)
            {
                if (vm.Type == ChangeRequestType.Category && (vm.TargetCategoryId is null || vm.TargetCategoryId < 1))
                    ModelState.AddModelError(nameof(vm.TargetCategoryId), "Select a category to modify.");
                if (vm.Type == ChangeRequestType.Product && (vm.TargetProductId is null || vm.TargetProductId < 1))
                    ModelState.AddModelError(nameof(vm.TargetProductId), "Select a product to modify.");
            }

            if (!ModelState.IsValid) return View(vm);

            string proposedJson;
            string? savedImagePath = vm.ExistingImagePath;

            if (vm.Type == ChangeRequestType.Product && vm.ImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(fileStream);
                }
                savedImagePath = "/images/products/" + uniqueFileName;
            }

            if (vm.Type == ChangeRequestType.Category)
            {
                var dto = new CategoryProposal
                {
                    Name = vm.CategoryName!,
                    Description = vm.CategoryDescription,
                    Disponibility = vm.CategoryAvailability
                };
                proposedJson = JsonSerializer.Serialize(dto);
            }
            else
            {
                var dto = new ProductProposal
                {
                    Name = vm.ProductName!,
                    Description = vm.ProductDescription,
                    Weight = vm.Weight!.Value,
                    Validity = vm.Validity!.Value.Date,
                    Price = vm.Price!.Value,
                    Stock = vm.Stock!.Value,
                    Personalised = vm.Personalised,
                    Category_Id = vm.Category_Id!.Value,
                    ImagePath = savedImagePath
                };
                proposedJson = JsonSerializer.Serialize(dto);
            }

            var req = new ChangeRequest
            {
                Type = vm.Type,
                Action = vm.RequestAction,
                Status = ChangeRequestStatus.Pending,
                CreatedByUserId = _userManager.GetUserId(User)!,
                ContribuitorNote = vm.ContribuitorNote,
                TargetCategoryId = vm.TargetCategoryId,
                TargetProductId = vm.TargetProductId,
                ProposedJson = proposedJson
            };

            _db.ChangeRequests.Add(req);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Your request has been sent to the Admin.";
            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inbox()
        {
            var pending = await _db.ChangeRequests.Where(r => r.Status == ChangeRequestStatus.Pending).OrderByDescending(r => r.CreatedAt).ToListAsync();
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
                if (string.IsNullOrWhiteSpace(vm.DraftCategoryName)) { TempData["Error"] = "Category name is mandatory."; return RedirectToAction(nameof(Review), new { id = vm.Id }); }
                var dto = new CategoryProposal
                {
                    Name = vm.DraftCategoryName!,
                    Description = vm.DraftCategoryDescription,
                    Disponibility = vm.DraftCategoryAvailability
                };
                draftJson = JsonSerializer.Serialize(dto);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(vm.DraftProductName) || string.IsNullOrWhiteSpace(vm.DraftProductDescription) || // <--- VALIDARE
                    vm.DraftWeight is null || vm.DraftValidity is null || vm.DraftPrice is null || vm.DraftStock is null || vm.DraftCategory_Id is null || vm.DraftCategory_Id < 1)
                {
                    TempData["Error"] = "Complete all product fields.";
                    return RedirectToAction(nameof(Review), new { id = vm.Id });
                }

                var oldDraft = string.IsNullOrEmpty(req.AdminDraftJson) ? null : JsonSerializer.Deserialize<ProductProposal>(req.AdminDraftJson);
                var proposal = JsonSerializer.Deserialize<ProductProposal>(req.ProposedJson);
                string? imagePathToKeep = oldDraft?.ImagePath ?? proposal?.ImagePath;

                var dto = new ProductProposal
                {
                    Name = vm.DraftProductName!,
                    Description = vm.DraftProductDescription, 
                    Weight = vm.DraftWeight.Value,
                    Validity = vm.DraftValidity.Value.Date,
                    Price = vm.DraftPrice.Value,
                    Stock = vm.DraftStock.Value,
                    Personalised = vm.DraftPersonalised,
                    Category_Id = vm.DraftCategory_Id.Value,
                    ImagePath = imagePathToKeep
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
                    _db.Categories.Add(new Category { Name = dto.Name, Description = dto.Description, Disponibility = dto.Disponibility });
                }
                else
                {
                    if (req.TargetCategoryId is null) return BadRequest();
                    var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Category_Id == req.TargetCategoryId.Value);
                    if (cat == null) return NotFound();
                    cat.Name = dto.Name; cat.Description = dto.Description; cat.Disponibility = dto.Disponibility;
                }
            }
            else
            {
                var dto = JsonSerializer.Deserialize<ProductProposal>(jsonToApply);
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();
                if (string.IsNullOrWhiteSpace(dto.Description))
                {
                    TempData["Error"] = "Cannot accept: The product description is missing. Please fill it in the 'Draft' section and click 'Save Draft' first.";
                    return RedirectToAction(nameof(Review), new { id = req.ChangeRequest_Id });
                }
                if (req.Action == ChangeAction.Create)
                {
                    _db.Products.Add(new Product
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        Weight = dto.Weight,
                        Validity = dto.Validity.Date,
                        Price = dto.Price,
                        Stock = dto.Stock,
                        Personalised = dto.Personalised,
                        Category_Id = dto.Category_Id,
                        ImagePath = dto.ImagePath
                    });
                }
                else
                {
                    if (req.TargetProductId is null) return BadRequest();
                    var prod = await _db.Products.FirstOrDefaultAsync(p => p.Product_Id == req.TargetProductId.Value);
                    if (prod == null) return NotFound();
                    prod.Name = dto.Name;
                    prod.Description = dto.Description; 
                    prod.Weight = dto.Weight;
                    prod.Validity = dto.Validity.Date;
                    prod.Price = dto.Price;
                    prod.Stock = dto.Stock;
                    prod.Personalised = dto.Personalised;
                    prod.Category_Id = dto.Category_Id;
                    if (dto.ImagePath != null) prod.ImagePath = dto.ImagePath;
                }
            }

            req.Status = ChangeRequestStatus.Accepted;
            req.ReviewedByUserId = _userManager.GetUserId(User);
            req.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Inbox));
        }

        private async Task<ChangeRequestReview> BuildReviewVm(ChangeRequest req)
        {
            var vm = new ChangeRequestReview
            {
                Id = req.ChangeRequest_Id,
                Type = req.Type,
                RequestAction = req.Action,
                Status = req.Status,
                ContribuitorNote = req.ContribuitorNote,
                AdminNote = req.AdminNote,
                Categories = await _db.Categories.OrderBy(c => c.Name).Select(c => new ValueTuple<int, string>(c.Category_Id, c.Name)).ToListAsync()
            };

            if (req.Type == ChangeRequestType.Category)
            {
                var proposed = JsonSerializer.Deserialize<CategoryProposal>(req.ProposedJson)!;
                var draft = JsonSerializer.Deserialize<CategoryProposal>(req.AdminDraftJson ?? req.ProposedJson)!;
                vm.ProposedCategoryName = proposed.Name; vm.ProposedCategoryDescription = proposed.Description; vm.ProposedCategoryAvailability = proposed.Disponibility;
                vm.DraftCategoryName = draft.Name; vm.DraftCategoryDescription = draft.Description; vm.DraftCategoryAvailability = draft.Disponibility;
            }
            else
            {
                var proposed = JsonSerializer.Deserialize<ProductProposal>(req.ProposedJson)!;
                var draft = JsonSerializer.Deserialize<ProductProposal>(req.AdminDraftJson ?? req.ProposedJson)!;

                vm.ProposedProductName = proposed.Name;
                vm.ProposedProductDescription = proposed.Description; 
                vm.ProposedWeight = proposed.Weight;
                vm.ProposedValidity = proposed.Validity;
                vm.ProposedPrice = proposed.Price;
                vm.ProposedStock = proposed.Stock;
                vm.ProposedPersonalised = proposed.Personalised;
                vm.ProposedCategory_Id = proposed.Category_Id;
                vm.ProposedImagePath = proposed.ImagePath;

                vm.DraftProductName = draft.Name;
                vm.DraftProductDescription = draft.Description; 
                vm.DraftWeight = draft.Weight;
                vm.DraftValidity = draft.Validity;
                vm.DraftPrice = draft.Price;
                vm.DraftStock = draft.Stock;
                vm.DraftPersonalised = draft.Personalised;
                vm.DraftCategory_Id = draft.Category_Id;
                vm.DraftImagePath = draft.ImagePath;
            }
            return vm;
        }
    }
}