using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryDesc { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
