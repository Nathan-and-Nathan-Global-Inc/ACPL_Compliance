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
using System.Collections.Generic;

[TableName("supplier_audit_plan")]
[PrimaryKey("plan_slno",AutoIncrement =true)]

/// <summary>
/// properties defined for supplier_audit_plan
/// </summary>
public class Class_supplier_audit_plan_old : IDisposable {
    [Display (Name="Plan Slno" )]
    public int plan_slno { get; set; }
    [Display(Name = "Plant Slno")]
    public int plant_slno { get; set; }
    [Display(Name = "Supplier")]
    public int supp_slno { get; set; }
    [Display(Name = "Supplier")]
    public String supp_name { get; set; }
    [Display (Name="Plan Year" )]
    public string plan_year { get; set; }
    [Display (Name="Jan-Mar" )]
    public String jan_feb_mar { get; set; }
    [Display(Name = "Apr-Jun")]
    public String apr_may_jun { get; set; }
    [Display(Name = "Jul-Sep")]
    public String jul_aug_sep { get; set; }
    [Display(Name = "Oct-Dec")]
    public String oct_nov_dec { get; set; }

    [Display(Name = "Submit Status")]
    public String submit_status { get; set; }
    
    [Display (Name="Completion Status" )]
    public String completion_status { get; set; }    
  
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

