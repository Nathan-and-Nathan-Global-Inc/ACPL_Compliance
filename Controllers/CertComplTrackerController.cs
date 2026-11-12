using MasterClass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using SupermarketRepository;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace calibpro.Controllers
{
    public class CertComplTrackerController : Controller
    {
        private readonly DBClass _db;
        private readonly ILogger<CertComplTrackerController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private object _env;

        public CertComplTrackerController(ILogger<CertComplTrackerController> logger, DBClass db, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /legalactmaster
        public IActionResult Index()
        {
            try
            {
                var certcompltrack = _db.SelectAll<Class_certi_comp_tracker>()
                    .Where(t => t.del_status == false)
                    .OrderBy(t => t.calib_donedt)
                    .ToList();
                return View(certcompltrack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Legal Acts");
                TempData["msg"] = "Error loading Legal Acts";
                return View(Enumerable.Empty<Class_certi_comp_tracker>());
            }
        }

        // GET: /Legal Acts/Details/5
        public IActionResult Details(int sl)
        {
            var legalact= _db.SelectById<Class_certi_comp_tracker>(sl);
            if (legalact== null || legalact.del_status)
            {
                return NotFound();
            }
            return View(legalact);
        }

        // GET: /Legal Acts/Create
        public IActionResult Create()
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
            return View(new Class_certi_comp_tracker { del_status = false });
        }

        // POST: /Legal Acts/Create
        [HttpPost]
        public IActionResult Create(Class_certi_comp_tracker model)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees clsemp = JsonConvert.DeserializeObject<Class_employees>(st);
            if (IsDuplicateCertificateNo(model.certificate_no))
            {
                ModelState.AddModelError(
                    nameof(model.certificate_no),
                    "This Certificate Number already exists."
                );
            }
            if (IsDuplicate(model.equipment, model.certificate_no))
                ModelState.AddModelError(nameof(model.certificate_no),
                    "This Equipment with the same Certificate Number already exists");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            { 
                model.created_by= clsemp.empl_slno;
                model.created_dt = DateTime.Now;
                model.del_status = false;
                if (string.IsNullOrWhiteSpace(model.calib_donedt))
                {
                    ModelState.AddModelError("calib_donedt", "Calibration Done Date is required.");
                }
                else if (DateTime.TryParse(model.calib_donedt, out DateTime doneDate))
                {
                    if (doneDate.Date > DateTime.Today)
                    {
                        ModelState.AddModelError(
                            "calib_donedt",
                            "Calibration Done Date cannot be a future date."
                        );
                    }
                }
                else
                {
                    ModelState.AddModelError("calib_donedt", "Invalid Calibration Done Date.");
                }

                if (!ModelState.IsValid)
                    return View(model);

                // Calculate due date on server
                DateTime calibrationDate = DateTime.Parse(model.calib_donedt);

                model.calib_duedt = model.freq_desc switch
                {
                    "Days" => calibrationDate.AddDays(model.freq_no).ToString("yyyy-MM-dd"),
                    "Weeks" => calibrationDate.AddDays(model.freq_no * 7).ToString("yyyy-MM-dd"),
                    "Months" => calibrationDate.AddMonths(model.freq_no).ToString("yyyy-MM-dd"),
                    "Years" => calibrationDate.AddYears(model.freq_no).ToString("yyyy-MM-dd"),
                    _ => string.Empty
                };
                _db.AddNew<Class_certi_comp_tracker>(model);
                TempData["msg"] = "Data Submitted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                ModelState.AddModelError(string.Empty, "Error creating group");
                return View(model);
            }
        }

        // GET: CertComplTracker/Edit/5
        public IActionResult Edit(int sl)
        {
            var model = _db.SelectById<Class_certi_comp_tracker>(sl);

            if (model == null || model.del_status)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: CertComplTracker/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int sl, Class_certi_comp_tracker model)
        {
            var st = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(st))
                return Unauthorized();

            var clsemp = JsonConvert.DeserializeObject<Class_employees>(st);

            if (clsemp == null)
                return Unauthorized();
            if (IsDuplicateCertificateNo(model.certificate_no, sl))
            {
                ModelState.AddModelError(
                    nameof(model.certificate_no),
                    "This Certificate Number already exists."
                );
            }
            if (IsDuplicate(model.equipment, model.certificate_no, sl))
            {
                ModelState.AddModelError(
                    nameof(model.certificate_no),
                    "This Equipment with the same Certificate Number already exists."
                );
            }

            if (model.freq_no <= 0)
            {
                ModelState.AddModelError(nameof(model.freq_no), "Frequency must be greater than zero.");
            }

            string[] validFrequencies = { "Days", "Weeks", "Months", "Years" };

            if (!validFrequencies.Contains(model.freq_desc))
            {
                ModelState.AddModelError(nameof(model.freq_desc), "Please select a valid frequency.");
            }

            if (!DateTime.TryParseExact(
                    model.calib_donedt,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime calibrationDate))
            {
                ModelState.AddModelError(nameof(model.calib_donedt), "Invalid Calibration Done Date.");
            }
            else if (calibrationDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.calib_donedt), "Calibration Done Date cannot be a future date.");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var existing = _db.SelectById<Class_certi_comp_tracker>(sl);

                if (existing == null || existing.del_status)
                    return NotFound();

                // Preserve original creation details
                model.slno = existing.slno;
                model.created_by = existing.created_by;
                model.created_dt = existing.created_dt;
                model.del_status = existing.del_status;

                // Update modification details
                model.modified_by = clsemp.empl_slno;
                model.modified_dt = DateTime.Now;

                // Calculate due date on the server
                model.calib_duedt = model.freq_desc switch
                {
                    "Days" => calibrationDate.AddDays(model.freq_no).ToString("yyyy-MM-dd"),
                    "Weeks" => calibrationDate.AddDays(model.freq_no * 7).ToString("yyyy-MM-dd"),
                    "Months" => calibrationDate.AddMonths(model.freq_no).ToString("yyyy-MM-dd"),
                    "Years" => calibrationDate.AddYears(model.freq_no).ToString("yyyy-MM-dd"),
                    _ => string.Empty
                };

                _db.Update<Class_certi_comp_tracker>(model);

                TempData["msg"] = "Data Saved";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating certificate compliance record {sl}", sl);
                ModelState.AddModelError(string.Empty, "Error updating certificate compliance record.");
                return View(model);
            }
        }

        // GET: /Legal Acts/Delete/5
        public IActionResult Delete(int sl)
        {
            var legalact= _db.SelectById<Class_certi_comp_tracker>(sl);
            if (legalact== null || legalact.del_status)
            {
                return NotFound();
            }
            return View(legalact);
        }

        // POST: /Legal Acts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int sl)
        {
            try
            {
                var legalact= _db.SelectById<Class_certi_comp_tracker>(sl);
                if (legalact== null)
                {
                    return NotFound();
                }

                legalact.del_status = true;
                _db.Update<Class_certi_comp_tracker>(legalact);
                TempData["msg"] = "Data Deleted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legalact{groupId}", sl);
                TempData["msg"] = "Error deleting group";
                return RedirectToAction(nameof(Index));
            }
        }

        private void LoadGroups()
        {
            ViewBag.group_desc = _db.SelectAll<Class_group_master>()
                .Where(t => t.del_status == false)
                .ToList();
        }

        // Same act name + same activity description = duplicate
        private bool IsDuplicate(string equipment, string certificate_no, int excludeId = 0)
        {
            var n = equipment?.Trim();
            var d = certificate_no?.Trim();
            return _db.SelectAll<Class_certi_comp_tracker>().Any(t =>
                t.del_status == false
                && t.slno != excludeId
                && string.Equals(t.equipment?.Trim(), n, StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.certificate_no?.Trim(), d, StringComparison.OrdinalIgnoreCase));
        }
        // Check duplicate Certificate Number
        private bool IsDuplicateCertificateNo(string certificateNo, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(certificateNo))
                return false;

            string certNo = certificateNo.Trim();

            return _db.SelectAll<Class_certi_comp_tracker>().Any(t =>
                t.del_status == false
                && t.slno != excludeId
                && !string.IsNullOrWhiteSpace(t.certificate_no)
                && string.Equals(
                    t.certificate_no.Trim(),
                    certNo,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }


        public IActionResult Download()
        {
            try
            {
                // Get all active certificate compliance records
                var records = _db.SelectAll<Class_certi_comp_tracker>()
                    .Where(t => t.del_status == false)
                    .OrderBy(t => t.calib_donedt)
                    .ToList();

                // Excel template path
                string templatePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "Templates",
                    "Legal Compliance Tracker.xlsx"
                );

                if (!System.IO.File.Exists(templatePath))
                {
                    return NotFound("Excel template file not found.");
                }

                using (var package = new ExcelPackage(new FileInfo(templatePath)))
                {
                    var ws = package.Workbook.Worksheets["Sheet1"];

                    if (ws == null)
                        return BadRequest("Excel worksheet not found.");

                    const int headerRow = 6;
                    const int firstDataRow = 7;
                    const int firstColumn = 2;
                    const int lastColumn = 9;

                    int templateLastRow = ws.Dimension?.End.Row ?? headerRow;

                    // Fill records
                    for (int i = 0; i < records.Count; i++)
                    {
                        int row = firstDataRow + i;

                        // Copy existing row formatting for additional records
                        if (row > templateLastRow)
                        {
                            ws.Cells[firstDataRow, firstColumn,
                                     firstDataRow, lastColumn]
                              .Copy(ws.Cells[row, firstColumn,
                                             row, lastColumn]);
                        }

                        var item = records[i];

                        // B - SL.No
                        ws.Cells[row, 2].Value = i + 1;

                        // C - Equipment Description
                        ws.Cells[row, 3].Value = item.equipment;

                        // D - Certificate Number
                        ws.Cells[row, 4].Value = item.certificate_no;

                        // E - Agency
                        ws.Cells[row, 5].Value = item.agency;

                        // F - Frequency
                        ws.Cells[row, 6].Value =
                            $"{item.freq_no} {item.freq_desc}".Trim().ToUpperInvariant();

                        // G - Calibration Done On
                        ws.Cells[row, 7].Value = ParseExcelDate(item.calib_donedt);

                        // H - Due On
                        ws.Cells[row, 8].Value = ParseExcelDate(item.calib_duedt);

                        // I - Remarks
                        ws.Cells[row, 9].Value = item.remarks;
                    }

                    // Apply borders to all populated data cells
                    if (records.Count > 0)
                    {
                        int lastDataRow = firstDataRow + records.Count - 1;

                        for (int row = firstDataRow; row <= lastDataRow; row++)
                        {
                            for (int col = firstColumn; col <= lastColumn; col++)
                            {
                                var cell = ws.Cells[row, col];

                                cell.Style.Border.Top.Style =
                                    OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                cell.Style.Border.Bottom.Style =
                                    OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                cell.Style.Border.Left.Style =
                                    OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                cell.Style.Border.Right.Style =
                                    OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            }
                        }
                    }
                    // Remove unused sample rows from the template
                    int populatedLastRow = firstDataRow + records.Count - 1;

                    if (templateLastRow >= firstDataRow &&
                        populatedLastRow < templateLastRow)
                    {
                        int deleteFromRow = Math.Max(firstDataRow, populatedLastRow + 1);

                        ws.DeleteRow(
                            deleteFromRow,
                            templateLastRow - deleteFromRow + 1
                        );
                    }

                    // Generate Excel file
                    string fileName =
                        "Certificate_Compliance_Tracker_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                        ".xlsx";

                    byte[] fileBytes = package.GetAsByteArray();

                    return File(
                        fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Certificate Compliance Tracker");

                return StatusCode(
                    500,
                    "Unable to download Certificate Compliance Tracker."
                );
            }
        }

        // Convert database date string into Excel date value
        private static object? ParseExcelDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] formats =
            {
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "MM-dd-yyyy",
        "dd/MM/yyyy",
        "MM/dd/yyyy"
    };

            if (DateTime.TryParseExact(
                    value.Trim(),
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
            {
                return date.Date;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date))
            {
                return date.Date;
            }

            return value;
        }

    }
}
