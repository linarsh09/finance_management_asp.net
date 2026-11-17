(() => {
    // Session Storage: remember the last visited app page for this tab only.
    try {
        sessionStorage.setItem("finance-management.last-page", location.pathname);
    } catch { /* Storage can be disabled by browser policy. */ }

    // IndexedDB: keep a small local history of operation IDs viewed in this browser.
    const rows = [...document.querySelectorAll("[data-transaction-id]")];
    if (!rows.length || !("indexedDB" in window)) return;

    const request = indexedDB.open("finance-management", 1);
    request.onupgradeneeded = () => {
        const db = request.result;
        if (!db.objectStoreNames.contains("recent-transactions")) {
            db.createObjectStore("recent-transactions", { keyPath: "id" });
        }
    };
    request.onsuccess = () => {
        const db = request.result;
        const tx = db.transaction("recent-transactions", "readwrite");
        const store = tx.objectStore("recent-transactions");
        for (const row of rows) {
            store.put({ id: Number(row.dataset.transactionId), viewedAt: Date.now() });
        }
        tx.oncomplete = () => db.close();
        tx.onerror = () => db.close();
    };
})();
