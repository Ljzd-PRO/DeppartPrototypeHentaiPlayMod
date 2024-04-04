# DeppartPrototype HentaiPlay

- DeppartPrototype: https://n4ba.itch.io/deppart
- Buttplug protocol: https://github.com/buttplugio/buttplug
- Intiface® Central: https://intiface.com/central/

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

## Preference

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
```

## Example reporter handler for `HttpHandler`

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

## Extra

### DLLs

- DeppartPrototypeHentaiPlayMod.dll
- Buttplug.Client.Connectors.WebsocketConnector.dll
- Buttplug.dll
- deniszykov.WebSocketListener.dll
- Mono.HttpUtility.dll
- Newtonsoft.Json.dll
- System.IO.dll
- System.Net.Http.dll
- System.Runtime.CompilerServices.Unsafe.dll
- System.Runtime.dll
- System.Security.Cryptography.Algorithms.dll
- System.Security.Cryptography.Encoding.dll
- System.Security.Cryptography.Primitives.dll
- System.Security.Cryptography.X509Certificates.dll
- System.Threading.Channels.dll
- System.Threading.Tasks.Extensions.dll
- System.ValueTuple.dll
