using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Infrastructure.Logging;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels.Custom;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.ViewModels;

public partial class BaseDocumentsListViewModel: ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IUserService _userService;
    private readonly IDictionariesService _dictionaryService;
    private readonly ILogger<ApiClient> _log;

    [ObservableProperty] private MultiSelectViewModel<SimpleDto>? _categoryMultiSelect;
    [ObservableProperty] private MultiSelectViewModel<SimpleDto>? _statusMultiSelect;
    [ObservableProperty] private MultiSelectViewModel<UserFullDto>? _authorMultiSelect;
    [ObservableProperty] private string? _searchQuery;
    [ObservableProperty] private DateTimeOffset? _fromDate;
    [ObservableProperty] private DateTimeOffset? _toDate;
    [ObservableProperty] private UserFullDto? _selectedAuthor;
    [ObservableProperty] private ObservableCollection<DocumentFullReadDto> _documents = [];
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _loadingMessage = "Загрузка...";

    
    public BaseDocumentsListViewModel(
        ILogger<ApiClient> log,
        IDictionariesService dictionaryService,
        IDocumentService documentService,
        IUserService userService)
    {
        _log = log;
        _dictionaryService = dictionaryService;
        _documentService = documentService;
        _userService = userService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        LoadingMessage = "Загрузка документов...";
        try
        {
            var statuses = GetStatuses();
            var categories = GetCategories();
            var documents = LoadDocumentsAsync();
            var authors = GetAuthors();

            await Task.WhenAll(statuses, categories, documents, authors);

            LoadingMessage = "Загрузка данных пользователя...";
            CategoryMultiSelect = new MultiSelectViewModel<SimpleDto>(categories.Result, c => c.Name, "Тип документа",
                selectAllByDefault: true);

            LoadingMessage = "Загрузка...";
            AuthorMultiSelect =
                new MultiSelectViewModel<UserFullDto>(authors.Result, u => u.FullName, "Автор",
                    selectAllByDefault: true);

            StatusMultiSelect =
                new MultiSelectViewModel<SimpleDto>(statuses.Result, d => d.Name, "Статус", selectAllByDefault: true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error");
        }
        finally
        {
            IsLoading = false;
        }
        
    }

    private async Task<IReadOnlyCollection<DocumentFullReadDto>> LoadDocumentsAsync()
    {
        return await _documentService.ListAsync();
    }

    private async Task<IReadOnlyCollection<SimpleDto>> GetStatuses()
    {
        return  await _dictionaryService.GetStatusesAsync();
    }

    private async Task<IReadOnlyCollection<SimpleDto>> GetCategories()
    {
        return await _dictionaryService.GetCategoriesAsync();
    }

    private async Task<IReadOnlyCollection<UserFullDto>> GetAuthors()
    {
        return await _userService.GetAllAsync();
    }


}