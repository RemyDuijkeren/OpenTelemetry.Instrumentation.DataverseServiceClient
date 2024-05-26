using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class EntityExtensionsTests
{
    [Fact]
    public void ReturnsCorrectInsertStatement_WhenEntityWithAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity")
        {
            Attributes =
            {
                ["Attribute1"] = "Value1",
                ["Attribute2"] = 42,
                ["Attribute3"] = 0.5,
                ["Attribute4"] = true,
                ["Attribute5"] = "Value5"
            }
        };

        // Act
        var result = entity.ToInsertStatement();

        // Assert
        result.Should().Be($"INSERT INTO testentity (attribute1, attribute2, attribute3, attribute4, attribute5) VALUES ('Value1', 42, 0.5, TRUE, 'Value5')");
    }

    [Fact]
    public void ReturnsInsertStatementWithoutValues_WhenEntityWithoutAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity");

        // Act
        var result = entity.ToInsertStatement();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsInsertStatementWithNullValues_WhenEntityWithNullAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity")
        {
            Attributes =
            {
                ["Attribute1"] = null,
                ["Attribute2"] = "Value2",
                ["Attribute3"] = null
            }
        };

        // Act
        var result = entity.ToInsertStatement();

        // Assert
        result.Should().Be($"INSERT INTO testentity (attribute1, attribute2, attribute3) VALUES (NULL, 'Value2', NULL)");
    }

    [Fact]
    public void ReturnEmptyInsertStatement_WhenNullEntity()
    {
        // Arrange
        Entity entity = null!;

        // Act
        var result = entity.ToInsertStatement();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsCorrectUpdateStatement_WhenEntityWithAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity", Guid.NewGuid())
        {
            Attributes =
            {
                ["Attribute1"] = "Value1",
                ["Attribute2"] = 42,
                ["Attribute3"] = 0.5,
                ["Attribute4"] = true,
                ["Attribute5"] = "Value5"
            }
        };

        // Act
        var result = entity.ToUpdateStatement();

        // Assert
        result.Should().Be($"UPDATE testentity SET attribute1 = 'Value1', attribute2 = 42, attribute3 = 0.5, attribute4 = TRUE, attribute5 = 'Value5' WHERE testentityid = '{entity.Id.ToString()}'");
    }

    [Fact]
    public void ReturnsUpdateStatementWithoutValues_WhenEntityWithoutAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity", Guid.NewGuid());

        // Act
        var result = entity.ToUpdateStatement();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsUpdateStatementWithNullValues_WhenEntityWithNullAttributes()
    {
        // Arrange
        var entity = new Entity("TestEntity", Guid.NewGuid())
        {
            Attributes =
            {
                ["Attribute1"] = null,
                ["Attribute2"] = "Value2",
                ["Attribute3"] = null
            }
        };

        // Act
        var result = entity.ToUpdateStatement();

        // Assert
        result.Should().Be($"UPDATE testentity SET attribute1 = NULL, attribute2 = 'Value2', attribute3 = NULL WHERE testentityid = '{entity.Id.ToString()}'");
    }

    [Fact]
    public void ReturnEmptyUpdateStatement_WhenNullEntity()
    {
        // Arrange
        Entity entity = null!;

        // Act
        var result = entity.ToUpdateStatement();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsCorrectSqlColumns_WhenColumnSetWithColumns()
    {
        // Arrange
        var columnSet = new ColumnSet("Attribute1", "Attribute2", "Attribute3");

        // Act
        var result = columnSet.ToSqlColumns();

        // Assert
        result.Should().BeEquivalentTo(["attribute1", "attribute2", "attribute3"]);
    }

    [Fact]
    public void ReturnsNoSqlColumns_WhenEmptyColumnSet()
    {
        // Arrange
        var columnSet = new ColumnSet();

        // Act
        var result = columnSet.ToSqlColumns();

        // Assert
        result.Should().BeEquivalentTo([]);
    }

    [Fact]
    public void ReturnsNoSqlColumns_WhenAllColumnSet()
    {
        // Arrange
        var columnSet = new ColumnSet(true);

        // Act
        var result = columnSet.ToSqlColumns();

        // Assert
        result.Should().BeEquivalentTo(["*"]);
    }
}
