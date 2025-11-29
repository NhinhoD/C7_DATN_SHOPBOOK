namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IMoMoPaymentService
    {
        Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string returnUrl, string notifyUrl, string extraData);
        //Task<IActionResult> HandlePaymentCallbackAsync(MoMoResult result);
    }
}
