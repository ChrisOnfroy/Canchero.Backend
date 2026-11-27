using Infrastructure.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Canchero.Infrastructure.UnitTest;

public sealed class PersistenceMappingTests
{
    [Fact]
    public void ReservationMapping_UsesExpectedTableAndOwnedColumns()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=metadata_only;Username=test")
            .Options;
        using var context = new AppDbContext(options);
        var reservation = context.Model.FindEntityType(typeof(Reservation))!;
        Assert.Equal("reservations", reservation.GetTableName());
        var table = context.Model.GetRelationalModel().Tables.Single(item => item.Name == "reservations");
        var columns = table.Columns.Select(column => column.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("slot_start", columns);
        Assert.Contains("slot_end", columns);
        Assert.Contains("total_price_amount", columns);
        Assert.Contains("status", columns);
    }
}
