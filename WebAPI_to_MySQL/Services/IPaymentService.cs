public interface IPaymentService
{
    Task<bool> VerifyPaymentAsync(string paymentId);
}