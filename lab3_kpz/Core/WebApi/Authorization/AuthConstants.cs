namespace lab3_kpz.Core.WebApi.Authorization;

public static class AppRoles
{
    public const string Patient = "patient";
    public const string Doctor = "doctor";
    public const string Admin = "admin";
}

public static class AppPolicies
{
    public const string RequireAdminOnly = "RequireAdminOnly";
    public const string RequireDoctorOrAdmin = "RequireDoctorOrAdmin";
    public const string OwnerOrAdmin = "OwnerOrAdmin";
    public const string OwnerDoctorOrAdmin = "OwnerDoctorOrAdmin";
}

