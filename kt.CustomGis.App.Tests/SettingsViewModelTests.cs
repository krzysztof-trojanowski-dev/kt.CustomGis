using kt.CustomGis.AppMain.Services;
using kt.CustomGis.AppMain.ViewModels;

namespace kt.CustomGis.App.Tests;

[TestFixture]
public class SettingsViewModelTests
{
    [Test]
    public void SaveCommand_UpdatesSettings_AndRequestsCloseWithTrue()
    {
        var settings = new AppSettings { PageSize = 50 };
        var vm = new SettingsViewModel(settings)
        {
            SelectedPageSize = 20,
        };

        bool? closeResult = null;
        vm.CloseRequested += result => closeResult = result;

        vm.SaveCommand.Execute(null);

        Assert.That(settings.PageSize, Is.EqualTo(20));
        Assert.That(closeResult, Is.True);
    }

    [Test]
    public void CloseCommand_RequestsCloseWithFalse()
    {
        var vm = new SettingsViewModel(new AppSettings());

        bool? closeResult = null;
        vm.CloseRequested += result => closeResult = result;

        vm.CloseCommand.Execute(null);

        Assert.That(closeResult, Is.False);
    }
}
