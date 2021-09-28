_G.UIModule = {
    empty_background = "empty_background",
    gm = "game_command/gm",
    login = "login_server/login",
    main_page = "main_city/main_page",
    
}

--此层级,是设置固定层级;不需要设置固定层的模块是按照刷油漆规则进行的自增层级效果.设置的是 canvas 的 OrderInLayer/sortingOrder 属性,设置的 SortingOrder必须要大于 20000,否则就会自动刷新层级
UIManager.Instance:UpdateFixedSortingOrder(UIModule.gm,30000)


--此数据是设置某个UI 模块优化,当打开这个 UI 模块时,需要将其隐藏的 UI 模块全部关闭,关闭这个 UI 模块时,再将其隐藏的 UI 模块内全部打开.
UIManager.Instance:UpdateOptCullUI(UIModule.login,true)