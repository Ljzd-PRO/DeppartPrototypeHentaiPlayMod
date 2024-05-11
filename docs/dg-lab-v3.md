# DG-Lab-V3 Guide

由于 [buttplugio/buttplug](https://github.com/buttplugio/buttplug) 未适配郊狼，且由于其不适合该类产品，buttplug 不接受适配 PR。
因此需要使用专门适配过的 buttplug 分支，包括 Intiface Central。

## 安装

1. 前往 [**Ljzd-PRO/buttplug-dg-lab**](https://github.com/Ljzd-PRO/buttplug-dg-lab) 
的 Releases 页面下载最新的 Intiface Central 并安装
2. 启动 Intiface Central 的引擎服务，在 Devices 页面扫描蓝牙设备并连接上郊狼 3.0 \
    可以测试一下各项控制条是否有作用
3. 完成主页 [`README.md`](../README.md) 中剩余的其他安装步骤
4. **为郊狼 3.0 配置 Mod**，可参考已配置好的示例文件 [`examples/dg-lab-v3.cfg`](../examples/dg-lab-v3.cfg) \
    复制其全部内容，替换游戏目录下 `UserData\MelonPreferences.cfg` 配置文件中的 `[HentaiPlay]` 部分
5. 如果您是通过**手机等其他设备**的 Intiface Central App 连接的郊狼 3.0，也就是**不在运行游戏的电脑**上，
   那么需要修改默认的 `ButtPlugServerUrl` 选项，修改其中的 IP 地址为 Intiface Central 所在设备的 IP 地址，否则将无法连接

## 配置说明

一些修改了的配置选项，其中与强度相关的参数可以自行调整：

```cfg
# Set the ButtPlug vibrate scalar when game events active
# * 郊狼 3.0 虽然电源强度可调范围为 0-200，但默认上限为 100，因此一般此处 0.5 就已经是最大了
ButtPlugActiveVibrateScalar = 0.1

# Set the ButtPlug vibrate scalar when gun shot
# * 郊狼 3.0 虽然电源强度可调范围为 0-200，但默认上限为 100，因此一般此处 0.5 就已经是最大了
ButtPlugShotVibrateScalar = 0.25

# Set the ButtPlug vibrate duration when gun shot (Millisecond)
# * 郊狼 3.0 如果持续时间太短反馈不明显，太长则可能跟不上开枪速度
ButtPlugVibrateDuration = 300

# Set the index of ButtPlug vibrate scalar commands, you can set multiple index or empty as default. (e.g. [0,1])
# * 郊狼 3.0 电源强度 (Vibrate) 命令序号，0, 1 即 A 通道和 B 通道
ButtPlugVibrateCmdIndexList = [ 0, 1, ]

# Set the additional ButtPlug scalar commands, which called during vibrate (It will set to 0 after vibrate stop)

# * 郊狼 3.0 波形频率 (Oscillate) 命令序号 2 (A通道)
[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = true
ActuatorType = "Oscillate"
Index = 2
Scalar = 0.5
                                                  
# * 郊狼 3.0 波形频率 (Oscillate) 命令序号 3 (B 通道)
[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = true
ActuatorType = "Oscillate"
Index = 3
Scalar = 0.5

# * 郊狼 3.0 波形强度 (Inflate) 命令序号 4 (A 通道)
[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = true
ActuatorType = "Inflate"
Index = 4
Scalar = 0.5

# * 郊狼 3.0 波形强度 (Inflate) 命令序号 5 (B 通道)
[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = true
ActuatorType = "Inflate"
Index = 5
Scalar = 0.5
```