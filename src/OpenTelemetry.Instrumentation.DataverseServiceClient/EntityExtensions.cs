using System.Globalization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient;

public static class EntityExtensions
{
    public static string ToInsertStatement(this Entity entity) =>
        entity is null || entity.Attributes.Count == 0
            ? string.Empty
            : $"INSERT INTO {entity.LogicalName.ToLower()} ({string.Join(", ", entity.Attributes.ToSqlColumnValues().Keys)}) VALUES ({string.Join(", ", entity.Attributes.ToSqlColumnValues().Values)})";

    public static string ToUpdateStatement(this Entity entity) =>
        entity is null || entity.Attributes.Count == 0
            ? string.Empty
            : $"UPDATE {entity.LogicalName.ToLower()} SET {string.Join(", ", entity.Attributes.ToSqlColumnValues().Select(a => $"{a.Key} = {a.Value}"))} WHERE {entity.LogicalName.ToLower()}id = '{entity.Id}'";

    public static ICollection<string> ToSqlColumns(this ColumnSet columnSet) =>
        columnSet.AllColumns
            ? new List<string> { "*" }
            : columnSet.Columns.Select(c => c.ToLower()).ToList();

    static IDictionary<string, string> ToSqlColumnValues(this AttributeCollection attributes) =>
        attributes.Select(attr => new KeyValuePair<string, string>(attr.Key.ToLower(),
                attr.Value switch
                {
                    null => "NULL",
                    bool b => b ? "TRUE" : "FALSE",
                    int i => i.ToString(CultureInfo.InvariantCulture),
                    decimal d => d.ToString(CultureInfo.InvariantCulture),
                    double f => f.ToString(CultureInfo.InvariantCulture),
                    long l => l.ToString(CultureInfo.InvariantCulture),
                    _ => $"'{attr.Value}'"
                }))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
}
