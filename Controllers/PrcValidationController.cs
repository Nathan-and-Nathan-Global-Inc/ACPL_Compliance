
using CommonClassLibrary;
using ElmahCore;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Experimental.FileAccess;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using NPoco;
using prvalid.Models;
using SupermarketRepository;
using System.Reflection;

namespace prvalid.Controllers
{
    public class PrcValidationController : Controller
    {
        public readonly DBClass _crudRepository;
        public readonly ILogger<PrcValidationController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        private readonly MasterCrudClass _masterCrudClass;
        public PrcValidationController(ILogger<PrcValidationController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass)
        {
            logger = _logger;
            masterlogger = _masterlogger;
            _crudRepository = _db;
            _crudRepository.LogLastCommand = true;
            _crudRepository.GetCommand += Dbc_GetCommand;
            _crudRepository.MailSendingRequested += Dbc_MailSendingRequested;
            _masterCrudClass = masterClass;
            _crudRepository.UseExperimentalFeature = true;
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

        // GET: PrcValidationController
        public ActionResult Index()
        {
            string sql = $@"select 
vh.proc_slno,pr.proc_code,pr.proc_name,vh.proc_slno,vh.fg_part_slno,
pt.ItemName from validation_hdr vh
left outer join acpl_common.dbo.process pr on vh.proc_slno=pr.proc_slno
left outer join acpl_sap_dqms.dbo.dqms_fg_parts pt on pt.fg_slno=vh.fg_part_slno";
            var lst = _crudRepository.SelectBySQL<Class_validation_hdr>(sql);
            return View(lst);
        }

        // GET: PrcValidationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PrcValidationController/Create
        public ActionResult Create()
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees cls = JsonConvert.DeserializeObject<Class_employees>(st);
            string sql = $@"select  * from acpl_sap_dqms.dbo.dqms_fg_parts";
            var lst = _crudRepository.SelectBySQL<Class_dqms_fg_parts>(sql);
            ViewBag.parts = lst;
            string sql2 = $@"SELECT * from acpl_common..process";
            var lst2 = _crudRepository.SelectBySQL<Class_process>(sql2);
            ViewBag.process = lst2;
            string sql3 = $@"SELECT * from acpl_common..machines";
            var lst3 = _crudRepository.SelectBySQL<Class_machines>(sql3);
            ViewBag.machines = lst3;

            ViewBag.EmplName = cls.empl_name;
            ViewBag.EmplSlno = cls.empl_slno;
            return View();
        }

        // POST: PrcValidationController/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                var mdata = collection["mdata"];
                var data1 = collection["fdata"];
                var data2 = collection["f2data"];

                Class_validation_hdr cls = JsonConvert.DeserializeObject<Class_validation_hdr>(mdata);
                List<Class_validation_proc_char> lstchild1 = JsonConvert.DeserializeObject<List<Class_validation_proc_char>>(data1);
                List<Class_validation_pdt_char> lstchild2 = JsonConvert.DeserializeObject<List<Class_validation_pdt_char>>(data2);

                var hdr_slno = _crudRepository.AddNew<Class_validation_hdr>(cls);
                Class_validation_trials clsnew = new Class_validation_trials();
                clsnew.hdr_slno = Convert.ToInt32(hdr_slno);
                clsnew.trial_inference = cls.trial_inference;
                clsnew.trial_selected = cls.trial_selected ?? false;
                clsnew.trial_no = cls.trial_no;
                if (clsnew.trial_selected == true)
                    clsnew.selected_date = DateTime.Now.ToString("dd/MM/yyyy");
                var trialid = _crudRepository.AddNew<Class_validation_trials>(clsnew);
                if (lstchild1 != null && lstchild1.Count > 0)
                {
                    foreach (Class_validation_proc_char c in lstchild1)
                    {
                        Class_validation_proc_char newcls = new Class_validation_proc_char();
                        newcls.trial_slno = Convert.ToInt32(trialid);
                        newcls.hdr_slno = Convert.ToInt32(hdr_slno);
                        newcls.proc_char_desc = c.proc_char_desc;
                        newcls.is_attribute = c.is_attribute;
                        newcls.nominal_val = c.nominal_val;
                        newcls.specifications = c.specifications;
                        newcls.min_val = c.min_val;
                        newcls.max_val = c.max_val;
                        _crudRepository.AddNew<Class_validation_proc_char>(newcls);
                    }
                }
                if (lstchild2 != null && lstchild2.Count > 0)
                {
                    foreach (Class_validation_pdt_char c in lstchild2)
                    {
                        Class_validation_pdt_char newcls = new Class_validation_pdt_char();
                        newcls.trial_slno = Convert.ToInt32(trialid);
                        newcls.hdr_slno = Convert.ToInt32(hdr_slno);
                        newcls.pdt_char_desc = c.pdt_char_desc;
                        newcls.is_attribute = c.is_attribute;
                        newcls.nominal_val = c.nominal_val;
                        newcls.specifications = c.specifications;
                        newcls.min_val = c.min_val;
                        newcls.max_val = c.max_val;
                        newcls.obs_1 = c.obs_1;
                        newcls.obs_2 = c.obs_2;
                        newcls.obs_3 = c.obs_3;
                        newcls.obs_4 = c.obs_4;
                        newcls.obs_5 = c.obs_5;
                        _crudRepository.AddNew<Class_validation_pdt_char>(newcls);
                    }
                }
                TempData["msg"] = "Data Submitted";
                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                // Handle error and log it
                ElmahCore.ElmahExtensions.RaiseError(ex);
                throw;
            }
        }

        // GET: PrcValidationController/Edit/5
        public ActionResult Edit(int id)
        {
            var st = HttpContext.Session.GetString("User");
            Class_employees cls = JsonConvert.DeserializeObject<Class_employees>(st);
            string sql = $@"select  * from acpl_sap_dqms.dbo.dqms_fg_parts";
            var lst = _crudRepository.SelectBySQL<Class_dqms_fg_parts>(sql);
            ViewBag.parts = lst;
            string sql2 = $@"SELECT * from acpl_common..process";
            var lst2 = _crudRepository.SelectBySQL<Class_process>(sql2);
            ViewBag.process = lst2;
            string sql3 = $@"SELECT * from acpl_common..machines";
            var lst3 = _crudRepository.SelectBySQL<Class_machines>(sql3);
            ViewBag.machines = lst3;

            ViewBag.EmplName = cls.empl_name;
            ViewBag.EmplSlno = cls.empl_slno;
            string sqltrails = $@"select * from validation_hdr vh
left outer join validation_trials vt on vh.hdr_slno=vt.hdr_slno
where vh.hdr_slno={id}";
            var clsval = _crudRepository.SelectBySQL<Class_validation_hdr>(sqltrails).ToList();
            var trialSlnoList = GetTrials(clsval[0].fg_part_slno ?? 0, clsval[0].proc_slno ?? 0, id);
            ViewBag.TrialSlnoList = trialSlnoList;
            var clstrial = GetmaxTrial(id.ToString());
            ViewBag.trialno = clstrial;
            //string sqlTno = $@"select max(trial_no)+1 as trial_no from validation_trials where hdr_slno={id}";
            //var trialno = _crudRepository.SelectBySQL<Class_validation_trials>(sqlTno).ToList();
            //ViewBag.trialno = trialno[0].trial_no;
            return View(clsval[0]);
        }

        // POST: PrcValidationController/Edit/5
        [HttpPost]
        public ActionResult Edit(IFormCollection collection)
        {
            try
            {
                var mdata = collection["mdata"];
                var data1 = collection["fdata"];
                var data2 = collection["f2data"];

                Class_validation_hdr cls = JsonConvert.DeserializeObject<Class_validation_hdr>(mdata);
                List<Class_validation_proc_char> lstchild1 = JsonConvert.DeserializeObject<List<Class_validation_proc_char>>(data1);
                List<Class_validation_pdt_char> lstchild2 = JsonConvert.DeserializeObject<List<Class_validation_pdt_char>>(data2);

                _crudRepository.Update<Class_validation_hdr>(cls);
                Class_validation_trials clsnew = new Class_validation_trials();
                clsnew.hdr_slno = Convert.ToInt32(cls.hdr_slno);
                clsnew.trial_inference = cls.trial_inference;
                clsnew.trial_selected = cls.trial_selected ?? false;
                clsnew.trial_slno = Convert.ToInt16(cls.trial_slno);
                clsnew.trial_no = cls.trial_no;
                if (clsnew.trial_selected == true)
                    clsnew.selected_date = DateTime.Now.ToString("dd/MM/yyyy");
                string sql3 = $@"select trial_no from validation_trials  where trial_slno={clsnew.trial_slno}";
                var trial_no = _crudRepository.SelectBySQL<Class_validation_trials>(sql3).ToList();
                string trialid = "0";
                if (trial_no[0].trial_no == cls.trial_no)
                {
                    _crudRepository.Update<Class_validation_trials>(clsnew);
                    trialid = Convert.ToInt16(clsnew.trial_slno).ToString();
                }
                else
                {
                    var trialnum = _crudRepository.AddNew<Class_validation_trials>(clsnew);
                    trialid = trialnum.ToString();
                }
                //string sql = $@"delete validation_proc_char where hdr_slno={cls.hdr_slno}";
                //_crudRepository.Execute<Class_validation_proc_char>(sql);
                //string sql2 = $@"delete validation_pdt_char where hdr_slno={cls.hdr_slno}";
                //_crudRepository.Execute<Class_validation_pdt_char>(sql2);
                if (lstchild1 != null && lstchild1.Count > 0)
                {
                    foreach (Class_validation_proc_char c in lstchild1)
                    {
                        Class_validation_proc_char newcls = new Class_validation_proc_char();
                        newcls.trial_slno = Convert.ToInt32(trialid);
                        newcls.hdr_slno = Convert.ToInt32(cls.hdr_slno);
                        newcls.proc_char_desc = c.proc_char_desc;
                        newcls.is_attribute = c.is_attribute;
                        newcls.nominal_val = c.nominal_val;
                        newcls.specifications = c.specifications;
                        newcls.min_val = c.min_val;
                        newcls.max_val = c.max_val;
                        if (c.procslno > 0)
                            _crudRepository.Update<Class_validation_proc_char>(newcls);
                        else
                            _crudRepository.AddNew<Class_validation_proc_char>(newcls);
                    }
                }
                if (lstchild2 != null && lstchild2.Count > 0)
                {
                    foreach (Class_validation_pdt_char c in lstchild2)
                    {
                        Class_validation_pdt_char newcls = new Class_validation_pdt_char();
                        newcls.trial_slno = Convert.ToInt32(trialid);
                        newcls.hdr_slno = Convert.ToInt32(cls.hdr_slno);
                        newcls.pdt_char_desc = c.pdt_char_desc;
                        newcls.is_attribute = c.is_attribute;
                        newcls.nominal_val = c.nominal_val;
                        newcls.specifications = c.specifications;
                        newcls.min_val = c.min_val;
                        newcls.max_val = c.max_val;
                        newcls.obs_1 = c.obs_1;
                        newcls.obs_2 = c.obs_2;
                        newcls.obs_3 = c.obs_3;
                        newcls.obs_4 = c.obs_4;
                        newcls.obs_5 = c.obs_5;
                        if (c.pdtslno > 0)
                            _crudRepository.Update<Class_validation_pdt_char>(newcls);
                        else
                            _crudRepository.AddNew<Class_validation_pdt_char>(newcls);
                    }
                }
                TempData["msg"] = "Data Submitted";
                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                // Handle error and log it
                ElmahCore.ElmahExtensions.RaiseError(ex);
                throw;
            }
        }
        // GET: PrcValidationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PrcValidationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Handle error and log it
                ElmahCore.ElmahExtensions.RaiseError(ex);
                throw;
            }
        }
        public List<Class_control_plan_child> GetProcess(int partSlno, int procSlno, int machineSlno)
        {
            string sql = $@"
select * from acpl_apqp..control_plan cp
inner join acpl_apqp.dbo.control_plan_child cpc on cp.cp_slno=cpc.cp_slno where cp.part_slno={partSlno}
and cp.proc_slno={procSlno}
and cp.machine_slno={machineSlno}";
            var lst = _crudRepository.SelectBySQL<Class_control_plan_child>(sql).ToList();
            if (lst == null || !lst.Any() || lst.Count < 0)
            {
                return null;
            }
            return lst;
        }
        public List<Class_validation_trials> GetTrials(int partSlno, int procSlno, int hdr_slno)
        {
            string sql = $@"SELECT vt.trial_no
        FROM validation_trials vt
        LEFT OUTER JOIN validation_hdr vh ON vh.hdr_slno = vt.hdr_slno
        WHERE vt.hdr_slno = {hdr_slno}
          AND vh.proc_slno = {procSlno}
          AND vh.fg_part_slno = {partSlno}";
            List<Class_validation_trials> lst = _crudRepository.SelectBySQL<Class_validation_trials>(sql).ToList();
            return lst;
        }
        public IActionResult callEdit(int partSlno, int procSlno, int machineSlno)
        {
            string sql = $@"SELECT *
        FROM validation_hdr vh
        WHERE vh.proc_slno = {procSlno}
          AND vh.machine_slno = {machineSlno} 
          AND vh.fg_part_slno = {partSlno}";
            List<Class_validation_hdr> lst = _crudRepository.SelectBySQL<Class_validation_hdr>(sql).ToList();
            if (lst.Count == 0)
                return Json(new { redirectToEdit = (string)null });  // No redirect if not found
            else
                return Json(new { redirectToEdit = Url.Action("Edit", new { id = lst[0].hdr_slno }) });
        }
        public JsonResult GetPrevData(string trial_no, String hdr_slno)
        {
            string sql = $@"select * from validation_hdr vh
left outer join validation_trials vt on vh.hdr_slno=vt.hdr_slno
where vh.hdr_slno={hdr_slno} and vt.trial_no={trial_no}";
            var trials = _crudRepository.SelectBySQL<Class_validation_hdr>(sql).ToList();

            string sql2 = $@"select pd.* from validation_pdt_char pd
left outer join validation_trials vt on  pd.trial_slno=vt.trial_slno
where pd.hdr_slno={hdr_slno} and vt.trial_no={trial_no}";
            var product = _crudRepository.SelectBySQL<Class_validation_pdt_char>(sql2);

            string sql3 = $@"select pd.* from validation_proc_char pd
left outer join validation_trials vt on  pd.trial_slno=vt.trial_slno
where pd.hdr_slno={hdr_slno} and vt.trial_no={trial_no}";
            var process = _crudRepository.SelectBySQL<Class_validation_proc_char>(sql3);
            return Json(new { trials = trials, product = product, process = process });
        }
        public string GetmaxTrial( string id)
        {
            string sqlTno = $@"select max(trial_no)+1 as trial_no from validation_trials where hdr_slno={id}";
            var trialno = _crudRepository.SelectBySQL<Class_validation_trials>(sqlTno).ToList();
            //ViewBag.trialno = trialno[0].trial_no;
            return trialno[0].trial_no;
        }
        public JsonResult GetBestTrial(string hdr_slno)
        {
            string sqlTno = $@"select * from validation_trials where hdr_slno={hdr_slno} and trial_selected='true'";
            var trialno = _crudRepository.SelectBySQL<Class_validation_trials>(sqlTno).ToList();
            if(!trialno.Any())
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }
    }
}
