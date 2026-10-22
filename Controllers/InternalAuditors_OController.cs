using CommonClassLibrary;
using ElmahCore;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NPoco;
using Polly;
using SupermarketRepository;
using System.Diagnostics;
using System.Text;


namespace ams.Controllers
{
    public class InternalAuditors_OController : Controller
    {
        public readonly DBClass _crudRepository;
        public readonly ILogger<InternalAuditors_OController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        public int plantslno = 0;
        private readonly MasterCrudClass _masterCrudClass;
        public InternalAuditors_OController(ILogger<InternalAuditors_OController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            _crudRepository = _db;
            _crudRepository.LogLastCommand = true;
            _crudRepository.GetCommand += Dbc_GetCommand;
            _crudRepository.MailSendingRequested += Dbc_MailSendingRequested;
            _crudRepository.UseExperimentalFeature = true;
            _masterCrudClass = masterClass;
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
        // GET: InternalAuditors_OController
        public ActionResult Index()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }
            string sql = $@"select em.*,e.empl_name,dp.dept_name,ds.desig_name from internal_auditors em
left outer join acpl_common.dbo.employees e on em.empl_slno=e.empl_slno
left outer join acpl_common.dbo.department dp on em.dept_slno=dp.dept_slno
left outer join acpl_common.dbo.designation ds on em.desig_slno=ds.desig_slno
where em.del_status='false' and em.plant_slno={plantslno}";
            var lst = _crudRepository.SelectBySQL<Class_internal_auditors>(sql);
            return View(lst);
        }

        // GET: InternalAuditors_OController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: InternalAuditors_OController/Create
        public ActionResult Create()
        {
            ViewBag.empl=GetEmplDetails();
            //ViewBag.system = GetSystem();
            GetSystemList("");
            return View();
        }


        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                string plantSlnoString = HttpContext.Session.GetString("plantslno");

                if (!string.IsNullOrEmpty(plantSlnoString))
                {
                    plantslno = int.Parse(plantSlnoString);
                }
                string formdata = collection["masterdata"];
                Class_internal_auditors cls = JsonConvert.DeserializeObject<Class_internal_auditors>(formdata);
                var lst = GetEmplDetails(cls.empl_slno);
                cls.dept_slno = lst[0].dept_slno;
                cls.desig_slno = lst[0].desig_slno;
                cls.plant_slno = plantslno;
                cls.del_status = false;
                _crudRepository.AddNew(cls);
                return Json(new { status = "success" });

            }
            catch (Exception ex)
            {
                ElmahCore.ElmahExtensions.RaiseError(ex);
                ModelState.AddModelError(string.Empty, ex.Message.ToString());
                return null;
            }
        }
        // GET: InternalAuditors_OController/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.empl = GetEmplDetails();
            //ViewBag.system = GetSystem();
            Class_internal_auditors cls=_crudRepository.SelectById<Class_internal_auditors>(id);
            List<Class_internal_auditors> lstsys = _crudRepository.SelectAll<Class_internal_auditors>().Where(x => x.intl_slno == id).ToList();
            var systemValues = lstsys.Select(x => x.system_desc);

            // Convert to a comma-separated string
            string commaSeparatedValues = string.Join(",", systemValues);

            GetSystemList(commaSeparatedValues);
            return View(cls);
        }

      
        [HttpPost]
        public ActionResult Edit(IFormCollection collection)
        {
            try
            {
                string formdata = collection["masterdata"];
                Class_internal_auditors cls = JsonConvert.DeserializeObject<Class_internal_auditors>(formdata);
                Class_internal_auditors clsnew = _crudRepository.SelectById<Class_internal_auditors>(cls.intl_slno);
                clsnew.empl_slno = cls.empl_slno;
                clsnew.system_desc = cls.system_desc;
                clsnew.dept_slno = cls.dept_slno;
                clsnew.desig_slno = cls.desig_slno;
                clsnew.plant_slno = cls.plant_slno;

                clsnew.qualification = cls.qualification;
                clsnew.experience_years = cls.experience_years;
                clsnew.trained_dates = cls.trained_dates;
                clsnew.trained_by = cls.trained_by;
                clsnew.del_status = false;
                clsnew.intl_slno = cls.intl_slno;
                _crudRepository.Update(clsnew);
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                ElmahCore.ElmahExtensions.RaiseError(ex);
                ModelState.AddModelError(string.Empty, ex.Message.ToString());
                return null;
            }
        }
        // GET: InternalAuditors_OController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InternalAuditors_OController/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                Class_internal_auditors cls = _crudRepository.SelectById<Class_internal_auditors>(id);
                cls.del_status = true;
                _crudRepository.Update<Class_internal_auditors>(cls);
                //TempData["msg"] = "Data Deleted";
                return Json(new { status = "success" });
            }
            catch
            {
                return View();
            }
        }
        public List<Class_employees> GetEmplDetails()
        {
            string sql = "SELECT * FROM acpl_common.dbo.employees where del_status='false'";
            var lst=_crudRepository.SelectBySQL<Class_employees>(sql).ToList();
            return lst;
        }
        public List<Class_employees> GetEmplDetails(int empl_slno)
        {
            string sql = "SELECT * FROM acpl_common.dbo.employees where empl_slno="+ empl_slno;
            var lst = _crudRepository.SelectBySQL<Class_employees>(sql).ToList();
            return lst;
        }

        public List<Class_audit_systems> GetSystem()
        {
            var lst = _crudRepository.SelectAll<Class_audit_systems>().Where(x=>x.del_status==false).ToList();
            return lst;
        }

        [HttpGet]
        public JsonResult CheckDuplicateEmpl(int empl_slno)
        {
            try
            {
                bool exists = CheckIfEmployeeExists(empl_slno);

                // Return the result as JSON
                return Json(new { exists = exists });
            }
            catch (Exception ex)
            {
                // Handle error and log it
                ElmahExtensions.RaiseError(ex);
                return Json(new { exists = false }); // Return false in case of an error
            }
        }
        public bool CheckIfEmployeeExists(int empl_slno)
        {
            // Check if there's already a record with the same empl_slno and del_status = false
            var exists = _crudRepository.SelectAll<Class_internal_auditors>()
                .Any(x => x.empl_slno == empl_slno && x.del_status == false);

            return exists;
        }

        public ActionResult GetSystemList(string stringList)
        {
            
                List<string> lstselected = (stringList ?? "").Split(',').Select(x => x.Trim()).ToList();
          
                List<Class_audit_systems> lst = _crudRepository.SelectAll<Class_audit_systems>().Where(x => x.del_status == false).ToList();

                var selectAllItem = new SelectListItem
                {
                    Value = "select all", // You can use a specific value that represents "Select All"
                    Text = "Select all",
                    Selected = lstselected.Contains("0")
                };

                List<SelectListItem> selectListItems = lst.Select(x => new SelectListItem
                {
                    Value = x.system_desc,
                    Text = x.system_desc,
                }).ToList();

                selectListItems.Insert(0, selectAllItem);

                List<string> selectedValues = lstselected; // Change this according to your model structure

                foreach (var item in selectListItems)
                {
                    if (selectedValues != null && selectedValues.Contains(item.Value))
                    {
                        item.Selected = true;
                    }
                }

                // Store the prepared select list in ViewBag or pass it to the view model
                ViewBag.systemlst = selectListItems;
                return View();
            
        }
    }
}
