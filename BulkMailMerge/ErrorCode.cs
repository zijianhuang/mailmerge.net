namespace BulkMailMerge
{
    public enum ErrorCode
    {
        Success = 0,
        InvalidArguments = 1,
        InvalidOptions = 2,
        EmailAddressParsingError = 12,
        FileIOError = 13,
        NothingPending = 3,
    }
}
