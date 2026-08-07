using System.Net;
using System.Net.Http.Json;
using DevicePulse.Api.Contracts;
using DevicePulse.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevicePulse.Tests.Integration;

public sealed class EquipmentApiTests(DevicePulseApiFactory factory)
    : IClassFixture<DevicePulseApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task EquipmentWorkflowUpdatesHistoryAndDashboard()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/equipments",
            new CreateEquipmentRequest(" Compressor ", 10, 90, 45));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var equipment = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>();
        Assert.NotNull(equipment);
        Assert.Equal("Compressor", equipment.Name);
        Assert.Equal(ReadingSource.Initial, equipment.LastReadingSource);

        var readingResponse = await client.PostAsJsonAsync(
            $"/api/equipments/{equipment.Id}/readings",
            new CreateReadingRequest(52.5, ReadingSource.Manual));

        Assert.Equal(HttpStatusCode.Created, readingResponse.StatusCode);

        var readings = await client.GetFromJsonAsync<List<EquipmentReadingResponse>>(
            $"/api/equipments/{equipment.Id}/readings");
        Assert.NotNull(readings);
        Assert.Collection(
            readings,
            reading => Assert.Equal(ReadingSource.Manual, reading.Source),
            reading => Assert.Equal(ReadingSource.Initial, reading.Source));

        var summary = await client.GetFromJsonAsync<DashboardSummaryResponse>(
            "/api/dashboard/summary");
        Assert.NotNull(summary);
        Assert.Equal(1, summary.EquipmentCount);
        Assert.Equal(2, summary.ReadingCount);
        Assert.Equal(1, summary.InitialReadingCount);
        Assert.Equal(1, summary.ManualReadingCount);
    }

    [Fact]
    public async Task InvalidReadingLimitReturnsProblemDetails()
    {
        var response = await client.GetAsync("/api/equipments/1/readings?limit=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Validation failed", problem?.Title);
    }
}
