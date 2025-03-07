using System.Globalization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient;

public static class EntityExtensions
{
    public static string ToInsertStatement(this Entity entity)
    {
        if (entity is null || entity.Attributes.Count == 0)
        {
            return string.Empty;
        }

        var sqlColumnValues = entity.Attributes.ToSqlColumnValues();
        var columns = string.Join(", ", sqlColumnValues.Keys);
        var values = string.Join(", ", sqlColumnValues.Values);

        return $"INSERT INTO {entity.LogicalName.ToLower()} ({columns}) VALUES ({values})";
    }

    public static string ToUpdateStatement(this Entity entity)
    {
        if (entity is null || entity.Attributes.Count == 0)
        {
            return string.Empty;
        }

        var sqlColumnValues = entity.Attributes.ToSqlColumnValues();
        var setClauses = string.Join(", ", sqlColumnValues.Select(a => $"{a.Key} = {a.Value}"));

        return $"UPDATE {entity.LogicalName.ToLower()} SET {setClauses} WHERE {entity.LogicalName.ToLower()}id = '{entity.Id}'";
    }

    public static string ToSelectStatement(this EntityReference entityReference, ColumnSet columnSet)
    {
        var entityId = $"{entityReference.LogicalName?.ToLower()}id";
        var columns = columnSet.ToSqlColumns(entityId);
        var selectColumns = string.Join(", ", columns);

        return $"SELECT {selectColumns} FROM {entityReference.LogicalName?.ToLower()} WHERE {entityId} = '{entityReference.Id}'";
    }

    public static string ToDeleteStatement(this EntityReference entityReference)
    {
        var entityId = $"{entityReference.LogicalName?.ToLower()}id";

        return $"DELETE FROM {entityReference.LogicalName?.ToLower()} WHERE {entityId} = '{entityReference.Id}'";
    }

    public static ICollection<string> ToSqlColumns(this ColumnSet columnSet, string? entityId = null)
    {
        if (columnSet.AllColumns || columnSet.Columns.Count == 0)
            return new[] { "*" };

        return entityId is null
            ? columnSet.Columns.Select(c => c.ToLower()).ToList()
            : new[] { entityId.ToLower() }.Concat(columnSet.Columns.Select(c => c.ToLower())).ToList();
    }

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
