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
            // ファイルタイプの設定
            if (fileOpenPickerNotification.FileTypeFilter != null)
                foreach (var fileType in fileOpenPickerNotification.FileTypeFilter)
                    picker.FileTypeFilter.Add(fileType);

            // 表示場所・表示モードの設定
            picker.SuggestedStartLocation = fileOpenPickerNotification.SuggestedStartLocation;
            picker.ViewMode = fileOpenPickerNotification.ViewMode;

            // 複数選択かどうかを見て表示方法を変える
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
        /// <summary>
        ///     ファイルの種類
        /// </summary>
        public IEnumerable<string> FileTypeFilter { get; set; }

        /// <summary>
        ///     開始場所のサジェスト
        /// </summary>
        public PickerLocationId SuggestedStartLocation { get; set; }

        /// <summary>
        ///     表示モード
        /// </summary>
        public PickerViewMode ViewMode { get; set; }

        /// <summary>
        ///     複数選択可能かどうか
        /// </summary>
        public bool IsMultiple { get; set; }

        /// <summary>
        ///     選択結果
        /// </summary>
        public IEnumerable<StorageFile> Result { get; set; }
    }
}