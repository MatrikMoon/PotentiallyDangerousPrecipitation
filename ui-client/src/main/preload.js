const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {
    send: (channel, data) => {
        // whitelist channels
        let validChannels = ['install-ipa'];
        if (validChannels.includes(channel)) {
            ipcRenderer.send(channel, data);
        }
    },
    receive: (channel, func) => {
        let validChannels = ['ipa-installed-status'];
        if (validChannels.includes(channel)) {
            // Deliberately strip event as it includes `sender`
            ipcRenderer.on(channel, (event, ...args) => func(...args));
        }
    },
});
