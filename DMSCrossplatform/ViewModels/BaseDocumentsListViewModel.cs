using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform;
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
    private readonly INavigationService<MenuRegionState> _navigationService;
    
    public static string? Mode = null;

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
    
    private DocumentFullReadDto? _selectedDocument;

    public DocumentFullReadDto? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            SetProperty(ref _selectedDocument, value);
            if (value == null)
                return;
            DocumentViewModel.Mode = Mode;
            App.SelectedDocumentId = value.Id;
            _navigationService.NavigateTo<DocumentViewModel>();
        } 
    }

    
    public BaseDocumentsListViewModel(
        INavigationService<MenuRegionState> navigationService,
        ILogger<ApiClient> log,
        IDictionariesService dictionaryService,
        IDocumentService documentService,
        IUserService userService)
    {
        _navigationService = navigationService;
        _log = log;
        _dictionaryService = dictionaryService;
        _documentService = documentService;
        _userService = userService;
        _ = InitializeAsync();
    }

    protected async Task InitializeAsync()
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
            
            CategoryMultiSelect = new MultiSelectViewModel<SimpleDto>(categories.Result, c => c.Name, "Тип документа",
                selectAllByDefault: true);

            var selectedAuthors = authors.Result.Where(a => a.RoleId != 2 && a.RoleId != 3).ToList();
            
            AuthorMultiSelect =
                new MultiSelectViewModel<UserFullDto>(selectedAuthors, u => u.FullName, "Автор",
                    selectAllByDefault: true);

            StatusMultiSelect =
                new MultiSelectViewModel<SimpleDto>(statuses.Result, d => d.Name, "Статус", selectAllByDefault: true);

            var documentFullReadDtos = documents.Result;
            Documents = new ObservableCollection<DocumentFullReadDto>(documentFullReadDtos);
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

    [RelayCommand]
    private async Task Search()
    {
        var categories = CategoryMultiSelect?
            .Items
            .Where(i => i.IsSelected)
            .Select(c => new SimpleDto()
            {
                Id = c.Item.Id
            })
            .Select(c => c.Id)
            .ToList();

        var statuses = StatusMultiSelect?
            .Items
            .Where(i => i.IsSelected)
            .Select(s => new SimpleDto()
            {
                Id = s.Item.Id
            }).
            Select(s => s.Id)
            .ToList();

        var authors = AuthorMultiSelect?
            .Items
            .Where(i => i.IsSelected)
            .Select(u => new UserFullDto()
            {
                UserId = u.Item.UserId 
            })
            .Select(u => u.UserId).ToList();
        var s1 = FromDate?.DateTime.ToString("yyyy-MM-dd");
        var s2 = ToDate?.DateTime.ToString("yyyy-MM-dd");


        var docs = 
            await LoadDocumentsAsync(s1, s2, statuses, categories, authors, SearchQuery);
        Documents = new ObservableCollection<DocumentFullReadDto>(docs);
    }

    private async Task<IReadOnlyCollection<DocumentFullReadDto>> 
        LoadDocumentsAsync
        (
            string? fromDate = null,
            string? toDate = null,
            List<int>? statuses = null, 
            List<int>? categories = null, 
            List<Guid>? authors = null,
            string? search = null
        )
    {
        return await _documentService.ListAsync(
            mode: Mode,
            search: search,
            authors: authors,
            categoryId: categories,
            statusId: statuses,
            startDate: fromDate,
            endDate: toDate
            );
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
    [RelayCommand]
    private void OpenDocument(Guid? documentId)
    {
        if (documentId == null)
        {
            return;
        }
        App.SelectedDocumentId = documentId;
        DocumentViewModel.Mode = Mode;
        _navigationService.NavigateTo<DocumentViewModel>();
    }


}
