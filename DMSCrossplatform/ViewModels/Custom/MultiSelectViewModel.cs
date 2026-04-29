using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSCrossplatform.Models;

namespace DMSCrossplatform.ViewModels.Custom;

    public class MultiSelectViewModel<T> : ObservableObject
    {
        private readonly ObservableCollection<SelectableItem<T>> _items = new();
        public ObservableCollection<SelectableItem<T>> Items => _items;

        private string _placeholder = "Выберите";
        public string Placeholder
        {
            get => _placeholder;
            set => SetProperty(ref _placeholder, value);
        }

        private string _displayText = "Выберите";
        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        private string _displayForeground = "#9E9E9E"; // Gray
        public string DisplayForeground
        {
            get => _displayForeground;
            set => SetProperty(ref _displayForeground, value);
        }

        private bool _isListVisible;
        public bool IsListVisible
        {
            get => _isListVisible;
            set => SetProperty(ref _isListVisible, value);
        }

        private bool _enableSearch;
        public bool EnableSearch
        {
            get => _enableSearch;
            set => SetProperty(ref _enableSearch, value);
        }

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    UpdateFilteredItems();
            }
        }

        private ObservableCollection<SelectableItem<T>> _filteredItems = new();
        public ObservableCollection<SelectableItem<T>> FilteredItems
        {
            get => _filteredItems;
            set => SetProperty(ref _filteredItems, value);
        }

        public IRelayCommand ToggleListCommand { get; }
        public IRelayCommand ClearSelectionCommand { get; }
        public IRelayCommand SelectAllCommand { get; }

        private Func<T, string> _displaySelector;
        private IEnumerable<T> _sourceItems;

        public MultiSelectViewModel(IEnumerable<T> sourceItems, Func<T, string> displaySelector, string placeholder, bool selectAllByDefault = false)
        {
            _sourceItems = sourceItems ?? throw new ArgumentNullException(nameof(sourceItems));
            _displaySelector = displaySelector ?? throw new ArgumentNullException(nameof(displaySelector));
            Placeholder = placeholder;

            ToggleListCommand = new RelayCommand(ToggleList);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            SelectAllCommand = new RelayCommand(SelectAll);

            InitializeItems(selectAllByDefault);
            UpdateDisplay();
        }

        private void InitializeItems(bool selectAllByDefault = false)
        {
            _items.Clear();

            foreach (var item in _sourceItems)
            {
                var selectable = new SelectableItem<T>(item, _displaySelector(item))
                {
                    // Устанавливаем IsSelected в зависимости от параметра
                    IsSelected = selectAllByDefault
                };

                selectable.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableItem<T>.IsSelected))
                    {
                        UpdateDisplay();
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                };

                _items.Add(selectable);
            }

            UpdateFilteredItems();
        }

        private void UpdateFilteredItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems = new ObservableCollection<SelectableItem<T>>(_items);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = _items
                    .Where(i => i.DisplayText.ToLower().Contains(searchLower))
                    .ToList();

                FilteredItems = new ObservableCollection<SelectableItem<T>>(filtered);
            }
        }

        private void ToggleList()
        {
            IsListVisible = !IsListVisible;

            if (IsListVisible && EnableSearch)
            {
                SearchText = string.Empty;
            }
        }

        private void ClearSelection()
        {
            foreach (var item in _items)
                item.IsSelected = false;

            UpdateDisplay();
        }

        private void SelectAll()
        {
            foreach (var item in FilteredItems)
                item.IsSelected = true;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var selectedCount = _items.Count(i => i.IsSelected);

            if (selectedCount == 0)
            {
                DisplayText = Placeholder;
                DisplayForeground = "#9E9E9E"; // Gray
            }
            else if (selectedCount == 1)
            {
                var selected = _items.First(i => i.IsSelected);
                DisplayText = selected.DisplayText;
                DisplayForeground = "#000000"; // Black
            }
            else
            {
                DisplayText = $"{selectedCount} выбрано";
                DisplayForeground = "#000000"; // Black
            }
        }

        public IEnumerable<T> GetSelectedItems()
        {
            return _items
                .Where(i => i.IsSelected)
                .Select(i => i.Item)
                .ToList();
        }

        public List<T> GetSelectedItemsList()
        {
            return GetSelectedItems().ToList();
        }

        public void UpdateSource(IEnumerable<T> newSource, bool selectAllByDefault = false)
        {
            _sourceItems = newSource;
            InitializeItems(selectAllByDefault);
        }

        public event EventHandler? SelectionChanged;
    }