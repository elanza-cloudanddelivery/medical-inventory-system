namespace MedicalInventory.API.Services.Common;
public class ValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ValidationResult SuccessResult() => new() { Success = true };
    public static ValidationResult ErrorResult(string message) => new() { Success = false, Message = message };
}