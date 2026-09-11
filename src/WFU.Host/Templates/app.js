// WFU Mini-App entry point
document.addEventListener('DOMContentLoaded', () => {
 const btn = document.getElementById('btn-hello');
 const output = document.getElementById('output');

 if (!btn || !output) return;

 btn.addEventListener('click', async () => {
 output.textContent = 'Button clicked!';
 // Call native bridge (Unity / WebView2 host)
 if (window.UnityBridge) {
 const result = await window.UnityBridge.call('hello', { time: Date.now() });
 console.log('Native response:', result);
 output.textContent += '\nNative: ' + JSON.stringify(result);
 }
 });
});
