namespace MachSoft.UI.Showcase.Models;

public static class CatalogRegistry
{
    public static IReadOnlyList<(string Title, string Route)> FoundationPages { get; } =
    [
        ("Colors", "/foundations/colors")
    ];

    public static IReadOnlyList<(string Title, string Route)> ComponentPages { get; } =
    [
        ("Buttons", "/components/buttons"),
        ("TextFields", "/components/textfields"),
        ("Cards", "/components/cards"),
        ("Select", "/components/select"),
        ("TextArea", "/components/textarea"),
        ("Checkbox", "/components/checkbox"),
        ("Switch", "/components/switch"),
        ("Radio", "/components/radio"),
        ("Alert", "/components/alert"),
        ("Badge", "/components/badge"),
        ("Chip", "/components/chip"),
        ("Tag", "/components/tag"),
        ("Avatar", "/components/avatar"),
        ("Progress", "/components/progress"),
        ("Skeleton", "/components/skeleton"),
        ("Tooltip", "/components/tooltip"),
        ("Accordion", "/components/accordion"),
        ("Tabs", "/components/tabs"),
        ("Table", "/components/table"),
        ("DataGrid", "/components/datagrid"),
        ("SearchBox", "/components/searchbox"),
        ("FilterBar", "/components/filterbar"),
        ("Dialog", "/components/dialog"),
        ("EmptyState", "/components/emptystate"),
        ("LoadingState", "/components/loadingstate"),
        ("ErrorState", "/components/errorstate"),
        ("DatePicker", "/components/datepicker"),
        ("DateRangePicker", "/components/daterangepicker"),
        ("TimePicker", "/components/timepicker"),
        ("FileUpload", "/components/fileupload"),
        ("Autocomplete", "/components/autocomplete"),
        ("MultiSelect", "/components/multiselect")
    ];
}
