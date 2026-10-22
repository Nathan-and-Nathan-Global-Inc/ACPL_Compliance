using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using Newtonsoft.Json;
using ElmahCore;
using System.Linq;
using System.Web;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

[TableName("internal_auditors_O")]
[PrimaryKey("intl_slno")]

/// <summary>
/// properties defined for internal_auditors
/// </summary>
public class Class_internal_auditors_O : IDisposable {
    [Display (Name="Intl Slno" )]
    public int intl_slno { get; set; }
    [Display (Name="Auditor Name" )]
    public int empl_slno { get; set; }
    [Display (Name="Plant Slno" )]
    public int plant_slno { get; set; }
    [Display (Name="Dept Slno" )]
    public int dept_slno { get; set; }
    [Display (Name="Desig Slno" )]
    public int desig_slno { get; set; }
    [Display (Name="Qualification" )]
    public string? qualification { get; set; }
    [Display (Name="Experience Years" )]
    public int? experience_years { get; set; }
    [Display (Name="Trained Dates" )]
    public string? trained_dates { get; set; }
    [Display (Name="Trained By" )]
    public string? trained_by { get; set; }
    [Display (Name="Audit System" )]
    [Required(ErrorMessage = "Audit System field is required.")]
  
    public string? system_desc { get; set; }
    [Display (Name="Del Status" )]
    public bool del_status { get; set; }
    [ResultColumn]
    public String dept_name { get; set; }
    [ResultColumn]
    public String desig_name { get; set; }
    [ResultColumn]
    public String empl_name { get; set; }

    #region "IDisposable Support"
    // To detect redundant calls
    private bool disposedValue;

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            // TODO: set large fields to null.
        }
        this.disposedValue = true;
    }

    // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    //Protected Overrides Sub Finalize()
    //    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    //    Dispose(False)
    //    MyBase.Finalize()
    //End Sub

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

