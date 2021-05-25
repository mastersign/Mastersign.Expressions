# Packages

_Mastersign.Expressions_ comes with a number of packages for standard applications.
A package contains constants and functions.

* [Logic](#logic)
* [Math](#math)
* [String Manipulation](#string-manipulation)
* [Regular Expressions](#regular-expressions)

A package can be loaded by calling `EvaluationContext.Load...Package()`.
Calling `EvaluationContext.LoadAllPackages()` loads all packages that are currently included in _Mastersign.Expressions_.

## Logic

* `not(value)` - Inverts a logical value. `not(true)` is _false_ and `not(false)` is _true_

## Math

### Constants
* `pi` - The natural constant &pi; as 64 bit floating point number
* `e` - The natural constant _e_ or Euler number as 64 bit floating point number

### Functions
* `mod(num, den)` - The residue or modulo of the division between _num_ and _den_
* `abs(value)` - The absolute value of a number (`System.Math.Abs()`)
* `sign(value)` - Returns a 32 bit integer value, representing the sign of the number (`System.Math.Sign()`)
* `floor(value)` - Rounds a number downwards (`System.Math.Floor()`)
* `round(value)` - Rounds a number (`System.Math.Round()`)
* `ceil(calue)` - Rounds a number upwards (`System.Math.Ceiling()`)
* `trunc(value)` - Truncates the fraction of a value (`System.Math.Truncate()`)

* `sin(value)` - Computes the sine of an angle (`System.Math.Sin()`)
* `cos(value)` - Computes the cosine of an angle (`System.Math.Cos()`)
* `tan(value)` - Computes the tangent of an angle (`System.Math.Tan()`)
* `asin(value)` - Computes the angle, which sine is the given number (`System.Math.Asin()`)
* `acos(value)` - Computes the angle, which cosine is the given number (`System.Math.Acos()`)
* `atan(value)` - Computes the angle, which tangent is the given number (`System.Math.Atan()`)
* `atan2(y, x)` - Computes the angle, which tangent is the ratio of the given numbers (`System.Math.Atan2()`)
* `sinh(value)` - Computes the hyperbolic sine of an angle (`System.Math.Sinh()`)
* `cosh(value)` - Computes the hyperbolic cosine of an angle (`System.Math.Cosh()`)
* `tanh(value)` - Computes the hyperbolic tangent of an angle (`System.Math.Tanh()`)

* `sqrt(value)` - Computes the square root of the given number (`System.Math.Sqrt()`)
* `exp(value)` - Computes the given number to the power of e (Euler number) (`System.Math.Exp()`)
* `log(value)` - Computes the natural logarithm of the given number (`System.Math.Log()`)
* `log(base, exp)` - Computes the logarithm of _exp_ to the base of _base_ (`System.Math.Log()`)
* `log10(value)` - Computes the logarithm of the given value to the base of 10 (`System.Math.Log10()`)

* `min(a, b)` - Returns the lesser value of _a_ and _b_ (`System.Math.Min()`)
* `max(a,b)` - Returns the greater value of _a_ and _b_ (`System.Math.Max()`)

* `rand()` - Returns a random number between _0.0_ and _1.0_ as 64 bit floating point (`System.Random.NextDouble()`)

## Conversion

All conversion functions start with the prefix `c_` followed by a name describing the target type.

* `c_byte(value)` - Converts a numerical value into `System.Byte`
* `c_sbyte(value)` - Converts a numerical value into `System.SByte`
* `c_int16(value)` - Converts a numerical value into `System.Int16`
* `c_uint16(value)` - Converts a numerical value into `System.UInt16`
* `c_int32(value)` - Converts a numerical value into `System.Int32`
* `c_uint32(value)` - Converts a numerical value into `System.UInt32`
* `c_int64(value)` - Converts a numerical value into `System.Int64`
* `c_uint64(value)` - Converts a numerical value into `System.UInt64`
* `c_single(value)` - Converts a numerical value into `System.Single`
* `c_double(value)` - Converts a numerical value into `System.Double`
* `c_decimal(value)` - Converts a numerical value into `System.Decimal`
* `c_str(value)` - Converts an arbitrary value into a `System.String` by calling `System.Object.ToString()`

## String Manipulation

* `len(value)` - Returns the length of a string
* `to_lower(value)` - Converts the casing of a string into lower case (`System.String.ToLowerInvariant()`)
* `to_upper(value)` - Converts the casing of a string into upper case (`System.String.ToUpperInvariant()`)
* `trim(value)` - Removes leading and trailing whitespaces from a string (`System.String.Trim()`)
* `trim_start(value)` - Removes leading white spaces from a string (`System.String.TrimStart()`)
* `trim_end(value)` - Removes trailing whitespaces from a string (`System.String.TrimEnd()`)
* `substr(str, start)` - Returns a rest of a string, beginning with the character at zero-based position _start_ (`System.String.Substring()`)
* `substr(str, start, length)` - Returns a part of a string, starting with the character at zero-based position _start_ and being _length_ characters long (`System.String.Substring()`)
* `remove(str, start)` - Removes a trailing part from a string, starting with the character at zero-based position _start_ (`System.String.Remove()`)
* `remove(str, start, length)` - Removes a part from a string, starting with the character at zero-based position _start_ and being _length_ characters long (`System.String.Remove()`)
* `replace(str, old, new)` - Replaces every occurance of _new_ in _str_ with _old_ (`System.String.Replace()`)

* `find(haystack, needle)` - Searches for the first occurance of _needle_ in _haystack_ and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find(haystack, needle, start)` - Searches for the first occurance of _needle_ in _haystack_, beginning at _start_, and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find(haystack, needle, start, count)` - Searches for the first occurance of _needle_ in _haystack_, beginning at _start_ but only for the next _count_ characters, and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find_i(haystack, needle)` - Like `find(haystack, neddle)`, but case-insensitive (`System.String.IndexOf()`)
* `find_i(haystack, needle, start)` - Like `find(haystack, neddle, start)`, but case-insensitive (`System.String.IndexOf()`)
* `find_i(haystack, needle, start, count)` - Like `find(haystack, neddle, start, count)`, but case-insensitive (`System.String.IndexOf()`)
* `find_last(haystack, needle)` - Searches for the last occurance of _needle_ in _haystack_ and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find_last(haystack, needle, start)` - Searches for the last occurance of _needle_ in _haystack_, beginning at _start_, and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find_last(haystack, needle, start, count)` - Searches for the last occurance of _needle_ in _haystack_, beginning at _start_ but only for the next _count_ characters, and returns the zero-based position of the first character of _needle_ in _haystack_; if _needle_ is not found, _-1_ is returned (`System.String.IndexOf()`)
* `find_last_i(haystack, needle)` - Like `find_last(haystack, needle)`, but case-insensitive (`System.String.IndexOf()`)
* `find_last_i(haystack, needle, start)` - Like `find_last(haystack, needle, start)`, but case-insensitive (`System.String.IndexOf()`)
* `find_last_i(haystack, needle, start, count)` - Like `find_last(haystack, needle, start, count)`, but case-insensitive (`System.String.IndexOf()`)

* `contains(haystack, needle)` - Returns _true_ if _needle_ is found at least once in _haystack_; otherwise it returns _false_ (`System.String.Contains()`)
* `starts_with(str, start)` - Returns _true_ if _str_ begins with _start_; otherwise it returns _false_ (`System.String.StartsWith()`)
* `ends_with(str, end)` - Returns _true_ if _str_ ends with _end_; otherwise it returns _false_ (`System.String.StartsWith()`)

* `min(a, b)` - Returns the most less string, according to the alphabetical order
* `max(a, b)` - Returns the greatest string, according to the alphabetical order
* `min_i(a, b)` - Returns the most less string, according to the alphabetical order but case-insensitive
* `max_i(a, b)` - Returns the greatest string, according to the alphabetical order but case-insensitive

## Regular Expressions
* `regex(str, pattern)` - Returns _true_, if regular expression _pattern_ matches _str_ at least once (`System.Text.RegularExpressions.Regex.IsMatch()`)
* `regex_match(str, pattern)` - Returns the first character sequence in _str_, matched by the regular expression _pattern_ (`System.Text.RegularExpressions.Regex.Match()`)
* `regex_replace(str, patttern, new)` - Replaces every match of the regular expression _pattern_ in _str_ by _new_ (`System.Text.RegularExpressions.Regex.Replace()`)
