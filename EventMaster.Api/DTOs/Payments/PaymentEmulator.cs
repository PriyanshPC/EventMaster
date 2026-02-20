using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventMaster.Api.DTOs.Payments;

public class PaymentEmulatorStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PaymentEmulatorStore(IWebHostEnvironment env)
    {
        // wwwroot/payment.json
        _filePath = Path.Combine(env.WebRootPath ?? "wwwroot", "payment.json");
    }

    public async Task<PaymentMockData> ReadAsync()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"payment.json not found at: {_filePath}");

        var json = await File.ReadAllTextAsync(_filePath);
        var data = JsonSerializer.Deserialize<PaymentMockData>(json, _jsonOptions);
        return data ?? new PaymentMockData();
    }

    public async Task WriteAsync(PaymentMockData data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public static string MaskCard(string cardNumber14, string exp)
    {
        var last4 = cardNumber14.Length >= 4 ? cardNumber14[^4..] : cardNumber14;
        return $"**** **** **** {last4} exp {exp}";
    }
}

public class PaymentMockData
{
    public List<CardDetail> Card_Details { get; set; } = new();
    public List<Coupon> Coupons { get; set; } = new();
}

public class CardDetail
{
    public string Name_On_Card { get; set; } = "";
    public string Card_Number { get; set; } = "";   // 14 digits
    public string Exp { get; set; } = "";           // MM/YY
    public string Cvv { get; set; } = "";           // 3 digits
    public string Postal_Code { get; set; } = "";
    public decimal Amount_Balance { get; set; }
}

public class Coupon
{
    public string Code { get; set; } = "";
    public string Type { get; set; } = "Percent";   // Percent | Fixed
    public decimal Value { get; set; }
    public decimal Min_Amount { get; set; }
    public bool Is_Active { get; set; }
    public DateOnly Expires_At { get; set; }
}