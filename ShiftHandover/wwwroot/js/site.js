function confirmCloseShift() {
    if (confirm('Are you sure you want to close your shift? This action cannot be undone.')) {
        document.getElementById('closeShiftForm').submit();
    }
    // else: do nothing (stay on page)
}


function confirmSubmitLog() {
    const type = document.querySelector('select[name="Type"]').value;
    const involvedPerson = document.getElementById('involvedPersonInput').value.trim();

    if (type !== "Manpower" && involvedPerson === "") {
        alert("Please enter the Involved Person username.");
        return;
    }

    if (type === "Manpower" && (involvedPerson !== "" && isNaN(involvedPerson))) {
        alert("Please enter a valid manpower number.");
        return;
    }

    if (confirm("Are you sure you want to submit this log?")) {
        document.getElementById('logShiftForm').submit();
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const typeSelect = document.querySelector('select[name="Type"]');
    const involvedInput = document.getElementById('involvedPersonInput');
    const datalist = document.getElementById('usernamesList');

    if (typeSelect && involvedInput) {  // <-- ✅ Check if they exist
        typeSelect.addEventListener('change', function () {
            if (this.value === 'Manpower') {
                involvedInput.removeAttribute('list');
                involvedInput.placeholder = 'Enter manpower number';
            } else {
                involvedInput.setAttribute('list', 'usernamesList');
                involvedInput.placeholder = 'Enter username';
            }
        });
    }
});


//Session Checker
let sessionCheckInterval;
let sessionTimeout;

function resetSessionTimer() {
    clearTimeout(sessionTimeout);
    sessionTimeout = setTimeout(function () {
        alert('Your session has expired due to inactivity.');
        window.location.href = '/Account/Login';
    }, 15 * 60 * 1000); // 15 minutes
}

function checkSession() {
    fetch('/Account/CheckSession', { method: 'GET' })
        .then(response => {
            if (response.status === 401) {
                alert('Session expired. Please log in again.');
                window.location.href = '/Account/Login';
            }
        })
        .catch(error => {
            console.error('Session check failed:', error);
        });
}

document.addEventListener('DOMContentLoaded', function () {
    // Check session every 2 minutes
    sessionCheckInterval = setInterval(checkSession, 2 * 60 * 1000);

    // Reset timer on mouse move or key press
    document.addEventListener('mousemove', resetSessionTimer);
    document.addEventListener('keydown', resetSessionTimer);

    resetSessionTimer(); // Start the timer immediately
});