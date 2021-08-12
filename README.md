
* 1. 使用优化规则以空间换时间所做的 UI 框架.

* 2. GitHub: https://github.com/xinzhuzi/UIKit

* 3. QQ 群内部有一大波资料: 861960832

# UI 框架介绍

* 1. 开箱即用,Lua 或者 C# 及其方便,以 tolua 框架为底制作,移植及其方便.

* 2. 支持 2019.4 以及 2020 版

* 3. 先将设置 UI 环境,在 Project Settings的 Editor 选项中,将 Editing Environments 中的 UIEnvironment 设置为 UIEditorScene.      

Sprite Packer 选择 Sprite Atlas V2即可.         

添加 Layer --> NoGraphics.      

在 Package Manager 中添加 2D Sprite 插件

* 4. 没有 AB 加载模块,借助 toLua 框架,所有的 UI 都从 Resources 文件夹下模拟加载,如有需求,请自行编写.

* 5. 如有自己完整的项目,请直接将 Assets/Lua 文件夹下的文件拷贝到自己的项目中.

* 6. UIKit 中的 UIManager 是管理器, UIPoolManager 是缓存器.Lua 采用 MVC 的模式进行编写.

* 7. 使用时请将 lua 路径设置正确.

* 8. Analyze 是比较详尽的优化分析.

* 9. UITableView 是滚动视图,Events 中是事件快捷方式,DymincSpriteAtlas 是动态图集

* 10. 如果想要特殊效果,请参考 https://github.com/mob-sakai 此人仓库.


* 11. 感谢支持