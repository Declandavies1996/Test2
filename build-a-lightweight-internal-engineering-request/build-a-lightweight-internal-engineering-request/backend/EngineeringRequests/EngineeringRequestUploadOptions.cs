namespace CaeDashboard.EngineeringRequests;

public class EngineeringRequestUploadOptions
{
    public string RootPath { get; set; } = "Uploads/Requests";
    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } =
    [
        ".msg", ".pdf", ".xlsx", ".xls", ".csv", ".txt", ".log",
        ".png", ".jpg", ".jpeg", ".zip"
    ];
}
