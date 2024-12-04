using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsvDataGridPresenter.Models;
using Eremex.AvaloniaUI.Controls.DataControl;
using Eremex.AvaloniaUI.Controls.DataGrid;

namespace CsvDataGridPresenter.Views;

public partial class MainView : UserControl
{
    private readonly Dictionary<Tuple<Guid, string>, object?> _valuesCache = new();

    public MainView()
    {
        InitializeComponent();
    }

    private void DataGridControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DataControlBase.ItemsSourceProperty)
            _valuesCache.Clear();
    }

    private void DataGridControl_CustomUnboundColumnData(object sender, DataGridUnboundColumnDataEventArgs e)
    {
        if (e.Item is not CsvDataModel item)
            return;
        if (e.Column.FieldName == null)
            return;

        var key = Tuple.Create(item.Id, e.Column.FieldName);
        if (e.IsGettingData)
        {
            if (!_valuesCache.TryGetValue(key, out var value))
            {
                value = item.GetValue(e.Column.FieldName);
                _valuesCache[key] = value;
            }

            e.Value = value;
        }
        else
        {
            _valuesCache[key] = e.Value!;
        }
    }
}

public class DataGridCsvDataViewColumnTemplate : ITemplate<object, GridColumn>
{
    public GridColumn Build(object param)
    {
        var csvDataColumn = (CsvDataColumn)param;
        var gridColumn = new GridColumn
        {
            FieldName = csvDataColumn.FieldName,
            Header = csvDataColumn.Header,
            UnboundDataType = csvDataColumn.CustomType
        };

        return gridColumn;
    }
}