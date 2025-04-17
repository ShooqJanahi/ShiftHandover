function confirmCloseShift() {
    if (confirm('Are you sure you want to close your shift? This action cannot be undone.')) {
        document.getElementById('closeShiftForm').submit();
    }
    // else: do nothing (stay on page)
}
