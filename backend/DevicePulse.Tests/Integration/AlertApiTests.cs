using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevicePulse.Api.Contracts;
using DevicePulse.Api.Models;

namespace DevicePulse.Tests.Integration;

public sealed class AlertApiTests(DevicePulseApiFactory factory) : IClassFixture<DevicePulseApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task AlertIsTriggeredWhenCurrentValueIsInsideItsOwnRange()
    {
        var equipmentResponse = await client.PostAsJsonAsync(
            "/api/equipments", new CreateEquipmentRequest("Sensor de teste", 0, 100, 50), JsonOptions);
        var equipment = await equipmentResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOptions);
        Assert.NotNull(equipment);

        var createResponse = await client.PostAsJsonAsync(
            "/api/alerts", new CreateAlertRequest("Faixa restrita", equipment.Id, 10, 20), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<AlertResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.False(created.IsTriggered);

        await client.PostAsJsonAsync(
            $"/api/equipments/{equipment.Id}/readings",
            new CreateReadingRequest(15, ReadingSource.Manual), JsonOptions);
        var alerts = await client.GetFromJsonAsync<List<AlertResponse>>("/api/alerts", JsonOptions);

        var alert = Assert.Single(alerts!, item => item.Id == created.Id);
        Assert.True(alert.IsTriggered);
        Assert.Equal(15, alert.CurrentValue);
        Assert.Equal("Sensor de teste", alert.EquipmentName);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    public async Task AlertRangeIncludesMinimumAndMaximumValues(double currentValue)
    {
        var equipmentResponse = await client.PostAsJsonAsync(
            "/api/equipments", new CreateEquipmentRequest($"Sensor {currentValue}", 0, 100, currentValue), JsonOptions);
        var equipment = await equipmentResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOptions);
        Assert.NotNull(equipment);

        var createResponse = await client.PostAsJsonAsync(
            "/api/alerts", new CreateAlertRequest($"Limite {currentValue}", equipment.Id, 10, 20), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var alert = await createResponse.Content.ReadFromJsonAsync<AlertResponse>(JsonOptions);
        Assert.NotNull(alert);
        Assert.True(alert.IsTriggered);
    }

    [Fact]
    public async Task InvalidAlertRangeReturnsBadRequest()
    {
        var response = await client.PostAsJsonAsync(
            "/api/alerts", new CreateAlertRequest("Inválido", 1, 30, 20), JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
