namespace MediPro.Api.Data;

/// <summary>Reference cities/areas for demo data and order filter dropdowns (RWP/ISB pilot).</summary>
public static class DemoStoreLocations
{
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> AreasByCity { get; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rawalpindi"] =
            [
                "Moti Mehel", "Saddar", "Raja Bazaar", "Satellite Town", "Westridge",
                "Bahria Town Phase 7", "Commercial Market", "Chaklala Scheme 3",
            ],
            ["Islamabad"] =
            [
                "F-7 Markaz", "G-9", "G-11", "I-8", "Blue Area", "Sector H-8", "Bahria Town Islamabad",
            ],
            ["Lahore"] =
            [
                "Gulberg III", "Model Town", "Johar Town", "DHA Phase 5", "Ichhra",
            ],
            ["Karachi"] =
            [
                "Saddar", "Clifton", "Nazimabad", "North Nazimabad", "Gulshan-e-Iqbal",
            ],
            ["Faisalabad"] =
            [
                "Susan Road", "D Ground", "Peoples Colony", "Jinnah Colony",
            ],
        };

    public static IReadOnlyList<string> AllCities { get; } =
        AreasByCity.Keys.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
}
