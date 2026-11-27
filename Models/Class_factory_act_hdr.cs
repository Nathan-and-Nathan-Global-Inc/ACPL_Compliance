using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("factory_act_hdr")]
[PrimaryKey("fact_act_hdr_slno")]
/// <summary>
/// Properties defined for factory_act_hdr
/// </summary>
public class Class_factory_act_hdr : IDisposable
{
    [Display(Name = "Factory Act Header Slno")]
    public int fact_act_hdr_slno { get; set; }

    [Required(ErrorMessage = "Entry By is required.")]
    [Display(Name = "Entry By")]
    public string entry_by { get; set; }

    [Required(ErrorMessage = "Entry Date is required.")]
    [Display(Name = "Entry Date")]
    public string entry_dt { get; set; }

    [Display(Name = "Created By")]
    public int created_by { get; set; }

    [Display(Name = "Created Date")]
    public DateTime? created_dt { get; set; }

    [Display(Name = "Modified By")]
    public int? modified_by { get; set; }

    [Display(Name = "Modified Date")]
    public DateTime? modified_dt { get; set; }
    public string submit_status { get; set; } = "N";

    public string submitted_by { get; set; }
    public string submitted_dt { get; set; }
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