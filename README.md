# DeppartPrototype HentaiPlay

- DeppartPrototype Game Download: https://n4ba.itch.io/deppart
- Buttplug protocol: https://github.com/buttplugio/buttplug
- Intiface® Central: https://intiface.com/central/

### About the game DeppartPrototype

> Deppart is an indie first-person horror game with shooter elements. \
> Be very careful, enemies kill you with one hit. \
> https://n4ba.itch.io/deppart

## Feature

- Buttplug devices will be activated on these situations:
   - Gun shot
   - Jump-scares
   - During battle
   - Player died
   - Game end
- Mainly use `Vibrate` command, but you can add other scalar commands
- Provide an alternative event reporter instead of buttplug (`HttpReporter`)

## Usage

1. Install MelonLoader:
    - https://melonwiki.xyz/#/README?id=automated-installation
2. Download latest release and extract:
   - https://github.com/Ljzd-PRO/DeppartPrototypeHentaiPlayMod/releases/latest
3. Place the `Mods` directory under the game path.
4. Install [Intiface® Central](https://intiface.com/central/)
5. Launch Intiface® Central, start the engine server.
6. Launch the game, connect you Buttplug device to Intiface® Central.
7. (Optional) Configure the mod preference in `UserData\MelonPreferences.cfg` under the game path.
8. Enjoy the game.

## For DG-Lab Users

该 Mod 已适配 郊狼 2.0 3.0 即 DG-Lab-V2, V3，但是需要修改 Mod 配置，同时需要使用专门适配的 buttplug 分支。

- 郊狼 3.0 具体请查看文档：[`docs/dg-lab-v3.md`](docs/dg-lab-v3.md)
- 郊狼 2.0 由于没有设备可测试，无法给出具体配置参考，但可以参考 3.0 进行配置。

## Preference

Some important options:
- ButtPlugServerUrl
- ButtPlugActiveVibrateScalar
- ButtPlugShotVibrateScalar
- ButtPlugVibrateDuration
- ButtPlugVibrateCmdIndexList
- ButtPlugAdditionalScalarList

```cfg
[HentaiPlay]
# Type of reporter that report events in game (Available: BaseReporter, HttpReporter, ButtPlugReporter)
EventReporterType = "ButtPlugReporter"
# Report URL for HttpReporter
HttpReporterUrl = "http://127.0.0.1:7788/report"
# Time interval for HttpReporter to reporting InGame events
HttpReportInGameInterval = 3000
# Not to report events to Console
DisableEventLog = false
# Websocket URL of ButtPlug server (Intiface Central)
ButtPlugServerUrl = "ws://localhost:12345"
# Set the ButtPlug vibrate scalar when game events active
ButtPlugActiveVibrateScalar = 0.5
# Set the ButtPlug vibrate scalar when gun shot
ButtPlugShotVibrateScalar = 1.0
# Set the ButtPlug vibrate duration when gun shot (Millisecond)
ButtPlugVibrateDuration = 300
# Set the index of ButtPlug vibrate scalar commands, you can set multiple index or empty as default. (e.g. [0,1])
ButtPlugVibrateCmdIndexList = [ ]
# Set the additional ButtPlug scalar commands, which called during vibrate (It will set to 0 after vibrate stop)

[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = false
ActuatorType = "Oscillate"
Index = 0
Scalar = 0.5

[[HentaiPlay.ButtPlugAdditionalScalarList]]
Enable = false
ActuatorType = "Inflate"
Index = 0
Scalar = 0.5
```

## About `HttpReporter`

This is **OPTIONAL**, you can setup an HTTP server to handle events in the mod instead of using buttplug.

Set the option `EventReporterType` in `UserData\MelonPreferences.cfg` to `"HttpReporter"` if you want to use this.

### `HttpReporter` API

Define in [`openapi.yaml`](openapi.yaml).

### Example implementation for the server of `HttpReporter`

```python3
import datetime
from enum import StrEnum
from typing import Literal

from fastapi import FastAPI
from loguru import logger

app = FastAPI()


class EventNameEnum(StrEnum):
    GameEnter = "GameEnter"
    GameExit = "GameExit"
    InGame = "InGame"
    BulbBroken = "BulbBroken"
    ZombieRun = "ZombieRun"
    EnterLevel1 = "EnterLevel1"
    Level1Zombie = "Level1Zombie"
    EndZombie = "EndZombie"
    PlayerDied = "PlayerDied"
    Shot = "Shot"

@app.get("/report")
async def report(
        event_name: Literal[
            EventNameEnum.GameEnter,
            EventNameEnum.GameExit,
            EventNameEnum.InGame,
            EventNameEnum.BulbBroken,
            EventNameEnum.ZombieRun,
            EventNameEnum.EnterLevel1,
            EventNameEnum.Level1Zombie,
            EventNameEnum.EndZombie,
            EventNameEnum.PlayerDied,
            EventNameEnum.Shot
        ],
        status: Literal["activate", "deactivate"] = None,
        t: datetime.datetime = None
):
    logger.info(f"event_name: {event_name}, "
                f"status: {status}, "
                f"t: {t.astimezone(tz=None)}, "
                f"latency: {datetime.datetime.now(datetime.timezone.utc) - t}")

```
