using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("certi_comp_tracker")]
[PrimaryKey("slno", AutoIncrement = true)]
/// <summary>
/// Properties defined for certi_comp_tracker
/// </summary>
public class Class_certi_comp_tracker : IDisposable
{
    [Display(Name = "Slno")]
    public int slno { get; set; }

    [Required(ErrorMessage = "Equipment is required.")]
    [Display(Name = "Equipment")]
    public string equipment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Certificate Number is required.")]
    [Display(Name = "Certificate Number")]
    public string certificate_no { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agency is required.")]
    [Display(Name = "Agency")]
    public string agency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Frequency Number is required.")]
    [Display(Name = "Frequency Number")]
    public int freq_no { get; set; }

    [Required(ErrorMessage = "Frequency Unit is required.")]
    [Display(Name = "Frequency Unit")]
    public string freq_desc { get; set; } = string.Empty;

    [Required(ErrorMessage = "Calibration Done Date is required.")]
    [Display(Name = "Calibration Done Date")]
    public string calib_donedt { get; set; }

    [Required(ErrorMessage = "Calibration Due Date is required.")]
    [Display(Name = "Calibration Due Date")]
    public String calib_duedt { get; set; }

    [Display(Name = "Remarks")]
    public string? remarks { get; set; }

    [Display(Name = "Created By")]
    public int created_by { get; set; }

    [Display(Name = "Created Date")]
    public DateTime? created_dt { get; set; }

    [Display(Name = "Modified By")]
    public int? modified_by { get; set; }

    [Display(Name = "Modified Date")]
    public DateTime? modified_dt { get; set; }

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