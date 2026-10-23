"""Duskbound static WebGL host and bounded, two-person WebSocket room relay.

Run: python server.py. No accounts, database, or paid backend are required.
The host player's browser owns simulation; this relay only forwards messages.
"""
import asyncio
import os
import re
import time
from pathlib import Path
from aiohttp import web, WSMsgType

PUBLIC = Path(__file__).parent / "public"
ROOMS = {}
MAX_ROOMS = 32


async def room_socket(request):
    code = request.match_info["code"].upper()
    role = request.query.get("role", "")
    ws = web.WebSocketResponse(heartbeat=20, max_msg_size=65536, compress=False)
    await ws.prepare(request)
    if not re.fullmatch(r"[A-F0-9]{8}", code) or role not in ("host", "guest"):
        await ws.send_str("!Enter the eight-character room code shown on your partner's screen.")
        await ws.close()
        return ws
    room = ROOMS.get(code)
    if role == "host":
        if room is not None or len(ROOMS) >= MAX_ROOMS:
            await ws.send_str("!Room unavailable. Create a new room.")
            await ws.close()
            return ws
        room = {"host": ws, "guest": None, "created": time.monotonic()}
        ROOMS[code] = room
        await ws.send_str("@Room open. Share " + code + " with your partner.")
    elif room is None or room["guest"] is not None:
        await ws.send_str("!Room not found. Ask your partner to create a room and keep that screen open." if room is None else "!Room is full. Two players are already connected; ask your partner for a new room.")
        await ws.close()
        return ws
    else:
        room["guest"] = ws
        await room["host"].send_str("@Connected")
        await ws.send_str("@Connected")
    window, messages = time.monotonic(), 0
    try:
        async for message in ws:
            if message.type != WSMsgType.TEXT:
                break
            now = time.monotonic()
            if now - window >= 1:
                window, messages = now, 0
            messages += 1
            if messages > 240 or not message.data.startswith("{"):
                await ws.send_str("!Invalid room traffic.")
                break
            peer = room["guest" if role == "host" else "host"]
            if peer is not None and not peer.closed:
                await asyncio.wait_for(peer.send_str(message.data), 5)
    except (ConnectionError, asyncio.TimeoutError):
        pass
    finally:
        if ROOMS.get(code) is room:
            ROOMS.pop(code, None)
        peer = room["guest" if role == "host" else "host"]
        if peer is not None and not peer.closed:
            try:
                await peer.send_str("!Your partner left. Create a new room to resume.")
                await peer.close()
            except ConnectionError:
                pass
        await ws.close()
    return ws


async def health(request):
    return web.json_response({"status": "ok", "rooms": len(ROOMS), "version": "2.0"})


async def static_file(request):
    relative = request.match_info.get("path", "") or "index.html"
    path = (PUBLIC / relative).resolve()
    if not path.is_relative_to(PUBLIC.resolve()) or not path.is_file():
        raise web.HTTPNotFound()
    response = web.FileResponse(path)
    if path.name.endswith(".gz"):
        response.headers["Content-Encoding"] = "gzip"
        extension = Path(path.stem).suffix
        response.content_type = {".wasm": "application/wasm", ".js": "application/javascript"}.get(extension, "application/octet-stream")
    response.headers["X-Content-Type-Options"] = "nosniff"
    response.headers["Cache-Control"] = "no-cache"
    return response


async def cleanup_rooms(app):
    async def reap():
        while True:
            await asyncio.sleep(30)
            for room in list(ROOMS.values()):
                if room["guest"] is None and time.monotonic() - room["created"] > 600:
                    await room["host"].close()
    task = asyncio.create_task(reap())
    yield
    task.cancel()
    for room in list(ROOMS.values()):
        for role in ("host", "guest"):
            if room[role] is not None:
                await room[role].close()


def create_app():
    app = web.Application(client_max_size=65536)
    app.router.add_get("/health", health)
    app.router.add_get("/room/{code}", room_socket)
    app.router.add_get("/{path:.*}", static_file)
    app.cleanup_ctx.append(cleanup_rooms)
    return app


if __name__ == "__main__":
    web.run_app(create_app(), host=os.environ.get("HOST", "0.0.0.0"), port=int(os.environ.get("PORT", "8787")), access_log=None)
