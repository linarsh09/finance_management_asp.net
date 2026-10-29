document.addEventListener("click", (event) => {
    const button = event.target.closest("[data-password-toggle]");
    if (!button) return;

    const input = document.getElementById(button.dataset.target);
    if (!input) return;

    const showPassword = input.type === "password";
    input.type = showPassword ? "text" : "password";
    button.setAttribute("aria-pressed", String(showPassword));
    button.setAttribute("aria-label", showPassword ? "Скрыть пароль" : "Показать пароль");
    button.querySelector("[data-eye]").classList.toggle("d-none", showPassword);
    button.querySelector("[data-eye-off]").classList.toggle("d-none", !showPassword);
});
