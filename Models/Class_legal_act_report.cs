using System;
using System.ComponentModel.DataAnnotations;
using NPoco;

[TableName("legal_act_master")]
[PrimaryKey("legal_act_slno")]
public class Class_legal_act_report : IDisposable
{
    public int legal_act_slno { get; set; }

    [Display(Name = "Act Groups")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a group.")]
    public int group_slno { get; set; }

    public string? group_desc { get; set; }

    [Required(ErrorMessage = "Activity Description is required.")]
    [Display(Name = "Activity Description")]
    public string activity_description { get; set; }

    [Required(ErrorMessage = "Name of the Act/Law/Rules is required.")]
    [Display(Name = "Name of the Act/Law/Rules")]
    public string act_law_rules_name { get; set; }

    [Required(ErrorMessage = "Amendment / Version / Date / Year of Release is required.")]
    [Display(Name = "Amendment / Version / Date / Year of Release")]
    public string amendment_version_date_year { get; set; }

    [Required(ErrorMessage = "Rule / Section Applicable is required.")]
    [Display(Name = "Rule / Section Applicable")]
    public string rule_section_applicable { get; set; }

    [Required(ErrorMessage = "Regulatory Body is required.")]
    [Display(Name = "Regulatory Body")]
    public string regulatory_body { get; set; }

    public int created_by { get; set; }
    public DateTime created_dt { get; set; }
    public int? modified_by { get; set; }
    public DateTime? modified_dt { get; set; }

    [Required(ErrorMessage = "Effective Date is required.")]
    [Display(Name = "Effective Date")]
    public DateTime? effective_dt { get; set; }

    public bool del_status { get; set; }
    [ResultColumn]
    public int? plan_year { get; set; }
    [ResultColumn]
    public string? status { get; set; }
    [ResultColumn]
    public decimal jan { get; set; } = 0;
    [ResultColumn]
    public decimal feb { get; set; } = 0;
    [ResultColumn]
    public decimal mar { get; set; } = 0;
    [ResultColumn]
    public decimal apr { get; set; } = 0;
    [ResultColumn]
    public decimal may { get; set; } = 0;
    [ResultColumn]
    public decimal jun { get; set; } = 0;
    [ResultColumn]
    public decimal jul { get; set; } = 0;
    [ResultColumn]
    public decimal aug { get; set; } = 0;
    [ResultColumn]
    public decimal sep { get; set; } = 0;
    [ResultColumn]
    public decimal oct { get; set; } = 0;
    [ResultColumn]
    public decimal nov { get; set; } = 0;
    [ResultColumn]
    public decimal dec { get; set; } = 0;

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed resources if required.
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