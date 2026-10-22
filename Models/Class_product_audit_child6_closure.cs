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

[TableName("product_audit_child6_closure")]
[PrimaryKey("closure_slno", AutoIncrement = true)]
public class Class_product_audit_child6_closure : IDisposable {
    [Display (Name="Slno" )]
    public int closure_slno { get; set; }
    [Display (Name="Hdr Slno" )]
    public int pdt_hdr_slno { get; set; }
    [Display(Name = "Date")]
    public String? closure_date { get; set; }
    [Display (Name= "MRM Reference")]
    public String? mrm_ref { get; set; }
    [Display(Name = "MR Name")]
    public String? mr_name { get; set; }  

    [Display(Name = "Submitted by")]
    public String? closure_submitted_by { get; set; }
    [Display(Name = "Submitted Dt")]
    public String? closure_submitted_dt { get; set; }
    [Display(Name = "Status")]
    public String? closure_submitted_status { get; set; }
    [Display(Name = "Obs/Checklist")]
    public String? obs_flag { get; set; }
    [Display(Name = "Slno")]
    public int obs_slno { get; set; }

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

