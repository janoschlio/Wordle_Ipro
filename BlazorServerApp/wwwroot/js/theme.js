// Theme Management
// Handles light/dark mode switching with localStorage persistence

window.initTheme = function () {
    const storedTheme = localStorage.getItem('theme');
    const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    // Use stored theme if available, otherwise use system preference
    const theme = storedTheme || (systemPrefersDark ? 'dark' : 'light');
    applyTheme(theme);

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
            applyTheme(e.matches ? 'dark' : 'light');
        }
    });
};

window.toggleTheme = function () {
    const html = document.documentElement;
    const currentTheme = html.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

    applyTheme(newTheme);
    localStorage.setItem('theme', newTheme);
};

function applyTheme(theme) {
    const html = document.documentElement;
    html.setAttribute('data-theme', theme);

    // Update color-scheme to trigger light-dark() function
    html.style.colorScheme = theme;
}
