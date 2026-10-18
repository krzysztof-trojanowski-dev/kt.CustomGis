
using kt.CustomGis.AppMain.Services;

namespace kt.CustomGis.App.Tests.Services;

[TestFixture]
public class AppSettingsTests
{
    [Test]
    public void PageSize_SetToNewValue_RaisesPropertyChanged()
    {
        var settings = new AppSettings();
        var events = new List<string?>();
        settings.PropertyChanged += (_, e) =>
        {
            events.Add(e.PropertyName);
        };

        settings.PageSize = 20;

        Assert.That(events, Contains.Item(nameof(AppSettings.PageSize)));
    }

    [Test]
    public void PageSize_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        var settings = new AppSettings();
        var raised = false;
        settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppSettings.PageSize))
            {
                raised = true;
            }
        };

        settings.PageSize = settings.PageSize;

        Assert.That(raised, Is.False);
    }
}
