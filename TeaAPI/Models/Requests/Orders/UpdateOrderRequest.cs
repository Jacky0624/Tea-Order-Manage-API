namespace TeaAPI.Models.Requests.Orders
{
    public class UpdateOrderRequest : EditOrderRequest
    {
        public int Id { get; set; }    
        public int OrderStatus { get; set; }
    }
}
