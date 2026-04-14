function startCountdown() {
    const element = document.body;
    let count = 5;
    let dots = 0;

    const interval = setInterval(() => {
        dots = (dots % 3) + 1;
        element.textContent = `Unfinished, redirecting to repo${'.'.repeat(dots)} ${count}`;
        
        count--;
        
        if (count < 0) {
            clearInterval(interval);
             window.location.href = 'https://github.com/MachineDisease/YoutubeGUI';
        }
    }, 1000);
}

startCountdown();