using CommonClassLibrary;
using ElmahCore;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPoco;
using Polly;
using SupermarketRepository;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;

namespace ams.Controllers
{
    public class CustomerClosureController : Controller
    {
        public readonly ILogger<CustomerClosureController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        public readonly DBClass _crudRepository;
        private readonly MasterCrudClass _masterCrudClass;
        private readonly IConfiguration apppath;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public int plantslno = 0;
        public CustomerClosureController(ILogger<CustomerClosureController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass, IConfiguration _config, IWebHostEnvironment webHostEnvironment)
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
        // GET: CustomerClosureController
        public ActionResult PendingAnalysis()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string sql = $@"select distinct hdr.audit_number, FORMAT(CAST(audit_date AS DATE), 'dd-MM-yyyy') AS audit_date, cust_slno, customercode, customername, 
                cust_audit_number,
                cust_partno_revno, auditor, e.empl_name as auditee_name,
                rca.rca_submitted_status,chd.obs_slno,chd.nc_observation,chd.nc_type,resp_slno,rca.why1,'Closure' as next_action
                from customer_audit_hdr hdr
                left join acpl_common.dbo.employees e on e.empl_slno=hdr.auditee_slno 
                inner join customer_audit_child1_observation chd on chd.audit_number=hdr.audit_number
                INNER JOIN customer_audit_child2_rca rca on rca.audit_number=hdr.audit_number and chd.obs_slno=rca.obs_slno
                inner join customer_audit_child5_verification att on att.audit_number=hdr.audit_number and att.obs_slno=rca.obs_slno
                where att_submitted_status='Y' and rca.obs_slno not in (select obs_slno from customer_audit_child6_closure)";
            var lst = _crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql);
            TempData.Remove("msg");
            return View(lst);
        }

        public ActionResult Index()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string sql = $@"select distinct hdr.audit_number, FORMAT(CAST(audit_date AS DATE), 'dd-MM-yyyy') AS audit_date, cust_slno, customercode, customername, 
cust_audit_number,
cust_partno_revno, auditor, e.empl_name as auditee_name,
rca.rca_submitted_status,chd.obs_slno,chd.nc_observation,chd.nc_type,resp_slno,rca.why1,att.upload1, 
FORMAT(CAST(cls.closure_dt AS DATE), 'dd-MM-yyyy') as closed_date,
case when cls.closure_submitted_status='Y' then 'Closed' else 'Closure' end as audit_status
from customer_audit_hdr hdr
left join acpl_common.dbo.employees e on e.empl_slno=hdr.auditee_slno 
inner join customer_audit_child1_observation chd on chd.audit_number=hdr.audit_number
INNER JOIN customer_audit_child2_rca rca on rca.audit_number=hdr.audit_number and chd.obs_slno=rca.obs_slno
inner join customer_audit_child5_verification att on att.audit_number=hdr.audit_number and att.obs_slno=rca.obs_slno
inner join customer_audit_child6_closure cls on cls.audit_number=hdr.audit_number and cls.obs_slno=rca.obs_slno
where att_submitted_status='Y'";
            var lst = _crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql);
            TempData.Remove("msg");
            return View(lst);
        }

        // GET: CustomerClosureController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CustomerClosureController/Create
        public ActionResult Create(int id)
        {
            string sql = $@"select hdr.audit_number, audit_date, cust_slno, customercode, customername, 
cust_location, cust_application, cust_audit_number, audit_subject, 
cust_partno_revno, auditor, e.empl_name as auditee_name,chd.obs_slno,nc_observation,nc_type,resp_slno,why1,upload1
from customer_audit_hdr hdr
inner join customer_audit_child1_observation chd on chd.audit_number=hdr.audit_number
left join acpl_common.dbo.employees e on e.empl_slno=hdr.auditee_slno
inner join customer_audit_child2_rca rca on chd.obs_slno=rca.obs_slno
inner join customer_audit_child5_verification att on att.audit_number=rca.audit_number and att.obs_slno=rca.obs_slno where att.obs_slno={id}";
            Class_customer_audit_hdr cls = _crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql).FirstOrDefault();
            ViewBag.AuditDtls = cls;
            return View(cls);
        }

        // POST: CustomerClosureController/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection, string? auditnum, string? obsslno)
        {
            try
            {
                string userid = collection["userid"];
                string headerdata = collection["headerdata"];
                var hdata = JsonConvert.DeserializeObject<List<string>>(headerdata);
                Class_customer_audit_child6_closure c = new Class_customer_audit_child6_closure();
                
                c.audit_number = Convert.ToInt32(auditnum);
                c.nc_status= hdata[0].ToString();
                c.closure_dt = hdata[1].ToString();
                c.closure_submitted_status = "Y";
                c.closure_submitted_by = userid;
                c.obs_slno = Convert.ToInt32(obsslno);
                c.closure_submitted_dt = DateTime.Now.ToString("yyyy-MM-dd");
                _crudRepository.AddNew<Class_customer_audit_child6_closure>(c);

                string sql = $@"select count(obs_slno) from customer_audit_child1_observation 
                    where obs_slno not in (select obs_slno from customer_audit_child6_closure where audit_number={auditnum}) 
                    and audit_number={auditnum}";
                var cnt = _crudRepository.ExecuteScalar<Class_customer_audit_child1_observation>(sql);

                if (int.Parse(cnt.ToString())==0)
                {
                    Class_customer_audit_hdr hdr = _crudRepository.SelectById<Class_customer_audit_hdr>(Convert.ToInt32(auditnum));
                    hdr.audit_status = hdata[0].ToString();
                    hdr.closed_date = hdata[1].ToString();
                    _crudRepository.Update<Class_customer_audit_hdr>(hdr);
                }

                
                return Json(new { status = "Submitted" });
            }
            catch
            {
                return View();
            }
        }

        // GET: CustomerClosureController/Edit/5
        public ActionResult Edit(int id)
        {
            string sql = $@"select hdr.audit_number, audit_date, cust_slno, customercode, customername, 
cust_location, cust_application, cust_audit_number, audit_subject,audit_status, FORMAT(CAST(closed_date AS DATE), 'dd-MM-yyyy') as closed_date,
cust_partno_revno, auditor, e.empl_name as auditee_name,chd.obs_slno,nc_observation,nc_type,resp_slno,why1,upload1
from customer_audit_hdr hdr
inner join customer_audit_child1_observation chd on chd.audit_number=hdr.audit_number
left join acpl_common.dbo.employees e on e.empl_slno=hdr.auditee_slno
inner join customer_audit_child2_rca rca on chd.obs_slno=rca.obs_slno and hdr.audit_number=rca.audit_number
inner join customer_audit_child5_verification att on att.audit_number=rca.audit_number and att.obs_slno=rca.obs_slno 
where att.obs_slno={id}";
            Class_customer_audit_hdr cls = _crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql).FirstOrDefault();
            ViewBag.AuditDtls = cls;
            return View(cls);
        }

        
        // GET: CustomerClosureController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CustomerClosureController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public string GetAuditfindingsId(long slno)
        {
            string sql = $@"select audit_number,part_slno, audit_date, cust_slno, customercode, customername, 
cust_location, cust_application, cust_audit_number, audit_subject, 
cust_partno_revno, auditor,auditee_slno as empl_slno, proj_ref, file_upload1, 
audit_status, closed_date, hdr.plant_slno, created_by, created_dt, modified_by, modified_dt
from customer_audit_hdr hdr
left join acpl_common.dbo.employees e on e.empl_slno=hdr.auditee_slno where audit_number={slno}";
            //Class_customer_audit_hdr cls = _crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql).FirstOrDefault();
            String ret = JsonConvert.SerializeObject(_crudRepository.SelectBySQL<Class_customer_audit_hdr>(sql).FirstOrDefault());
            return ret;
        }

        public string GetempHot()
        {
            string qry = $@"select distinct empl_name,empl_id from acpl_common.dbo.employees where del_status=0";
            var lst = _crudRepository.SelectBySQL<Class_employees>(qry).ToList();
            return JsonConvert.SerializeObject(lst);
        }
        public string GetRca(int? obs_slno = 0)
        {
            string qry = string.Empty;
            if (obs_slno > 0)
            {
                qry = $@"select rca_slno,why1,why2,why3,why4,why5,rca_submitted_status from customer_audit_child2_rca
                        where obs_slno={obs_slno}";
            }
            else
            {
                qry = $@"select 0 as rca_slno,'' as why1,'' as why2,'' as why3,'' as why4,'' as why5,'' as rca_submitted_status from customer_audit_child2_rca";
            }

            var lst = _crudRepository.SelectBySQL<Class_customer_audit_child2_rca>(qry).ToList();


            return JsonConvert.SerializeObject(lst);
        }
        public string GetCorr(int? obs_slno = 0)
        {
            string qry = string.Empty;
            if (obs_slno > 0)
            {
                qry = $@"select ca_slno,action_taken,action_taken_by,action_taken_dt,ca_submitted_status from customer_audit_child3_ca
                        where obs_slno={obs_slno}";
            }
            else
            {
                qry = $@"select 0 as ca_slno,'' as action_taken,'' as action_taken_by,'' as action_taken_dt,'' as ca_submitted_status from customer_audit_child3_ca";
            }

            var lst = _crudRepository.SelectBySQL<Class_customer_audit_child3_ca>(qry).ToList();


            return JsonConvert.SerializeObject(lst);
        }
        public string GetPrev(int? obs_slno = 0)
        {
            string qry = string.Empty;
            if (obs_slno > 0)
            {
                qry = $@"select prev_slno,action_taken,action_taken_by,action_taken_dt,prev_submitted_status from customer_audit_child4_prev
                        where obs_slno={obs_slno}";
            }
            else
            {
                qry = $@"select 0 as prev_slno,'' action_taken,'' action_taken_by,'' action_taken_dt,'' prev_submitted_status from customer_audit_child4_prev";
            }

            var lst = _crudRepository.SelectBySQL<Class_customer_audit_child4_prev>(qry).ToList();


            return JsonConvert.SerializeObject(lst);
        }
    }
}
