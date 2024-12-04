namespace CsvDataGridPresenter.Models;

public class CsvDataModel
{
    private readonly Dictionary<string, object> _data;

    public CsvDataModel(Dictionary<string, object> data)
    {
        _data = data;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public object? GetValue(string columnName)
    {
        return _data.GetValueOrDefault(columnName);
    }
}

public class CsvDataColumn
{
    private Type _customType;

    public CsvDataColumn(string filedName)
    {
        _customType = typeof(string);
        FieldName = filedName;
        Header = filedName;
    }

    public string FieldName { get; }
    public string Header { get; }

    public Type CustomType
    {
        get => _customType;
        set
        {
            // if (value == typeof(int) && (_customType == typeof(float) ||
            //                              _customType == typeof(double) ||
            //                              _customType == typeof(decimal)))
            //     return;

            _customType = value;
        }
    }
}