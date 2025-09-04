namespace WebAPI_to_MySQL.DTOs
{
    public class SubscriptionCreateDto
    {
        public int UserID { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int PaymentStatusID { get; set; } // Foreign key to payment_status table
    }
}