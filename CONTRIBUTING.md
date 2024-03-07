# Pixeval 贡献指南

## LoginPage

用户登录的界面，应该以尽量简单可靠的方式完成登录。

如果可以找到pixiv登录的api是最好的方法，可以直接不使用WebView来登录，但可能没有这种方法，毕竟官方的app也是使用网页登录。

如果可以使用外部浏览器来登录也很好，但这样缺点是难以操作代理设置，返回token的时候也需要注册应用链接打开，十分麻烦。

原来使用Playwright当自动化登录的方式，但Playwright包太大了（WebView包不包含在应用包内）故使用js代码直接操作。
pixiv的页面是使用react写的，设置自动化费了好大劲，最终参考[这个方法](https://www.jianshu.com/p/78f5a4baf88c)实现了填写表单。

## 控件命名方式

### XXXItem

如`IllustrationItem`等，表示可以在列表视图中展示的一个卡片元素，可以展示一个画师，一幅插画等。
一般由`AdvancedItemsView`进行承载，并支持加载更多。

### XXXView

如`IllustrationView`等，是包含了一个`AdvancedItemsView`来展示`XXXItem`的控件，方便控件的复用。
也有一些`AdvancedItemsView`控件，由于目前没有复杂的复用需求，所以被直接包含在某些页面里了。

### XXXViewerPage

如`IllustrationViewerPage`等（`ImageViewerPage`除外），表示用一个单独窗口来展示详细信息的页面，可以展示一个画师，一幅插画等。
由于单独使用一个窗口，所以要继承自`SupportCustomTitleBarDragRegionPage`来支持拖拽区域定义，
同时搭配一个`XXXViewerPageHelper`的静态类，用来方便地从任何代码上下文呼出包含`XXXViewerPage`的窗口。

### EntryXXX

一般表示被Illustration、Illustrator共用的模型，如`IllustrationView`、`IllustratorView`都包含`EntryView`。

### AdvancedItemsView

继承自`ItemsView`，除了可以方便地指定`ItemsViewLayoutType`外，还支持滚动到底部时增量加载新的数据（需要`ItemsSource`实现`ISupportIncrementalLoading`）。
和`ItemsView`一样，它要求`ItemTemplate`中的控件被`ItemContainer`包裹。

## 模型类设计

### 下载模型

以`XXXDownloadTask`格式命名的都是下载模型，每种模型分为`XXXDownloadTask`、`IntrinsicXXXDownloadTask`、`LazyInitializedXXXDownloadTask`三种，
分别是普通的下载，已经完成的下载，和已经完成并且要懒加载的下载。其中后两种直接继承于第一种，第三种主要用在重新打开应用时显示。

### 增量加载器

与此相关的有`XXXDataProvider`和`XXXIncrementalSource`。
一般来说它们分别继承于`IDataProvider<T, TViewModel>`和`FetchEngineIncrementalSource<T, TModel>`，
（如`IllustrationViewDataProvider`和`IllustrationFetchEngineIncrementalSource`）但并非必要。
如果不是从网络上获取的数据源（如本地数据源），则不需要继承那么多方法，可以直接自己实现一个类似的即可
（如`DownloadListEntryDataProvider`和`DownloadListEntryIncrementalSource`）。

总的来说，`XXXIncrementalSource`是从数据源（如`IFetchEngine<T>`等）获取数据，并对外封装为`IEnumerable<T>`的形式，
`XXXDataProvider`是对`XXXIncrementalSource`的封装，管理它的新建、刷新、`Dispose`等功能，所以不是必要的。

## 工具类设计思想

### WindowsFactory

一个统一的窗口构建类，窗口默认包含一个`Frame`，用来承载页面。
由于`Window`类并不是继承自`UIElement`，在XAML等各处使用都很不方便，故通过封装隐藏所有操作。
除了第一个窗口，其他都是第一个窗口的子窗口。

### EnhancedPage/EnhancedWindowPage/SupportCustomTitleBarDragRegionPage

`EnhancedPage`通过封装将`OnNavigatedTo`、`OnNavigatingFrom`封装为更简单的`OnPageActivated`、`OnPageDeactivated`。
并且记录了同一个页面的导航次数、自动清理页面缓存。

`EnhancedWindowPage`继承自`EnhancedPage`，唯一的区别是在导航参数中隐式传递了所在的`Window`，可以方便使用一些需要`HWnd`的api。
这个参数在`EnhancedWindowPage`之间传递时是透明的。
如果调用导航的地方无法获取到现在所在的`Window`（比如说在层次很深的控件内），此时就应该使用`EnhancedPage`。

`SupportCustomTitleBarDragRegionPage`除了拥有`EnhancedWindowPage`的功能外，还添加一些方法用来计算、指定所在窗口的拖拽区域，
这对于自定义标题栏来说十分重要。一般这个页面会作为窗口内的底层页面。

### SharedRef

`SharedRef<T>`类似于C++的`shared_ptr<T>`，但由于C#语言限制，并不能做的十分完美。大致思想就是通过引用计数，来同步不同对象内对同一个对象引用的释放。
为了防止同一个对象错误地多次释放，释放时需要提供本对象的哈希值。

### AdvancedObservableCollection

类似于`AdvancedCollectionView`的泛型版本，但更好用，专门为`ItemsView/AdvancedItemsView`设计。
本质上是`ObservableCollection<T>`的底子上加了`ISupportIncrementalLoading`和筛选、排序功能。

`AdvancedCollectionView`实现了`ICollectionView`，所以支持和`ListView/GridView`中的`SelectedItems`同步，
但`ItemsView/AdvancedItemsView`并不支持`ICollectionView`，所以本类没有实现`ICollectionView`，也没有`SelectedItems`。
`AdvancedCollectionView`由于编写者疏忽，在许多地方都有一些BUG（尤其是插入新元素的逻辑），被发现的BUG在本类中都悉数修复了。

## 总体思想

### 少用WinRT API

例如`IRandomAccessStream`和`Stream`，项目中更倾向于使用原生的后者。除了因为WinRT需要COM可能会降低效率，也是因为在类型封送的时候不稳定，
可能导致`Position`为负数的情况。

WinRT自带的Bitmap解码器功能较差，经常出现正常图片无法解码的功能。本项目使用`ImageSharp`代替其解码、编码的职能。

剪切板api必须传入原生的`IRandomAccessStream`才能正常展示（`Stream.AsRandomAccessStream()`是不行的），但这个api别无选择。

### 重视内存管理

由于WinUI的内存泄露问题十分严重，在一切可以释放内存的地方都应该注意释放。例如项目中使用`SoftwareBitmap`和`SharedRef`就是为了尽快释放内存。

`RecyclableMemoryStream`可以提高`MemoryStream`的利用效率，在`IoHelper`中声明了`RecyclableMemoryStreamManager`，
如果需要大量使用`MemoryStream`，应该调用`IoHelper`。

### 通过继承、组合减少代码量

如`IllustrationView`、`IllustratorView`都包含`EntryView`，这是为了不要将相似的逻辑写两遍，否则在重构某部分时也许会漏掉另一部分。
提取不同类的共同部分并不总是为了抽象。

### XAML中少用复杂的Margin、Padding

布局如果可以就尽量使用`Grid`实现，`StackPanel`有时也可以使用，但它的堆叠方向长度是无穷的，导致难以适配父控件大小。
尽量使用父控件`Spacing`参数实现控件的间隔，指定`Padding`时也使用比较统一、简单的值（如资源中的`CardControlPadding`），
因为阅读XAML时复杂的值不方便人理解想象，在修改界面时也更难维护。

### 注意控件回收（Recycle）带来的BUG

被`DataTemplate`包裹的控件都可能会被回收。回收时XAML属性会被重新赋值，但不会重新触发从构造函数到`Loaded`中的内容。
如果XAML中的绑定没有使用`OneWay`，或者把某些数据加载逻辑写在`Loaded`中或之前，都有可能导致数据对不上的问题。

### 捕获网络异常

项目中最常出现的异常就是网络的异常，而且是不可避免的。如果要让网络的异常不影响到应用的正常运行，则需要抑制这些异常。
项目中引入了`FileLogger`，在抑制所有异常的同时记录，方便崩溃分析。
为了不需要重构更多代码（也为了不需要在每次网络请求时都考虑失败情况），在获得数据失败后会返回默认数据，给界面渲染。

相关api：`FactoryAttribute`，根据属性默认值自动生成默认赋值方法`CreateDefault`，在失败后会调用它并返回。

### 使用新的控件

在WASDK1.4中引入了新的控件`ScrollView`、`ItemsView`等，这些控件完全重写，使用更简洁的实现，获得比原来更多的功能。
而且在设计上也更符合直觉，黑箱操作更少。不过缺点是不够稳定，如`TagsEntry`中为了避免渲染BUG，
只好使用旧的`ItemsControl`+`ItemsPanel`代替新的`ItemsRepeater`+`Layout`

### 使用统一的命名空间，不必与文件夹结构同步

为了使`Controls`文件夹内文件更有层次，项目使用文件夹包裹这些控件，但可能会被提示应该与文件夹同步命名空间。
这时应该无视，因为在使用这些控件的时候可以用更加统一的命名空间（如`Pixeval.Controls`）来指定，无需写冗长的命名空间声明。
*CommunityToolkit*中也是这样做的。
