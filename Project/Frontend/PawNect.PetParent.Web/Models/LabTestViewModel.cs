namespace PawNect.PetParent.Web.Models;

/// <summary>
/// A2. Lab Tests Landing – sections: Preventive Health Tests, Wellness Packages, Individual Tests
/// </summary>
public class LabTestLandingViewModel
{
    public bool ShowPreventiveHealthTests { get; set; } = true;
    public bool ShowWellnessPackages { get; set; } = true;
    public bool ShowIndividualTests { get; set; } = true;
}

/// <summary>
/// A3. Package or individual test item – price, sample type, CTA
/// </summary>
public class LabTestPackageItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string SampleType { get; set; } = "Blood";
    public string[] TestsIncluded { get; set; } = Array.Empty<string>();
}

public class LabTestIndividualItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public decimal Price { get; set; }
    public string SampleType { get; set; } = "Blood";
}

/// <summary>
/// A3. Select Test Type – packages list + individual tests list
/// </summary>
public class LabTestSelectTypeViewModel
{
    public List<LabTestPackageItemViewModel> Packages { get; set; } = new();
    public List<LabTestIndividualItemViewModel> IndividualTests { get; set; } = new();
}

/// <summary>
/// A4. Select Pet & Collection Type – pet selection, Home collection OR In-clinic
/// </summary>
public class LabOrderSelectPetViewModel
{
    public string TestId { get; set; } = string.Empty;
    public string TestType { get; set; } = "package"; // package | individual
    public string TestDisplayName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SampleType { get; set; } = "Blood";
    public int SelectedPetId { get; set; }
    public string CollectionType { get; set; } = "Home"; // Home | InClinic
    public List<PetOptionViewModel> Pets { get; set; } = new();
}

/// <summary>
/// A6. Review & Confirm – placeholder payment, order confirmation
/// </summary>
public class LabOrderReviewViewModel
{
    public string TestId { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string TestDisplayName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SampleType { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public int PetId { get; set; }
    public string CollectionType { get; set; } = string.Empty;
    public string CollectionTypeDisplay => CollectionType == "Home" ? "Home collection" : "In-clinic";
}

/// <summary>
/// B. Lab Flow — Preventive status: Ordered → Sample Scheduled → Sample Collected → Processing → Report Uploaded
/// </summary>
public static class LabOrderStatus
{
    public const string Ordered = "Ordered";
    public const string SampleScheduled = "Sample Scheduled";
    public const string SampleCollected = "Sample Collected";
    public const string Processing = "Processing";
    public const string ReportUploaded = "Report Uploaded";
}

public class LabOrderListItemViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string TestDisplayName { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string Status { get; set; } = LabOrderStatus.Ordered;
    public string CollectionType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string OrderedAt { get; set; } = string.Empty;
    public bool HasReport { get; set; }
}

public class LabOrderDetailViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string TestDisplayName { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CollectionType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string OrderedAt { get; set; } = string.Empty;
    public string? SampleScheduledAt { get; set; }
    public string? SampleCollectedAt { get; set; }
    public string? ReportUploadedAt { get; set; }
    public bool HasReport { get; set; }
}

/// <summary>
/// C. Report Flow – Report PDF, interpretation note, CTAs: Book Online Consult, Book In-Clinic
/// </summary>
public class LabReportPageViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string TestDisplayName { get; set; } = string.Empty;
    public string ReportFileName { get; set; } = string.Empty;
    public string? ReportUrl { get; set; }
    public string? InterpretationNote { get; set; }
    public string? ReviewedAt { get; set; }
}
