using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DMSCrossplatform.ViewModels;

public partial class UserEditViewModel : ViewModelBase
{
    [ObservableProperty] private UserProfileEditViewModel _userProfile;
    [ObservableProperty] private RoleEditViewModel _roleEditor;
    [ObservableProperty] private UnitEditViewModel _unitEditor;

    public UserEditViewModel(
        UserProfileEditViewModel userProfile,
        RoleEditViewModel roleEditor,
        UnitEditViewModel unitEditor)
    {
        UserProfile = userProfile;
        RoleEditor = roleEditor;
        UnitEditor = unitEditor;

        UserProfile.Saved += OnChildSaved;
        RoleEditor.Saved += OnDictionarySaved;
        UnitEditor.Saved += OnDictionarySaved;
    }

    public event EventHandler? Saved;

    public void BeginCreateUser()
    {
        UserProfile.BeginCreate();
    }

    public async Task BeginEditUser(Models.Dto.UserFullDto user)
    {
        await UserProfile.BeginEdit(user);
    }

    private void OnChildSaved(object? sender, EventArgs e)
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }

    private async void OnDictionarySaved(object? sender, EventArgs e)
    {
        await RefreshDictionaries();
        Saved?.Invoke(this, EventArgs.Empty);
    }

    private async Task RefreshDictionaries()
    {
        await Task.WhenAll(
            UserProfile.LoadDictionaries(),
            RoleEditor.LoadDictionaries(),
            UnitEditor.LoadDictionaries());
    }
}
