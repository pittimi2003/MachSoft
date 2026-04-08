using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Model
{
    public class ComboBoxSelectItem<T>
    {
        private readonly ILocalizationService _l; 
        private string _text = string.Empty;

        public ComboBoxSelectItem(ILocalizationService localization)
        {
            _l = localization;
        }

        public T? Value { get; set; }

        public string Text 
        {
            get => _l.Loc(_text);
            set => _text = value;
        }
    }
}
