using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class FileOpenPickerAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return ExecuteAsync((FileOpenPickerNotification) parameter);
        }

        private async Task ExecuteAsync(FileOpenPickerNotification fileOpenPickerNotification)
        {
            var picker = new FileOpenPicker();
            if (fileOpenPickerNotification.FileTypeFilter != null)
                foreach (var fileType in fileOpenPickerNotification.FileTypeFilter)
                    picker.FileTypeFilter.Add(fileType);
            
            picker.SuggestedStartLocation = fileOpenPickerNotification.SuggestedStartLocation;
            picker.ViewMode = fileOpenPickerNotification.ViewMode;
            
            if (fileOpenPickerNotification.IsMultiple)
            {
                fileOpenPickerNotification.Result = await picker.PickMultipleFilesAsync();
            }
            else
            {
                var result = await picker.PickSingleFileAsync();
                fileOpenPickerNotification.Result = result != null ? new[] {result} : Enumerable.Empty<StorageFile>();
            }
        }
    }

    public class FileOpenPickerNotification : Notification
    {
        public IEnumerable<string> FileTypeFilter { get; set; }
        
        public PickerLocationId SuggestedStartLocation { get; set; }
        
        public PickerViewMode ViewMode { get; set; }
        
        public bool IsMultiple { get; set; }
        
        public IEnumerable<StorageFile> Result { get; set; }
    }
}