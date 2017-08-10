# mvc-numericinput
### Description
A asp.net-mvc HtmlHelper extension method and jquery plug-in for creating a numeric input control the displays the formatted value of the property in accordance with the properties `type` and any `[DataType]` or `[DisplayFormat]` attributes. When focussed, the formatted value is overlaid with an `<input>` element that allows only valid entry (digits, decimal separator and negative sign) in accordance with the server culture.

#### Examples
`public int Quantity { get; set; }` with `en-AU` (Australia) culture

<img src="/Images/numeric-input-australia-normal.png" /> &nbsp;&nbsp;&nbsp; <img src="/Images/numeric-input-australia-focus.png" />

`[DataType(DataType.Currency)] public decimal Price { get; set; }` with `fr-FR` (France) culture

<img src="/Images/numeric-input-france-normal.png" /> &nbsp;&nbsp;&nbsp; <img src="/Images/numeric-input-france-focus.png" />

`[DisplayFormat(DataFormatString = "{0:P1}")] public decimal Rate { get; set; }` with `tr-TR` (Turkey) culture

<img src="/Images/numeric-input-turkey-normal.png" /> &nbsp;&nbsp;&nbsp; <img src="/Images/numeric-input-turkey-focus.png" />

### How it works
The extension method generates a parent `<div>` element containing `data-*` attributes that describe how the raw input value should be formatted based on the server culture. The parent contains a `<div>` for the formatted value and an overlaid transparent text `<input>`.

The jQuery plugin handles the
- `.focus()` and `.blur()` events to toggle the transparency of the `<input>` and generate the formated value
- `.keypress()` event to allow only valid input (digits, decimal separator and negative sign)

The plugin also provides an option for rounding the raw input

### Usage

To generate the html

    @Html.NumericInputFor(m => m.PropertyName)

where `PropertyName` is a numeric data type (`int`, `float`, `double` `decimal` etc)

To attach the plug-in

    $('#PropertyName').numeric();

or to round values

    $('#PropertyName').numeric({ 
        rounding: 0.25
    });

## To do
- Add parameter for html attributes
- Support for Bootstrap
