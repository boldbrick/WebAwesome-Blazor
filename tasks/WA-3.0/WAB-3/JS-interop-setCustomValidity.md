In the @src\WebAwesome.Blazor\ there are many TODOs to implement JS interop-dependent features. One of them is support for the setCustomValidity() method. Theare are TODOs in only two Wa* components. However, the Web Awesome documentation and source code indicates this is supported by many elements, namely:

- wa-button
- wa-checkbox
- wa-color-picker
- wa-input
- wa-radio
- wa-radio-group
- wa-select
- wa-slider
- wa-switch
- wa-textarea

The feature is described in @inputs\WebAwesome\form-controls.md.

The goal is to provide JS interop-based method that invokes setCustomValidity(). Analyze the situation, propose whether the feature can be implemented in a common ancestor and/or and interface is needed (can we implement the method in the interface?).