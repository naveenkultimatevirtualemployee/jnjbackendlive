namespace JNJServices.Utility.Extensions
{
    public static class ValidateInputExtensions
    {
        public static bool ToValidateInt(this int? value)
        {
            if (value != null)
            {
                return true;
            }

            return false;
        }

        public static bool ToValidateIntWithZero(this int? value)
        {
            if (value != null && value > 0)
            {
                return true;
            }

            return false;
        }

        public static bool ToValidateInActiveFlag(this int? value)
        {
            if (value != null && value != -2)
            {
                return true;
            }

            return false;
        }
    }
}
