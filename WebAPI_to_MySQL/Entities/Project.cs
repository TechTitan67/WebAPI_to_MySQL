using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string? ProjectDesc { get; set; }

    public string? Prompt { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
}
