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

    typeSelect.addEventListener('change', function () {
        if (this.value === 'Manpower') {
            involvedInput.removeAttribute('list');
            involvedInput.placeholder = 'Enter manpower number';
        } else {
            involvedInput.setAttribute('list', 'usernamesList');
            involvedInput.placeholder = 'Enter username';
        }
    });
});