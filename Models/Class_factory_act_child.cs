
using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("factory_act_child")]
[PrimaryKey("fact_act_chd_slno", AutoIncrement = true)]
/// <summary>
/// Properties defined for factory_act_child
/// </summary>
public class Class_factory_act_child : IDisposable
{
    [Display(Name = "Factory Act Child Slno")]
    public int fact_act_chd_slno { get; set; }

    [Required(ErrorMessage = "Factory Act Header is required.")]
    [Display(Name = "Factory Act Header Slno")]
    public int fact_act_hdr_slno { get; set; }

    [Required(ErrorMessage = "Legal Requirement is required.")]
    [Display(Name = "Legal Requirement")]
    public string legal_requirement { get; set; }

    [Required(ErrorMessage = "Compliance Rule / Act Reference is required.")]
    [Display(Name = "Compliance Rule / Act Reference")]
    public string compliance_rule_act_reference { get; set; }

    [Required(ErrorMessage = "Compliance Approving Authority is required.")]
    [Display(Name = "Compliance Approving Authority")]
    public string compliance_approving_authority { get; set; }

    [Required(ErrorMessage = "Appointed Competent Person is required.")]
    [Display(Name = "Appointed Competent Person")]
    public string appointed_competent_person { get; set; }

    [Required(ErrorMessage = "Factory Act is required.")]
    [Display(Name = "Factory Act Slno")]
    public int factory_act_slno { get; set; }

    [Display(Name = "Form Nos")]
    public string form_no { get; set; }

    [Display(Name = "Frequency of Renewal")]
    public string freq_renewal { get; set; }

    [Display(Name = "Frequency")]
    public int? freq { get; set; }

    [Display(Name = "Date of Filing")]
    public string date_of_filing { get; set; }

    [Display(Name = "Valid From")]
    public string valid_from { get; set; }

    [Display(Name = "Valid Upto")]
    public string valid_upto { get; set; }

    [Display(Name = "Next Due Date")]
    public string next_due_date { get; set; }

    [Display(Name = "Responsibility")]
    public string responsibility { get; set; }

    [Display(Name = "Responsible Person")]
    public string responsible_person { get; set; }

    [Display(Name = "Responsible Person Slno")]
    public int? resp_Slno { get; set; }

    [Display(Name = "Status")]
    public string status { get; set; }

    [Display(Name = "Remarks")]
    public string remarks { get; set; }
    public string submitted_by { get; set; }
    public string submitted_dt { get; set; }
    public string submit_status { get; set; } = "N";
    public string fin_year { get; set; }

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
