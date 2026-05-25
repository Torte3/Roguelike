var IndexedDBSavePlugin = {
    $idbAllocateUtf8: function(str)
    {
        var size = lengthBytesUTF8(str) + 1;
        var ptr = _malloc(size);
        stringToUTF8(str, ptr, size);
        return ptr;
    },

    $idbEnsureInit: function()
    {
        if (Module.indexedDBSaveDb)
        {
            return;
        }

        if (Module.indexedDBSaveInitStarted)
        {
            return;
        }

        Module.indexedDBSaveInitStarted = 1;
        Module.indexedDBSaveDb = null;
        Module.indexedDBSaveInitDone = 0;

        var request = indexedDB.open('LogRogueSave', 1);
        request.onupgradeneeded = function(event)
        {
            var db = event.target.result;
            if (!db.objectStoreNames.contains('saves'))
            {
                db.createObjectStore('saves', { keyPath: 'save_id' });
            }
            if (!db.objectStoreNames.contains('latest_tracker'))
            {
                db.createObjectStore('latest_tracker', { keyPath: 'save_id' });
            }
            if (!db.objectStoreNames.contains('maps'))
            {
                db.createObjectStore('maps', { keyPath: 'key' });
            }
            if (!db.objectStoreNames.contains('global_statistics'))
            {
                db.createObjectStore('global_statistics', { keyPath: 'id' });
            }
            if (!db.objectStoreNames.contains('statistics'))
            {
                db.createObjectStore('statistics', { keyPath: 'save_id' });
            }
            if (!db.objectStoreNames.contains('global_settings'))
            {
                db.createObjectStore('global_settings', { keyPath: 'name' });
            }
            if (!db.objectStoreNames.contains('settings'))
            {
                db.createObjectStore('settings', { keyPath: 'key' });
            }
        };
        request.onsuccess = function(event)
        {
            Module.indexedDBSaveDb = event.target.result;
            Module.indexedDBSaveInitDone = 1;
        };
        request.onerror = function()
        {
            console.error('IndexedDB open failed');
            Module.indexedDBSaveInitDone = -1;
        };
    },

    IndexedDB_GetInitState: function()
    {
        return Module.indexedDBSaveInitDone || 0;
    },

    IndexedDB_GetOperationState: function()
    {
        return Module.indexedDBOpDone || 0;
    },

    IndexedDB_GetResultPtr: function()
    {
        return Module.indexedDBResultPtr || 0;
    },

    IndexedDB_GetResultInt: function()
    {
        return Module.indexedDBResultInt || 0;
    },

    IndexedDB_Init: function()
    {
        idbEnsureInit();
    },

    IndexedDB_Save: function(saveId, textPtr, turnWaitTime, bgm)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var text = UTF8ToString(textPtr);
        var tx = Module.indexedDBSaveDb.transaction(['saves'], 'readwrite');
        tx.objectStore('saves').put({ save_id: saveId, text: text, turnWaitTime: turnWaitTime, bgm: bgm });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveTurn: function(saveId, turn)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var tx = Module.indexedDBSaveDb.transaction(['latest_tracker'], 'readwrite');
        tx.objectStore('latest_tracker').put({ save_id: saveId, turn: turn });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveMap: function(saveId, mapIdPtr, textPtr)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var mapId = UTF8ToString(mapIdPtr);
        var text = UTF8ToString(textPtr);
        var key = saveId + '|' + mapId;
        var tx = Module.indexedDBSaveDb.transaction(['maps'], 'readwrite');
        tx.objectStore('maps').put({ key: key, text: text });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveGlobalStatistics: function(textPtr)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var text = UTF8ToString(textPtr);
        var tx = Module.indexedDBSaveDb.transaction(['global_statistics'], 'readwrite');
        var store = tx.objectStore('global_statistics');
        store.clear();
        store.put({ id: 0, text: text });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveStatistics: function(saveId, textPtr)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var text = UTF8ToString(textPtr);
        var tx = Module.indexedDBSaveDb.transaction(['statistics'], 'readwrite');
        tx.objectStore('statistics').put({ save_id: saveId, text: text });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveGlobalSetting: function(namePtr, value)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var name = UTF8ToString(namePtr);
        var tx = Module.indexedDBSaveDb.transaction(['global_settings'], 'readwrite');
        tx.objectStore('global_settings').put({ name: name, value: value });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_SaveSetting: function(saveId, namePtr, value)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var name = UTF8ToString(namePtr);
        var key = saveId + '|' + name;
        var tx = Module.indexedDBSaveDb.transaction(['settings'], 'readwrite');
        tx.objectStore('settings').put({ key: key, name: name, value: value });
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_ExistSave: function(saveId)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultInt = 0;
        var tx = Module.indexedDBSaveDb.transaction(['saves'], 'readonly');
        var request = tx.objectStore('saves').get(saveId);
        request.onsuccess = function()
        {
            Module.indexedDBResultInt = request.result ? 1 : 0;
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_ExistGlobal: function()
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultInt = 0;
        var tx = Module.indexedDBSaveDb.transaction(['global_statistics'], 'readonly');
        var request = tx.objectStore('global_statistics').count();
        request.onsuccess = function()
        {
            Module.indexedDBResultInt = request.result > 0 ? 1 : 0;
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_Load: function(saveId)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var tx = Module.indexedDBSaveDb.transaction(['saves'], 'readonly');
        var request = tx.objectStore('saves').get(saveId);
        request.onsuccess = function()
        {
            if (request.result)
            {
                var payload = JSON.stringify({
                    text: request.result.text,
                    turnWaitTime: request.result.turnWaitTime,
                    bgm: request.result.bgm
                });
                Module.indexedDBResultPtr = idbAllocateUtf8(payload);
            }
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadLatestTurn: function(saveId)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultInt = 0;
        var tx = Module.indexedDBSaveDb.transaction(['latest_tracker'], 'readonly');
        var request = tx.objectStore('latest_tracker').get(saveId);
        request.onsuccess = function()
        {
            if (request.result)
            {
                Module.indexedDBResultInt = request.result.turn;
            }
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadMap: function(saveId, mapIdPtr)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var mapId = UTF8ToString(mapIdPtr);
        var key = saveId + '|' + mapId;
        var tx = Module.indexedDBSaveDb.transaction(['maps'], 'readonly');
        var request = tx.objectStore('maps').get(key);
        request.onsuccess = function()
        {
            if (request.result)
            {
                Module.indexedDBResultPtr = idbAllocateUtf8(request.result.text);
            }
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadGlobalStatistics: function()
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var tx = Module.indexedDBSaveDb.transaction(['global_statistics'], 'readonly');
        var request = tx.objectStore('global_statistics').get(0);
        request.onsuccess = function()
        {
            if (request.result)
            {
                Module.indexedDBResultPtr = idbAllocateUtf8(request.result.text);
            }
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadStatistics: function(saveId)
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var tx = Module.indexedDBSaveDb.transaction(['statistics'], 'readonly');
        var request = tx.objectStore('statistics').get(saveId);
        request.onsuccess = function()
        {
            if (request.result)
            {
                Module.indexedDBResultPtr = idbAllocateUtf8(request.result.text);
            }
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadGlobalSettings: function()
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var tx = Module.indexedDBSaveDb.transaction(['global_settings'], 'readonly');
        var request = tx.objectStore('global_settings').getAll();
        request.onsuccess = function()
        {
            Module.indexedDBResultPtr = idbAllocateUtf8(JSON.stringify({ entries: request.result }));
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_LoadSettings: function()
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        Module.indexedDBResultPtr = 0;
        var tx = Module.indexedDBSaveDb.transaction(['settings'], 'readonly');
        var request = tx.objectStore('settings').getAll();
        request.onsuccess = function()
        {
            Module.indexedDBResultPtr = idbAllocateUtf8(JSON.stringify({ entries: request.result }));
            Module.indexedDBOpDone = 1;
        };
        request.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_ClearSave: function()
    {
        idbEnsureInit();
        Module.indexedDBOpDone = 0;
        var tx = Module.indexedDBSaveDb.transaction(['saves', 'maps'], 'readwrite');
        tx.objectStore('saves').clear();
        tx.objectStore('maps').clear();
        tx.oncomplete = function() { Module.indexedDBOpDone = 1; };
        tx.onerror = function() { Module.indexedDBOpDone = -1; };
    },

    IndexedDB_Free: function(ptr)
    {
        if (ptr)
        {
            _free(ptr);
        }
    }
};

autoAddDeps(IndexedDBSavePlugin, '$idbEnsureInit');
autoAddDeps(IndexedDBSavePlugin, '$idbAllocateUtf8');
mergeInto(LibraryManager.library, IndexedDBSavePlugin);
