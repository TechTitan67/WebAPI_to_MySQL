using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class Criterion
{
    public int CriterionId { get; set; }

    public string? Regarding { get; set; }

    public string? CriterionDesc { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }
}
