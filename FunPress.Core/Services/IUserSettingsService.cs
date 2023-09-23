using FunPress.Common.Types.Models;

namespace FunPress.Core.Services
{
    public interface IUserSettingsService
    {
        UserSettings GetUserSettings();
        bool SaveSettings(UserSettings userSettings);
    }
}