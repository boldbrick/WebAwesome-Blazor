Render tree step numbering must be constant.

`sequence++` is an antipattern, because it may lead to false-positive DOM changes. Only correct pattern are constant:
```
        // Link behavior attributes
        builder.AddAttributeIfNotNullOrEmpty(10, "href", Href);
        builder.AddAttributeIfNotNullOrEmpty(11, "target", Target);
        builder.AddAttributeIfNotNullOrEmpty(12, "download", Download);
        builder.AddAttributeIfNotNullOrEmpty(13, "rel", Rel);

        // Add event handlers
        if (OnClick.HasDelegate)
            builder.AddAttribute(14, "onclick", OnClick);
```

Gaps in sequences are OK. Hence, when calling a method to build a portion, pass constant (e.g. 15) and in the method do `sequence + 0`, `sequence + 1` etc.

For editable values, you **MUST** produce specific bindings, like in inputs\Blazor\InputTextArea.cs or inputs\Blazor\InputCheckbox.cs:

```
        builder.AddAttribute(4, "value", CurrentValueAsString);
        builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");
```