using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// In-memory store for consultation details (Vet Doctor Flow – Pet Parent view).
/// Merges booking data with vitals, notes, diagnosis, prescription, diagnostics, report.
/// </summary>
public static class ConsultationStore
{
    private static readonly object Lock = new();
    /// <summary>Extended consultation data keyed by BookingId (demo / vet-entered).</summary>
    private static readonly Dictionary<string, ConsultationDetailsViewModel> ByBookingId = new();

    static ConsultationStore()
    {
        SeedDemoConsultation();
    }

    private static void SeedDemoConsultation()
    {
        lock (Lock)
        {
            var demo = new ConsultationDetailsViewModel
            {
                BookingId = "BK-DEMO",
                ConsultStatus = ConsultStatus.Completed,
                PetName = "Max",
                PetSpecies = "Dog",
                PetBreed = "Golden Retriever",
                PetWeightKg = 28.5,
                ParentName = "Pet Parent",
                ParentPhone = "+1 555-0123",
                ParentEmail = "parent@example.com",
                VetName = "Dr. Sarah Chen",
                ClinicName = "Happy Paws Clinic",
                ClinicAddress = "123 Pet Care Ave",
                SlotDate = "2025-02-01",
                SlotTime = "10:00 AM",
                ReasonForVisit = "Annual check-up and vaccination",
                PreviousConsults = new List<PreviousConsultSummaryViewModel>
                {
                    new() { Date = "2024-08-15", VetName = "Dr. Sarah Chen", Summary = "Vaccination booster" },
                    new() { Date = "2024-02-10", VetName = "Dr. Sarah Chen", Summary = "Annual check-up" }
                },
                Vitals = new List<VitalViewModel>
                {
                    new() { Name = "Weight", Value = "28.5", Unit = "kg", RecordedAt = "2025-02-01 10:15" },
                    new() { Name = "Temp", Value = "38.2", Unit = "°C", RecordedAt = "2025-02-01 10:16" },
                    new() { Name = "Heart rate", Value = "88", Unit = "bpm", RecordedAt = "2025-02-01 10:17" }
                },
                Notes = "Pet in good spirits. No visible issues. Owner advised on diet and exercise.",
                ProvisionalDiagnosis = "Healthy; routine wellness.",
                PrescriptionFileName = "prescription_max_20250201.pdf",
                ConsultationComplete = true,
                DiagnosticsRecommended = true,
                DiagnosticOrder = new DiagnosticOrderViewModel
                {
                    OrderId = "DX-001",
                    Status = DiagnosticStatus.ReportAvailable,
                    TestsOrdered = new List<string> { "CBC", "Blood chemistry", "Urinalysis" },
                    CollectionType = "In-Clinic",
                    TotalPrice = 85.00m,
                    AssignedLab = "PawNect Lab – Downtown",
                    OrderedAt = "2025-02-01 10:45",
                    SampleCollectedAt = "2025-02-01 11:00",
                    ReportUploadedAt = "2025-02-02 09:00"
                },
                Report = new LabReportViewModel
                {
                    ReportId = "RPT-001",
                    ReportFileName = "Max_CBC_Chemistry_20250202.pdf",
                    VetAdvice = "All values within normal range. No follow-up tests needed. Continue current diet.",
                    NextSteps = "Next annual check-up in Feb 2026. Keep vaccination schedule.",
                    ReviewedAt = "2025-02-02 10:30"
                }
            };
            ByBookingId["BK-DEMO"] = demo;
        }
    }

    /// <summary>
    /// Get full consultation details for a booking. If no extended data exists, builds from booking only.
    /// When bookingId is "BK-DEMO" and booking is null, returns demo consultation for preview.
    /// </summary>
    public static ConsultationDetailsViewModel? GetDetails(int userId, string bookingId, BookingConfirmViewModel? booking, string? parentName, string? parentEmail, string? parentPhone)
    {
        if (booking == null)
        {
            if (bookingId == "BK-DEMO")
            {
                lock (Lock)
                {
                    return ByBookingId.TryGetValue("BK-DEMO", out var demo) ? demo : null;
                }
            }
            return null;
        }

        lock (Lock)
        {
            if (ByBookingId.TryGetValue(bookingId, out var existing))
            {
                existing.BookingId = bookingId;
                existing.ConsultStatus = booking.Status;
                existing.VetId = booking.VetId;
                existing.ParentUserId = booking.ParentUserId;
                existing.VetName = booking.VetName;
                existing.ClinicName = booking.ClinicName;
                existing.ClinicAddress = booking.ClinicAddress ?? "";
                existing.SlotDate = booking.SlotDate;
                existing.SlotTime = booking.SlotTime;
                existing.PetName = booking.PetName;
                existing.ParentName = parentName ?? existing.ParentName;
                existing.ParentEmail = parentEmail ?? existing.ParentEmail;
                existing.ParentPhone = parentPhone ?? existing.ParentPhone;
                existing.ReasonForVisit ??= booking.ReasonForVisit;
                return existing;
            }
        }

        return new ConsultationDetailsViewModel
        {
            BookingId = bookingId,
            ConsultStatus = booking.Status,
            VetId = booking.VetId,
            ParentUserId = booking.ParentUserId,
            PetName = booking.PetName,
            VetName = booking.VetName,
            ClinicName = booking.ClinicName,
            ClinicAddress = booking.ClinicAddress ?? "",
            SlotDate = booking.SlotDate,
            SlotTime = booking.SlotTime,
            ReasonForVisit = booking.ReasonForVisit ?? "—",
            ParentName = parentName,
            ParentEmail = parentEmail,
            ParentPhone = parentPhone
        };
    }

    /// <summary>
    /// Get or create consultation details for vet consult screen. Uses booking's parent info.
    /// </summary>
    public static ConsultationDetailsViewModel? GetDetailsForVet(string bookingId)
    {
        var booking = BookingStore.GetByBookingId(bookingId);
        if (booking == null)
            return null;
        lock (Lock)
        {
            if (ByBookingId.TryGetValue(bookingId, out var existing))
            {
                existing.ConsultStatus = booking.Status;
                existing.VetId = booking.VetId;
                existing.ParentUserId = booking.ParentUserId;
                existing.ParentName = booking.ParentName ?? existing.ParentName;
                existing.ParentEmail = booking.ParentEmail ?? existing.ParentEmail;
                existing.ParentPhone = booking.ParentPhone ?? existing.ParentPhone;
                existing.ReasonForVisit ??= booking.ReasonForVisit;
                return existing;
            }
            var created = new ConsultationDetailsViewModel
            {
                BookingId = bookingId,
                ConsultStatus = booking.Status,
                VetId = booking.VetId,
                ParentUserId = booking.ParentUserId,
                PetName = booking.PetName,
                VetName = booking.VetName,
                ClinicName = booking.ClinicName,
                ClinicAddress = booking.ClinicAddress ?? "",
                SlotDate = booking.SlotDate,
                SlotTime = booking.SlotTime,
                ReasonForVisit = booking.ReasonForVisit ?? "—",
                ParentName = booking.ParentName,
                ParentEmail = booking.ParentEmail,
                ParentPhone = booking.ParentPhone
            };
            ByBookingId[bookingId] = created;
            return created;
        }
    }

    /// <summary>
    /// Save vet input: vitals, notes, provisional diagnosis, prescription file name, next actions. Optionally update consult status.
    /// </summary>
    public static void SaveVetInput(string bookingId, List<VitalViewModel>? vitals, string? notes, string? provisionalDiagnosis,
        string? prescriptionFileName, bool consultationComplete, bool diagnosticsRecommended, string? newStatus)
    {
        lock (Lock)
        {
            if (!ByBookingId.TryGetValue(bookingId, out var details))
            {
                var booking = BookingStore.GetByBookingId(bookingId);
                if (booking == null) return;
                details = new ConsultationDetailsViewModel
                {
                    BookingId = bookingId,
                    ConsultStatus = booking.Status,
                    VetId = booking.VetId,
                    ParentUserId = booking.ParentUserId,
                    PetName = booking.PetName,
                    VetName = booking.VetName,
                    ClinicName = booking.ClinicName,
                    ClinicAddress = booking.ClinicAddress ?? "",
                    SlotDate = booking.SlotDate,
                    SlotTime = booking.SlotTime,
                    ReasonForVisit = booking.ReasonForVisit ?? "—",
                    ParentName = booking.ParentName,
                    ParentEmail = booking.ParentEmail,
                    ParentPhone = booking.ParentPhone
                };
                ByBookingId[bookingId] = details;
            }
            if (vitals != null) details.Vitals = vitals;
            if (notes != null) details.Notes = notes;
            if (provisionalDiagnosis != null) details.ProvisionalDiagnosis = provisionalDiagnosis;
            if (prescriptionFileName != null) details.PrescriptionFileName = prescriptionFileName;
            details.ConsultationComplete = consultationComplete;
            details.DiagnosticsRecommended = diagnosticsRecommended;
            if (!string.IsNullOrEmpty(newStatus))
            {
                details.ConsultStatus = newStatus;
                BookingStore.UpdateStatus(bookingId, newStatus);
            }
        }
    }
}
