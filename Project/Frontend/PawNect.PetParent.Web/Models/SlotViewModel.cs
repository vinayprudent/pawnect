namespace PawNect.PetParent.Web.Models;

public class SlotViewModel
{
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd
    public string Time { get; set; } = string.Empty; // "09:00", "09:30"
    public string DisplayText { get; set; } = string.Empty; // "Mon, 3 Feb · 09:00"
    public bool IsAvailable { get; set; }
}
