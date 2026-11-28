using MasterClass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SupermarketRepository;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
namespace calibpro.Controllers
{
    public class FactoryReportController : Controller
    {
        private readonly IConfiguration _config;
        private readonly DBClass _db;
        private readonly ILogger<FactoryReportController> _logger;
        private readonly IWebHostEnvironment _env;
        public FactoryReportController(ILogger<FactoryReportController> logger, DBClass db, IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _logger = logger;
            _db = db;
            _env = env;
        }

        public IActionResult ViewReport()
        {
            string currentFY = DateTime.Now.Month >= 4
                ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";

            var years = _db.SelectAll<Class_factory_act_hdr>()
                .Where(x => !string.IsNullOrWhiteSpace(x.fin_year))
                .Select(x => x.fin_year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            if (!years.Contains(currentFY))
            {
                years.Add(currentFY);
                years = years.OrderByDescending(x => x).ToList();
            }

            ViewBag.Years = new SelectList(years, currentFY);

            ViewBag.groups = _db.SelectAll<Class_group_master>()
                .Where(x => x.del_status == false)
                .ToList();

            // Load report data for the default financial year
            var rows = GetFactoryActReportData(currentFY, "0")
                       ?? new List<Class_factory_act_report>();

            // Pass report rows separately
            ViewBag.indexdata = rows;

            // Keep the main view model as Class_factory_act_report
            var filterModel = new Class_factory_act_report
            {
                plan_year = currentFY,
                group_slno = 0,
                status = "0"
            };

            return View(filterModel);
        }

        private List<Class_factory_act_report> GetFactoryActReportData(
    string plan_year,
    string status)
        {
            var headers = _db.SelectAll<Class_factory_act_hdr>()
                .Where(x => x.fin_year == plan_year)
                .ToList();

            var headerIds = headers
                .Select(x => x.fact_act_hdr_slno)
                .ToList();

            var children = _db.SelectAll<Class_factory_act_child>()
                .Where(x => headerIds.Contains(x.fact_act_hdr_slno))
                .ToList();

            if (!string.IsNullOrWhiteSpace(status) && status != "0")
            {
                children = children
                    .Where(x => string.Equals(
                        x.status,
                        status,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var masters = _db.SelectAll<Class_factory_act_master>()
                .Where(x => !x.del_status)
                .ToList();

            var result = (
                from child in children
                join master in masters
                    on child.factory_act_slno equals master.factory_act_slno
                join header in headers
                    on child.fact_act_hdr_slno equals header.fact_act_hdr_slno
                orderby master.factory_act_slno
                select new Class_factory_act_report
                {
                    fact_act_hdr_slno = header.fact_act_hdr_slno,
                    fact_act_chd_slno = child.fact_act_chd_slno,
                    factory_act_slno = master.factory_act_slno,

                    plan_year = header.fin_year,

                    legal_requirement = master.legal_requirement,
                    compliance_rule_act_reference = master.compliance_rule_act_reference,
                    compliance_approving_authority = master.compliance_approving_authority,
                    appointed_competent_person = master.appointed_competent_person,

                    form_no = child.form_no,
                    freq_renewal = child.freq_renewal,
                    freq = child.freq,

                    date_of_filing = child.date_of_filing,
                    valid_from = child.valid_from,
                    valid_upto = child.valid_upto,
                    next_due_date = child.next_due_date,

                    responsibility = child.responsibility,
                    responsible_person = child.responsible_person,
                    status = child.status,
                    remarks = child.remarks
                }
            ).ToList();

            return result;
        }
        [HttpPost]
        public IActionResult ViewFilteredReport(
    string plan_year,
    int group_slno,
    string status)
        {
            var rows = GetFactoryActReportData(plan_year, status);

            // Group filter will be added once its table relationship is confirmed.
            ViewBag.indexdata = rows;

            return PartialView("DisplayFactoryActReport", rows);
        }
        [HttpPost]
        public IActionResult DownloadFilteredReport(
    string plan_year,
    int group_slno,
    string status)
        {
            try
            {
                var rows = GetFactoryActReportData(plan_year, status);

                if (rows.Count == 0)
                {
                    return NotFound("No data exists for the selected filters.");
                }

                var headerIds = rows
                    .Select(x => x.fact_act_hdr_slno)
                    .Distinct()
                    .ToList();

                var headers = _db.SelectAll<Class_factory_act_hdr>()
                    .Where(x => headerIds.Contains(x.fact_act_hdr_slno))
                    .ToList();

                string formatNo = _config["FactoryActReport:FormatNo"] ?? "";
                string formatRev = _config["FactoryActReport:FormatRevNo"] ?? "";
                string formatRevDt = _config["FactoryActReport:FormatRevDate"] ?? "";

                string templatePath = Path.Combine(
                    _env.WebRootPath,
                    "templates",
                    "Factory_Act_Report.xlsx"
                );

                if (!System.IO.File.Exists(templatePath))
                {
                    return NotFound("Report template not found.");
                }

                DateTime? updatedOn = headers
                    .Select(h => h.modified_dt ?? h.created_dt)
                    .Where(d => d.HasValue)
                    .Max();

                using (var tplStream = new MemoryStream(
                    System.IO.File.ReadAllBytes(templatePath)))
                using (var package = new ExcelPackage(tplStream))
                {
                    var ws = package.Workbook.Worksheets[0];

                    ws.Cells["N1"].Value = formatNo;
                    ws.Cells["N2"].Value = $"{formatRev}, {formatRevDt}";
                    ws.Cells["N3"].Value = updatedOn?.ToString("dd.MM.yyyy") ?? "";

                    int r = 8;
                    int sn = 1;

                    foreach (var item in rows)
                    {
                        string freqRenewal = FormatFrequencyOfRenewal(
                            item.freq_renewal,
                            Convert.ToString(item.freq));

                        PutReportCell(ws, r, 1, 1, sn.ToString(), true);
                        PutReportCell(ws, r, 2, 3, item.legal_requirement, false);
                        PutReportCell(ws, r, 4, 5, item.compliance_rule_act_reference, false);
                        PutReportCell(ws, r, 6, 7, item.compliance_approving_authority, false);
                        PutReportCell(ws, r, 8, 9, item.appointed_competent_person, false);
                        PutReportCell(ws, r, 10, 11, item.form_no, true);
                        PutReportCell(ws, r, 12, 13, freqRenewal, true);
                        PutReportCell(ws, r, 14, 15, FormatReportDate(item.date_of_filing), true);
                        PutReportCell(ws, r, 16, 17, FormatReportDate(item.valid_from), true);
                        PutReportCell(ws, r, 18, 19, FormatReportDate(item.valid_upto), true);
                        PutReportCell(ws, r, 20, 21, FormatReportDate(item.next_due_date), true);
                        PutReportCell(ws, r, 22, 22, item.responsibility, true);
                        PutReportCell(ws, r, 23, 23, item.responsible_person, true);
                        PutReportCell(ws, r, 24, 24, item.status, true);
                        PutReportCell(ws, r, 25, 25, item.remarks, false);

                        int lines = new[]
                        {
                    LineCount(item.legal_requirement, 18),
                    LineCount(item.compliance_rule_act_reference, 18),
                    LineCount(item.compliance_approving_authority, 18),
                    LineCount(item.appointed_competent_person, 18),
                    LineCount(item.responsible_person, 9),
                    LineCount(item.remarks, 10)
                }.Max();

                        ws.Row(r).Height = Math.Max(24, lines * 12.5);

                        r++;
                        sn++;
                    }

                    byte[] bytes = package.GetAsByteArray();

                    string safeFY = (plan_year ?? "")
                        .Replace("/", "-")
                        .Replace("\\", "-");

                    string fileName = $"Factory_Act_Report_{safeFY}.xlsx";

                    return File(
                        bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating filtered Factory Act report");

                return StatusCode(500, "Error generating Factory Act report.");
            }
        }

        // "Months"/"Years" + 6  ->  "6 Months";  1 -> "1 Month"
        private static string FormatFrequencyOfRenewal(string unit, string freq)
        {
            unit = (unit ?? "").Trim();
            freq = (freq ?? "").Trim();

            if (unit == "" && freq == "") return "";

            if (decimal.TryParse(freq, NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
            {
                if (n == 1 && unit.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    unit = unit.Substring(0, unit.Length - 1);          // Months -> Month

                return $"{n.ToString("0.##", CultureInfo.InvariantCulture)} {unit}".Trim();
            }
            return $"{freq} {unit}".Trim();
        }

        // dates are stored as text (dd-MM-yyyy from the grid, sometimes yyyy-MM-dd) -> dd-MM-yyyy
        private static string FormatReportDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            string[] formats =
            {
                "dd-MM-yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss",
                "dd/MM/yyyy", "dd.MM.yyyy"
            };

            if (DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var d))
                return d.ToString("dd-MM-yyyy");

            return value;   // unknown format - show as stored
        }

        private static int LineCount(string text, int charsPerLine)
        {
            if (string.IsNullOrEmpty(text)) return 1;
            return text.Split('\n').Sum(p => Math.Max(1, (int)Math.Ceiling(p.Length / (double)charsPerLine)));
        }

        // writes one (optionally merged) bordered data cell in the template's style
        private static void PutReportCell(ExcelWorksheet ws, int row, int fromCol, int toCol,
                                          string text, bool center)
        {
            using (var rng = ws.Cells[row, fromCol, row, toCol])
            {
                if (toCol > fromCol) rng.Merge = true;

                rng.Style.Font.Name = "Calibri";
                rng.Style.Font.Size = 9;
                rng.Style.HorizontalAlignment = center ? ExcelHorizontalAlignment.Center
                                                       : ExcelHorizontalAlignment.Left;
                rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                rng.Style.WrapText = true;
                rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            ws.Cells[row, fromCol].Value = text ?? "";
        }

    }
}
