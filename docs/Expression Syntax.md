# Expression Syntax

The syntax of _Mastersign.Expressions_ supports literals, symbols (variable, constant, parameter), operators, groups, and function calls.

* [Literals](#literals)
    + [Boolean Literals](#boolean-literals)
    + [Numeric Literals](#numeric-literals)
    + [String Literals](#string-literals)
    + [Null Literal](#null-literal)
* [Operators](#operators)
    + [Logical Operators](#logical-operators)
    + [Numerical Operators](#numerical-operators)
    + [String Operators](#string-operators)
    + [Comparison Operators](#comparison-operators)
    + [Operator Priority and Binding](#operator-priority-and-binding)
* [Grouping](#grouping)
* [Variables, Constants, Parameters](#variables-constants-parameter)
* [Function Calls](#function-calls)
    + [Integrated Functions](#integrated-functions)
* [Member Read](#member-read)
* [Language Options](#language-options)

## Literals
There are boolean, numerical, and string literals. Additionally there is the null literal for empty CLR references.

### Boolean Literals
Boolean literals are `true` and `false`.
By default, they must be lowercase and are evaluated as values of the primitive CLR type `System.Boolean`.

### Numeric Literals
The numerical literals split up into integer, floating point, and decimal literals. All numerical literals are signed values. Integer literals are evaluated as values of the primitive CLR type `System.Int32` or `System.Int64`. Floating point literals are evaluated as values of the primitive CLR type `System.Single` or `System.Double`. Decimal literals are evaluated as values of the primitive CLR type `System.Decimal`. The literals are only recognized in international number style, what means that the decimal separator is the point. Additional separators for groups of thousand or scientific notation with some kind of exponent are not supported. Between a sign (`+`, `-`) and the first digit of a number no whitespace is allowed.

| Typ | Examples | Description |
|-----|----------|-------------|
| Auto Integer | `1`, `98765443`, `-400`, `+200`, `0020` | The size of the value decides, if 32 or 64 Bit are necessary |
| 32 Bit Integer | `24i`, `42I`, `-300i`, `+400i` | The postfix 'i' can be lower or upper case. The value is represented by `System.Int32`. |
| 64 Bit Integer | `6000L`, `-1234L`, `+2l` | The postfix 'L' can be lower or upper case. The value is represented by `System.Int64`. |
| Auto Floating Point | `0.0`, `123.567`, `-1.0`, `+0.02`, `001.001` | The precision of the value decides, if 32 or 64 Bit are necessary. |
| 32 Bit Floating Point | `0.0f`, `42F`, `-30.0f`, `+002.1F` | The postfix 'f' can be lower or upper case. The value is represented by `System.Single`. |
| 64 Bit Floating Point | `0.0d`, `42D`, `-30.0D`, `+002.1D` | The postfix 'd' can be lower or upper case. The value is represented by `System.Double`. |
| Decimal | `24.99m`, `-90.95M`, `+00.001m` | The postfix 'm' can be lower or upper case. The value is represented by `System.Decimal`. |

### String Literals
Strings are encased with double quotes, by default.
The quote style of strings can be changed in the language options to single quotes or backticks.
They can contain escape characters, which are masked by the back slash `\`.

Examples: `"abc"`, `"Hello World!"`, `"He said, \"What a weather!\""`, `"C:\\User\\"`

| Escape Character | Meaning |
|------------------|---------|
| `\\` | A single backslash _\_. |
| `\"` | Double quotes _"_, in case it is a string quote character. |
| `\'` | Single quotes _'_, in case it is a string quote character. |
| `\&#96;` | Backticks _`_, in case it is a string quote character. |
| `\a` | Ring (Unicode (0x0007). |
| `\b` | Backspace, or one step back resp.  (Unicode 0x0008). |
| `\f` | New page (Unicode 0x000C). |
| `\n` | New line (Unicode 0x000A). |
| `\r` | Carriage return (Unicode 0x000D). |
| `\t` | Tab stop (Unicode 0x0009). |
| `\v` | Vertical tab stop (Unicode 0x000B). |

### Null Literal
The null literal is `null`.
By default, it must be lower case and represents an empty CLR reference.
Null is compatible with every reference type of the CLR type system.

## Operators
Operators serve to combine two values like `1 + 2`.
There are logical, numerical, string, and comparison operators.

### Logical Operators
Logical operators only combine boolean values or expressions, which have the type `System.Boolean`.
By default the operator keywords must be lower case.

| Operator | Example | Description |
|----------|---------|-------------|
| `and` | `true and false` | The logical conjunction does only result in _true_, if both operands are _true_. |
| `or` | `true or false` | The logical disjunction does result in _true_, if at least one of the operands is _true_. |
| `xor` | `true xor false` | The logical exclusive disjunction does result in _true_, if exactly one of the operands is _true_. |

Hint: There is no operator for negating a boolean value. But there is a function `not()` for that purpose.

### Numerical Operators
The following operators combine two numerical values. If two values with less than 32 Bit precision are combined (e.g. `System.Byte`, `System.Int16`), they are converted to `System.Int32` before the operation. If two values of different precision are combined, then the less precise value is converted into the type of the more precise value before the operation.

| Operator | Examples | Description |
|----------|----------|-------------|
| `+` | `1+2`, `-4 + -9.99m`, `3F+4F` | The addition computes the sum of tow values. |
| `-` | `3-4`, `18 - -2.0`, `4L-30m` | The subtraction computes the differenz of two values. |
| `*` | `10 * 2`, `42*-100.001` | The multiplication computes the product of two values. |
| `/` | `1/3`, `2/3.0`, `-8.0m/2` | The division computes the ration of two values. |
| `^` | `2^10`, `10^3.151492D` | The exponential notation computes one value to the power of another number. |

### String Operators
There is only one string operator, which is the concatenation. Is an operand no string (`System.String`), the value is converted to a string by calling the method `System.Object.ToString()`. If an operand a null reference, it is converted into an empty string.

| Operator | Examples | Description |
|----------|----------|-------------|
| `&` | `"Hallo " & "Welt!"`, `1&"x"`, `true & false` | The concatenation chains two strings. If an operand is not a string, it is converted to a string. |

### Comparison Operators
Comparison operators server to determine the relation between two values. Usally only values of the same type can be compared. But when comparing numerical values, a less precise operand is converted into the type of the more precise operand. Logical and string values can only be tested for equality or unequality.

| Operator | Examples | Description |
|----------|----------|-------------|
| `<` | `1 < 2`, `"a" < "b"` | The less-operator is evaluated to _true_, if the left operand is less the right operand. |
| `<=` | `3 <= 1.0`, `"Abc" <= "abc"` | The less-equal-operator is evaluated to _true_, if the left operand is less or equal the right operand. |
| `=` | `4.0 = 4`, `"b" = "c"`, `true = false` | The equality-operator is evaluated to _true_, if both operands are equal. |
| `<>` | `4.0 <> 4`, `"a" <> null`, `true <> false` | The unequality-operator is evaluated to _true_, if the operands or not equal. |
| `>=` | `8.0 >= 8F`, `"400" >= "200"` | The greater-equal-operator is evaluated to _true_, if the left operand is greater or equal the right operand. |
| `>` | `100 > 0`, `"B" > "b"` | The greater-operand is evaluated to _true_, if the left operand is greater than the right operand. |

### Operator Priority and Binding
If more than one operator is used without grouping, the priority between the operators decides on the binding. Do two operators have the same priority, the left operator is evaluated first and the right operator is evaluated second.

Example: If the operators `*` and `+` would have the same priority, the expression `1 + 2 * 3` would be evaluated like `(1 + 2) * 3`, which is 9. But because the multiplication operator `*` has stronger binding than the addition operator `+`, the expression is evaluated like `1 + (2 * 3)`, which is 7.

The lower the priority level, the stronger the binding.

| Priority Level | Operators |
|----------------|-----------|
| 0 | `^` |
| 1 | `*`, `/` |
| 2 | `+`, `-` |
| 3 | `&` |
| 4 | `<`, `<=`, `=`, `<>`, `>=`,`>` |
| 5 | `and` |
| 6 | `or`, `xor` |

## Grouping
To control the binding of different operators, operations can be grouped by using parantheses.

Example: `1 + 2 * 3` is evaluated to 7, because of the stronger binding of the multiplication. By using parantheses, the addition can be computed before the multiplictaion: `(1 + 2) * 3`, which is evaluated to 9.

## Variables, Constants, Parameter

_since version: 0.2_

Variables are values which are stored in the evaluation context, and which can be referenced by a label or name respectively. If e.g. the value _100_ is registered under the name _x_, the expression `2 * x` is evaluated to _200_. Variables can contain value of any type: e.g. numerical values, string, and logical values. Variables do represent the labeled storage in the evaluation context and not the stored value. Therefore, if the value of a variable is changed in the evaluation context, after an expression referencing the variable is compiled, executing the expression will use the new value of the variable. 

A variable can be declared as constant by the developer, while registering in the evaluation context. If an expression referencing a constant is compiled, the value of the constant is included in the compiled expression. Therefore, if the value of a constant is changed in the evaluation context, after an expression referencing the constant is compiled, executing the expression will use the old value from the time of compiling. Where variables are more flexible in application, constants are faster in execution.

A parameter is syntactically identical to a variable (and a constant). But its value is not stored in the evaluation context, but is given when executing the compiled expression. If a parameter and a variable (or constant) have the same name, the parameter is hiding the variable (or constant).

## Function Calls
Functions are registered in the evaluation context and are referenced by a label or name respectively. When calling a function in an expression, the name is followed by the parameter list, which is encased by parantheses. The individual parameters are separated by commas. The call to a function _f_ without any parameters looks like `f()`.

If a function named _sin_ with one parameter of the type `System.Double` for computing the sine of a numerical value is registered in the evaluation context, the expression `sin(3.1514926535)` evaluates to approximately _0.0_. The call to a function which multiplies strings could look like `repeat("abc", 3)` and would result in `"abcabcabc"`.

Under one name, more than one function can be registered if the parameterlists are distinctive. This is called overloading. When calling an overloaded function within an expression, the best fitting alternative is used.

### Integrated Functions
There is currently one function integrated into the syntax of Mastersign.Expressions.

#### `if(condition, then, else)`

_since version: 0.4.2_

Conditional to select one of two values. The first argument must be an expression that yields a boolean value (_true_ or _false_),
second and third argument can be arbitrary expressions, but must result in values of the same type (e.g. both integer numbers, or both strings).
If the first argument is evaluated to _true_, the if expression results in the value of the
second argument. Otherwise, it results in the value of the third argument.

* Syntax: if(_condition_, _then-expr_, _else-expr_)
* Example: `if(a > b, 10, 1)`

Hint: Since version 0.6, the name of the conditional function can be changed in the language options, e. g. to `iif`.

## Member Read

_since version: 0.4.0_

To allow read-only access to fields and properties of values (or objects), the member read syntax can be used. To read a member of the resulting value of an expression, the dot notation is used. To read the field _b_ of expression _a_ the syntax is `a.b`. The expression _a_ can be anything which results in a value, having a public property or field _b_. Therefore, the expression `("abc" & "def").Length` results in _6_.

## Language Options

_since version: 0.6.0_

A number of language characteristics can be changed by the language options.

* capabilities like _Member Read_
* case ignorance
* boolean and null literal names
* conditional function name
* string quotation style
