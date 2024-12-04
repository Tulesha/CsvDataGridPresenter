using System.Globalization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvDataGridPresenter.Models;
using CsvHelper;

namespace CsvDataGridPresenter.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private IEnumerable<CsvDataModel>? _items;
    [ObservableProperty] private IEnumerable<CsvDataColumn>? _columns;

    [RelayCommand]
    private async Task UploadCsv(Window window)
    {
        var result = await TopLevel.GetTopLevel(window)?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("CSV Files")
                {
                    Patterns = new[] { "*.csv" }
                }
            }
        })!;

        if (result.FirstOrDefault() is not { } file)
            return;

        SetCsv(file.Path.LocalPath);
    }

    [RelayCommand]
    private void UnloadCsv()
    {
        Columns = null;
        Items = null;
    }

    [RelayCommand]
    private void Exit(Window window)
    {
        window.Close();
    }

    private void SetCsv(string path)
    {
        var columns = ReadCsvColumns(path).ToList();
        var items = ReadCsvData(path, columns);
        Columns = columns;
        Items = items;
    }

    private IEnumerable<CsvDataModel> ReadCsvData(string path, IEnumerable<CsvDataColumn> columns)
    {
        var data = new List<CsvDataModel>();

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        var headerRecord = csv.HeaderRecord;
        if (headerRecord == null)
            return data;

        var columnList = columns.ToList();

        while (csv.Read())
        {
            var row = new Dictionary<string, object>();
            foreach (var header in headerRecord)
            {
                var field = csv.GetField(header);
                if (field == null) continue;
                object value = field;
                var column = columnList.FirstOrDefault(x => x.FieldName == header);
                if (column != null)
                {
                    // if (int.TryParse(field, out var iR))
                    // {
                    //     column.CustomType = typeof(int);
                    //     value = iR;
                    // }
                    // else if (float.TryParse(field, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var fR))
                    // {
                    //     column.CustomType = typeof(float);
                    //     value = fR;
                    // }
                    // else if (decimal.TryParse(field, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var decR))
                    // {
                    //     column.CustomType = typeof(decimal);
                    //     value = decR;
                    // }
                    /*else*/
                    if (double.TryParse(field, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var dR))
                    {
                        column.CustomType = typeof(double);
                        value = dR;
                    }
                    else if (bool.TryParse(field, out var bR))
                    {
                        column.CustomType = typeof(bool);
                        value = bR;
                    }
                    else if (Guid.TryParse(field, out var gR))
                    {
                        column.CustomType = typeof(Guid);
                        value = gR;
                    }
                    else if (DateTime.TryParse(field, CultureInfo.InvariantCulture, out var dateR))
                    {
                        column.CustomType = typeof(DateTime);
                        value = dateR;
                    }
                }

                row[header] = value;
            }

            data.Add(new CsvDataModel(row));
        }

        return data;
    }

    private IEnumerable<CsvDataColumn> ReadCsvColumns(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        var headerRecord = csv.HeaderRecord;
        return headerRecord?.Select(x => new CsvDataColumn(x)) ?? Enumerable.Empty<CsvDataColumn>();
    }
}