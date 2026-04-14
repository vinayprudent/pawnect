using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// In-memory store for lab test catalog and preventive lab orders (Flow 3 – Preventive Lab Tests, Pet Parent–Initiated).
/// PawNect assigns lab internally – no lab selection UI.
/// </summary>
public static class LabTestStore
{
    private static readonly List<LabTestPackageItemViewModel> Packages = new();
    private static readonly List<LabTestIndividualItemViewModel> IndividualTests = new();
    private static readonly Dictionary<string, LabOrderDetailViewModel> Orders = new();
    private static readonly Dictionary<int, List<string>> OrdersByUser = new();
    private static readonly Dictionary<string, LabReportPageViewModel> Reports = new();
    private static int _orderSequence = 1;
    private static readonly object Lock = new();

    static LabTestStore()
    {
        SeedCatalog();
        SeedDemoOrderAndReport();
    }

    private static void SeedCatalog()
    {
        Packages.AddRange(new[]
        {
            new LabTestPackageItemViewModel
            {
                Id = "pkg-basic",
                Name = "Basic Health Check",
                Description = "Essential screening for routine wellness.",
                Price = 1_200m,
                SampleType = "Blood",
                TestsIncluded = new[] { "CBC", "Basic biochemistry" }
            },
            new LabTestPackageItemViewModel
            {
                Id = "pkg-annual",
                Name = "Annual Wellness",
                Description = "Comprehensive annual panel for dogs and cats.",
                Price = 2_500m,
                SampleType = "Blood",
                TestsIncluded = new[] { "CBC", "LFT", "KFT", "Thyroid", "Urinalysis" }
            },
            new LabTestPackageItemViewModel
            {
                Id = "pkg-senior",
                Name = "Senior Panel",
                Description = "Extended panel for senior pets (7+ years).",
                Price = 3_200m,
                SampleType = "Blood",
                TestsIncluded = new[] { "CBC", "LFT", "KFT", "Thyroid", "Urinalysis", "Cardiac markers" }
            }
        });

        IndividualTests.AddRange(new[]
        {
            new LabTestIndividualItemViewModel { Id = "ind-cbc", Name = "CBC", FullName = "Complete Blood Count", Price = 450m, SampleType = "Blood" },
            new LabTestIndividualItemViewModel { Id = "ind-lft", Name = "LFT", FullName = "Liver Function Test", Price = 550m, SampleType = "Blood" },
            new LabTestIndividualItemViewModel { Id = "ind-kft", Name = "KFT", FullName = "Kidney Function Test", Price = 550m, SampleType = "Blood" },
            new LabTestIndividualItemViewModel { Id = "ind-thyroid", Name = "Thyroid", FullName = "Thyroid Profile", Price = 650m, SampleType = "Blood" }
        });
    }

    private static void SeedDemoOrderAndReport()
    {
        lock (Lock)
        {
            var orderId = "LAB-0001";
            var order = new LabOrderDetailViewModel
            {
                OrderId = orderId,
                TestDisplayName = "Annual Wellness",
                PetName = "Max",
                Status = Models.LabOrderStatus.ReportUploaded,
                CollectionType = "Home",
                Price = 2_500m,
                OrderedAt = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd HH:mm"),
                SampleScheduledAt = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd 09:00"),
                SampleCollectedAt = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd 10:30"),
                ReportUploadedAt = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd 14:00"),
                HasReport = true
            };
            Orders[orderId] = order;
            OrdersByUser[0] = new List<string> { orderId };
            OrdersByUser[1] = new List<string> { orderId };

            Reports[orderId] = new LabReportPageViewModel
            {
                OrderId = orderId,
                PetName = "Max",
                TestDisplayName = "Annual Wellness",
                ReportFileName = "Max_Annual_Wellness_Report.pdf",
                ReportUrl = "#",
                InterpretationNote = "All parameters within normal range. No follow-up tests needed. Continue current diet and annual check-ups.",
                ReviewedAt = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd 15:00")
            };
        }
    }

    public static IReadOnlyList<LabTestPackageItemViewModel> GetPackages() => Packages.AsReadOnly();
    public static IReadOnlyList<LabTestIndividualItemViewModel> GetIndividualTests() => IndividualTests.AsReadOnly();

    public static LabTestPackageItemViewModel? GetPackage(string id) => Packages.FirstOrDefault(p => p.Id == id);
    public static LabTestIndividualItemViewModel? GetIndividualTest(string id) => IndividualTests.FirstOrDefault(t => t.Id == id);

    public static string CreateOrder(int userId, string testId, string testType, string testDisplayName, decimal price, string petName, int petId, string collectionType)
    {
        lock (Lock)
        {
            var orderId = $"LAB-{_orderSequence:D4}";
            _orderSequence++;
            var order = new LabOrderDetailViewModel
            {
                OrderId = orderId,
                TestDisplayName = testDisplayName,
                PetName = petName,
                Status = Models.LabOrderStatus.Ordered,
                CollectionType = collectionType,
                Price = price,
                OrderedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                HasReport = false
            };
            Orders[orderId] = order;
            if (!OrdersByUser.TryGetValue(userId, out var list))
            {
                list = new List<string>();
                OrdersByUser[userId] = list;
            }
            list.Add(orderId);
            return orderId;
        }
    }

    public static LabOrderDetailViewModel? GetOrder(string orderId)
    {
        lock (Lock)
        {
            return Orders.TryGetValue(orderId, out var o) ? o : null;
        }
    }

    public static IReadOnlyList<LabOrderListItemViewModel> GetOrdersByUser(int userId)
    {
        lock (Lock)
        {
            if (!OrdersByUser.TryGetValue(userId, out var ids)) return Array.Empty<LabOrderListItemViewModel>();
            return ids
                .Select(id => Orders.TryGetValue(id, out var o) ? o : null)
                .Where(o => o != null)
                .Select(o => new LabOrderListItemViewModel
                {
                    OrderId = o!.OrderId,
                    TestDisplayName = o.TestDisplayName,
                    PetName = o.PetName,
                    Status = o.Status,
                    CollectionType = o.CollectionType,
                    Price = o.Price,
                    OrderedAt = o.OrderedAt,
                    HasReport = o.HasReport
                })
                .OrderByDescending(x => x.OrderedAt)
                .ToList();
        }
    }

    public static bool UserOwnsOrder(int userId, string orderId)
    {
        lock (Lock)
        {
            return OrdersByUser.TryGetValue(userId, out var ids) && ids.Contains(orderId);
        }
    }

    public static LabReportPageViewModel? GetReport(string orderId)
    {
        lock (Lock)
        {
            return Reports.TryGetValue(orderId, out var r) ? r : null;
        }
    }

    /// <summary>
    /// For demo: mark order as Report Uploaded and attach a report.
    /// </summary>
    public static void SetReportUploaded(string orderId, string interpretationNote)
    {
        lock (Lock)
        {
            if (Orders.TryGetValue(orderId, out var order))
            {
                order.Status = Models.LabOrderStatus.ReportUploaded;
                order.ReportUploadedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
                order.HasReport = true;
            }
            Reports[orderId] = new LabReportPageViewModel
            {
                OrderId = orderId,
                PetName = Orders.TryGetValue(orderId, out var o) ? o.PetName : "",
                TestDisplayName = Orders.TryGetValue(orderId, out var o2) ? o2.TestDisplayName : "",
                ReportFileName = $"{orderId}_Report.pdf",
                ReportUrl = "#",
                InterpretationNote = interpretationNote,
                ReviewedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
            };
        }
    }
}
