using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

public static class MockVetData
{
    private static readonly List<VetViewModel> Vets = new()
    {
        new VetViewModel
        {
            Id = 1,
            Name = "Dr. Sarah Mitchell",
            Specialization = "Small Animal Medicine",
            Degree = "DVM, MVSc",
            ClinicName = "Paw Care Clinic",
            ClinicAddress = "123 Pet Lane, Downtown",
            ClinicLocation = "Downtown",
            ExperienceYears = 12,
            ConsultationFee = 45,
            Rating = 4.8,
            AvailabilityIndicator = "Today",
            AvailableToday = true,
            AvailableTomorrow = true,
            Bio = "Dr. Sarah Mitchell has over 12 years of experience in small animal medicine. She is passionate about preventive care and surgery.",
            Qualifications = "DVM (2010), MVSc Surgery (2012)",
            AreasOfExpertise = "Surgery, Internal Medicine, Vaccinations",
            PracticeRegistrationNumber = "VET-2010-001",
            TypicalCasesHandled = "Routine checkups, vaccinations, spay/neuter, dental care",
            WeeklyAvailability = "Mon–Fri 9:00 AM – 6:00 PM, Sat 9:00 AM – 1:00 PM"
        },
        new VetViewModel
        {
            Id = 2,
            Name = "Dr. James Chen",
            Specialization = "Exotic Pets & Birds",
            Degree = "BVSc, PhD",
            ClinicName = "Green Valley Veterinary Hospital",
            ClinicAddress = "456 Valley Road",
            ClinicLocation = "Green Valley",
            ExperienceYears = 8,
            ConsultationFee = 55,
            Rating = 4.6,
            AvailabilityIndicator = "Tomorrow",
            AvailableToday = false,
            AvailableTomorrow = true,
            Bio = "Dr. James Chen specializes in exotic pets and avian medicine. He has published several papers on bird health.",
            Qualifications = "BVSc (2014), PhD Avian Medicine (2018)",
            AreasOfExpertise = "Birds, Reptiles, Small Mammals",
            PracticeRegistrationNumber = "VET-2014-042",
            TypicalCasesHandled = "Bird wellness, reptile care, exotic pet emergencies",
            WeeklyAvailability = "Tue–Sat 10:00 AM – 5:00 PM"
        },
        new VetViewModel
        {
            Id = 3,
            Name = "Dr. Emily Rodriguez",
            Specialization = "Emergency & Critical Care",
            Degree = "DVM, DACVECC",
            ClinicName = "24/7 Pet Emergency",
            ClinicAddress = "789 Emergency Way",
            ClinicLocation = "Central",
            ExperienceYears = 15,
            ConsultationFee = 75,
            Rating = 4.9,
            AvailabilityIndicator = "Today",
            AvailableToday = true,
            AvailableTomorrow = true,
            Bio = "Board-certified in emergency and critical care. Dr. Rodriguez leads the emergency team at 24/7 Pet Emergency.",
            Qualifications = "DVM (2007), DACVECC (2012)",
            AreasOfExpertise = "Emergency Medicine, Critical Care, Trauma",
            PracticeRegistrationNumber = "VET-2007-015",
            TypicalCasesHandled = "Accidents, poisoning, acute illness, post-surgery care",
            WeeklyAvailability = "24/7"
        },
        new VetViewModel
        {
            Id = 4,
            Name = "Dr. Michael Brown",
            Specialization = "General Practice",
            Degree = "DVM",
            ClinicName = "Happy Paws Veterinary",
            ClinicAddress = "321 Oak Street",
            ClinicLocation = "Oak Park",
            ExperienceYears = 5,
            ConsultationFee = 35,
            Rating = 4.5,
            AvailabilityIndicator = "This week",
            AvailableToday = false,
            AvailableTomorrow = true,
            Bio = "Dr. Michael Brown provides comprehensive general practice care for dogs and cats.",
            Qualifications = "DVM (2019)",
            AreasOfExpertise = "General Practice, Vaccinations, Dermatology",
            PracticeRegistrationNumber = "VET-2019-088",
            TypicalCasesHandled = "Annual checkups, vaccinations, skin conditions, diet advice",
            WeeklyAvailability = "Mon–Fri 8:00 AM – 7:00 PM"
        },
        new VetViewModel
        {
            Id = 5,
            Name = "Dr. Lisa Wang",
            Specialization = "Dentistry & Oral Surgery",
            Degree = "DVM, AVDC",
            ClinicName = "Bright Smiles Pet Dental",
            ClinicAddress = "555 Dental Drive",
            ClinicLocation = "Medical Park",
            ExperienceYears = 10,
            ConsultationFee = 65,
            Rating = 4.7,
            AvailabilityIndicator = "Tomorrow",
            AvailableToday = false,
            AvailableTomorrow = true,
            Bio = "Dr. Lisa Wang is a board-certified veterinary dentist. She focuses on oral health and dental surgery.",
            Qualifications = "DVM (2012), AVDC (2016)",
            AreasOfExpertise = "Dental Surgery, Oral Health, Extractions",
            PracticeRegistrationNumber = "VET-2012-033",
            TypicalCasesHandled = "Dental cleanings, extractions, oral tumors, fractured teeth",
            WeeklyAvailability = "Wed–Sun 9:00 AM – 4:00 PM"
        }
    };

    public static IReadOnlyList<VetViewModel> GetAll() => Vets;

    public static VetViewModel? GetById(int id) => Vets.FirstOrDefault(v => v.Id == id);

    public static IEnumerable<VetViewModel> Search(
        string? specialization,
        int? minExperience,
        decimal? maxFee,
        double? minRating,
        string? availability,
        string sortBy,
        bool sortDesc)
    {
        var q = Vets.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(specialization))
            q = q.Where(v => v.Specialization.Contains(specialization, StringComparison.OrdinalIgnoreCase));
        if (minExperience.HasValue)
            q = q.Where(v => v.ExperienceYears >= minExperience.Value);
        if (maxFee.HasValue)
            q = q.Where(v => v.ConsultationFee <= maxFee.Value);
        if (minRating.HasValue)
            q = q.Where(v => v.Rating >= minRating.Value);
        if (!string.IsNullOrWhiteSpace(availability))
        {
            if (availability.Equals("Today", StringComparison.OrdinalIgnoreCase))
                q = q.Where(v => v.AvailableToday);
            else if (availability.Equals("Tomorrow", StringComparison.OrdinalIgnoreCase))
                q = q.Where(v => v.AvailableTomorrow);
        }
        q = sortBy?.ToLowerInvariant() switch
        {
            "rating" => sortDesc ? q.OrderByDescending(v => v.Rating) : q.OrderBy(v => v.Rating),
            "fee" => sortDesc ? q.OrderByDescending(v => v.ConsultationFee) : q.OrderBy(v => v.ConsultationFee),
            "experience" => sortDesc ? q.OrderByDescending(v => v.ExperienceYears) : q.OrderBy(v => v.ExperienceYears),
            _ => q.OrderByDescending(v => v.Rating)
        };
        return q.ToList();
    }
}
