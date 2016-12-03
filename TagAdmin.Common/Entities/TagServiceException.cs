namespace TagAdmin.Common.Entities
{
    public class TagServiceException
    {
        public string InnerException { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }
        public int EventId { get; set; }
    }
}
