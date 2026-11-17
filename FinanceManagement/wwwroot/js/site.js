// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    const themeKey = "finance-management.theme";
    const themeToggle = document.getElementById("theme-toggle");

    const updateThemeButton = () => {
        const isDark = document.documentElement.classList.contains("dark-theme");
        if (!themeToggle) return;
        themeToggle.setAttribute("aria-pressed", String(isDark));
        themeToggle.setAttribute("aria-label", isDark ? "Включить светлую тему" : "Включить тёмную тему");
        themeToggle.querySelector(".theme-icon").textContent = isDark ? "☀" : "☾";
        themeToggle.querySelector(".theme-label").textContent = isDark ? "Светлая тема" : "Тёмная тема";
    };

    updateThemeButton();
    themeToggle?.addEventListener("click", () => {
        const isDark = document.documentElement.classList.toggle("dark-theme");
        updateThemeButton();
        try { localStorage.setItem(themeKey, isDark ? "dark" : "light"); }
        catch { /* Theme still works for this page if storage is unavailable. */ }
    });
})();
