using Microsoft.Toolkit.Mvvm.ComponentModel;
using spotify.companion.Enums;

namespace spotify.companion.Model
{
    public class Category : ObservableObject
    {
        public Category(string title, CategoryType type)
        {
            Title = title;
            Type = type;
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private CategoryType _type;
        public CategoryType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
    }
}
