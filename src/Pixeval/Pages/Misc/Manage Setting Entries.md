To add a new setting entry, follow the instructions:

1. Create a property that corresponds to the setting entry you want to add in Pixeval.AppManagement.AppSetting class.
2. Add a [DefaultValue] attribute to indicates the default value of your property, if the value is of primitive type, provide the value directly to the constructor of `[DefaultValue]`, otherwise, create a class that implements Pixeval.Misc.IDefaultValueProvider and use it as factory to create the default value, then pass that class to the `[DefaultValue]` attribute using `typeof`, e.g., `[DefaultValue(typeof(AppWidthDefaultValueProvider))]`
3. If that property is non-trivial, i.e., you want to have some side-effects when you set the value of that property you just created (you want to do something other that setting the value of the property), create a property in `Pixeval.Pages.Misc.SettingsPageViewModel`, provided the same `[DefaultValue]` attribute as in the `AppSetting` class, then define your own logic in the setting, e.g.:
    ```cs
    [DefaultValue(false)]
    public bool DisableDomainFronting
    {
        get => _appSetting.DisableDomainFronting;
        set => SetProperty(_appSetting.DisableDomainFronting, value, _appSetting, (setting, value) =>
        {
            setting.DisableDomainFronting = value;
            // This line defines the extra logic
            App.AppViewModel.MakoClient.Configuration.Bypass = !value;
        });
    }
    ```
4. Create a setting entry UI in the `Pixeval.Pages.Misc.SettingsPage.xaml`, there are several kinds of setting entry control you can use, they reside in `Pixeval.Controls.Setting.UI` namespace. Specifically, if you are using the `SingleSelectionSettingEntry`, that is, a setting entry that contains a single selection value, usually binds to a list of enum values and select one of which, follow the following pattern:
    1. Create a class that implements the `Pixeval.Controls.Setting.UI.Model.IStringRepresentableItem`, the `IStringRepresentableItem.Item`, each `IStringRepresentableItem` represents an option that can be bind to a `ComboBox` or `RadioButtons`, the `IStringRepresentableItem.Item` will be the enum value, and the `IStringRepresentableItem.StringRepresentation` will control how the item will be displayed on the UI. After all of this, create a field named `AvailableItems` and include all the possible `IStringRepresentableItem`, a typical implementation would be:
        ```cs
        public record IllustrationViewOptionSettingEntryItem : IStringRepresentableItem
        {
            public IllustrationViewOptionSettingEntryItem(IllustrationViewOption item)
            {
                Item = item;
                StringRepresentation = item switch
                {
                    IllustrationViewOption.Regular => MiscResources.IllustrationViewRegularLayout,
                    IllustrationViewOption.Justified => MiscResources.IllustrationViewJustifiedLayout,
                    _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
                };
            }

            public object Item { get; }

            public string StringRepresentation { get; }

            public static IEnumerable<IStringRepresentableItem> AvailableItems { get; } = Enum.GetValues<IllustrationViewOption>().Select(i => new IllustrationViewOptionSettingEntryItem(i));
        }
        ```

        **Note that the implementation is a `record`, which defines the by-value comparison semantic automatically, if you are defining an implementation of `IStringRepresentableItem` using a `class`, remember to override the `Equals` and `HashCode` method to perform a by-value comparison, this is vital.**