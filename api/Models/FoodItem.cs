namespace FridgeApi.Models;

// A food item stored in the fridge (one row in the FoodItems table).
public class FoodItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public DateOnly ExpiryDate { get; set; }
    public DateOnly AddedOn { get; set; }
}

// What the UI sends when adding a new item.
public record CreateFoodItemRequest(string? Name, int? Quantity, DateOnly? ExpiryDate)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(Name))
            errors["name"] = ["Name is required."];
        else if (Name.Trim().Length > 100)
            errors["name"] = ["Name must be 100 characters or fewer."];
        if (ExpiryDate is null)
            errors["expiryDate"] = ["Expiry date is required."];
        if (Quantity is < 1 or > 999)
            errors["quantity"] = ["Quantity must be between 1 and 999."];
        return errors;
    }
}

// What the API sends back to the UI, including the calculated status.
public record FoodItemResponse(
    int Id, string Name, int Quantity, DateOnly ExpiryDate, DateOnly AddedOn,
    int DaysLeft, string Status)
{
    public static FoodItemResponse From(FoodItem item, DateOnly today)
    {
        var daysLeft = item.ExpiryDate.DayNumber - today.DayNumber;
        return new FoodItemResponse(item.Id, item.Name, item.Quantity, item.ExpiryDate,
            item.AddedOn, daysLeft, ExpiryRules.GetStatus(daysLeft));
    }
}

// Business rule: how many days before expiry an item counts as "going off soon".
public static class ExpiryRules
{
    public const int SoonThresholdDays = 3;

    public static string GetStatus(int daysLeft) => daysLeft switch
    {
        < 0 => "expired",
        <= SoonThresholdDays => "soon",
        _ => "fresh"
    };
}
