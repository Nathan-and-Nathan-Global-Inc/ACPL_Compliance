using System.Collections.Generic;
using MasterClass.Models;

public class FactoryActTransSaveModel
{

    public int fact_act_hdr_slno { get; set; }
    public string entry_by { get; set; }

    public string entry_dt { get; set; }

    public string submit_status { get; set; }

    public string fin_year { get; set; }

    public List<Class_factory_act_child> children { get; set; }
        = new List<Class_factory_act_child>();
}