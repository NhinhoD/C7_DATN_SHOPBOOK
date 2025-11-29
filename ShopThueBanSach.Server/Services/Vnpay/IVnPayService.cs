using ShopThueBanSach.Server.Models.Vnpay;

namespace ShopThueBanSach.Server.Services.Vnpay
{
	public interface IVnPayService
	{
		string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
		PaymentResponseModel PaymentExecute(IQueryCollection collections);
		string CreatePaymentUrlForRent(PaymentInformationRentModel model, HttpContext context);
		string GetRentReturnUrl();
	}
}