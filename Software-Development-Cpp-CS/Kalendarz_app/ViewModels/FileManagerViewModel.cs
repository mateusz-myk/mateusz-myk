using Kalendarz.Helpers;
using Kalendarz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace Kalendarz.ViewModels
{
    public class FileManagerViewModel : ViewModelBase
    {
        private readonly string DataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Kalendarz", "filemanager.json");

        public Plan Plan { get; }
        public ICommand BackCommand { get; }

        private ObservableCollection<FolderItem> _rootFolders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> RootFolders
        {
            get => _rootFolders;
            set => SetProperty(ref _rootFolders, value);
        }

        private ObservableCollection<StickyNote> _rootNotes = new ObservableCollection<StickyNote>();
        public ObservableCollection<StickyNote> RootNotes
        {
            get => _rootNotes;
            set => SetProperty(ref _rootNotes, value);
        }

        private ObservableCollection<FolderItem> _currentFolders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> CurrentFolders
        {
            get => _currentFolders;
            set => SetProperty(ref _currentFolders, value);
        }

        private ObservableCollection<StickyNote> _currentNotes = new ObservableCollection<StickyNote>();
        public ObservableCollection<StickyNote> CurrentNotes
        {
            get => _currentNotes;
            set => SetProperty(ref _currentNotes, value);
        }

        private ObservableCollection<Arrow> _rootArrows = new ObservableCollection<Arrow>();
        public ObservableCollection<Arrow> RootArrows
        {
            get => _rootArrows;
            set => SetProperty(ref _rootArrows, value);
        }

        private ObservableCollection<Arrow> _currentArrows = new ObservableCollection<Arrow>();
        public ObservableCollection<Arrow> CurrentArrows
        {
            get => _currentArrows;
            set => SetProperty(ref _currentArrows, value);
        }

        private ObservableCollection<ImageNote> _rootImages = new ObservableCollection<ImageNote>();
        public ObservableCollection<ImageNote> RootImages
        {
            get => _rootImages;
            set => SetProperty(ref _rootImages, value);
        }

        private ObservableCollection<ImageNote> _currentImages = new ObservableCollection<ImageNote>();
        public ObservableCollection<ImageNote> CurrentImages
        {
            get => _currentImages;
            set => SetProperty(ref _currentImages, value);
        }

        private ObservableCollection<PlanShortcut> _shortcuts = new ObservableCollection<PlanShortcut>();
        public ObservableCollection<PlanShortcut> Shortcuts
        {
            get => _shortcuts;
            set => SetProperty(ref _shortcuts, value);
        }

        private FolderItem? _currentFolder;
        public FolderItem? CurrentFolder
        {
            get => _currentFolder;
            set
            {
                SetProperty(ref _currentFolder, value);
                UpdateBreadcrumb();
                LoadCurrentFolderContent();
                OnPropertyChanged(nameof(CurrentCanvasBackgroundImagePath));
            }
        }

        private ObservableCollection<FolderItem> _breadcrumb = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> Breadcrumb
        {
            get => _breadcrumb;
            set => SetProperty(ref _breadcrumb, value);
        }

        private string? _rootCanvasBackgroundImagePath;
        public string? RootCanvasBackgroundImagePath
        {
            get => _rootCanvasBackgroundImagePath;
            set => SetProperty(ref _rootCanvasBackgroundImagePath, value);
        }

        public string? CurrentCanvasBackgroundImagePath
        {
            get => CurrentFolder?.CanvasBackgroundImagePath ?? RootCanvasBackgroundImagePath;
        }

        private StickyNote? _clipboardNote;
        private bool _isCut;
        private FolderItem? _clipboardFolder;
        private bool _isFolderCut;

        public ICommand AddFolderCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand NavigateToFolderCommand { get; }
        public ICommand GoToRootCommand { get; }
        public ICommand EditFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand CopyFolderCommand { get; }
        public ICommand CutFolderCommand { get; }
        public ICommand PasteFolderCommand { get; }
        public ICommand EditNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand CopyNoteCommand { get; }
        public ICommand CutNoteCommand { get; }
        public ICommand PasteNoteCommand { get; }
        public ICommand DeleteArrowCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand EditImageCommand { get; }
        public ICommand DeleteImageCommand { get; }

        public FileManagerViewModel(Plan plan, Action navigateBack)
        {
            Plan = plan;
            BackCommand = new RelayCommand(_ => NavigateBack(navigateBack));

            AddFolderCommand = new RelayCommand(_ => AddFolder());
            AddNoteCommand = new RelayCommand(_ => AddNote());
            OpenFolderCommand = new RelayCommand(param => OpenFolder(param as FolderItem));
            NavigateToFolderCommand = new RelayCommand(param => NavigateToFolder(param as FolderItem));
            GoToRootCommand = new RelayCommand(_ => GoToRoot());
            EditFolderCommand = new RelayCommand(param => EditFolder(param as FolderItem));
            DeleteFolderCommand = new RelayCommand(param => DeleteFolder(param as FolderItem));
            CopyFolderCommand = new RelayCommand(param => CopyFolder(param as FolderItem));
            CutFolderCommand = new RelayCommand(param => CutFolder(param as FolderItem));
            PasteFolderCommand = new RelayCommand(_ => PasteFolder());
            EditNoteCommand = new RelayCommand(param => EditNote(param as StickyNote));
            DeleteNoteCommand = new RelayCommand(param => DeleteNote(param as StickyNote));
            SaveNoteCommand = new RelayCommand(param => SaveNote(param as StickyNote));
            CopyNoteCommand = new RelayCommand(param => CopyNote(param as StickyNote));
            CutNoteCommand = new RelayCommand(param => CutNote(param as StickyNote));
            PasteNoteCommand = new RelayCommand(_ => PasteNote());
            DeleteArrowCommand = new RelayCommand(param => DeleteArrow(param as Arrow));
            AddImageCommand = new RelayCommand(_ => AddImage());
            EditImageCommand = new RelayCommand(param => EditImage(param as ImageNote));
            DeleteImageCommand = new RelayCommand(param => DeleteImage(param as ImageNote));

            LoadData();
            InitializeDefaultShortcuts();
            LoadCurrentFolderContent();
            UpdateBreadcrumb();
        }

        private void AddFolder()
        {
            var folder = new FolderItem
            {
                Name = "Nowy folder",
                ParentId = CurrentFolder?.Id,
                Left = 20,
                Top = 20
            };

            if (CurrentFolder == null)
            {
                RootFolders.Add(folder);
            }
            else
            {
                CurrentFolder.SubFolders.Add(folder);
            }

            CurrentFolders.Add(folder);
            SaveData();
        }

        private void AddNote()
        {
            var note = new StickyNote
            {
                Title = "Nowa notatka",
                Description = "",
                Text = "Nowa notatka",
                ParentFolderId = CurrentFolder?.Id,
                Left = 50,
                Top = 50
            };

            if (CurrentFolder == null)
            {
                RootNotes.Add(note);
                CurrentNotes.Add(note);
            }
            else
            {
                CurrentFolder.Notes.Add(note);
                CurrentNotes.Add(note);
            }

            SaveData();
        }

        private void OpenFolder(FolderItem? folder)
        {
            if (folder != null)
            {
                CurrentFolder = folder;
            }
        }

        private void NavigateToFolder(FolderItem? folder)
        {
            CurrentFolder = folder;
        }

        private void GoToRoot()
        {
            CurrentFolder = null;
        }

        private void NavigateBack(Action navigateBackToStart)
        {
            if (CurrentFolder == null)
            {
                // Jesteśmy w root, więc wracamy do ekranu startowego
                navigateBackToStart();
            }
            else
            {
                // Wracamy do folderu nadrzędnego
                if (CurrentFolder.ParentId == null)
                {
                    // Folder nadrzędny to root
                    CurrentFolder = null;
                }
                else
                {
                    // Znajdujemy folder nadrzędny
                    var parentFolder = FindFolderById(CurrentFolder.ParentId);
                    CurrentFolder = parentFolder;
                }
            }
        }

        private void EditFolder(FolderItem? folder)
        {
            if (folder != null)
            {
                SaveData();
            }
        }

        private void DeleteFolder(FolderItem? folder)
        {
            if (folder == null) return;

            if (CurrentFolder == null)
            {
                RootFolders.Remove(folder);
            }
            else
            {
                CurrentFolder.SubFolders.Remove(folder);
            }

            CurrentFolders.Remove(folder);
            SaveData();
        }

        private void CopyFolder(FolderItem? folder)
        {
            if (folder != null)
            {
                _clipboardFolder = folder;
                _isFolderCut = false;
            }
        }

        private void CutFolder(FolderItem? folder)
        {
            if (folder != null)
            {
                _clipboardFolder = folder;
                _isFolderCut = true;
            }
        }

        private void PasteFolder()
        {
            PasteFolderAt(null, null);
        }

        public void PasteFolderAt(double? x, double? y)
        {
            if (_clipboardFolder == null) return;

            if (_isFolderCut)
            {
                // Sprawdź czy nie próbujemy wkleić folderu do samego siebie
                if (CurrentFolder?.Id == _clipboardFolder.Id)
                {
                    System.Windows.MessageBox.Show(
                        "Nie można wkleić folderu do samego siebie.",
                        "Błąd", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Sprawdź czy nie próbujemy wkleić folderu do jego własnego podfolderu
                if (CurrentFolder != null && IsFolderDescendant(CurrentFolder, _clipboardFolder))
                {
                    System.Windows.MessageBox.Show(
                        "Nie można przenieść folderu do jego własnego podfolderu.",
                        "Błąd", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Wytnij: przenieś folder
                var oldParentId = _clipboardFolder.ParentId;
                _clipboardFolder.ParentId = CurrentFolder?.Id;

                // Ustaw pozycję jeśli została podana
                if (x.HasValue && y.HasValue)
                {
                    _clipboardFolder.Left = x.Value;
                    _clipboardFolder.Top = y.Value;
                }

                if (oldParentId == null)
                {
                    RootFolders.Remove(_clipboardFolder);
                }
                else
                {
                    var oldParent = FindFolderById(oldParentId);
                    oldParent?.SubFolders.Remove(_clipboardFolder);
                }

                if (CurrentFolder == null)
                {
                    RootFolders.Add(_clipboardFolder);
                    CurrentFolders.Add(_clipboardFolder);
                }
                else
                {
                    CurrentFolder.SubFolders.Add(_clipboardFolder);
                    CurrentFolders.Add(_clipboardFolder);
                }

                _clipboardFolder = null;
                _isFolderCut = false;
            }
            else
            {
                // Kopiuj: utwórz nowy folder z kopiami zawartości
                var newFolder = CloneFolder(_clipboardFolder);
                newFolder.ParentId = CurrentFolder?.Id;

                // Ustaw pozycję jeśli została podana, w przeciwnym razie offset o 20
                if (x.HasValue && y.HasValue)
                {
                    newFolder.Left = x.Value;
                    newFolder.Top = y.Value;
                }
                else
                {
                    newFolder.Left = _clipboardFolder.Left + 20;
                    newFolder.Top = _clipboardFolder.Top + 20;
                }

                if (CurrentFolder == null)
                {
                    RootFolders.Add(newFolder);
                    CurrentFolders.Add(newFolder);
                }
                else
                {
                    CurrentFolder.SubFolders.Add(newFolder);
                    CurrentFolders.Add(newFolder);
                }
            }

            SaveData();
        }

        private FolderItem CloneFolder(FolderItem source)
        {
            var clone = new FolderItem
            {
                Name = source.Name + " (kopia)",
                Color = source.Color,
                NameColor = source.NameColor,
                IconColor = source.IconColor,
                NameFontSize = source.NameFontSize,
                Icon = source.Icon,
                Width = source.Width,
                Height = source.Height,
                BackgroundImagePath = source.BackgroundImagePath,
                CanvasBackgroundImagePath = source.CanvasBackgroundImagePath
            };

            // Klonuj podkatalogi
            foreach (var subFolder in source.SubFolders)
            {
                var clonedSubFolder = CloneFolder(subFolder);
                clonedSubFolder.ParentId = clone.Id;
                clone.SubFolders.Add(clonedSubFolder);
            }

            // Klonuj notatki
            foreach (var note in source.Notes)
            {
                var clonedNote = new StickyNote
                {
                    Title = note.Title,
                    Description = note.Description,
                    Text = note.Text,
                    Color = note.Color,
                    Width = note.Width,
                    Height = note.Height,
                    Left = note.Left,
                    Top = note.Top,
                    FontFamily = note.FontFamily,
                    TitleFontSize = note.TitleFontSize,
                    DescriptionFontSize = note.DescriptionFontSize,
                    IsTitleBold = note.IsTitleBold,
                    IsDescriptionBold = note.IsDescriptionBold,
                    ParentFolderId = clone.Id
                };
                clone.Notes.Add(clonedNote);
            }

            // Klonuj strzałki
            foreach (var arrow in source.Arrows)
            {
                var clonedArrow = new Arrow
                {
                    StartX = arrow.StartX,
                    StartY = arrow.StartY,
                    EndX = arrow.EndX,
                    EndY = arrow.EndY,
                    Color = arrow.Color,
                    Thickness = arrow.Thickness,
                    ParentFolderId = clone.Id
                };
                clone.Arrows.Add(clonedArrow);
            }

            // Klonuj obrazy
            foreach (var image in source.Images)
            {
                var clonedImage = new ImageNote
                {
                    ImagePath = image.ImagePath,
                    Description = image.Description,
                    Width = image.Width,
                    Height = image.Height,
                    Left = image.Left,
                    Top = image.Top,
                    DescriptionFontSize = image.DescriptionFontSize,
                    ParentFolderId = clone.Id
                };
                clone.Images.Add(clonedImage);
            }

            return clone;
        }

        private bool IsFolderDescendant(FolderItem potentialDescendant, FolderItem ancestor)
        {
            // Sprawdź czy potentialDescendant jest potomkiem ancestor
            // Innymi słowy: czy ancestor zawiera potentialDescendant w swoich podfolderach
            return IsFolderDescendantRecursive(potentialDescendant.Id, ancestor);
        }

        private bool IsFolderDescendantRecursive(string descendantId, FolderItem ancestor)
        {
            // Sprawdź bezpośrednie podfoldery
            foreach (var subFolder in ancestor.SubFolders)
            {
                if (subFolder.Id == descendantId)
                    return true;

                // Rekurencyjnie sprawdź podfoldery
                if (IsFolderDescendantRecursive(descendantId, subFolder))
                    return true;
            }

            return false;
        }

        private void EditNote(StickyNote? note)
        {
            if (note != null)
            {
                note.IsEditing = true;
            }
        }

        private void SaveNote(StickyNote? note)
        {
            if (note != null)
            {
                note.IsEditing = false;
                SaveData();
            }
        }

        private void DeleteNote(StickyNote? note)
        {
            if (note == null) return;

            if (CurrentFolder == null)
            {
                RootNotes.Remove(note);
                CurrentNotes.Remove(note);
            }
            else
            {
                CurrentFolder.Notes.Remove(note);
                CurrentNotes.Remove(note);
            }

            SaveData();
        }

        private void CopyNote(StickyNote? note)
        {
            if (note != null)
            {
                _clipboardNote = note;
                _isCut = false;
            }
        }

        private void CutNote(StickyNote? note)
        {
            if (note != null)
            {
                _clipboardNote = note;
                _isCut = true;
            }
        }

        private void PasteNote()
        {
            if (_clipboardNote == null) return;

            if (_isCut)
            {
                var oldParentId = _clipboardNote.ParentFolderId;
                _clipboardNote.ParentFolderId = CurrentFolder?.Id;

                if (oldParentId == null)
                {
                    RootNotes.Remove(_clipboardNote);
                }
                else
                {
                    var oldParent = FindFolderById(oldParentId);
                    oldParent?.Notes.Remove(_clipboardNote);
                }

                if (CurrentFolder == null)
                {
                    RootNotes.Add(_clipboardNote);
                    CurrentNotes.Add(_clipboardNote);
                }
                else
                {
                    CurrentFolder.Notes.Add(_clipboardNote);
                    CurrentNotes.Add(_clipboardNote);
                }

                _clipboardNote = null;
                _isCut = false;
            }
            else
            {
                var newNote = new StickyNote
                {
                    Title = _clipboardNote.Title,
                    Description = _clipboardNote.Description,
                    Text = _clipboardNote.Text,
                    Color = _clipboardNote.Color,
                    Width = _clipboardNote.Width,
                    Height = _clipboardNote.Height,
                    Left = _clipboardNote.Left + 20,
                    Top = _clipboardNote.Top + 20,
                    ParentFolderId = CurrentFolder?.Id
                };

                if (CurrentFolder == null)
                {
                    RootNotes.Add(newNote);
                    CurrentNotes.Add(newNote);
                }
                else
                {
                    CurrentFolder.Notes.Add(newNote);
                    CurrentNotes.Add(newNote);
                }
            }

            SaveData();
        }

        public void AddArrow(Arrow arrow)
        {
            arrow.ParentFolderId = CurrentFolder?.Id;

            if (CurrentFolder == null)
            {
                RootArrows.Add(arrow);
            }
            else
            {
                CurrentFolder.Arrows.Add(arrow);
            }

            CurrentArrows.Add(arrow);
            SaveData();
        }

        private void DeleteArrow(Arrow? arrow)
        {
            if (arrow == null) return;

            if (CurrentFolder == null)
            {
                RootArrows.Remove(arrow);
            }
            else
            {
                CurrentFolder.Arrows.Remove(arrow);
            }

            CurrentArrows.Remove(arrow);
            SaveData();
        }

        private void AddImage()
        {
            var image = new ImageNote
            {
                Left = 20,
                Top = 20,
                ParentFolderId = CurrentFolder?.Id
            };

            var dialog = new Views.Dialogs.EditImageDialog(image);
            if (dialog.ShowDialog() == true)
            {
                if (CurrentFolder == null)
                {
                    RootImages.Add(image);
                }
                else
                {
                    CurrentFolder.Images.Add(image);
                }

                CurrentImages.Add(image);
                SaveData();
            }
        }

        private void EditImage(ImageNote? image)
        {
            if (image == null) return;

            var dialog = new Views.Dialogs.EditImageDialog(image);
            if (dialog.ShowDialog() == true)
            {
                SaveData();
            }
        }

        private void DeleteImage(ImageNote? image)
        {
            if (image == null) return;

            if (CurrentFolder == null)
            {
                RootImages.Remove(image);
            }
            else
            {
                CurrentFolder.Images.Remove(image);
            }

            CurrentImages.Remove(image);
            SaveData();
        }

        private FolderItem? FindFolderById(string id)
        {
            return FindFolderByIdRecursive(id, RootFolders);
        }

        private FolderItem? FindFolderByIdRecursive(string id, ObservableCollection<FolderItem> folders)
        {
            foreach (var folder in folders)
            {
                if (folder.Id == id) return folder;
                var found = FindFolderByIdRecursive(id, folder.SubFolders);
                if (found != null) return found;
            }
            return null;
        }

        private void LoadCurrentFolderContent()
        {
            CurrentFolders.Clear();
            CurrentNotes.Clear();
            CurrentArrows.Clear();
            CurrentImages.Clear();

            if (CurrentFolder == null)
            {
                foreach (var folder in RootFolders)
                {
                    CurrentFolders.Add(folder);
                }
                foreach (var note in RootNotes)
                {
                    CurrentNotes.Add(note);
                }
                foreach (var arrow in RootArrows)
                {
                    CurrentArrows.Add(arrow);
                }
                foreach (var image in RootImages)
                {
                    CurrentImages.Add(image);
                }
            }
            else
            {
                foreach (var folder in CurrentFolder.SubFolders)
                {
                    CurrentFolders.Add(folder);
                }
                foreach (var note in CurrentFolder.Notes)
                {
                    CurrentNotes.Add(note);
                }
                foreach (var arrow in CurrentFolder.Arrows)
                {
                    CurrentArrows.Add(arrow);
                }
                foreach (var image in CurrentFolder.Images)
                {
                    CurrentImages.Add(image);
                }
            }
        }

        private void UpdateBreadcrumb()
        {
            Breadcrumb.Clear();

            if (CurrentFolder != null)
            {
                var path = new List<FolderItem>();
                BuildBreadcrumbPath(CurrentFolder, path);
                path.Reverse();

                foreach (var folder in path)
                {
                    Breadcrumb.Add(folder);
                }
            }
        }

        private void InitializeDefaultShortcuts()
        {
            if (Shortcuts.Count == 0)
            {
                Shortcuts.Add(new PlanShortcut 
                { 
                    PlanName = "Plan uczelniany", 
                    Icon = "🎓", 
                    Color = "#FFFFEB99", 
                    IconColor = "#FF000000", 
                    NameColor = "#FF000000", 
                    Left = 250, 
                    Top = 250,
                    Width = 200,
                    Height = 200,
                    NameFontSize = 30
                });
                Shortcuts.Add(new PlanShortcut 
                { 
                    PlanName = "Zajęcia jednorazowe", 
                    Icon = "📚", 
                    Color = "#FFAADDFF", 
                    IconColor = "#FF000000", 
                    NameColor = "#FF000000", 
                    Left = 450, 
                    Top = 250,
                    Width = 200,
                    Height = 200,
                    NameFontSize = 30
                });
                Shortcuts.Add(new PlanShortcut 
                { 
                    PlanName = "Plan pracy", 
                    Icon = "💼", 
                    Color = "#FFCCB3FF", 
                    IconColor = "#FF000000", 
                    NameColor = "#FF000000", 
                    Left = 650, 
                    Top = 300,
                    Width = 200,
                    Height = 200,
                    NameFontSize = 30
                });
                Shortcuts.Add(new PlanShortcut 
                { 
                    PlanName = "Planer", 
                    Icon = "📅", 
                    Color = "#FFAAFFAA", 
                    IconColor = "#FF000000", 
                    NameColor = "#FF000000", 
                    Left = 850, 
                    Top = 350,
                    Width = 200,
                    Height = 200,
                    NameFontSize = 30
                });
                Shortcuts.Add(new PlanShortcut 
                { 
                    PlanName = "Zarządzaj uczniami", 
                    Icon = "👤", 
                    Color = "#FFFFAADD", 
                    IconColor = "#FF000000", 
                    NameColor = "#FF000000", 
                    Left = 400, 
                    Top = 450,
                    Width = 200,
                    Height = 200,
                    NameFontSize = 30
                });
                SaveData();
            }
        }

        private void BuildBreadcrumbPath(FolderItem folder, List<FolderItem> path)
        {
            path.Add(folder);
            if (folder.ParentId != null)
            {
                var parent = FindFolderById(folder.ParentId);
                if (parent != null)
                {
                    BuildBreadcrumbPath(parent, path);
                }
            }
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    var json = File.ReadAllText(DataFilePath);
                    var data = JsonSerializer.Deserialize<FileManagerData>(json);
                    if (data != null)
                    {
                        RootFolders = new ObservableCollection<FolderItem>(data.RootFolders);
                        RootNotes = new ObservableCollection<StickyNote>(data.RootNotes);
                        RootArrows = new ObservableCollection<Arrow>(data.RootArrows);
                        RootImages = new ObservableCollection<ImageNote>(data.RootImages);
                        RootCanvasBackgroundImagePath = data.RootCanvasBackgroundImagePath;
                        Shortcuts = new ObservableCollection<PlanShortcut>(data.Shortcuts);
                        OnPropertyChanged(nameof(RootFolders));
                        OnPropertyChanged(nameof(RootNotes));
                        OnPropertyChanged(nameof(RootArrows));
                        OnPropertyChanged(nameof(RootImages));
                        OnPropertyChanged(nameof(RootCanvasBackgroundImagePath));
                        OnPropertyChanged(nameof(CurrentCanvasBackgroundImagePath));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd wczytywania danych file manager: {ex.Message}");
            }
        }

        public void SaveData()
        {
            try
            {
                var dir = Path.GetDirectoryName(DataFilePath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var data = new FileManagerData 
                { 
                    RootFolders = RootFolders.ToList(),
                    RootNotes = RootNotes.ToList(),
                    RootArrows = RootArrows.ToList(),
                    RootImages = RootImages.ToList(),
                    RootCanvasBackgroundImagePath = RootCanvasBackgroundImagePath,
                    Shortcuts = Shortcuts.ToList()
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd zapisywania danych file manager: {ex.Message}");
            }
        }

        private class FileManagerData
        {
            public List<FolderItem> RootFolders { get; set; } = new List<FolderItem>();
            public List<StickyNote> RootNotes { get; set; } = new List<StickyNote>();
            public List<Arrow> RootArrows { get; set; } = new List<Arrow>();
            public List<ImageNote> RootImages { get; set; } = new List<ImageNote>();
            public string? RootCanvasBackgroundImagePath { get; set; }
            public List<PlanShortcut> Shortcuts { get; set; } = new List<PlanShortcut>();
        }
    }
}
