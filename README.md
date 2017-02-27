# mvc-numericinput
### Description
A asp.net-mvc HtmlHelper extension method and jquery plug-in for creating a numeric input control the displays the formatted value of the property in accordance with the properties `type` and any `[DataType]` or `[DisplayFormat]` attributes. When focussed, the formatted value is overlaid with an `<input>` element that allows only valid entry (digits, decimal separator and negative sign) in accordance with the server culture.

####Examples
`[DataType(DataType.Currency) public decimal Price { get; set; }` with `en-US` culture

<img src="/Images/numeric-input normal.png" /> &nbsp;&nbsp;&nbsp; <img src="/Images/numeric-input focus.png" />

###How it works
The extension method generates a parent `<div>` element containing `data-*` attributes that describe how the raw input value should be formatted based on the server culture. The parent contains a `<div>` for the formatted value and an overlaid transparent text `<input>`.

The jQuery plugin handles the
- `.focus()` and `.blur()` events to toggle the transparency of the `<input>` and generate the formated value
- `.keypress()` event to allow only valid input (digits, decimal separator and negative sign)
The plugin also provides an option for rounding the raw input
