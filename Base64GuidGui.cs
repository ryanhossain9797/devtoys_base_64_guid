using Base64Guid;
using DevToys.Api;
using System.ComponentModel.Composition;
using static DevToys.Api.GUI;

namespace Base64Guid;

[Export(typeof(IGuiTool))]
[Name("Base64Guid")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0106',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(Base64GuidResourceAssemblyIdentifier),
    ResourceManagerBaseName = "Base64Guid.Base64Guid",
    ShortDisplayTitleResourceName = nameof(Base64Guid.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(Base64Guid.LongDisplayTitle),
    DescriptionResourceName = nameof(Base64Guid.Description),
    AccessibleNameResourceName = nameof(Base64Guid.AccessibleName))]
internal sealed class Base64GuidGui : IGuiTool
{
    private UIToolView? _view;
    private readonly ISettingsProvider _settingsProvider;

    private static readonly SettingDefinition<bool> UrlSafeSetting
        = new(
            name: $"{nameof(Base64GuidGui)}.{nameof(UrlSafeSetting)}",
            defaultValue: true);
    private readonly IUISwitch _urlSafeSwitch = Switch("url-safe-switch");
    
    private static readonly SettingDefinition<bool> TrimPaddingSetting
        = new(
            name: $"{nameof(Base64GuidGui)}.{nameof(TrimPaddingSetting)}",
            defaultValue: true);
    private readonly IUISwitch _trimPaddingSwitch = Switch("trim-padding-switch");
    
    private readonly IUILabel _errorMessage = Label("").Style(UILabelStyle.BodyStrong).Hide();
    private readonly IUISingleLineTextInput _guidText = SingleLineTextInput("_guidText").Title(Base64Guid.GuidText);
    private readonly IUISingleLineTextInput _base64GuidText = SingleLineTextInput("_base64GuidText").Title(Base64Guid.Base64GuidText);

    [ImportingConstructor]
    public Base64GuidGui(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        var urlSafeSetting = _settingsProvider.GetSetting(UrlSafeSetting);
        var trimPaddingSetting = _settingsProvider.GetSetting(TrimPaddingSetting);

        if (urlSafeSetting) { _urlSafeSwitch.On(); } else { _urlSafeSwitch.Off(); }
        if (trimPaddingSetting) { _trimPaddingSwitch.On(); } else { _trimPaddingSwitch.Off(); }
    }

    public UIToolView View
    {
        get
        {
            _view ??=
                new UIToolView(
                    Stack().Vertical().WithChildren(
                        Setting("url-safe-setting")
                            .Icon("FluentSystemIcons", '\uF18D')
                            .Title(Base64Guid.UrlSafeTitle)
                            .Description(Base64Guid.UrlSafeDescription)
                            .InteractiveElement(
                                _urlSafeSwitch
                                    .OnToggle(OnUrlSafeToggle)
                            ),
                        Setting("trim-padding-setting")
                            .Icon("FluentSystemIcons", '\uF18D')
                            .Title(Base64Guid.TrimPaddingTitle)
                            .Description(Base64Guid.TrimPaddingDescription)
                            .InteractiveElement(
                                _trimPaddingSwitch
                                    .OnToggle(OnTrimPaddingToggle)
                            ),
                        _guidText,
                        Stack().Horizontal().WithChildren(
                            Button().Text(Base64Guid.ToBase64Guid).OnClick(OnToBase64GuidClick),
                            Button().Text(Base64Guid.ToGuid).OnClick(OnToGuidClick),
                            _errorMessage),
                        _base64GuidText
                    ));

            return _view;
        }
    }

    private void OnUrlSafeToggle(bool toolMode)
    {
        _settingsProvider.SetSetting(UrlSafeSetting, toolMode);
    }
    
    private void OnTrimPaddingToggle(bool toolMode)
    {
        _settingsProvider.SetSetting(TrimPaddingSetting, toolMode);
    }
    
    private async ValueTask OnToBase64GuidClick()
    {
        var guidInput = _guidText.Text;

        if (Guid.TryParse(guidInput.Trim(), out Guid result))
        {
            var urlSafeSetting = _settingsProvider.GetSetting(UrlSafeSetting);
            var trimPaddingSetting = _settingsProvider.GetSetting(TrimPaddingSetting);
            
            var base64GuidString =
                Convert.ToBase64String(result.ToByteArray());

            if (urlSafeSetting) { base64GuidString = base64GuidString.Replace('+', '-').Replace('/', '_'); }
            if (trimPaddingSetting) { base64GuidString = base64GuidString[..^2]; }

            _errorMessage.Text("").Hide();
            _base64GuidText.Text(base64GuidString);
        }
        else
        {
            _errorMessage.Text("Not a valid GUID").Show();
        }
    }

    private async ValueTask OnToGuidClick()
    {
        var urlSafeSetting = _settingsProvider.GetSetting(UrlSafeSetting);
        var trimPaddingSetting = _settingsProvider.GetSetting(TrimPaddingSetting);
        var base64GuidString = _base64GuidText.Text;

        if (urlSafeSetting)
        {
            if (base64GuidString.Contains('/') || base64GuidString.Contains('+'))
            {
                _errorMessage.Text("Url safe Base 64 Guid can't contain / or +").Show();
                return;
            }
            base64GuidString = base64GuidString.Replace('_', '/').Replace('-', '+');
        }

        if (trimPaddingSetting)
        {
            if (base64GuidString.EndsWith("=="))
            {
                _errorMessage.Text("Trimmed padding Base 64 Guid cannot end with ==").Show();
                return;
            }

            base64GuidString += "==";
        }

        try
        {
            var guid = new Guid(Convert.FromBase64String(base64GuidString));
            _errorMessage.Text("").Hide();
            _guidText.Text(guid.ToString());
        }
        catch (Exception)
        {
            _errorMessage.Text("Not a valid Base 64 GUID").Show();
        }
    }

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}