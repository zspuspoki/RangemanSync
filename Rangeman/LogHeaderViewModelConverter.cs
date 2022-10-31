namespace Rangeman
{
    internal static class LogHeaderViewModelConverter
    {
        public static LogHeaderViewModel ToViewModel(this LogHeaderDataInfo logHeaderDataInfo)
        {
            return new LogHeaderViewModel
            {
                HeaderTime = logHeaderDataInfo.Date
            };
        }
    }
}