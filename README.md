# 对象池

对于Unity中对象池的封装。

池系统分为池工厂和缓存器两个抽象部分。池工厂需要实现对象真正创建销毁的方法，并服务于上层调用；缓存器负责元素淘汰策略，是系统的下层实现，对上层不可见。一个池工厂可以自由选择各式各样的缓存器。

## 优势

+ 简洁、高效
+ 缓存策略与工厂分离。可以自由组合为新的池。
+ 写过的代码可复用程度高
+ 池对象可以在Inspector中可视化
+ 区分LowMemory和用户主动释放，可执行不同的释放策略

## 灵活组合

可以使用容器为不同的Pool注册不同的缓存器。

假如有UI池，子弹池。可配置如下：

```csharp
void Init(){
    // UI池使用45秒计时缓存
    ServiceContainer.In<UIPool>().Bind<ICache>().To<MaxLifetimeCache>(45);

    // 子弹池使用300上限的LRU缓存
    ServiceContainer.In<BulletPool>().Bind<ICache>().To<LruCache>(300);
    // 切换为不缓存，对比效果
    // ServiceContainer.In<BulletPool>().Bind<ICache>().To<NoCache>(); 
}
```

## 内置缓存策略

内置了几个常用的缓存器

|名称|简述|
|:-|:-|
|NoCache|不使用缓存|
|GreedCache|不主动淘汰元素的缓存 |
|MaxLifetimeCache|非活动元素计时淘汰|
|WeakCache|使用弱引用保存非活动元素（交由GC淘汰）|
|LruCache|使用LRU算法的缓存，需要提供Capacity|

## 自由扩展

首先，创建一个名为`BBFramework.Main`的程序集，之后的相关扩展类、实现类、容器注册都在此程序集内完成。

如果需要自定义新的缓存器，则继承ICache，并实现以下方法（可参考Caches目录下实现）：

 + Rent - 申请一个元素，没有则创建
 + Return - 归还一个元素

如果需要自定义新的池，则继承IPool，实现对象真正创建和销毁的方法

## TODO LIST

1. More samples
