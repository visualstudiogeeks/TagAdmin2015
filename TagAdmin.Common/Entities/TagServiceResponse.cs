using System;

namespace TagAdmin.Common.Entities
{
    public class TagServiceResponse<T> where T : class
    {
        public T Data { get; set; }
        public Exception Exception { get; set; }
        public bool IsSuccessStatusCode { get; set; }
    }

    public class TagServiceResponse
    {
        public Exception Exception { get; set; }
        public bool IsSuccessStatusCode { get; set; }
    }
}
