namespace ShopThueBanSach.Server.Models
{
    public enum OrderStatus
    {
        Pending,// chờ xử lý
        Confirmed,// đã xác nhận
        Shipping,//đang giao
		Completed,//hoàn thành
		Renting,//đang thuê
		Overdue,//quá hạn
		Canceled,//đã hủy
			Refund//Hoàn tiền

	}
}
