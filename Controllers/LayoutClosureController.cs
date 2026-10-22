using CommonClassLibrary;
using ElmahCore;
using MasterClass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NPoco;
using Polly;
using SupermarketRepository;
using System.Diagnostics;
using System.Text;

namespace ams.Controllers
{
    public class LayoutClosureController : Controller
    {
        public readonly ILogger<LayoutClosureController> logger;
        public readonly ILogger<MasterRepository> masterlogger;
        public readonly DBClass _crudRepository;
        private readonly MasterCrudClass _masterCrudClass;
        private readonly IConfiguration apppath;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public int plantslno = 0;
        public LayoutClosureController(ILogger<LayoutClosureController> _logger, DBClass _db, ILogger<MasterRepository> _masterlogger, MasterCrudClass masterClass, IConfiguration _config, IWebHostEnvironment webHostEnvironment)
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
        public ActionResult Index()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            // Check if the session value is null or empty
            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string sql = $@"select distinct hdr.layout_hdr_slno,FORMAT(CAST(hdr.audit_date AS DATE), 'dd-MM-yyyy') AS audit_date,
            hdr.customer_code, cust.CustomerName as cust_name,p.part_no,p.part_desc,hdr.audit_by,concat(obs.obs_sample1,',',obs.obs_sample2,',',obs.obs_sample3) as obs1,obs.dimensions_drawing,
            rca.why_why_5w_1h,rca.why1,rca.rca_submitted_status,rca.rca_slno,rca.w_what,obs.dimensions_drawing,hdr.revision,hdr.batch_no,
            case when cl.closure_submitted_status='Y' then 'Closed' else 'Closure' end as closure_submitted_status,
            v.verify_date,v.verify_implementation,v.verify_effectiveness,v.verify_submitted_status,v.verify_slno,cl.closure_date,
            cl.mrm_ref,cl.mr_name,cl.closure_slno
            from Layout_audit_hdr hdr 
            INNER JOIN acpl_common.dbo.customer_supplier cust ON cust.CustomerCode=hdr.customer_code and cust.status='Active'
            INNER JOIN acpl_common.dbo.parts p ON p.part_slno=hdr.part_slno and p.del_status=0 and p.customer_slno=cust.slno
            INNER JOIN Layout_audit_child2_observation obs ON obs.layout_hdr_slno=hdr.layout_hdr_slno
            INNER JOIN Layout_audit_child3_rca rca ON rca.layout_hdr_slno=hdr.layout_hdr_slno and rca.obs_slno=obs.obs_slno 
            INNER JOIN Layout_audit_child5_verification v ON v.layout_hdr_slno=hdr.layout_hdr_slno and v.obs_slno=obs.obs_slno
            INNER JOIN Layout_audit_child6_closure cl ON cl.layout_hdr_slno=hdr.layout_hdr_slno and cl.obs_slno=obs.obs_slno 
            where hdr.plant_slno={plantslno} ";
            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_hdr>(sql);
            TempData.Remove("msg");
            return View(lst);
        }

        public ActionResult PendingList()
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }

            string sql = $@"select distinct hdr.layout_hdr_slno,FORMAT(CAST(hdr.audit_date AS DATE), 'dd-MM-yyyy') AS audit_date,
                hdr.customer_code, cust.CustomerName as cust_name,p.part_no,p.part_desc,hdr.audit_by,hdr.plant_slno,obs.dimensions_drawing,
                concat(obs.obs_sample1,',',obs.obs_sample2,',',obs.obs_sample3) as obs1,obs.dimensions_drawing,hdr.revision,hdr.batch_no,obs.obs_slno,obs.obs_date,
                'Closure' as next_action,rca.why1,rca.w_what,
                v.verify_date,v.verify_implementation,v.verify_effectiveness,v.verify_submitted_status,v.verify_slno
                from Layout_audit_hdr hdr 
                INNER JOIN acpl_common.dbo.customer_supplier cust ON cust.CustomerCode=hdr.customer_code and cust.status='Active'
                INNER JOIN acpl_common.dbo.parts p ON p.part_slno=hdr.part_slno and p.del_status=0 and p.customer_slno=cust.slno
                INNER JOIN Layout_audit_child2_observation obs ON obs.layout_hdr_slno=hdr.layout_hdr_slno
                INNER JOIN Layout_audit_child3_rca rca ON rca.layout_hdr_slno=hdr.layout_hdr_slno and rca.obs_slno=obs.obs_slno 
				INNER JOIN Layout_audit_child5_verification v ON v.layout_hdr_slno=hdr.layout_hdr_slno and v.obs_slno=rca.obs_slno and v.verify_submitted_status='Y'  
and obs.obs_slno not in (select obs_slno from Layout_audit_child6_closure cls where cls.layout_hdr_slno=hdr.layout_hdr_slno and
				cls.obs_slno=obs.obs_slno)
                where hdr.plant_slno='{plantslno}' and obs.obs_submitted_status='Y' and hdr.audit_status='Open'
              ";

            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_hdr>(sql);
            TempData.Remove("msg");
            return View(lst);
        }
        public JsonResult Loademployees(int plantslno)
        {
            try
            {
                string qry = $@"select distinct empl_name,empl_id from acpl_common.dbo.employees where del_status=0 and plant_slno='{plantslno}'";
                var lst = _crudRepository.SelectBySQL<Class_employees>(qry).ToList();
                return Json(lst);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RaiseError(ex);
                return Json(new { success = false, message = "Error loading Employees" });
            }
        }

        // GET: LayoutClosureController/Create
        public ActionResult Create(int? id, string obsflag, int? slno)
        {
            string plantSlnoString = HttpContext.Session.GetString("plantslno");

            // Check if the session value is null or empty
            if (!string.IsNullOrEmpty(plantSlnoString))
            {
                plantslno = int.Parse(plantSlnoString);
            }
            string sql = string.Empty;
            sql = $@"select distinct hdr.layout_hdr_slno,FORMAT(CAST(hdr.audit_date AS DATE), 'dd-MM-yyyy') AS audit_date,hdr.customer_code, cust.CustomerName as cust_name,p.part_no,
                p.part_desc,hdr.audit_by,hdr.plant_slno,obs.dimensions_drawing,obs.obs_date,
                obs.obs_date,obs.obs_sample1,obs.obs_sample2,obs.obs_sample3,
                obs.obs_slno,rca.rca_slno,v.verify_slno
                from Layout_audit_hdr hdr 
                INNER JOIN acpl_common.dbo.customer_supplier cust ON cust.CustomerCode=hdr.customer_code and cust.status='Active'
                INNER JOIN acpl_common.dbo.parts p ON p.part_slno=hdr.part_slno and p.del_status=0 and p.customer_slno=cust.slno
                INNER JOIN Layout_audit_child2_observation obs ON obs.layout_hdr_slno=hdr.layout_hdr_slno  
                INNER JOIN Layout_audit_child3_rca rca ON rca.layout_hdr_slno=hdr.layout_hdr_slno and rca.obs_slno=obs.obs_slno
                INNER JOIN Layout_audit_child5_verification v ON v.layout_hdr_slno=hdr.layout_hdr_slno and v.obs_slno=rca.obs_slno 
                where hdr.plant_slno='{plantslno}' and obs.obs_submitted_status='Y' and hdr.audit_status='Open' and 
                hdr.layout_hdr_slno='{id}' and obs.obs_slno='{slno}'";
            
            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_hdr>(sql).ToList();

            return View(lst[0]);
        }
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {                     
            int obs_slno = 0;
            string obs_flag = string.Empty;

            try
            {
                string closuredata = collection["closuredata"];                
                string userid = collection["userid"];
                string submitstatus = collection["submitstatus"];

                var hdata = JsonConvert.DeserializeObject<List<string>>(closuredata);

                Class_Layout_audit_child6_closure c = new Class_Layout_audit_child6_closure();

                c.layout_hdr_slno = int.Parse(hdata[0].ToString());
                c.mrm_ref = hdata[1].ToString();
                c.mr_name = hdata[2].ToString();
                c.closure_date = DateTime.Now.ToString("yyyy-MM-dd");
                
                obs_slno = int.Parse(hdata[3].ToString());
                c.obs_slno = obs_slno;
                c.closure_submitted_status = submitstatus;                

                if (submitstatus == "Y")
                {
                    c.closure_submitted_by = userid;
                    c.closure_submitted_dt = DateTime.Now.ToString("yyyy-MM-dd");
                }

                var ret = _crudRepository.AddNew<Class_Layout_audit_child6_closure>(c);

                if (submitstatus == "Y")
                {
                    string sql = $@"select count(obs_slno) from Layout_audit_child2_observation 
    where obs_slno not in (select obs_slno from Layout_audit_child6_closure where layout_hdr_slno={c.layout_hdr_slno}) 
    and layout_hdr_slno={c.layout_hdr_slno}";
                    var cnt = _crudRepository.ExecuteScalar<Class_Layout_audit_child2_observation>(sql);

                    if (int.Parse(cnt.ToString()) == 0)
                    {
                        string sql2 = $@"update Layout_audit_hdr set audit_status='Closed' where layout_hdr_slno='{c.layout_hdr_slno}'";
                        _crudRepository.Execute<Class_Layout_audit_hdr>(sql2);
                    }
                }
            }
            catch (Exception ex)
            {
                ElmahExtensions.RaiseError(ex);
                throw;
            }

            return Json(new { status = "Submitted" });
        }
        public string GetRcadata(long slno)
        {
            string sql = string.Empty;

            sql = $@"select distinct rca.layout_hdr_slno,rca.rca_slno,rca.rca_date,rca.why_why_5w_1h,
                rca.why1,rca.why2,rca.why3,rca.why4,rca.why5,rca.w_what,rca.w_where,rca.w_when,rca.w_why,rca.w_who,
                rca.w_how,rca.rca_submitted_status,
                rca.obs_slno,capa.capa_slno,capa.capa_date,capa.disposal_action_done,capa.disposal_action_resp_slno,capa.disposal_action_targetdt,
				capa.corr_action_done,capa.corr_action_resp_slno,capa.corr_action_targetdt,capa.corr_action_impact_done,
				capa.corr_action_impact_resp_slno,capa.corr_action_impact_targetdt,capa.prev_action_done,
				capa.prev_action_resp_slno,capa.prev_action_targetdt,capa.capa_submitted_status
                from Layout_audit_hdr hdr                 
                INNER JOIN Layout_audit_child3_rca rca ON rca.layout_hdr_slno=hdr.layout_hdr_slno 
                LEFT OUTER JOIN Layout_audit_child4_capa capa ON capa.layout_hdr_slno=capa.layout_hdr_slno and rca.obs_slno=capa.obs_slno
                where rca.rca_slno={slno}";

            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_child3_rca>(sql).ToList();

            if (lst == null || !lst.Any())
                throw new Exception("No data found.");

            var clsrca = lst.First();
            return JsonConvert.SerializeObject(clsrca);
        }
        public string GetVerifydata(long slno)
        {
            string sql = string.Empty;

            sql = $@"select distinct v.layout_hdr_slno,v.verify_slno,v.verify_date,v.verify_implementation,v.verify_effectiveness,
                v.auditee_verify,v.auditee_verify_dt,v.auditor_verify,v.auditor_verify_dt,v.verify_submitted_dt,v.verify_submitted_by,
                v.verify_submitted_status,v.auditor_verify_name,v.auditee_verify_name,
                v.obs_slno
                from Layout_audit_hdr hdr                 
                INNER JOIN Layout_audit_child5_verification v ON v.layout_hdr_slno=hdr.layout_hdr_slno 
                where v.verify_slno={slno}";

            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_child5_verification>(sql).ToList();

            if (lst == null || !lst.Any())
                throw new Exception("No data found.");

            var clsrca = lst.First();
            return JsonConvert.SerializeObject(clsrca);
        }
        public string GetAuditfindingsId(long slno)
        {
            var lst = _crudRepository.SelectById<Class_Layout_audit_child6_closure>(slno);
            return JsonConvert.SerializeObject(lst);
        }
        public string Getmasterdata(int id, string obsflag, int? slno)
        {
            var cls = _crudRepository.SelectById<Class_Layout_audit_hdr>(id);
            string sql = string.Empty;
            sql = $@"select distinct hdr.layout_hdr_slno,FORMAT(CAST(hdr.audit_date AS DATE), 'dd-MM-yyyy') AS audit_date,hdr.customer_code, cust.CustomerName as cust_name,p.part_no,
                p.part_desc,hdr.audit_by,hdr.plant_slno,
                obs.obs_date,concat(obs.obs_sample1,',',obs.obs_sample2,',',obs.obs_sample3)as obs1,obs.dimensions_drawing,hdr.revision,hdr.batch_no,
                obs.obs_slno
                from Layout_audit_hdr hdr 
                INNER JOIN acpl_common.dbo.customer_supplier cust ON cust.CustomerCode=hdr.customer_code and cust.status='Active'
                INNER JOIN acpl_common.dbo.parts p ON p.part_slno=hdr.part_slno and p.del_status=0 and p.customer_slno=cust.slno
                INNER JOIN Layout_audit_child2_observation obs ON obs.layout_hdr_slno=hdr.layout_hdr_slno                
                where hdr.plant_slno='{cls.plant_slno}' and obs.obs_submitted_status='Y'  
                and hdr.layout_hdr_slno='{id}' and obs.obs_slno='{slno}'";
            
            var lst = _crudRepository.SelectBySQL<Class_Layout_audit_hdr>(sql).ToList();
            if (lst == null || !lst.Any())
                throw new Exception("No data found.");

            var clsrca = lst.First();
            return JsonConvert.SerializeObject(clsrca);
        }
        public IActionResult Edit(int id)
        {
            var cls = _crudRepository.SelectById<Class_Layout_audit_child6_closure>(id);
            var clshdr = _crudRepository.SelectById<Class_Layout_audit_hdr>(cls.layout_hdr_slno);

            string sql = $@"select rca_slno from Layout_audit_child3_rca where layout_hdr_slno='{cls.layout_hdr_slno}' and obs_slno='{cls.obs_slno}'";
            var rcaslno = _crudRepository.ExecuteScalar<Class_Layout_audit_child3_rca>(sql);

            sql = $@"select verify_slno from Layout_audit_child5_verification where layout_hdr_slno='{cls.layout_hdr_slno}' and obs_slno='{cls.obs_slno}'";
            var verifyslno = _crudRepository.ExecuteScalar<Class_Layout_audit_child5_verification>(sql);

            clshdr.verify_slno = int.Parse(verifyslno.ToString());
            clshdr.obs_slno = cls.obs_slno;
            clshdr.rca_slno = int.Parse(rcaslno.ToString());
            clshdr.closure_slno = cls.closure_slno;
            return View(clshdr);
        }
        [HttpPost]
        public ActionResult Edit(IFormCollection collection)
        {
            
            int obs_slno = 0;            
            string obs_flag = string.Empty;

            try
            {
                string closuredata = collection["closuredata"];                
                string userid = collection["userid"];
                string submitstatus = collection["submitstatus"];

                var hdata = JsonConvert.DeserializeObject<List<string>>(closuredata);

                Class_Layout_audit_child6_closure c = new Class_Layout_audit_child6_closure();

                c.layout_hdr_slno = int.Parse(hdata[0].ToString());
                c.mrm_ref = hdata[1].ToString();
                c.mr_name = hdata[2].ToString();
                c.closure_date = DateTime.Now.ToString("yyyy-MM-dd");

                obs_slno = int.Parse(hdata[3].ToString());
                c.obs_slno = obs_slno;
                c.closure_slno = int.Parse(hdata[5].ToString());                
                c.closure_submitted_status = submitstatus;               

                if (submitstatus == "Y")
                {
                    c.closure_submitted_by = userid;
                    c.closure_submitted_dt = DateTime.Now.ToString("yyyy-MM-dd");
                }

                _crudRepository.Update<Class_Layout_audit_child6_closure>(c);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RaiseError(ex);
                throw;
            }

            return Json(new { status = "Submitted" });
        }
    }
}
