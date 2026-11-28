
using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("factory_act_master")]
[PrimaryKey("factory_act_slno")]
/// <summary>
/// Properties defined for factory_act_report
/// </summary>
public class Class_factory_act_report : IDisposable
{
    public int fact_act_hdr_slno { get; set; }
    public int fact_act_chd_slno { get; set; }
    public int factory_act_slno { get; set; }

    public string plan_year { get; set; }
    public int group_slno { get; set; }
    public string status { get; set; }

    public string legal_requirement { get; set; }
    public string compliance_rule_act_reference { get; set; }
    public string compliance_approving_authority { get; set; }
    public string appointed_competent_person { get; set; }

    public string form_no { get; set; }
    public string freq_renewal { get; set; }
    public int? freq { get; set; }

    public string date_of_filing { get; set; }
    public string valid_from { get; set; }
    public string valid_upto { get; set; }
    public string next_due_date { get; set; }

    public string responsibility { get; set; }
    public string responsible_person { get; set; }
    public string remarks { get; set; }
    

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed resources here if required.
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
