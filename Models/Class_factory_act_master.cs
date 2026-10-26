
using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("factory_act_master")]
[PrimaryKey("factory_act_slno")]
/// <summary>
/// Properties defined for factory_act_master
/// </summary>
public class Class_factory_act_master : IDisposable
{
    [Display(Name = "Factory Act Slno")]
    public int factory_act_slno { get; set; }

    [Required(ErrorMessage = "Legal Requirement is required.")]
    [Display(Name = "Legal Requirement")]
    public string legal_requirement { get; set; }

    [Required(ErrorMessage = "Compliance Rule / Act Reference is required.")]
    [Display(Name = "Compliance Rule / Act Reference")]
    public string compliance_rule_act_reference { get; set; }

    [Required(ErrorMessage = "Compliance Approving Authority is required.")]
    [Display(Name = "Compliance Approving Authority")]
    public string compliance_approving_authority { get; set; }

    [Required(ErrorMessage = "Name of the appointed / designated competent person is required.")]
    [Display(Name = "Name of the appointed / designated competent person")]
    public string appointed_competent_person { get; set; }

    [Display(Name = "Created By")]
    public int created_by { get; set; }

    [Display(Name = "Created Date")]
    public DateTime created_dt { get; set; }

    [Display(Name = "Modified By")]
    public int? modified_by { get; set; }

    [Display(Name = "Modified Date")]
    public DateTime? modified_dt { get; set; }

    [Required(ErrorMessage = "Effective Date is required.")]
    [Display(Name = "Effective Date")]
    public DateTime? effective_dt { get; set; }

    [Display(Name = "Del Status")]
    public bool del_status { get; set; }

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
