namespace MetroShip.Service.ApiModels.Transaction;

public record VietQrBankResponse
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public List<VietQrBankDetail>? Data { get; set; }
}

public record VietQrBankDetail
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Bin { get; set; }
    public string? ShortName { get; set; }
    public string? Logo { get; set; }
    public int TransferSupported { get; set; }
    public int LookupSupported { get; set; }
}