using Microsoft.EntityFrameworkCore;
using Moq;
using Nocturne.Core.Contracts.Multitenancy;
using Nocturne.Infrastructure.Data.Services;
using Xunit;

namespace Nocturne.Infrastructure.Data.Tests.Services;

public class TenantDbContextFactoryTests
{
    [Fact]
    public async Task CreateAsync_SetsTenantIdOnContext_WhenTenantResolved()
    {
        var expectedTenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<NocturneDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new NocturneDbContext(options);

        var mockPool = new Mock<IDbContextFactory<NocturneDbContext>>();
        mockPool.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ctx);

        var mockAccessor = new Mock<ITenantAccessor>();
        mockAccessor.Setup(a => a.IsResolved).Returns(true);
        mockAccessor.Setup(a => a.TenantId).Returns(expectedTenantId);

        var factory = new TenantDbContextFactory(mockPool.Object, mockAccessor.Object);
        await using var result = await factory.CreateAsync();

        Assert.Equal(expectedTenantId, result.TenantId);
    }

    [Fact]
    public async Task CreateAsync_LeavesDefaultTenantId_WhenTenantNotResolved()
    {
        var options = new DbContextOptionsBuilder<NocturneDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new NocturneDbContext(options);

        var mockPool = new Mock<IDbContextFactory<NocturneDbContext>>();
        mockPool.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ctx);

        var mockAccessor = new Mock<ITenantAccessor>();
        mockAccessor.Setup(a => a.IsResolved).Returns(false);

        var factory = new TenantDbContextFactory(mockPool.Object, mockAccessor.Object);
        await using var result = await factory.CreateAsync();

        Assert.Equal(Guid.Empty, result.TenantId);
    }
}
