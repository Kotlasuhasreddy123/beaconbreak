import asyncio
import json
import sys
from pathlib import Path
from aiohttp import ClientSession, web, WSMsgType

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from server import create_app, ROOMS


async def run():
    runner = web.AppRunner(create_app())
    await runner.setup()
    site = web.TCPSite(runner, "127.0.0.1", 0)
    await site.start()
    port = site._server.sockets[0].getsockname()[1]
    base = f"http://127.0.0.1:{port}"
    class PassList(list):
        def append(self, value):
            super().append(value)
            print("PASS: " + value, flush=True)
    passed = PassList()
    async with ClientSession() as client:
        async def connect(code, role):
            return await client.ws_connect(f"{base}/room/{code}?role={role}")

        async with client.get(base + "/health") as response:
            assert response.status == 200
        passed.append("health endpoint")
        bad = await connect("bad", "guest")
        assert (await bad.receive()).data.startswith("!")
        passed.append("invalid code rejected")
        missing = await connect("ABCDEF12", "guest")
        assert (await missing.receive()).data.startswith("!")
        passed.append("missing room rejected")
        host = await connect("ABCDEF12", "host")
        assert (await host.receive()).data.startswith("@Room")
        duplicate = await connect("ABCDEF12", "host")
        assert (await duplicate.receive()).data.startswith("!")
        passed.append("duplicate host rejected")
        guest = await connect("ABCDEF12", "guest")
        assert (await host.receive()).data == "@Connected"
        assert (await guest.receive()).data == "@Connected"
        passed.append("two devices paired")
        third = await connect("ABCDEF12", "guest")
        assert (await third.receive()).data.startswith("!")
        passed.append("third player rejected")
        for sender, receiver in ((host, guest), (guest, host)):
            for tick in range(100):
                packet = json.dumps({"kind": "input", "tick": tick, "buttons": tick % 64})
                await sender.send_str(packet)
                assert (await receiver.receive(timeout=3)).data == packet
        passed.append("200 duplex messages preserved and ordered")
        other = await connect("12345678", "host")
        assert (await other.receive()).data.startswith("@Room")
        assert len(ROOMS) == 2
        passed.append("independent room isolation")
        await guest.close()
        assert (await host.receive(timeout=3)).data.startswith("!Your partner")
        await host.close()
        await other.close()
        await asyncio.sleep(0.1)
        assert not ROOMS
        passed.append("disconnect cleanup and partner notice")
        fresh = await connect("ABCDEF12", "host")
        assert (await fresh.receive()).data.startswith("@Room")
        await fresh.send_str("x" * 70000)
        event = await fresh.receive(timeout=3)
        assert event.type in (WSMsgType.CLOSE, WSMsgType.CLOSED, WSMsgType.ERROR)
        passed.append("oversized message closed")
    await runner.cleanup()
    target = Path(__file__).resolve().parents[2] / "Game" / "TestResults" / "relay-tests.json"
    target.parent.mkdir(exist_ok=True)
    target.write_text(json.dumps({"passed": passed, "failures": 0}, indent=2))
    print(f"RELAY_PASS: {len(passed)} tests")


if __name__ == "__main__":
    asyncio.run(asyncio.wait_for(run(), 45))
