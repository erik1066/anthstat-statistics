Latest version: [NuGet Package](https://www.nuget.org/packages/AnthStat.Statistics/2.0.0) (4 July 2017)

# Description

AnthStat computes z-scores for children and adolescents using the WHO 2006 Child Growth Standards, the WHO 2007 Growth Reference, and the CDC 2000 Growth Charts. Z-scores can be used to determine if a child is growing appropriately and to predict their expected adult height and weight. 

Z-scores for the following indicators can be computed using the WHO 2006 Growth Standard:

* Length/height-for-age
* Weight-for-age
* Weight-for-length/height
* Body mass index-for-age
* Head circumference-for-age
* Arm circumference-for-age
* Subscapular skinfold-for-age
* Triceps skinfold-for-age

Z-scores for the following indicators can be computed using the WHO 2007 Growth Reference:

* Body mass index-for-age (61 to 228 months)
* Height-for-age (61 to 228 months)
* Weight-for-age (61 to 120 months)

Z-scores for the following indicators can be computed using the CDC 2000 Growth Charts:

* Body mass index-for-age
* Length/Height-for-age
* Weight-for-age
* Weight-for-length/height
* Head circumference-for-age

For more information and further documentation:

* WHO 2006 Growth Standards: http://www.who.int/childgrowth/en/
* WHO 2007 Growth Reference: http://www.who.int/growthref/en/
* CDC 2000 Growth Charts: https://www.cdc.gov/growthcharts/index.htm

# Usage

Be sure to first read and agree to the [license](LICENSE.txt) and then include the latest [NuGet Package](https://www.nuget.org/packages/AnthStat.Statistics/).

To compute a z-score and percentile for a 64 month-old male who has a body mass index (BMI) of 17 using the WHO 2007 Growth Reference:

``` cs
var who2007 = new AnthStat.Statistics.WHO2007();

double bmi = 17.0;
double ageMonths = 64;

double z = who2007.CalculateZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Male);
double p = StatHelper.CalculatePercentile(z);
```

Their z-score is 1.22 which equates to a percentile of 88.90.

To use the WHO 2006 Growth Standards to compute a z-score for a 32 day-old female with a BMI of 16:

``` cs
var who2006 = new AnthStat.Statistics.WHO2006();

double bmi = 16.0;
double ageDays = 32;

double z = who2006.CalculateZScore(Indicator.BMIForAge, ageDays, bmi, Sex.Female);
```

To use the CDC 2000 Growth Charts to compute a z-score for a 24-month old female with a BMI of 15.25:

``` cs
var cdc2000 = new AnthStat.Statistics.CDC2000();

double bmi = 15.25;
double ageMonths = 24;

double z = cdc2000.CalculateZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Female);
```

Some inputs are undefined per the WHO and CDC specifications. For example, the WHO 2007 Growth Reference defines ages between 61 and 228 months for the BMI-for-Age indicator. Attempting to use an age value outside of this range will result in an ```ArgumentOutOfRangeException```. Callers can avoid exceptions by asking ahead of time for a validation check:

``` cs
var cdc2000 = new AnthStat.Statistics.CDC2000();
bool isValid = cdc2000.IsValidMeasurement(Indicator.LengthForAge, ageMonths);

if (isValid)
{
    double z = cdc2000.CalculateZScore(Indicator.LengthForAge, ageMonths, length, Sex.Female);
}
```

Version 2.0 of the API makes the validation check easier by implementing a ```TryCalculateZScore``` method:

``` cs
double z = 0.0;
if (cdc2000.TryCalculateZScore(Indicator.WeightForAge, 48, weight, Sex.Male, ref z))
{
    z = Math.Round(z, 2);
    Console.WriteLine($"Weight-for-age z-score for a 48-month old male weighing {weight.ToString("N2")} kg : {z.ToString("N2")} ");
}
else
{
    Console.WriteLine($"Weight-for-age z-score could not be calculated - inputs out of range.");
}
```

# Versioning

AnthStat.Statistics follows [semantic versioning](http://semver.org/) guidelines. The major points to remember about semantic versioning are:

> Given a version number MAJOR.MINOR.PATCH, increment the:
> 
> MAJOR version when you make incompatible API changes,
> MINOR version when you add functionality in a backwards-compatible manner, and
> PATCH version when you make backwards-compatible bug fixes.

This implies version 2.x of the AnthStat.Statistics API is not backwards-compatible with version 1.x.

# Important Notes

## Unit of measure for age and interpolation

The lookup values for the WHO 2006 Growth Standards are indexed by age in days. Therefore, all age-based input to the ComputeZScore method in the WHO2006 class must supply the age in days. Supplying a non-integer day value (such a 4.5) will cause the value to be rounded to the nearest whole number to avoid interpolation. The only two indicators for the WHO 2006 Growth Standards that can accept decimal values are weight-for-length and weight-for-height. If a decimal value is supplied that is not found in the lookup table for these two indicators, then interpolation of the L, M, and S values will occur.

The WHO 2007 and CDC 2000 data sets are indexed by age in months. Age-based inputs to their ComputeZScore methods may supply either a whole-numbered month or a decimal month. Age in days or other units is unsupported. If a decimal month value is supplied and the decimal value is not found in the lookup table for that indicator, then interpolation of the L, M, and S values will occur to derive a z-score.

## Z-score recomputation

When using the WHO 2006 Growth Standards and the WHO 2007 Growth Reference, z-scores that are above 3 or below -3 will be recomputed per WHO guidelines <cite>\[1\]</cite> <cite>\[2\]</cite> <cite>\[3\]</cite>. This recomputation does not occur when generating z-scores using the CDC 2000 Growth Charts.

## Discrepancies versus WHO Anthro

There will be discrepancies when comparing z-scores computed using AnthStat.Statistics and those computed using the "WHO Anthro" software package (http://www.who.int/childgrowth/software/en/) under certain conditions. This occurs because WHO Anthro adds 0.7cm to a child's length when the child is < 24 months old and measured standing up, and subtracts 0.7cm from the child's height when they are >= 24 months old and measured lying down <cite>\[4\]</cite>. These adjustments are made internally by the WHO Anthro software and are not shown to the user, though they are documented in the WHO Anthro user manual. This adjusted height or length is used to compute all other length/height-based indicators, which include BMI-for-age, Weight-for-Age, Weight-for-Length, Weight-for-Height, and Height-for-Age.

AnthStat.Statistics does __not__ change the measurement inputs it is provided. It assumes that the caller has made the decision about whether and how to adjust the inputs before supplying them to this library.

## Discrepancies versus Epi Info 7

CDC's Epi Info 7.2.1 for Windows Desktop software suite (see https://www.cdc.gov/epiinfo) uses a month-based set of L, M, and S data points for implementing the WHO 2006 Growth Standards. AnthStat.Statistics uses a day-based set of L, M, and S data points for implementing the WHO 2006 Growth Standards. AnthStat.Statistics is therefore more precise - it needs no interpolation when presented with specific age measurements that do not fall on whole-numbered month values.

# Quality Assurance

There are over 200 unit tests that check the software's computed z-scores against a set of hand-computed (using a calculator tool) z-scores for the same set of input measurements. The hand-computed z-scores relied on the WHO or CDC growth data files to obtain their L, M, and S values. The hand-computing process used L, M, and S values from the CDC and WHO websites, not from the software's source code; this was done in case the transformation from text files to C# was done incorrectly. Both interpolated and non-interpolated z-scores are checked under various conditions. Every indicator for each of the three growth data sets (WHO 2006, WHO 2007, and CDC 2000) are checked extensively. 

The WHO Anthro software was used as a second source of truth when deriving the expected z-score outputs for the WHO 2006 Growth Standards.

Performance testing shows that AnthStat.Statistics computes 1,000,000 z-scores from the CDC 2000 Growth Charts in about 450 milliseconds (interpolated) or 170 milliseconds (non-interpolated) on a modern desktop CPU using a single thread and the .NET Core runtime. Using .NET's Task Parallel Library and a ```Parallel.ForEach```, a quad-core desktop processor with hyperthreading sees a 4x performance boost when calculating z-scores for the same set of 1,000,000 records. An example of how to use a ```Parallel.ForEach``` loop to process z-scores can be found in the samples folder.

# References

\[1\] http://www.who.int/childgrowth/standards/Chap_7.pdf

\[2\] http://www.who.int/childgrowth/standards/second_set/tr2chap_7.pdf

\[3\] http://www.who.int/growthref/computation.pdf

\[4\] http://www.who.int/childgrowth/software/anthro_pc_manual_v322.pdf (pages 8-9)
