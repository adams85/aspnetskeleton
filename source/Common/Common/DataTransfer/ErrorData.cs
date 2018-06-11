namespace AspNetSkeleton.Common.DataTransfer
{
    public class ErrorData
    {
        public int Code { get; set; }
        public Polymorph<object>[] Args { get; set; }
    }
}
