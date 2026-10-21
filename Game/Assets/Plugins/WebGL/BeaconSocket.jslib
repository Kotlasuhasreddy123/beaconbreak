mergeInto(LibraryManager.library, {
  BB_Open: function(rolePtr, roomPtr) {
    if (window.bbSocket) { window.bbSocket.onclose = null; window.bbSocket.close(); }
    var role = UTF8ToString(rolePtr), room = UTF8ToString(roomPtr);
    var socket = new WebSocket((location.protocol === 'https:' ? 'wss://' : 'ws://') + location.host + '/room/' + encodeURIComponent(room) + '?role=' + role);
    window.bbSocket = socket;
    socket.onmessage = function(e) { if (window.bbSocket === socket) SendMessage('Co-op Fighter Game', 'ReceiveWebLine', e.data); };
    socket.onerror = function() { if (window.bbSocket === socket) SendMessage('Co-op Fighter Game', 'ReceiveWebLine', '!Cannot reach the room server. Refresh and try again.'); };
    socket.onclose = function() { if (window.bbSocket === socket) SendMessage('Co-op Fighter Game', 'ReceiveWebLine', '!Room closed. Create a new room to continue.'); };
  },
  BB_Send: function(dataPtr) { var s = window.bbSocket; if (s && s.readyState === 1) s.send(UTF8ToString(dataPtr)); },
  BB_Close: function() { var s = window.bbSocket; window.bbSocket = null; if (s) { s.onclose = null; s.close(); } },
  BB_Touch: function() { return navigator.maxTouchPoints > 0 ? 1 : 0; },
  BB_Status: function(dataPtr) {
    window.beaconbreakState = JSON.parse(UTF8ToString(dataPtr));
    var fighting = window.beaconbreakState.screen === 'Fight';
    var join = document.getElementById('join-tools'), full = document.getElementById('fullscreen');
    if (join) join.style.display = fighting ? 'none' : 'block';
    if (full) full.style.display = fighting ? 'none' : 'block';
  }
});
