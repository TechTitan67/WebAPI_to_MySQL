public class MockPaymentService : IPaymentService
{
    public Task<bool> VerifyPaymentAsync(string paymentId)
    {
        // Simulate always successful payment for testing
        return Task.FromResult(true);
    }
}