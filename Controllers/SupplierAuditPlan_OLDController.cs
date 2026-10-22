using CommonClassLibrary;
using ElmahCore;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using NPoco;
using Polly;
using SupermarketRepository;
using System.Diagnostics;
using System.Text;

namespace ams.Controllers
{
    public class SupplierAuditPlan_OLDController : Controller
    {
        public readonly ILogger<SupplierAuditPlan_OLDController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        public readonly DBClass _crudRepository;
        private readonly MasterCrudClass _masterCrudClass;
        private readonly IConfiguration apppath;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public int plantslno = 0;
        public SupplierAuditPlan_OLDController(ILogger<SupplierAuditPlan_OLDController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass, IConfiguration _config, IWebHostEnvironment webHostEnvironment)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            _crudRepository = _db;
            _crudRepository.LogLastCommand = true;
            _crudRepository.GetCommand += Dbc_GetCommand;
            _crudRepository.MailSendingRequested += Dbc_MailSendingRequested;
            _crudRepository.UseExperimentalFeature = true;
            _masterCrudClass = masterClass;
            apppath = _config;
            _webHostEnvironment = webHostEnvironment;
        }
        private void Dbc_MailSendingRequested(object? sender, bool e)
        {
            if (e)
            {
                logger.LogInformation("Mail Sending event triggered");
            }
        }
        private void Dbc_GetCommand(object? sender, DbCommandEventArgs e)
        {
            logger.LogInformation(e.CommandText);
        }

        // GET: SupplierAuditPlan_OLDController
        public ActionResult Index()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            // Check if the session value is null or empty
            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string sql = $@"select distinct plan_year,plant_slno,submit_status from supplier_audit_plan where plant_slno='{plantslno}'";
            var lst = _crudRepository.SelectBySQL<Class_supplier_audit_plan_old>(sql);
            TempData.Remove("msg");
            return View(lst);
        }
       
        // GET: SupplierAuditPlan_OLDController/Create
        public ActionResult Create()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            // Check if the session value is null or empty
            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }
            Class_supplier_audit_plan_old cls=new Class_supplier_audit_plan_old();
            return View(cls);
        }

        [HttpPost]
        public ActionResult Create(IFormCollection collection, string planyear)
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            try
            {
                string plandata = collection["plandata"];
                List<Class_supplier_audit_plan_old> lst = JsonConvert.DeserializeObject<List<Class_supplier_audit_plan_old>>(plandata);

                if (lst != null && lst.Count > 0)
                {
                    foreach (Class_supplier_audit_plan_old c in lst)
                    {
                        Class_supplier_audit_plan_old newcls = new Class_supplier_audit_plan_old();
                        newcls.plan_year = planyear;
                        newcls.supp_name = c.supp_name;
                        newcls.jan_feb_mar = c.jan_feb_mar;
                        newcls.apr_may_jun = c.apr_may_jun;
                        newcls.jul_aug_sep = c.jul_aug_sep;
                        newcls.oct_nov_dec = c.oct_nov_dec;
                        newcls.plant_slno = plantslno;
                        newcls.submit_status = c.submit_status;
                        if (c.plan_slno > 0)
                        {
                            newcls.plan_slno = c.plan_slno;
                            newcls.plant_slno = c.plant_slno;
                            _crudRepository.Update<Class_supplier_audit_plan_old>(newcls);
                        }
                        else
                        {
                            _crudRepository.AddNew<Class_supplier_audit_plan_old>(newcls);
                        }

                    }
                }
                return Json(new { status = "Submitted", planYear = planyear, plantSlno= plantslno });
            }
            catch
            {
                return View();
            }
        }
        // GET: SupplierAuditPlan_OLDController/Edit/5
        public ActionResult Edit(string? plan_year,int plantslno)
        {
            string sql = $@"select * from supplier_audit_plan where plant_slno='{plantslno}' and plan_year='{plan_year}'";
            var lst = _crudRepository.SelectBySQL<Class_supplier_audit_plan_old>(sql).ToList();
            if (lst == null || !lst.Any())
            {
                // Handle the case when no data is found
                TempData["msg"] = "No audit plan found for the given year and plant.";
                return RedirectToAction("Index"); // Redirect to a safe page, e.g., Index
            }
            return View(lst[0]);
        }

        // POST: SupplierAuditPlan_OLDController/Edit/5
        [HttpPost]        
        public ActionResult Edit(IFormCollection collection, string planyear)
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            try
            {
                string plandata = collection["plandata"];
                List<Class_supplier_audit_plan_old> lst = JsonConvert.DeserializeObject<List<Class_supplier_audit_plan_old>>(plandata);

                if (lst != null && lst.Count > 0)
                {
                    foreach (Class_supplier_audit_plan_old c in lst)
                    {
                        Class_supplier_audit_plan_old newcls = new Class_supplier_audit_plan_old();
                        newcls.plan_year = planyear;
                        newcls.supp_name = c.supp_name;
                        newcls.jan_feb_mar = c.jan_feb_mar;
                        newcls.apr_may_jun = c.apr_may_jun;
                        newcls.jul_aug_sep = c.jul_aug_sep;
                        newcls.oct_nov_dec = c.oct_nov_dec;
                        newcls.plant_slno = plantslno;
                        newcls.submit_status = c.submit_status;
                        if (c.plan_slno > 0)
                        {
                            newcls.plan_slno = c.plan_slno;
                            newcls.plant_slno = c.plant_slno;
                            _crudRepository.Update<Class_supplier_audit_plan_old>(newcls);
                        }
                        else
                        {
                            _crudRepository.AddNew<Class_supplier_audit_plan_old>(newcls);
                        }

                    }
                }
                return Json(new { status = "Submitted", planYear = planyear, plantSlno = plantslno });
            }
            catch
            {
                return View();
            }
        }
        public string Getplans(string? planyr="0")
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            // Check if the session value is null or empty
            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string qry = string.Empty;
            
            qry = "SELECT * FROM supplier_audit_plan  where plant_slno=@0 and plan_year=@1";
                
           
            var lst = _crudRepository.SelectBySQL<Class_supplier_audit_plan_old>(qry, plantslno, planyr).ToList();
            return JsonConvert.SerializeObject(lst);
        }

        public string GetSupHot()
        {
            string qry = $@"select distinct CustomerName as supp_name from acpl_common.dbo.customer_supplier 
                    where BPType='Supplier' and status='Active'";
            var lst = _crudRepository.SelectBySQL<Class_supplier_audit_plan_old>(qry).ToList();
            return JsonConvert.SerializeObject(lst);
        }

        [HttpPost]
        public IEnumerable<Class_supplier_audit_plan_old> Delete(string id,int plantslno, IFormCollection collection)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    string plantSlnoString = HttpContext.Session.GetString("plantslno");

                    // Check if the session value is null or empty
                    if (!string.IsNullOrEmpty(plantSlnoString))
                    {
                        plantslno = int.Parse(plantSlnoString);
                    }
                    string sql = $@"delete from supplier_audit_plan where plan_year='{id}' and plant_slno={plantslno}";
                    var del = _crudRepository.Execute<Class_supplier_audit_plan_old>(sql);
                    TempData["msg"] = "Data Deleted";
                    string lstsql = $@"select distinct plan_year,plant_slno from supplier_audit_plan where plant_slno='{plantslno}'";
                    var lst = _crudRepository.SelectBySQL<Class_supplier_audit_plan_old>(lstsql);
                    return lst;
                }
                else
                {
                    // return null
                    return null;

                }
            }
            catch (Exception ex)
            {
                ElmahCore.ElmahExtensions.RaiseError(ex);
                ModelState.AddModelError(string.Empty, ex.Message.ToString());
                return null;
            }
        }

    }
}
