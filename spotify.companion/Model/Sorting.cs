using spotify.companion.Enums;

namespace spotify.companion.Model
{
    internal class Sorting
    {
        public Sorting(string title, string property, SortingType type)
        {
            Title = title;
            Property = property;
            Type = type;
        }

        public string Title { get; set; }
        public string Property { get; set; }
        public SortingType Type { get; set; }
    }
}
