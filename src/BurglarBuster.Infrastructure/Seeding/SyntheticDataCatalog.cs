namespace BurglarBuster.Infrastructure.Seeding;

/// <summary>
/// Fixed pools of fictional name/place/appearance data used by both the fixture
/// scenarios and the bulk generator. Entirely invented — no real people, places or
/// organizations. See BURGLAR_BUSTER_PROJECT_SPEC.md §2.
/// </summary>
internal static class SyntheticDataCatalog
{
    public static readonly string[] GivenNames =
    [
        "Daniel", "Sarah", "Robert", "Emily", "Michael", "Olivia", "James", "Ava",
        "William", "Isabella", "David", "Sophia", "Joseph", "Mia", "Thomas", "Charlotte",
        "Christopher", "Amelia", "Matthew", "Harper", "Andrew", "Evelyn", "Benjamin", "Abigail",
        "Samuel", "Emma", "Nathan", "Grace", "Ethan", "Chloe", "Lucas", "Zoe",
        "Henry", "Ruby", "Jack", "Ella", "Owen", "Lily", "Noah", "Hannah",
    ];

    public static readonly string[] FamilyNames =
    [
        "Miller", "Nguyen", "Thompson", "Brooks", "Carter", "Diaz", "Ellison", "Foster",
        "Grant", "Harrington", "Ibrahim", "Jansen", "Kowalski", "Lindqvist", "Mercer", "Novak",
        "O'Brien", "Petrov", "Quintero", "Reyes", "Santos", "Tanaka", "Ueda", "Vasquez",
        "Whitfield", "Xu", "Yilmaz", "Zimmerman", "Bianchi", "Clarke", "Dubois", "Eriksson",
    ];

    public static readonly (string Locality, string State)[] Localities =
    [
        ("Bridgewater Hollow", "VC"), ("Fern Creek", "VC"), ("Maple Junction", "HR"),
        ("Stonebridge", "HR"), ("Amber Valley", "SB"), ("Cedar Falls", "SB"),
        ("Silverbrook", "AV"), ("Thistledown", "AV"), ("Fernhollow", "FH"),
        ("Millbrook", "FH"), ("Ravenswood", "VC"), ("Elmsworth", "HR"),
        ("Camden Reach", "SB"), ("Windermere", "AV"), ("Oakhaven", "FH"),
    ];

    public static readonly string[] StreetNames =
    [
        "Birchwood", "Hollyfield", "Kestrel", "Larkspur", "Meadowview", "Northgate",
        "Orchard", "Pinehurst", "Quarry", "Riverside", "Sycamore", "Thornbury",
        "Union", "Vineyard", "Westfield", "Ashgrove",
    ];

    public static readonly string[] StreetTypes = ["Street", "Road", "Avenue", "Lane", "Court", "Drive"];

    public static readonly string[] EyeColours = ["Brown", "Blue", "Green", "Hazel", "Grey"];

    public static readonly string[] HairColours = ["Black", "Brown", "Blond", "Red", "Grey", "White"];

    public static readonly string[] DistinguishingMarks =
    [
        "Small scar above left eyebrow",
        "Tattoo on right forearm",
        "Mole on left cheek",
        "Scar on chin",
        "Birthmark on neck",
    ];
}
