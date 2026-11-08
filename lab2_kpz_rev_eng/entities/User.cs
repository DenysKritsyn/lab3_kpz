using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class User
{
    public int UserId { get; set; }

    public string UserEmail { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateOnly? UserBirthDate { get; set; }

    public string? UserPhoneNumber { get; set; }

    public virtual ICollection<DiaryEntry> DiaryEntries { get; set; } = new List<DiaryEntry>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
