document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');

    if (calendarEl) {
        var calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'timeGridWeek',
            scrollTime: '07:00:00',   // Start view at 7am
            slotMinTime: '00:00:00',   // Allow scrolling from midnight
            slotMaxTime: '24:00:00',   // Until midnight
            height: 600,               //  Fix the height of the calendar
            contentHeight: 600,        //  Keep visible height fixed
            expandRows: false,         //  Disable automatic stretching
            handleWindowResize: true,  // Allow resize when window changes
            dayMaxEventRows: true,     // Allow multiple events per day
            nowIndicator: true,        // Show current time line
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            eventClick: function (info) {
                var shiftId = info.event.id;
                window.location.href = '/Shift/ViewShift/' + shiftId;
            },
            events: '/Shift/GetShifts'
        });

        calendar.render();
    } else {
        console.error('Calendar div not found!');
    }
});
