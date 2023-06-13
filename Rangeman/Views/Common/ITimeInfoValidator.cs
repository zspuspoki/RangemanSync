using System.Collections.Generic;

namespace Rangeman.Views.Common
{
    public interface ITimeInfoValidator
    {
        Dictionary<string, List<string>> PropErrors { get; }

        List<string> ValidateDay(string propertyName, int? day);
        List<string> ValidateHour(string propertyName, int? hour);
        List<string> ValidateMinute(string propertyName, int? minute);
        List<string> ValidateSecond(string propertyName, int? second);
        List<string> ValidateYear(string propertyName, int? year);
    }
}