# Tabalonia

[![Nuget](https://img.shields.io/nuget/v/Tabalonia?label=Tabalonia)](https://www.nuget.org/packages/Tabalonia)

Draggable tab items on Avalonia here!

This is a port of the [Draggablz](https://github.com/ButchersBoy/Dragablz)

![example](https://github.com/egorozh/Tabalonia/blob/main/workflows/demo.gif "Example application")

### Getting Started

Install the library as a NuGet package:

```powershell
Install-Package Tabalonia
# Or 'dotnet add package Tabalonia'
```

Add the following to your App.axaml:
```xml
...
xmlns:themes="using:Tabalonia.Themes.Custom;assembly=Tabalonia"
...
    <Application.Styles>
        ...
        <themes:FluentTheme/>
    </Application.Styles>
...
```

Done! Use TabsControl like: 
```xml
...
xmlns:controls="using:Tabalonia.Controls;assembly=Tabalonia"
...
<controls:TabsControl ItemsSource="{Binding TabItems}"
                      NewItemFactory="{Binding NewItemFactory}"
                      FixedHeaderCount="1">
    <TabControl.ContentTemplate>
        <DataTemplate DataType="{x:Type dragablzDemo:TabItemViewModel}">
            <Grid Background="{DynamicResource SelectedTabItemBackgroundBrush}">
                <TextBlock  Text="{Binding SimpleContent}"
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"/>
            </Grid>
        </DataTemplate>
    </TabControl.ContentTemplate>
    <TabControl.ItemTemplate>
        <DataTemplate DataType="{x:Type dragablzDemo:TabItemViewModel}">
            <TextBlock Text="{Binding Header}" />
        </DataTemplate>
    </TabControl.ItemTemplate>
</controls:TabsControl>
```
