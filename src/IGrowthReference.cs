namespace AnthStat.Statistics
{
    /// <summary>
    /// Defines all methods that growth reference classes are required to implement
    /// </summary>
    public interface IGrowthReference
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the growth reference data
        /// </summary>
        /// <param name="indicator">The indicator to check for validity</param>
        /// <returns>bool; whether or not the provided indicator is valid</return>
        bool IsValidIndicator(Indicator indicator);

        /// <summary>
        /// Determines whether the second measurement (in the form of measurement1-for-measurement2, as 
        /// in "Height-for-Age") is valid for a given indicator.
        /// </summary>
        /// <param name="indicator">The indicator to which the measurements belong</param>
        /// <param name="measurement">
        /// Typically age of the child. Note that some references assume age is provided in days (e.g. WHO 2006) 
        /// and others assume it is provided in months. For some indicators, such as Weight-for-Height and 
        /// Weight-for-Lengnth, the measurement will be height or length in centimeters instead of age.
        /// </param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the growth reference.</return>
        bool IsValidMeasurement(Indicator indicator, double measurement);

        /// <summary>
        /// Calculates a z-score for a given indicator, pair of measurements (measurement1-for-measurement2, as 
        /// in "Height-for-Age"), and gender. A return value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI-for-Age, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">
        /// The first measurement. Must be provided in metric units. For example, if the indiciator is 
        /// 'Height-for-Age', then measurement1 is the 'Height' and its argument must be in centimeters. 
        /// Note that subscapular skinfold and triceps skinfold must be provided in millimeters.
        /// </param>
        /// <param name="measurement">
        /// Typically age of the child. Note that some references assume age is provided in days (e.g. WHO 2006) 
        /// and others assume it is provided in months. For some indicators, such as Weight-for-Height and 
        /// Weight-for-Lengnth, the measurement will be height or length in centimeters instead of age.
        /// </param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="z">The calculated z-score for the given set of inputs</param>
        /// <returns>bool; whether the calculation succeeded or failed</return>
        bool TryCalculateZScore(Indicator indicator, double measurement1, double measurement2, Sex sex, ref double z);
    }
}