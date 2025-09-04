using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class Efmigrationshistory
{
    public string MigrationId { get; set; } = null!;

    public string ProductVersion { get; set; } = null!;
}
