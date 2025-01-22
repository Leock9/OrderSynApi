namespace OrderSyncApi.Infrastructure.Services;

public interface ILineParser
{
    (bool IsValid, ParsedLine Line, string ErrorMessage) Parse(string line);
}

public class LineParser : ILineParser
{
    public (bool IsValid, ParsedLine Line, string ErrorMessage) Parse(string line)
    {
        try
        {
            if (!int.TryParse(line[..10].TrimStart('0'), out var userId))
                return (false, null, "Erro ao converter UserId.");

            var userName = line.Substring(10, 45).Trim();
            if (string.IsNullOrWhiteSpace(userName))
                return (false, null, "Nome do usuário está vazio.");

            if (!int.TryParse(line.Substring(55, 10).TrimStart('0'), out var orderId))
                return (false, null, "Erro ao converter OrderId.");

            if (!int.TryParse(line.Substring(65, 10).TrimStart('0'), out var productId))
                return (false, null, "Erro ao converter ProductId.");

            if (!decimal.TryParse(line.Substring(75, 12).Trim(), out var value))
                return (false, null, "Erro ao converter Value.");

            if (!DateTime.TryParseExact(line.Substring(87, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                return (false, null, "Erro ao converter Data.");

            return (true, new ParsedLine(userId, userName, orderId, productId, value, date), null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}

public record ParsedLine(int UserId, string UserName, int OrderId, int ProductId, decimal Value, DateTime Date);