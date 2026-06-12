using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Infrastructure.Policy;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels.Custom;
using Microsoft.AspNetCore.StaticFiles;

namespace DMSCrossplatform.ViewModels;

public partial class UploadDocumentViewModel: ViewModelBase
{
    
        private readonly IDocumentService _documentService;
        private readonly IDictionariesService _dictionariesService;
        private readonly IStorageProvider? _storageProvider;
        private readonly ISessionService _sessionService;
        private readonly IPolicy _policy; 

        [ObservableProperty] private string? _selectedFileName;
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        [ObservableProperty] private string? _selectedFilePath;
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        [ObservableProperty] private string? _documentTitle = string.Empty;

        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        [ObservableProperty] private SimpleDto? _selectedFileCategory;

        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        [ObservableProperty] private bool _isEnabled = true;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _selectedFileNameVisbility;
        
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private MultiSelectViewModel<UnitReadDto> _units;
        
        [ObservableProperty] private DateTimeOffset? _toDate;
        
        [ObservableProperty] private ObservableCollection<SimpleDto> _categories  = [];

        public IAsyncRelayCommand SelectFileCommand { get; }
        public IAsyncRelayCommand SubmitCommand { get; }


        public UploadDocumentViewModel(
            IPolicy policy,
            IDictionariesService dictionariesService, 
            IDocumentService documentService, 
            ISessionService sessionService)
        {
            _policy = policy;
            _dictionariesService = dictionariesService;
            _documentService = documentService;
            _sessionService = sessionService;
            _storageProvider = App.storageProvider;
            SelectFileCommand = new AsyncRelayCommand(SelectFileAsync);
            SubmitCommand = new AsyncRelayCommand(SubmitDocumentAsync, CanSubmitDocument);
            
            _ = LoadCategories();
            _ = LoadUnits();
        }

        private async Task LoadCategories()
        {
            if (_policy is DirectorPolicy)
            {
                var categories = await _dictionariesService.GetCategoriesAsync();
                Categories = new ObservableCollection<SimpleDto>(categories);
            }
            else
            {
                var selectedCategoryIds = (await _dictionariesService.GetRoleCategoriesAsync(_sessionService.CurrentUser.RoleId))
                    .Select(c => c.CategoryId)
                    .ToHashSet();
            
                var categories = await _dictionariesService.GetCategoriesAsync();
                Categories = new ObservableCollection<SimpleDto>(categories.Where(c => selectedCategoryIds.Contains(c.Id)));
            }
        }

        private async Task SelectFileAsync()
        {
            if (_storageProvider == null)
            {
                ErrorMessage = "Выбор файла недоступен";
                return;
            }

            try
            {
                var options = new FilePickerOpenOptions
                {
                    Title = "Выберите документ",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Документы")
                        {
                            Patterns = ["*.pdf", "*.docx"],
                            AppleUniformTypeIdentifiers = ["com.adobe.pdf", "org.openxmlformats.wordprocessingml.document"],
                            MimeTypes = ["application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
                        }
                    ]
                };

                var files = await _storageProvider.OpenFilePickerAsync(options);

                if (files.Count == 1)
                {
                    var file = files[0];
                    SelectedFilePath = file.Path.LocalPath;
                    SelectedFileName = file.Name;
                    SelectedFileNameVisbility = true;


                   
                    if (OperatingSystem.IsAndroid())
                    {
                        await CopyFileForAndroidAsync(file);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
        }

        private async Task CopyFileForAndroidAsync(IStorageFile file)
        {

            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var targetPath = Path.Combine(appDataPath, "temp_documents", file.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

                await using var sourceStream = await file.OpenReadAsync();
                await using var targetStream = File.Create(targetPath);

                await sourceStream.CopyToAsync(targetStream);

                SelectedFilePath = targetPath;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
        }
        private async Task LoadUnits()
        {
            var units = await _dictionariesService.GetUnitsAsync();
            Units = new MultiSelectViewModel<UnitReadDto>(units, u => u.Name, "Отдел", selectAllByDefault: true);
        }

        private bool CanSubmitDocument()
        {
            return !string.IsNullOrWhiteSpace(DocumentTitle) &&
                   SelectedFileCategory != null &&
                   !string.IsNullOrWhiteSpace(SelectedFilePath) &&
                   IsEnabled;
        }

        private async Task SubmitDocumentAsync()
        {
            
            if (string.IsNullOrWhiteSpace(SelectedFilePath) || !File.Exists(SelectedFilePath))
            {
                ErrorMessage = "Файл не выбран";
                return;
            }

            if (string.IsNullOrEmpty(DocumentTitle))
            {
                ErrorMessage = "Имя документа не указано";
                return;
            }

            if (ToDate != null)
            {
                if (ToDate?.Date < DateTime.Now.Date)
                {
                    ErrorMessage = "Указана некорректная дата";
                    return;
                }
            }
            var units = Units?
                .Items
                .Where(i => i.IsSelected)
                .Select(c => new UnitReadDto()
                {
                    Id = c.Item.Id
                })
                .Select(c => c.Id)
                .ToList();
            if (units == null || units.Count == 0)
            {
                ErrorMessage = "Укажите хотя бы один отдел";
                return;
            }

            if (SelectedFileCategory == null)
            {
                ErrorMessage = "Выберете категорию";
                return;
            }
            

            try
            {
                IsEnabled = false;
                IsLoading = true;
                
                if (OperatingSystem.IsAndroid())
                {
                    await EnsureFileAccessAsync(SelectedFilePath);
                }
                var provider = new FileExtensionContentTypeProvider();

                
                string contentType = provider.TryGetContentType(SelectedFilePath, out contentType) ?  contentType : "application/octet-stream";

                var documentReadDto = await _documentService.CreateAsync(new DocumentCreateDto
                {
                    Title = DocumentTitle,
                    UnitId = (int)_sessionService.CurrentUser.UnitId,
                    CategoryId = SelectedFileCategory.Id,
                    ExpiresAt = ToDate?.Date
                });
                
                await _documentService.CreateDocumentUnits(documentReadDto.Id, new CreateDocumentUnitDto()
                {
                    UnitsIds = units
                });
                
                var fileBytes = File.ReadAllBytes(SelectedFilePath);
                
                var doc = await _documentService.UploadVersionAsync(
                    documentReadDto.Id, fileBytes, 
                    SelectedFileName, 
                    contentType
                    );
                
                ResetForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки: " + ex.Message;
            }
            finally
            {
                IsEnabled = true;
                IsLoading = false;
                SubmitCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task EnsureFileAccessAsync(string filePath)
        {
            try
            {
                await using var testStream = File.OpenRead(filePath);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException("Нет доступа к файлу. Разрешите приложению доступ к файлам.");
            }
        }

        private void ResetForm()
        {
            DocumentTitle = string.Empty;
            SelectedFileCategory = null;
            SelectedFileName = null;
            SelectedFilePath = null;
        }
}