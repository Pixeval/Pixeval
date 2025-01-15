# Structural Disposal
This is the design document of the structural disposal pattern, in this document we briefly discuss about this pattern and the guideline of using it.

The UI components, mostly the pages, requires itself or the resources that it manages to be disposed when it's unloaded, out of life, from the visual tree, moreover, since the resources they manage do not exploit a clear hierarchical structure, even the unload of one page will cause all of its children to be also unload, the resources held by children will, however, not necessarily be disposed alongside it, in order to handle this, we introduce the structural disposal pattern in Pixeval, and all subsequent scenarios should strictly follow this pattern.

The heart of the structural disposal pattern is an interface

```csharp
public interface IStructuralDisposalCompleter
{
    void CompleteDisposal();
    
    List<Action> ChildrenCompleters { get; }
}
```

The key is, **every page that requires such lifetime resource management should implement this interface**, if the page is derived from `EnhancedPage`, then nothing needs to be worried since all works are already done inside `EnhancedPage`, however, if such a component is a control other than page, or not a subclass of `EnhancedPage`, this interface needs thus to be implemented manually. The above code only reveals the most essential **abstract** functions of this interface, however, this interface also comes with several default-implemented functions, one of which is the `Hook()` function.

When deriving such a component, first, you should define the `Loaded` event of that component, which is supposed to be triggered when the content of the component is already rendered and ready to be mounted on the visual tree, inside the `Loaded` event, call `IStructuralDisposalCompleter.Hook()` method, then, implement `CompleteDisposal()` method, inside which is your custom disposal logic.

The mechanism is explained as follow: Every component will

1. Handle its own disposal by `CompleteDisposal` function.
2. Invoke the disposal logic of all its direct descendants.

 the disposal logics of the direct descendants are stored in `ChildrenCompleters`, so the point of `Hook()` is to register the `CompleteDisposal()` function of current component to its parent component.

By using this pattern, the disposal logics are chained, once a component disposes, all of its direct children are disposed and then all of its indirect children, however, this flow only propagates downward, if a child is disposed, nothing will happen to its parents.

Of course, all this chained needs a single source to invoke, the source of this chain will be some top-level components, currently, there are only two such components

1. `TabPage`, inside which the tab closed event will trigger the chain of disposal of itself and all its contents.
2. `EnhancedWindow`, when the window is closed, the chain of disposal of its content will be triggered.

Notice that the `IStructuralDisposalCompleter` comes with other two extra properties, `CompleterDisposed` and `CompleterRegistered`, so *there's no need to worry about the idempotency, the debounce is handled automatically by `IStructuralDisposalCompleter`.*

### Analyzer

It's noticed that the `Hook()` function **must be called manually each time you implementing such a component**, so Pixeval comes with an analyzer in `Pixeval.Analyzer` project, this analyzer monitors every class that implements `IStructuralDisposalCompleter`, and will issue an error if no call to `Hook()` is found in such a class.
