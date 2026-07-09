namespace Maham.Web.Helpers;

public static class TranslationHelper
{
    public static string ToArabicRole(string? role)
    {
        if (string.IsNullOrEmpty(role)) return "عضو";
        return role.ToLower().Trim() switch
        {
            "owner" => "مالك المشروع",
            "admin" => "مشرف النظام",
            "member" => "عضو",
            _ => role
        };
    }

    public static string ToArabicPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority)) return "متوسطة";
        return priority.ToLower().Trim() switch
        {
            "low" => "منخفضة",
            "medium" => "متوسطة",
            "high" => "عالية",
            "critical" => "حرجة",
            _ => priority
        };
    }

    public static string ToArabicStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return "قيد الانتظار";
        return status.ToLower().Trim() switch
        {
            "todo" => "قيد الانتظار",
            "inprogress" => "جاري العمل",
            "done" => "مكتمل",
            _ => status
        };
    }
}
