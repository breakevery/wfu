// Unity / WebView bridge adapter.
// In WFU preview mode, this provides a mock. In Unity host, it is replaced by the native bridge.
(function () {
 if (window.UnityBridge) return; // Already injected by native host

 // Mock for WFU preview mode
 window.UnityBridge = {
 call: async function (method, args) {
 console.log('[Mock UnityBridge]', method, args);
 return { ok: true, mock: true, method: method, args: args };
 }
 };
})();
