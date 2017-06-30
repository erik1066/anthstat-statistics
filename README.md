# Description

AnthStat is a .NET Core 1.1 class library that computes z-scores for children and adolescents using the WHO 2006 Child Growth Standards, the WHO 2007 Growth Reference, and the CDC 2000 Growth Charts. A z-score-to-percentile converter is provided. 

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

Let's assume one wishes to compute a z-score for a 64 month-old male who has a body mass index (BMI) of 17:

``` cs
var who2007 = new AnthStat.Statistics.WHO2007();

double bmi = 17.0;
double ageMonths = 64;

double z = who2007.ComputeZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Male);
```

To get a percentile from the z-score:

```
p = StatHelper.GetPercentile(z);
```

To use the WHO 2006 Growth Standards to compute a z-score for a 32 day-old female with a BMI of 16:

``` cs
var who2006 = new AnthStat.Statistics.WHO2006();

double bmi = 16.0;
double ageDays = 32;

double z = who2006.ComputeZScore(Indicator.BMIForAge, ageDays, bmi, Sex.Female);
```

Some inputs are undefined per the WHO and CDC specifications. For example, the WHO 2007 Growth Reference defines ages (in months) between 61 and 228 for the BMI-for-Age indicator. Attempting to use an age value outside of this range will result in an exception.