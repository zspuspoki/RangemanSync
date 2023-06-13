using System.Collections.Generic;

namespace Rangeman.Views.Common
{
    public class TimeInfoValidator : ITimeInfoValidator
    {
        private Dictionary<string, List<string>> propErrors = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> PropErrors
        {
            get
            {
                return propErrors;
            }
        }

        public List<string> ValidateYear(string propertyName, int? year)
        {
            List<string> errors;
            if (!(year >= 2000 && year <= 2200))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    errors.Add("Year should be between 2000 and 2200");
                    this.propErrors.Add(propertyName, errors);
                }
            }
            else if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.propErrors.Remove(propertyName);
            }

            return errors;
        }

        public List<string> ValidateHour(string propertyName, int? hour)
        {
            List<string> errors;
            if (!(hour >= 0 && hour <= 23))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    errors.Add("Hour should be between 0 and 23");
                    this.propErrors.Add(propertyName, errors);
                }
            }
            else if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.propErrors.Remove(propertyName);
            }

            return errors;
        }

        public List<string> ValidateMinute(string propertyName, int? minute)
        {
            List<string> errors;
            if (!(minute >= 0 && minute <= 59))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    errors.Add("Minute should be between 0 and 59");
                    this.propErrors.Add(propertyName, errors);
                }
            }
            else if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.propErrors.Remove(propertyName);
            }

            return errors;
        }

        public List<string> ValidateSecond(string propertyName, int? second)
        {
            List<string> errors;
            if (!(second >= 0 && second <= 59))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    errors.Add("Second should be between 0 and 59");
                    this.propErrors.Add(propertyName, errors);
                }
            }
            else if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.propErrors.Remove(propertyName);
            }

            return errors;
        }

        public List<string> ValidateDay(string propertyName, int? day)
        {
            List<string> errors;
            if (!(day >= 1 && day <= 31))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    errors.Add("Day should be between 1 and 31");
                    this.propErrors.Add(propertyName, errors);
                }
            }
            else if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.propErrors.Remove(propertyName);
            }

            return errors;
        }
    }
}