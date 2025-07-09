using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;    

    public class RfidLoginRequest
    {
        [Required(ErrorMessage = "El código RFID es obligatorio")]
        public string RfidCode { get; set; } = string.Empty;
    }