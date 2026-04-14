function startCountdown() {
    // Prefer a <header> element; create one if it doesn't exist.
    let headerEl = document.querySelector('header');
    if (!headerEl) {
        headerEl = document.createElement('header');
        document.body.insertBefore(headerEl, document.body.firstChild);
    }

    // Ensure there's an <h1> inside the header we can update.
    let title = headerEl.querySelector('h1');
    if (!title) {
        title = document.createElement('h1');
        headerEl.appendChild(title);
    }

    let count = 5;
    let dots = 0;

    const interval = setInterval(() => {
        dots = (dots % 3) + 1;
        title.textContent = `Unfinished, redirecting to repo${'.'.repeat(dots)} ${count}`;

        count--;

        if (count < 0) {
            clearInterval(interval);
            window.location.href = 'https://github.com/MachineDisease/YoutubeGUI';
        }
    }, 1000);
}

startCountdown();