# EnumSourceConverter
Directly bind items controls to enum properties

Add enum type properties to your view model:
```C#
  public enum ScopeType
    {
        Public,
        Private,
        Protected
    }
    public class ViewModel
    {
        public ScopeType? Scope { get; set; }
    }
```

Bind items controls to that property:
```xml
...
      xmlns:ui="http://toniccomputing.com/patterns/ui"
...
      <ComboBox ItemsSource="{ui:EnumSource Scope}" SelectedItem="{ui:EnumBinding Scope}" />

```
