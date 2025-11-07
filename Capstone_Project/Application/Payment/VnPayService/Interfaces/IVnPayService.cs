using static BE_Capstone_Project.Application.Payment.DTOs.PaymentInfDto;

namespace BE_Capstone_Project.Application.Payment.VnPayService.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
