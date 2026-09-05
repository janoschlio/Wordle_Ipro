// Theme Management

window.toggleTheme = function () {
    const html = document.documentElement;
    const newTheme = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';

    applyTheme(newTheme);
    localStorage.setItem('theme', newTheme);
};

// Systemwechsel nur uebernehmen, solange der Nutzer nichts eigenes gewaehlt hat.
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    if (!localStorage.getItem('theme')) {
        applyTheme(e.matches ? 'dark' : 'light');
    }
});

function applyTheme(theme) {
    const html = document.documentElement;
    html.setAttribute('data-theme', theme);

    // color-scheme aktualisieren, damit die light-dark() Funktion greift
    html.style.colorScheme = theme;
}
