namespace Rangeman
{
    internal static class LogHeaderViewModelConverter
    {
        public static LogHeaderViewModel ToViewModel(this LogHeaderDataInfo logHeaderDataInfo)
        {
            return new LogHeaderViewModel
            {
                HeaderTime = logHeaderDataInfo.Date,
                OrdinalNumber = logHeaderDataInfo.OrdinalNumber,
                DataSize = logHeaderDataInfo.DataSize,
                DataCount = logHeaderDataInfo.DataCount,
                LogAddress = logHeaderDataInfo.LogAddress,
                LogTotalLength = logHeaderDataInfo.LogTotalLength
            };
        }
    }
}