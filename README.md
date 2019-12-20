<div align="center">
  <h1>Pixeval - A Strong, Fast and Flexible Pixiv Client</h1>
  <img src="https://github.com/Rinacm/Pixeval/blob/master/IntroImages/pixeval.png"/>
</div>

## 开 幕 雷 击
  什么？你问我为什么README开头是这个东西？</br>
  ~~**当然是因为我怕你们太长懒得看然后看不到我最最最下面恰烂钱的那一段话**~~ 当然是因为我dc需要大家的激励啦！
  > 注: 接下来要说的话dc这个憨批会在结尾再重复一遍！！！所以你可以到结尾再看也无所谓！！！当然现在就看我会非常开心的！！！！！ </br>
  
  引用隔壁的一张图:</br>
  ![qiaqiaqia](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/%E4%B8%8D%E7%83%82%E7%9A%84%E9%92%B1%E4%B8%8D%E9%A6%99.png)       </br>
  经过深思熟虑我觉得LG说的非常有哲理</br>
  ~~要不然他妈的开头放一次广告结尾放一次广告，生怕别人看不出你在恰烂钱~~</br>
  **所以！！！！**</br>
  欢迎大家前往 =>>>>>> [恰烂钱](https://afdian.net/@dylech30th)</br> 来激励我的创作！！！你的支持是我创作和维护该项目的动力！！！</br>
  什么？你说拿到钱之后会干嘛？</br>
  ~~当然是每天早上出去喝完胡辣汤吃张葱油饼啦~~当然是用来维护这个项目的网站啦！(<<<= 截止2019年12月19日早上8:20这个网站还没有任何影子 by dc)
  
## TODOLIST（置顶）
  **如果有新的建议或者功能欢迎在issue区发表建议，如果有能力能够自己实现的话我会非常欢迎，届时请提交pull request，我会尽快查看并合并**
  - [X] 查看关注用户的最新作品
  - [ ] 添加查看画师的推特作品功能
  
## 写在前面
> **_本项目已经实现了ip直连，无需科学上网(虽然你用了其实也行....不过考虑到好多科学上网的速度还是比直连慢的...)_**</br>
> **_如果你是公司网/学校网等网络导致使用默认设置出现无法连接等网络问题，说明你的网络环境无法使用免代理，请使用压缩包内的Pixeval-UseDefaultProxy.bat并且打开电脑代理(ss/ssr/v2ray均可)之后使用_**

在使用Pixeval之前请**务必确保你安装了.NET Core 3.0+的运行环境**，你可以双击Pixeval文件夹里面的InstallDotNetCore3.0.bat，会自动帮你检测是否安装正确版本的.NET Core，没有安装的话会帮你自动下载安装；如果你不想使用该bat文件(咳咳，比如你担心我往你电脑里丢点不干净的东西)，你也可以按照下列步骤确认:</br>
* Win+R输入`cmd`，回车打开命令提示符窗口
* 输入`dotnet --list-runtimes`，回车执行
* 如果命令输出中包含 `Microsoft.NETCore.App 3.x.x`和`Microsoft.WindowsDesktop.App 3.x.x`则说明你的电脑上已经安装了.NET Core 3.0+的运行环境，如果没有这两行或者提示你dotnet命令不存在，说明你的电脑上没有安装.NET Core 3.0+，请前往[官网](https://dotnet.microsoft.com/download/dotnet-core/current/runtime)选择`Download x64`以下载.NET Core Runtime，下载完成后双击下载的exe文件按照指示步骤即可安装
* 本项目与.NET Core的关系好比Minecraft和java的关系，如果你连以上几步都不会，我建议你在理解上面这几句话之前不要使用任何.NET相关应用
  
## 我该如何获得Pixeval?
  点击Release选项卡或者戳[这里](https://github.com/Rinacm/Pixeval/releases)，选择最新的release版本，下载zip压缩包后解压，双击Pixeval.exe即可使用，如果你觉得Release界面下载太慢，我也同样在百度网盘维护着Pixeval的更新，你可以点击[这里](https://pan.baidu.com/s/1OMY06KduTk_js9L7YCGs3g)(提取码: 8vhx)前往百度网盘下载最新版本(其中有一份完整的说明，请认真阅读说明后再使用)
  
## 遇到BUG了怎么办?
  醒醒，难道你还停留在认为说一句“我程序崩了”我就能飞过去替你修好的年代吗？</br>
  **首先**，你要做的第一件事是来这里发ISSUE，每次程序报错都会生成一个错误日志</br>
  第一步，按下WIN+R，输入%appdata%，或者手动访问C:\Users\你的Windows账户\AppData\Local\pixeval\crash-reports</br>
  在这个文件夹里你能够找到一些错误日志，一般来说形如dd-MM-yyyy hh-mm-ss.txt，找到离你程序崩溃的时间点最近的那个文件(一般来说是最新的那个)，然后打开，把里面的内容复制出来，**在下面附上你是如何碰到这个BUG的，比如当时正在搜索/浏览什么，设置里面的选项哪些开了哪些没开，异常现象是什么(闪退/未响应/停止搜图)** 接着再在github下面提交issue，请尽可能详细的提供你的错误信息，这样有助于我排除BUG，如果做不到这一点，请不要抱有我能替你解决这个问题的希望，因为**抛开错误日志谈bug的行为无异于耍流氓**，谢谢配合
  
## 须知
  * **本项目所使用的依赖包** </br>
  
名称 | 链接 | 用途  
-|-|-
Newtonsoft.Json | https://github.com/JamesNK/Newtonsoft.Json | Json数据的解析 |
AngleSharp | https://github.com/AngleSharp/AngleSharp | HTML解析 |
FluentWPF | https://github.com/sourcechord/FluentWPF | 作品浏览器的Fluent窗口 |
Magick.NET | https://github.com/dlemstra/Magick.NET | GIF图片的合成 |
MahApps.Metro | https://github.com/MahApps/MahApps.Metro | 进度环 |
MaterialDesignTheme | https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit | 本项目所使用的UI库 |
Extended.Wpf.Toolkit | https://github.com/xceedsoftware/wpftoolkit | 水印文本框 |
PropertyChanged.Fody | https://github.com/Fody/PropertyChanged | 简化MVVM的数据绑定 |
System.Interactive.Async | https://github.com/dotnet/reactive | IAsyncEnumerable的异步linq操作支持 |
VirtualizingWrapPanel.NETCore | https://gitlab.com/sbaeumlisberger/virtualizing-wrap-panel | 虚拟化的WrapPanel |

  * **本项目绝对禁止商用**</br>
  Pixeval是一个兴趣使然的开源项目，一旦碰到任何商用行为请及时举报


## 什么是Pixeval
- Pixeval是一个Pixiv的桌面客户端，使用WPF和.NET Core 3.0编写，用来在Windows上方便快捷的访问Pixiv并提供大量P站作品的处理/过滤功能
- 本项目是一个兴趣使然的第三方开源项目，与Pixiv_Inc.没有任何关系

## 我可以用Pixeval做什么
  * **免代理访问Pixiv**
  * 根据关键字搜索作品
  * 根据关键字搜索用户
  * 查看自己的收藏
  * 查看自己的关注
  * 获取用户上传
  * 获取用户收藏夹
  * 获取每日推荐
  * 关注/取关用户
  * 收藏作品/取消收藏
  * 根据ID搜索作品
  * 根据ID搜索用户
  * 在搜索时根据收藏数排序
  * 设置搜索作品的最低收藏数
  * 过滤tag，开启/关闭R-18和R-18G搜索
  * 大批量下载图片，允许一次性下载用户的整个收藏夹/上传
  * 浏览/下载Pixivison(特辑)
  * 实时GIF显示
  * 查看作者的最新作品
 
  ## 我该如何使用Pixeval
  ### Pixeval的模块:
  * 主窗口:</br>
  ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/1.png)</br>
  主要的导航以及作品浏览窗口，里面包含了各种延展功能</br></br>
  * 设置窗口:</br>
  ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/2.png)</br>
  该窗口包含了对于Pixeval各项功能的设置，注意该设置中的所有选项作用域都是全局，也就是所有的作品/用户搜索例程都会以设置为标准
可供设置的选项:
    - 第1行：按照收藏数排序搜索到的所有作品，此选项关闭时默认按照时间顺序排序
    - 第2行：是否缓存作品的缩略图，打开之后可以优化搜索时对于缩略图的加载速度，缺点是会占用本地磁盘空间
    - 第3行：是否关闭对于R-18/R-18G作品的搜索，开启之后将会过滤掉所有包含R-18/R-18G元素的作品
    - 第4行：搜索作品的最低收藏数，默认为0，最高2000，Pixeval将会剔除查找到的所有收藏数低于该选项指定数值的作品
    - 第5行：要搜索多少页，由于考虑到电脑内存以及性能限制，Pixeval的单次最大搜索页数被设置为10页，对于普通的作品而言每页300张，10页共3000张，对于特辑     而言每页共10篇，10页共100篇，用户搜索/用户作品/用户收藏不受该选项影响
    - 第6行：对于普通作品搜索的起始页，当您设定该选项为1，搜索页数为10时将会搜索1-10共10页，其他数值同理，如果输入不是纯数字则会被忽略，使用最后一次设     置的合法数值，注意如果数值超过pixiv该作品的总页数将无法搜索到任何作品，用户搜索/用户作品/用户收藏不受该选项影响
    - 第7行：对于特辑搜索的起始页，用法与上一个选项相同，只是仅仅作用于特辑搜索
    - 第8行：下载位置，如果留空则默认为用户的图片文件夹
    - 第9行：要排除的标签，当关闭R-18/R-18G选项开启时会在该栏添加R-18 R-18G两个tag，用户可以自行添加其他想要排除的tag
    - 第10行：搜索的作品必须包含的tag，Pixeval会在搜索时剔除掉所有不包含该栏中的tag的作品
    - 最下方左侧是还原为默认设置按钮，右侧为清理缩略图缓存按钮</br></br>
* 作品列表:</br>
    诸如主页面的这种作品的集合被称为作品列表</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/3.png)</br>
    对一幅作品右键单击可以唤出右键菜单:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/4.png)</br>
    其中第一和第二个选项分别会立即下载该作品/当前作品列表的所有作品，第三和第四个选项会立即将该作品/该列表中的所有作品添加到下载列表</br>
    另外作品列表中单个作品左上角的心形按钮可以选择将该作品添加到收藏或者取消收藏</br></br>
    
* 作品浏览器:</br>
    对于作品列表中的任意一幅作品双击可以打开作品浏览器窗口:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/5.png)</br>
    作品浏览器可以查看当前列表中的所有图片，注意如果在一张作品被加载之前打开作品浏览器，则这幅作品不会出现在作品浏览器的列表中，作品浏览器的左右两侧     由上一张/下一张的按钮，可以根据其所在的作品列表进行翻页，当作品为GIF动图时会直接播放动画</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/6.png)</br>
    如果作品是一个图集，则右上角会显示一个图集样的小图标:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/7.png)</br>
    单击该图标可以打开另外一个作品浏览器，其中是该图集的所有作品</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/8.png)</br>
    作品浏览器上方的工具栏从左到右分别是查看作者，缩放图片，添加到收藏/取消收藏，下载图片，将该作品添加到下载列表以及在浏览器中访问该作品的页面</br>
    在图片的正下方是该作品的所有tag，单击其中任意一个就会导航到主页面方便搜索:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/9.png)</br></br>
    
* 用户浏览器:</br>
    在用户列表中单击任意一位用户可以打开用户浏览器，其封面如下:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/10.png)</br>
    下半部分为简单介绍，右侧的按钮可以关注/取关用户，点击下方的小箭头将会来到作品列表页：</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/11.png)</br>
    上方可以选择查看用户的作品(插画)或用户收藏的作品:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/12.png)</br>
    (该页面的作品列表与主页面的作品列表功能完全一致)</br></br>

* 下载列表:</br>
    当通过右键或者在作品浏览器中选择“添加到下载列表”时，该作品会被立即添加到下载列表，下载列表可以通过在主界面选择侧边栏的“下载列表”项查看</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/13.png)</br>
    每幅作品右边的下载按钮可以用来下载该作品，而最上方的下载按钮会下载列表内的所有作品，垃圾桶按钮则会清空下载列表，而右边的按钮可以用于收回下载列       表，双击下载列表内的任意一项可以打开图片浏览器，以下载列表内的作品作为数据源进行浏览:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/14.png)</br></br>
* 登出:</br>
    在主页面侧边栏选择登出后会立即退出当前账号，并且删除本地的配置文件，再次登录时会要求重新输入账号密码</br></br>
### Pixeval的功能</br>
* 主页面搜索栏:</br>
    在侧边栏选中“主页”即会跳转到搜索页面，搜索框提供四个选项: 通过tag查找作品，通过用户名查找用户，直接查找作品和直接查找用户，当鼠标点击搜索框         获取焦点时，下方会弹出一个选项栏，可以选择不同的搜索模式，从左到右分别是通过用户名查找用户，通过用户id查找用户和通过作品id查找作品，如果             三个选项都没有选中，则默认搜索模式为按照tag搜索作品，搜索模式选项栏效果如下:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/15.png)</br>
    当选择搜索用户时，Pixeval会根据搜索栏中的文本作为关键字对用户进行模糊搜索，效果如下: </br>
    搜索前:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/16.png)</br>
    搜索完成:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/17.png)</br>
    当选中搜索单个用户时，文本框中必须输入用户id，点击搜索后会直接打开用户浏览器以浏览该用户的作品/收藏，效果如下:</br>
    搜索前:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/18.png)</br>
    搜索完成，直接弹出用户浏览器窗口:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/19.png)</br>
    当选择搜索作品时，文本框中必须输入作品id，点击搜索后会直接打开作品浏览器以查阅作品</br></br>

* 我的收藏夹:</br>
    在侧边栏选择“我的收藏”一栏可以浏览自己收藏夹的全部内容，效果如下:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/20.png)</br></br>
* 我的关注:</br>
    在侧边栏选择我的关注，可以查看自己关注的所有用户，效果如下: </br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/21.png)</br></br>
* 特辑:</br>
    在侧边栏选择“特辑”，可以搜索总共100个不同的特辑(或者叫做Pixivision/Spotlight)，点击任意一栏即可打开作品浏览器对该特辑内的作品进行阅览:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/22.png)</br>
    浏览特辑作品:</br>
    ![pixeval](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/23.png)</br></br>
* 每日推荐:</br>
    每日推荐会显示当天所有的日推作品</br></br>
* 用户新作:</br>
    用户新作会显示关注用户的最新作品</br></br>
    
## 免代理访问Pixiv是如何实现的?
本质上是通过IP直连，具体实现请看`Data\Web\Delegation`文件夹中的`DnsResolver`及`DnsResolvedHttpClientHandler`类

## 写在最后
  这个项目从去年开始就在写，最开始是叫做pixivcs，其实那时候已经把大部分的功能啊UI啊什么的都写完了，本来可以直接放出来，可惜那时候的代码写的实在是太丑太乱了，大概19年6月份打算重构一下，然而因为本质懒狗所以重构了两天就不干了，一直拖到一个多星期前突然想起来这个项目，于是打算一鼓作气重构完，所以现在这个项目就是超级加倍重构之后的产物.....其实我对这个项目的代码还算是比较满意，至少比上一个要好得多，写这个项目也算是对于我自己的一个教训，深切的让我体会到了不事先打好框架就乱写一气的代价(所以千万不要学我啊！！！！！！
    
## 鸣谢
   感谢[@ControlNet](https://github.com/ControlNet) 在写这个项目的过程中一直支持我，每天无聊了就一起聊天，聊着聊着就安静下来了(x</br>
   感谢[@wulunshijian](https://github.com/wulunshijian) 在这一年一直都陪我学代码，在相互学习的过程中自己也巩固了很多知识</br>
   感谢[@duiweiya](https://github.com/duiweiya) 对味自发帮我制作的头图，我甚至都没有提出请求，这就是人与人之间的温情吗，泪流了下来</br>
   
## 恰烂钱
   引用隔壁的一张图:</br>
   ![qiaqiaqia](https://github.com/Rinacm/Pixeval/blob/master/IntroImages/%E4%B8%8D%E7%83%82%E7%9A%84%E9%92%B1%E4%B8%8D%E9%A6%99.png)</br>
   经过深思熟虑我觉得LG说的非常有哲理</br>
   ~~要不然他妈的开头放一次广告结尾放一次广告，生怕别人看不出你在恰烂钱~~</br>
   **所以！！！！**</br>
   欢迎大家前往 =>>>>>> [恰烂钱](https://afdian.net/@dylech30th)</br> 来激励我的创作！！！你的支持是我创作和维护该项目的动力！！！</br>
   什么？你说拿到钱之后会干嘛？</br>
   ~~当然是每天早上出去喝完胡辣汤吃张葱油饼啦~~当然是用来维护这个项目的网站啦！(<<<= 截止2019年12月19日早上8:20这个网站还没有任何影子)
