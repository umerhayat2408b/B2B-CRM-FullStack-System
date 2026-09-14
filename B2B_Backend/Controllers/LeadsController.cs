    using B2B_PRO.Models;
    using B2B_PRO.Services;
    using ClosedXML.Excel;
    using Hangfire;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

    namespace B2B_PRO.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class LeadsController : ControllerBase
        {
            private readonly SearchService _searchService;
            private readonly AppDbContext _context;
            private readonly ScraperService _scraperService;
        private readonly AuditService _auditService;
        public LeadsController(AppDbContext context, ScraperService scraperService, SearchService searchService, AuditService auditService)
            {
                _context = context;
                _scraperService = scraperService;
                _searchService = searchService;
            _auditService = auditService;
        }
            private string CurrentRole()
            {
                return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            }

            private int CurrentCompany()
            {
                return Convert.ToInt32(User.FindFirst("CompanyId")?.Value);
            }

        // 1. GET ALL LEADS
        [HttpGet]
        public async Task<IActionResult> GetAllLeads()
        {
            if (CurrentRole() == "SuperAdmin")
            {
                var leads = await _context.leadsDatas
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(leads);
            }

            var companyLeads = await _context.leadsDatas
                .Where(x => x.CompanyId == CurrentCompany())
                .AsNoTracking()
                .ToListAsync();

            return Ok(companyLeads);
        }

        // 2. GET LEAD BY ID
        [HttpGet("{id}")]
            public async Task<IActionResult> GetLeadById(int id)
            {
                var lead = await _context.leadsDatas.FindAsync(id);
                if (lead == null)
                {
                    return NotFound(new { message = "This Lead data is not found" });
                }
                if (CurrentRole() != "SuperAdmin")
                {
                    if (lead.CompanyId != CurrentCompany())
                    {
                        return Unauthorized(new
                        {
                            message = "You cannot access another company's lead."
                        });
                    }
                }

                return Ok(lead);
            }

            // 3. CREATE LEAD (MANUAL)
            [HttpPost]
            public async Task<IActionResult> CreateLead([FromBody] LeadsData newLead)
            {
                if (newLead == null)
                {
                    return BadRequest(new { message = "Please enter correct Data" });
                }

                newLead.CompanyId = CurrentCompany();

                _context.leadsDatas.Add(newLead);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Leads are successfully save.", data = newLead });
            }
            

            // 4. UPDATE LEAD
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateLead(int id, [FromBody] LeadsData updatedLead)
            {
            var lead = await _context.leadsDatas.FindAsync(id);

            if (lead == null)
            {
                return NotFound(new
                {
                    message = "Lead not found"
                });
            }

            if (CurrentRole() != "SuperAdmin")
            {
                if (lead.CompanyId != CurrentCompany())
                {
                    return Unauthorized(new
                    {
                        message = "You cannot update another company's lead."
                    });
                }
            }
            if (lead == null)
                {
                    return NotFound();
                }

                if (id != updatedLead.Id)
                {
                    return BadRequest(new { message = "Id are not matched" });
                }

                _context.Entry(updatedLead).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exist = await _context.leadsDatas.AnyAsync(e => e.Id == id);
                    if (!exist)
                    {
                        return NotFound(new { message = "Data are not found." });
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { message = "Lead data are successfully Update", data = updatedLead });
            }

        // 5. DELETE LEAD
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLead(int id)
        {

            var lead = await _context.leadsDatas.FindAsync(id);
            if (lead == null)
            {
                return NotFound(new { message = "This id data are not available" });
            }
            if (CurrentRole() == "TenantAdmin" || CurrentRole() == "Employee")
            {
                if (lead.CompanyId != CurrentCompany())
                    return Unauthorized();
            }
            _context.leadsDatas.Remove(lead);
            await _context.SaveChangesAsync();
            await _auditService.SaveLog(
"DELETE LEAD",
User.Identity.Name,
CurrentRole(),
CurrentCompany(),
lead.Website);

            return Ok(new { message = "Lead delete successfully" });
        } 

            // 6. SINGLE URL SCRAP

            [HttpPost("scrap")]
            [Authorize]
         public async Task<IActionResult> ScrapedAndSaveLead([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new
                {
                    message = "Please enter a URL"
                });
            }

            var scrapLead = await _scraperService.ScrapePageAsync(url);

            if (scrapLead == null)
            {
                return BadRequest(new
                {
                    message = "Scraper returned no data"
                });
            }

            if (scrapLead.Name == "Error" ||
                scrapLead.Name == "Scraping Error")
            {
                return BadRequest(new
                {
                    message = "Error while scraping website",
                    error = scrapLead.Email
                });
            }

            scrapLead.CompanyId = CurrentCompany();

            _context.leadsDatas.Add(scrapLead);

            await _context.SaveChangesAsync();

            await _auditService.SaveLog(
                "NEW LEAD",
                User.Identity?.Name,
                CurrentRole(),
                CurrentCompany(),
                scrapLead.Website
            );

            if (!string.IsNullOrEmpty(scrapLead.Email) &&
                scrapLead.Email != "Not Found")
            {
                BackgroundJob.Enqueue<EmailService>(
                    x => x.SendLeadEmailAsync(
                        scrapLead.Email,
                        scrapLead.Name,
                        scrapLead.CompanyId
                    )
                );
            }

            return Ok(new
            {
                message = "Website successfully scraped",
                data = scrapLead
            });
        }

        // 7. EXPORT TO EXCEL
        [HttpGet("export")]
            public async Task<IActionResult> ExportToExcel()
            {

                // 1. AsNoTracking se EF Core tracking disable hoti hai (Faster & Low RAM)
                List<LeadsData> leads;

                if (CurrentRole() == "SuperAdmin")
                {
                    leads = await _context.leadsDatas
                        .AsNoTracking()
                        .ToListAsync();
                }
                else
                {
                    leads = await _context.leadsDatas
                        .Where(x => x.CompanyId == CurrentCompany())
                        .AsNoTracking()
                        .ToListAsync();
                }
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Leads Data");

                // Headers setup
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Company Name";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Phon  e";
                worksheet.Cell(1, 5).Value = "Website";

                // Header styling (Optional - for clean design)
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Populating data
                int row = 2;
                foreach (var lead in leads)
                {
                    worksheet.Cell(row, 1).Value = lead.Id;
                    worksheet.Cell(row, 2).Value = lead.Name;
                    worksheet.Cell(row, 3).Value = lead.Email;
                    worksheet.Cell(row, 4).Value = lead.Phone;
                    worksheet.Cell(row, 5).Value = lead.Website;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                // 2. Stream handling without duplicate byte arrays in RAM
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0; // Stream head reset karna zaroori hai

                string fileName = $"B2B_Leads_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                // ToArray() ki jagah direct Stream pass karein
                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            [HttpPost("search-by-keyword")]
            public async Task<IActionResult> StartKeywordSearch([FromQuery] string keyword)
            {
            await _auditService.SaveLog(
    "SEARCH",
    User.Identity.Name,
    CurrentRole(),
    CurrentCompany(),
    keyword);
            if (string.IsNullOrEmpty(keyword))
                {
                    return BadRequest("Keyword is required");
                }

                var discoveredWebsites =
           await _searchService.SearchWebsites(keyword);

                // Har website ko background processing ke liye queue karein
                int companyId = CurrentCompany();
                foreach (var url in discoveredWebsites)
                {
                    BackgroundJob.Enqueue<LeadsController>(x => x.ScrapeAndProcessWebsite(url, keyword , companyId));
                }

                return Ok(new
                {
                    message = $"Keyword '{keyword}' processed successfully and sent to background scraper!",
                    totalWebsitesFound = discoveredWebsites.Count,
                    websitesList = discoveredWebsites
                });
            }

        // 🔥 YEH HAI BACKGROUND JOB JO DATABASE MEIN SAVE KAREGI AUR EMAIL BHI BHEJEGI
        [NonAction]
        public async Task ScrapeAndProcessWebsite(string url, string keyword, int companyId)
        {
            try
            {
                // Check duplicate website
                var existingLead = await _context.leadsDatas
                    .FirstOrDefaultAsync(l =>
                        l.Website == url &&
                        l.CompanyId == companyId);

                if (existingLead != null)
                {
                    Console.WriteLine($"[DUPLICATE] {url}");
                    return;
                }

                // Scrape website
                var scrapLead = await _scraperService.ScrapePageAsync(url);

                if (scrapLead == null)
                {
                    Console.WriteLine($"[ERROR] Scraper returned null: {url}");
                    return;
                }

                // IMPORTANT:
                // Background job mein CurrentCompany() use NAHI karna
                scrapLead.CompanyId = companyId;

                // Check scraper error
                if (scrapLead.Name == "Error" ||
                    scrapLead.Name == "Scraping Error")
                {
                    Console.WriteLine(
                        $"[SCRAPER ERROR] {url} | {scrapLead.Email}"
                    );
                    return;
                }

                // Save lead
                _context.leadsDatas.Add(scrapLead);

                await _context.SaveChangesAsync();

                Console.WriteLine(
                    $"[SAVED] Website: {url} | " +
                    $"CompanyId: {companyId} | " +
                    $"Name: {scrapLead.Name} | " +
                    $"Email: {scrapLead.Email} | " +
                    $"Phone: {scrapLead.Phone}"
                );

                // Send email if available
                if (!string.IsNullOrEmpty(scrapLead.Email) &&
                    scrapLead.Email != "Not Found")
                {
                    BackgroundJob.Enqueue<EmailService>(
                        x => x.SendLeadEmailAsync(
                            scrapLead.Email,
                            scrapLead.Name,
                            scrapLead.CompanyId
                        )
                    );

                    Console.WriteLine(
                        $"[EMAIL QUEUED] {scrapLead.Email}"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ERROR] Processing {url}: {ex.Message}"
                );

                Console.WriteLine(ex.ToString());
            }
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Leads Filtering Based on Role
            IQueryable<LeadsData> leadsQuery = _context.leadsDatas;
            IQueryable<User> usersQuery = _context.Users;

            if (CurrentRole() != "SuperAdmin")
            {
                leadsQuery = leadsQuery.Where(x => x.CompanyId == CurrentCompany());
                usersQuery = usersQuery.Where(x => x.CompanyId == CurrentCompany());
            }

            // 2. Counts Calculation
            var totalLeads = await leadsQuery.CountAsync();

            // SuperAdmin saare companies dekh sakta hai, jabke baki roles ke liye current company
            var totalCompanies = CurrentRole() == "SuperAdmin"
                ? await _context.companyName.CountAsync()
                : 1;

            var totalUsers = await usersQuery.CountAsync();

            var totalEmails = await leadsQuery
                .CountAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email != "Not Found");

            var totalPhones = await leadsQuery
                .CountAsync(x => !string.IsNullOrEmpty(x.Phone) && x.Phone != "Not Found");

            var totalWebsites = await leadsQuery
                .CountAsync(x => !string.IsNullOrEmpty(x.Website));

            // 3. Return Combined Payload
            return Ok(new
            {
                CompanyId = CurrentCompany(),
                UserRole = CurrentRole(),

                TotalLeads = totalLeads,
                TotalCompanies = totalCompanies,
                TotalUsers = totalUsers,
                TotalEmails = totalEmails,
                TotalPhones = totalPhones,
                TotalWebsites = totalWebsites
            });
        }
        [HttpGet("recent")]
            public async Task<IActionResult> RecentLeads()
            {
                IQueryable<LeadsData> query = _context.leadsDatas;

                if (CurrentRole() != "SuperAdmin")
                {
                    query = query.Where(x => x.CompanyId == CurrentCompany());
                }

                var leads = await query
                    .OrderByDescending(x => x.Id)
                    .Take(5)
                    .ToListAsync();

                return Ok(leads);
            }
            [HttpGet("stats")]
            public async Task<IActionResult> Statistics()
            {
                IQueryable<LeadsData> query = _context.leadsDatas;

                if (CurrentRole() != "SuperAdmin")
                {
                    query = query.Where(x => x.CompanyId == CurrentCompany());
                }

                var stats = new
                {
                    TotalLeads = await query.CountAsync(),

                    EmailsFound = await query.CountAsync(x => !string.IsNullOrEmpty(x.Email)),

                    PhonesFound = await query.CountAsync(x => !string.IsNullOrEmpty(x.Phone)),

                    WebsitesFound = await query.CountAsync(x => !string.IsNullOrEmpty(x.Website))
                };

                return Ok(stats);
            }
        }
    }